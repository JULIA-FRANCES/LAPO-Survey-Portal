# How to Stop Writing DB Logic in Endpoints

## The problem

Right now, our endpoint files (`Endpoints/SurveysEndpoints.cs`, `Endpoints/SubmissionsEndpoints.cs`, etc.)
do **two jobs at once**:

1. Handle the HTTP request (read the URL, read the body, decide what status code to return)
2. Talk directly to the database (`dbContext.Surveys.Where(...)`, `dbContext.SaveChangesAsync()`, etc.)

Example of what we have today, in `SurveysEndpoints.cs`:

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

This works, but it causes real problems as the app grows:

- **You can't test it easily.** To test "does GetById return null for a missing survey" you'd need
  a real database running.
- **You repeat yourself.** The same `dbContext.Surveys.Where(...)` pattern gets copy-pasted into
  every endpoint that needs a survey.
- **If the DB logic has a bug, you have to hunt through HTTP code to find it**, and vice versa.

## The fix: Repositories

A **repository** is just a class whose only job is talking to the database for one entity
(e.g. `Survey`). It has no idea that HTTP exists — no `Results.Ok()`, no status codes, nothing.

The endpoint's only job becomes: **read the request → call the repository → turn the result into
an HTTP response.**

```
Endpoint (HTTP concerns)  →  Repository (DB concerns)  →  Database
```

## Step by step: building `ISurveyRepository`

### 1. Create an interface

Put this in a new folder `Repositories/`. The interface describes *what* you can do with surveys,
not *how*.

```csharp
// Repositories/ISurveyRepository.cs
namespace SurveyPortal.Api.Repositories;

using SurveyPortal.Api.Models;

public interface ISurveyRepository
{
    Task<Survey?> GetByIdAsync(int id);
    Task<List<Survey>> GetAllAsync();
    Task<Survey?> GetActiveAsync(DateOnly today);
    Task AddAsync(Survey survey);
}
```

### 2. Write the implementation

This is where the actual EF Core / `dbContext` code lives — and it lives **here only**.

```csharp
// Repositories/SurveyRepository.cs
namespace SurveyPortal.Api.Repositories;

using Microsoft.EntityFrameworkCore;
using SurveyPortal.Api.Data;
using SurveyPortal.Api.Models;

public class SurveyRepository(SurveyPortalContext dbContext) : ISurveyRepository
{
    public Task<Survey?> GetByIdAsync(int id) =>
        dbContext.Surveys.AsNoTracking().FirstOrDefaultAsync(s => s.Id == id);

    public Task<List<Survey>> GetAllAsync() =>
        dbContext.Surveys.AsNoTracking().OrderByDescending(s => s.StartDate).ToListAsync();

    public Task<Survey?> GetActiveAsync(DateOnly today) =>
        dbContext.Surveys.AsNoTracking()
            .FirstOrDefaultAsync(s => today >= s.StartDate && today <= s.EndDate);

    public async Task AddAsync(Survey survey)
    {
        dbContext.Surveys.Add(survey);
        await dbContext.SaveChangesAsync();
    }
}
```

**Rule of thumb:** if a line of code mentions `dbContext`, it belongs in a repository, not an
endpoint.

### 3. Register it so the app knows how to create it

In `Program.cs`, add this near the top (next to `builder.AddSurveyPortalDb();`):

```csharp
builder.Services.AddScoped<ISurveyRepository, SurveyRepository>();
```

This tells the app: "whenever something asks for an `ISurveyRepository`, give it a
`SurveyRepository`." This is called **dependency injection** — you don't need to know how it
works internally, just that it lets you `ask for the interface` in your endpoint and the real
class shows up automatically.

### 4. Use it in the endpoint — no more `dbContext`

**Before:**

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

**After:**

```csharp
group.MapGet("/{id}", async (int id, ISurveyRepository surveys) =>
{
    var survey = await surveys.GetByIdAsync(id);

    if (survey is null)
    {
        return Results.NotFound();
    }

    var dto = new SurveyDto(survey.Id, survey.Name, survey.StartDate, survey.EndDate, ComputeStatus(survey));
    return Results.Ok(dto);
});
```

Notice: the endpoint no longer has the word `dbContext` anywhere. It just asks the repository
for a survey and decides what HTTP response to send back. That's its whole job now.

### 5. Do the same for `POST`

**Before:**

```csharp
group.MapPost("/", async (CreateSurveyDto newSurvey, SurveyPortalContext dbContext) =>
{
    var survey = new Survey
    {
        Name = newSurvey.Name,
        StartDate = newSurvey.StartDate,
        EndDate = newSurvey.EndDate
    };

    dbContext.Surveys.Add(survey);
    await dbContext.SaveChangesAsync();

    var dto = new SurveyDto(survey.Id, survey.Name, survey.StartDate, survey.EndDate, ComputeStatus(survey));
    return Results.Created($"/surveys/{survey.Id}", dto);
});
```

**After:**

