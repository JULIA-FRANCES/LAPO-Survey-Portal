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

        db.Questions.AddRange(
            new Question { Text = "How effectively does this department communicate with your team (clarity, timeliness, and appropriateness of information shared)?", DisplayOrder = 1 },
            new Question { Text = "How promptly does this department respond to your requests, queries, and escalations?", DisplayOrder = 2 },
            new Question { Text = "How would you rate the professionalism, courteousness, and overall attitude of staff in this department?", DisplayOrder = 3 },
            new Question { Text = "How knowledgeable and competent is this department's staff in their area of expertise?", DisplayOrder = 4 },
            new Question { Text = "How would you rate the overall quality and reliability of services delivered by this department?", DisplayOrder = 5 },
            new Question { Text = "How well does this department collaborate with your team to achieve shared organisational goals?", DisplayOrder = 6 },
            new Question { Text = "How consistently does this department meet agreed timelines and deliver on commitments?", DisplayOrder = 7 },
            new Question { Text = "How effectively does this department resolve issues, disputes, or complaints raised by your team?", DisplayOrder = 8 }
        );

        db.EvaluationCycles.Add(new EvaluationCycle
        {
            Name = "Q3 2026",
            StartDate = DateOnly.FromDateTime(DateTime.Today),
            EndDate = DateOnly.FromDateTime(DateTime.Today.AddDays(30)),
            IsActive = true,
            MinDepartmentsRequired = 5
        });

        db.SaveChanges();

        db.Staff.Add(new Staff
        {
            StaffId = "SN19876",
            Name = "Oluwaseun Adeyemi",
            Email = "oluwaseun.adeyemi@lapo-nigeria.org",
            Location = "Head Office Lagos",
            DepartmentId = finance.Id,
            UnitId = finance.Units.First().Id
        });

        db.SaveChanges();
    }
}