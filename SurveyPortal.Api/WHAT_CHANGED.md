# What Changed, and Why (Beginner's Walkthrough)

This document explains everything that changed in this project recently, in the order it
happened, and — more importantly — *why* each change was made. If you're new to the codebase,
read this top to bottom once and you'll understand how we got from where we started to where
we are now.

---

## 1. The database shape was wrong, so we fixed it first

### The problem

Before anything else could be fixed, the database structure itself needed to make sense,
because **every other layer of the app (models, endpoints, DTOs) is built on top of it.**
Fixing endpoints before fixing the database would have meant fixing the same code twice.

The original database had a few issues:

- `Staff` (a person) stored `DepartmentId` *and* `UnitId` — but a unit already belongs to a
  department, so storing both meant the two could disagree with each other (redundant data).
- `EvaluationCycle` (a survey/cycle) had a manually-set `IsActive` flag, even though it already
  had `StartDate` and `EndDate` — the "is this survey active" answer should be *calculated* from
  the dates, not hand-flipped by a person and forgotten.
- There was no table recording **who is allowed to rate whom.** Nothing stopped one department
  from rating a department it was never assigned to.
- `Submission` didn't record *which department* was being rated or *who* the rater was — it just
  had answers floating around with no owner.

### The fix

We redesigned the schema around these rules:

| Old name | New name | What changed |
|---|---|---|
| `EvaluationCycle` | `Survey` | Dropped the manual `IsActive` flag. Status (`upcoming` / `open` / `closed`) is now *computed* from `StartDate`/`EndDate` every time it's read. |
| `Staff` | `User` | Dropped the redundant `DepartmentId`. A user's department is now always found through `user.Unit.DepartmentId`. |
| `Feedback` | `UnitFeedback` | Renamed for clarity — it's feedback about a specific unit. |
| *(new)* | `DeptSurveyAssignment` | Records "in this survey, department A is allowed to rate department B." This is the missing piece that makes rating permissions explicit instead of implied. |
| `Submission` | `Submission` | Added `SurveyId`, `RaterId`, `DepartmentId` so every submission clearly says *who* rated *which department* in *which survey*. Added `CreatedAt` and a nullable `SubmittedAt` (null = still a draft). |

We also added **unique constraints** at the database level — for example, one rater can only
have *one* submission per department per survey. This isn't just a nice-to-have: it means even
if a bug in the code tries to create a duplicate, the database itself will refuse and throw an
error, instead of silently corrupting data.

**Thought process:** always ask "what real-world fact is this table recording, and what would
make that fact contradict itself?" `Staff.DepartmentId` could disagree with `Staff.Unit.DepartmentId`
— that's the smell that told us it didn't belong.

---

## 2. Endpoints had to catch up to the new schema

Once the database shape changed, every endpoint that touched the renamed/restructured tables had
to change too — DTOs, model references, everything. We rebuilt:

- `SurveysEndpoints` (previously `EvaluationCyclesEndpoints` + `QuestionsEndpoints` merged, since
  questions belong to a specific survey now)
- `DepartmentsEndpoints`
- `SubmissionsEndpoints`

**Thought process:** when a database column moves or a table gets renamed, grep for every place
that mentions the old name (`dbContext.Staff`, `EvaluationCycle`, etc.) — that's your checklist
of what needs updating. Nothing gets left silently broken.

---

## 3. We added the missing CRUD endpoints for Surveys

The original code could only *read* surveys — there was no way to create one, or add questions
to one, through the API. We added:

```
POST   /surveys                                        create a survey
GET    /surveys                                         list all surveys
GET    /surveys/:id                                      one survey's detail (with computed status)
POST   /surveys/:id/questions                            add a question to a survey
POST   /surveys/:id/departments/:deptId/submission        get-or-create a rater's in-progress submission
```

The last one is the interesting one: **"get or create"** means if the rater already started
rating this department in this survey, you get back their *existing* draft submission (so they
can resume it). If they haven't started, a new blank one is created for them. This works because
of the unique constraint mentioned in step 1 — the database itself guarantees there can only
ever be one submission per (survey, rater, department) combination, so "find one, or make one"
is always a safe, unambiguous operation.

**Thought process:** an endpoint spec like `POST .../submission` (get-or-create) isn't
"weird REST" — it's a deliberate choice because starting a survey submission and resuming one
are the *same user action* from the rater's point of view. Modeling it as one endpoint instead
of two (`GET` to check + `POST` to create) avoids a race condition where two requests could both
decide "it doesn't exist yet" and both try to create one.

---

## 4. Added Swagger so the API is browsable

Swagger (an interactive API documentation page) was added so you can open a browser, go to
`/swagger`, and see every endpoint, what it expects, and try it out — without needing Postman or
writing `curl` commands by hand.

**Thought process:** this has nothing to do with the database or business logic — it's a
developer-experience tool. It was added independently of the schema work, and it will keep
working automatically as new endpoints get added, because it reads the routes directly from the
running app.

---

## 5. The biggest change: moving database code *out* of the endpoints

