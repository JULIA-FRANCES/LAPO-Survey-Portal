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
            var departments = db.Departments.ToList();
            var existingAssignments = db.DeptSurveyAssignments.ToList();

            foreach (var existingSurvey in db.Surveys.ToList())
            {
                foreach (var rater in departments)
                {
                    foreach (var rated in departments.Where(department => department.Id != rater.Id))
                    {
                        if (existingAssignments.Any(assignment =>
                            assignment.SurveyId == existingSurvey.Id &&
                            assignment.RaterDepartmentId == rater.Id &&
                            assignment.RatedDepartmentId == rated.Id))
                        {
                            continue;
                        }

                        var assignment = new DeptSurveyAssignment
                        {
                            SurveyId = existingSurvey.Id,
                            RaterDepartmentId = rater.Id,
                            RatedDepartmentId = rated.Id
                        };
                        db.DeptSurveyAssignments.Add(assignment);
                        existingAssignments.Add(assignment);
                    }
                }
            }

            db.SaveChanges();
            return; // already seeded
        }

        var risk = new Department { Name = "Enterprise Risk Management" };
        var finance = new Department { Name = "Finance" };
        var tech = new Department { Name = "Information Technology" };
        var hr = new Department { Name = "Human Resources" };
        var marketing = new Department { Name = "Marketing" };
        var operations = new Department { Name = "Operations" };
        var customerSuccess = new Department { Name = "Customer Success" };
        var executive = new Department { Name = "Executive Leadership" };
        var qa = new Department { Name = "Quality Assurance" };
        var procurement = new Department { Name = "Procurement" };
        var sales = new Department { Name = "Sales" };
        var product = new Department { Name = "Product" };
        var legal = new Department { Name = "Legal & Compliance" };
        var rnd = new Department { Name = "Research & Development" };

        risk.Units.Add(new Unit { Name = "BCMS Manager" });
        risk.Units.Add(new Unit { Name = "Internal Control" });
        risk.Units.Add(new Unit { Name = "Risk Management" });

        finance.Units.Add(new Unit { Name = "Accounts and Reporting" });
        finance.Units.Add(new Unit { Name = "Banking & Treasury Operations" });
        finance.Units.Add(new Unit { Name = "Accounts Payable & Receivable" });
        finance.Units.Add(new Unit { Name = "Financial Planning & Analysis (FP&A)" });
        finance.Units.Add(new Unit { Name = "Payroll & Tax" });

        tech.Units.Add(new Unit { Name = "Cybersecurity & Compliance" });
        tech.Units.Add(new Unit { Name = "Software Engineering" });
        tech.Units.Add(new Unit { Name = "IT Support & Helpdesk" });
        tech.Units.Add(new Unit { Name = "DevOps & Cloud Infrastructure" });

        hr.Units.Add(new Unit { Name = "Talent Acquisition & Recruitment" });
        hr.Units.Add(new Unit { Name = "Employee Relations & Culture" });
        hr.Units.Add(new Unit { Name = "Compensation & Benefits" });
        hr.Units.Add(new Unit { Name = "Learning & Development (L&D)" });

       
        marketing.Units.Add(new Unit { Name = "Digital Marketing & Growth" });
        marketing.Units.Add(new Unit { Name = "Content & Communications" });
        marketing.Units.Add(new Unit { Name = "Brand Strategy" });

        
        operations.Units.Add(new Unit { Name = "Facilities & Workplace Management" });
        operations.Units.Add(new Unit { Name = "Supply Chain & Logistics" });
        operations.Units.Add(new Unit { Name = "Business Process Improvement" });

        customerSuccess.Units.Add(new Unit { Name = "Customer Experience & Support" });
        customerSuccess.Units.Add(new Unit { Name = "Account Management" });

       
        sales.Units.Add(new Unit { Name = "Enterprise Sales" });
        sales.Units.Add(new Unit { Name = "Channel Partners" });
        sales.Units.Add(new Unit { Name = "Inside Sales" });

       
        product.Units.Add(new Unit { Name = "Product Management" });
        product.Units.Add(new Unit { Name = "User Research & Design" });

        
        legal.Units.Add(new Unit { Name = "Corporate Legal" });
        legal.Units.Add(new Unit { Name = "Regulatory Compliance" });

       
        rnd.Units.Add(new Unit { Name = "Product Innovation" });
        rnd.Units.Add(new Unit { Name = "Technology Research" });

       
        procurement.Units.Add(new Unit { Name = "Strategic Sourcing" });
        procurement.Units.Add(new Unit { Name = "Vendor Management" });

        
        qa.Units.Add(new Unit { Name = "Quality Control" });
        qa.Units.Add(new Unit { Name = "Process Improvement" });

        
        executive.Units.Add(new Unit { Name = "Strategy & Planning" });
        executive.Units.Add(new Unit { Name = "Corporate Affairs" });

        db.Departments.AddRange(
            risk, finance, tech, hr,
            marketing, operations, customerSuccess, sales, product,
            legal, rnd, procurement, qa, executive
        );
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

        foreach (var rater in db.Departments.Local)
        {
            foreach (var rated in db.Departments.Local.Where(department => department.Id != rater.Id))
            {
                db.DeptSurveyAssignments.Add(new DeptSurveyAssignment
                {
                    SurveyId = survey.Id,
                    RaterDepartmentId = rater.Id,
                    RatedDepartmentId = rated.Id
                });
            }
        }

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