```csharp
group.MapPost("/", async (CreateSurveyDto newSurvey, ISurveyRepository surveys) =>
{
    var survey = new Survey
    {
        Name = newSurvey.Name,
        StartDate = newSurvey.StartDate,
        EndDate = newSurvey.EndDate
    };

    await surveys.AddAsync(survey);

    var dto = new SurveyDto(survey.Id, survey.Name, survey.StartDate, survey.EndDate, ComputeStatus(survey));
    return Results.Created($"/surveys/{survey.Id}", dto);
});
```

## A trickier example: the submission "get or create" endpoint

Some endpoints do more than one DB thing (check the rater exists, check they're allowed to rate
this department, then find-or-create a submission). That's fine — it just means the repository
method does a bit more work, but it's *still one call* from the endpoint's point of view.

```csharp
// Repositories/ISubmissionRepository.cs
public interface ISubmissionRepository
{
    Task<int?> GetRaterDepartmentIdAsync(int raterId);
    Task<bool> IsDepartmentAssignedAsync(int surveyId, int raterDepartmentId, int ratedDepartmentId);
    Task<Submission> GetOrCreateAsync(int surveyId, int raterId, int departmentId);
}
```

```csharp
// Repositories/SubmissionRepository.cs
public class SubmissionRepository(SurveyPortalContext dbContext) : ISubmissionRepository
{
    public Task<int?> GetRaterDepartmentIdAsync(int raterId) =>
        dbContext.Users
            .Where(u => u.Id == raterId)
            .Select(u => (int?)u.Unit!.DepartmentId)
            .FirstOrDefaultAsync();

    public Task<bool> IsDepartmentAssignedAsync(int surveyId, int raterDepartmentId, int ratedDepartmentId) =>
        dbContext.DeptSurveyAssignments.AnyAsync(a =>
            a.SurveyId == surveyId &&
            a.RaterDepartmentId == raterDepartmentId &&
            a.RatedDepartmentId == ratedDepartmentId);

    public async Task<Submission> GetOrCreateAsync(int surveyId, int raterId, int departmentId)
    {
        var submission = await dbContext.Submissions.FirstOrDefaultAsync(s =>
            s.SurveyId == surveyId && s.RaterId == raterId && s.DepartmentId == departmentId);

        if (submission is not null)
        {
            return submission;
        }

        submission = new Submission
        {
            SurveyId = surveyId,
            RaterId = raterId,
            DepartmentId = departmentId,
            CreatedAt = DateTime.UtcNow,
            SubmittedAt = null
        };

        dbContext.Submissions.Add(submission);
        await dbContext.SaveChangesAsync();
        return submission;
    }
}
```

The endpoint now reads like a short story instead of a wall of query code:

```csharp
group.MapPost("/{surveyId}/departments/{deptId}/submission", async (
    int surveyId, int deptId, GetOrCreateSubmissionDto request, ISubmissionRepository submissions) =>
{
    var raterDepartmentId = await submissions.GetRaterDepartmentIdAsync(request.RaterId);
    if (raterDepartmentId is null)
    {
        return Results.BadRequest("Rater not found.");
    }

    var isAssigned = await submissions.IsDepartmentAssignedAsync(surveyId, raterDepartmentId.Value, deptId);
    if (!isAssigned)
    {
        return Results.BadRequest("This department is not in scope for the rater in this survey.");
    }

    var submission = await submissions.GetOrCreateAsync(surveyId, request.RaterId, deptId);

    return Results.Ok(new SubmissionSummaryDto(
        submission.Id, submission.SurveyId, submission.RaterId,
        submission.DepartmentId, submission.CreatedAt, submission.SubmittedAt));
});
```

## The checklist to apply this everywhere

For every endpoint file in `Endpoints/`:

1. Look for the word `dbContext`. Every line that uses it is DB logic.
2. Ask: "does a repository for this entity already exist?" If not, create
   `I<Entity>Repository` + `<Entity>Repository` following the pattern above.
3. Add a method to the interface named after *what it does*, not *how it does it*
   (`GetByIdAsync`, not `RunSelectQuery`).
4. Move the `dbContext...` code into that method, in the `Repository` class.
5. Register the repository in `Program.cs`:
   `builder.Services.AddScoped<IThingRepository, ThingRepository>();`
6. In the endpoint, replace the `SurveyPortalContext dbContext` parameter with the
   interface (e.g. `ISurveyRepository surveys`), and call the new method instead.
7. Rebuild and re-test the endpoint — the HTTP behavior should be identical, only the
   internal wiring changed.

## Rules to remember

- **Endpoints never say `dbContext`.** If you type it in an `Endpoints/*.cs` file, stop —
  that line belongs in a repository.
- **One repository per entity** (`SurveyRepository`, `SubmissionRepository`,
  `DepartmentRepository`, `UserRepository`, `QuestionRepository`...). Don't make one giant
  repository for everything.
- **Repository methods return models (`Survey`, `Submission`), not DTOs.** Turning a model into
  a DTO (`SurveyDto`) is an HTTP-shaping decision — that stays in the endpoint.
- **Repositories don't know about `Results.Ok()` / `Results.NotFound()`.** They just return
  data, or `null` when something isn't found. The endpoint decides what that means for HTTP.
- **Always use the interface (`ISurveyRepository`), not the class (`SurveyRepository`), in your
  endpoint's parameters.** This is what makes the code testable later — you can swap in a fake
  repository in a test without touching a real database.
