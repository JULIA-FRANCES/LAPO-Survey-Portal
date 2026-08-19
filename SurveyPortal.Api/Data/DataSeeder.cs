using SurveyPortal.Api.Models;

namespace SurveyPortal.Api.Data;

public static class DataSeeder
{
    public static void SeedData(this WebApplication app)
    {
        using var scope = app.Services.CreateScope();
        var db = scope.ServiceProvider.GetRequiredService<SurveyPortalContext>();

        if (db.Departments.Any())
        {
            return; // already seeded
        }

        var risk = new Department { Name = "Enterprise Risk Management" };
        var finance = new Department { Name = "Finance" };

        risk.Units.Add(new Unit { Name = "BCMS Manager" });
        risk.Units.Add(new Unit { Name = "Internal Control" });
        risk.Units.Add(new Unit { Name = "Risk Management" });

        finance.Units.Add(new Unit { Name = "Accounts and Reporting" });

        db.Departments.AddRange(risk, finance);

        var survey = new Survey
        {
            Name = "Q3 2026",
            StartDate = DateOnly.FromDateTime(DateTime.Today),
            EndDate = DateOnly.FromDateTime(DateTime.Today.AddDays(30))
        };

        survey.Questions.AddRange(
        [
            new Question { Text = "How effectively does this department communicate with your team (clarity, timeliness, and appropriateness of information shared)?", SortOrder = 1, IsActive = true },
            new Question { Text = "How promptly does this department respond to your requests, queries, and escalations?", SortOrder = 2, IsActive = true },
            new Question { Text = "How would you rate the professionalism, courteousness, and overall attitude of staff in this department?", SortOrder = 3, IsActive = true },
            new Question { Text = "How knowledgeable and competent is this department's staff in their area of expertise?", SortOrder = 4, IsActive = true },
            new Question { Text = "How would you rate the overall quality and reliability of services delivered by this department?", SortOrder = 5, IsActive = true },
            new Question { Text = "How well does this department collaborate with your team to achieve shared organisational goals?", SortOrder = 6, IsActive = true },
            new Question { Text = "How consistently does this department meet agreed timelines and deliver on commitments?", SortOrder = 7, IsActive = true },
            new Question { Text = "How effectively does this department resolve issues, disputes, or complaints raised by your team?", SortOrder = 8, IsActive = true }
        ]);

        db.Surveys.Add(survey);

        db.SaveChanges();

        // Each department rates the other in this survey.
        db.DeptSurveyAssignments.AddRange(
            new DeptSurveyAssignment { SurveyId = survey.Id, RaterDepartmentId = finance.Id, RatedDepartmentId = risk.Id },
            new DeptSurveyAssignment { SurveyId = survey.Id, RaterDepartmentId = risk.Id, RatedDepartmentId = finance.Id }
        );

        db.Users.Add(new User
        {
            StaffId = "SN19876",
            Name = "Oluwaseun Adeyemi",
            Email = "oluwaseun.adeyemi@lapo-nigeria.org",
            Location = "Head Office Lagos",
            UnitId = finance.Units.First().Id
        });

        db.SaveChanges();
    }
}