This is the one worth understanding in the most depth, because it changes how you should write
*any* new endpoint from now on.

### What the code looked like before

```csharp
group.MapGet("/{id}", async (int id, SurveyPortalContext dbContext) =>
{
    var survey = await dbContext.Surveys
        .Where(s => s.Id == id)
        .AsNoTracking()
        .FirstOrDefaultAsync();

    if (survey is null)
    {
        return Results.NotFound();
    }

    var dto = new SurveyDto(survey.Id, survey.Name, survey.StartDate, survey.EndDate, ComputeStatus(survey));
    return Results.Ok(dto);
});
```

This one function does two unrelated jobs: it talks to the database (`dbContext.Surveys...`) *and*
it decides what HTTP response to send back (`Results.NotFound()`, `Results.Ok()`). That's a sign
it should be split in two.

### Why that's a problem

- You can't test "does this return 404 for a missing survey?" without a real database running.
- The same `dbContext.Surveys.Where(...)` pattern gets copy-pasted into every place that needs a
  survey, so a bug fix has to be repeated everywhere it was pasted.
- If something's broken, you can't tell at a glance whether the bug is in the HTTP handling or
  the database query — they're tangled together.

### The fix: Repositories + Controllers

We split every endpoint file into two layers:

```
Controller  (HTTP concerns: routes, status codes, request/response shape)
     │
     ▼
Repository  (database concerns: the actual dbContext queries)
     │
     ▼
Database
```

**A repository** is a small class whose only job is talking to the database for one entity (e.g.
`Survey`). It has no idea HTTP exists.

```csharp
// Repositories/Interface/ISurveyRepository.cs
public interface ISurveyRepository
{
    Task<Survey?> GetByIdAsync(int id);
    Task<List<Survey>> GetAllAsync();
    Task<Survey?> GetActiveAsync(DateOnly today);
    Task AddAsync(Survey survey);
}
```

```csharp
// Repositories/SurveyRepository.cs
public class SurveyRepository(SurveyPortalContext dbContext) : ISurveyRepository
{
    public Task<Survey?> GetByIdAsync(int id) =>
        dbContext.Surveys.AsNoTracking().FirstOrDefaultAsync(s => s.Id == id);

    // ...
}
```

**A controller** is now the *only* place that decides HTTP status codes. It asks the repository
for data and turns the result into a response:

```csharp
[ApiController]
[Route("surveys")]
public class SurveysController(ISurveyRepository surveys) : ControllerBase
{
    [HttpGet("{id:int}")]
    public async Task<IActionResult> GetById(int id)
    {
        var survey = await surveys.GetByIdAsync(id);
        return survey is null ? NotFound() : Ok(ToDto(survey));
    }
}
```

Notice the controller method no longer contains the word `dbContext` anywhere. That's the whole
point — **if you ever type `dbContext` inside a `Controllers/*.cs` file, that's a sign the line
belongs in a repository instead.**

### Why "Controllers" specifically, and not just "repositories"?

We *also* moved away from the old "minimal API" style (`app.MapGet("/surveys/{id}", ...)`
written as a big lambda inside `Program.cs`-adjacent files) to ASP.NET Core's **Controller**
classes (`[ApiController]`, `[HttpGet]`, etc.). This is a more conventional, more discoverable
structure:

- Every route for `/surveys/*` lives in one obvious file: `Controllers/SurveysController.cs`.
- Anyone who's worked with ASP.NET Core before will recognize the pattern immediately — it's the
  standard, most-taught way to structure a Web API.
- Routes are declared with attributes (`[HttpGet("{id:int}")]`) right above the method that
  handles them, so you never have to hunt through a big lambda tree to find where a route is
  wired up.

### How dependency injection makes this work

You might notice the controller's constructor just *asks* for `ISurveyRepository` — it never
does `new SurveyRepository(...)` anywhere. This is called **dependency injection**, and the setup
lives in `Program.cs`:

```csharp
builder.Services.AddScoped<ISurveyRepository, SurveyRepository>();
```

Read this line as: *"whenever some class asks for an `ISurveyRepository`, hand it a
`SurveyRepository`."* You don't need to know how ASP.NET Core does this under the hood — just
that it means:

1. Controllers stay decoupled from the concrete database implementation.
2. Later, if you wanted to swap `SurveyRepository` for a fake one in a test (so tests don't need
   a real database), you only change this one line — nothing in the controller changes.

---

## The rule to carry forward

Every time you add a new endpoint from now on, ask yourself two questions:

1. **"Does this database query already have a repository method for it?"**
   If yes, call it. If no, add the method to the right `I<Entity>Repository` /
   `<Entity>Repository` pair (see [`REPOSITORY_GUIDE.md`](REPOSITORY_GUIDE.md) for a full
   worked example).
2. **"Am I about to type `dbContext` inside a file in `Controllers/`?"**
   If yes — stop. That line belongs in a repository, not the controller.

Following just those two rules is what keeps HTTP concerns and database concerns from getting
tangled back together as the project grows.
