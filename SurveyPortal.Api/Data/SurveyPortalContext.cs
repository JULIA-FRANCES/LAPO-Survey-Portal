using Microsoft.EntityFrameworkCore;
using SurveyPortal.Api.Models;

namespace SurveyPortal.Api.Data;

public class SurveyPortalContext(DbContextOptions<SurveyPortalContext> options) : DbContext(options)
{
    public DbSet<Department> Departments => Set<Department>();
    public DbSet<Unit> Units => Set<Unit>();
    public DbSet<Question> Questions => Set<Question>();
    public DbSet<User> Users => Set<User>();
    public DbSet<Survey> Surveys => Set<Survey>();
    public DbSet<DeptSurveyAssignment> DeptSurveyAssignments => Set<DeptSurveyAssignment>();
    public DbSet<Submission> Submissions => Set<Submission>();
    public DbSet<Answer> Answers => Set<Answer>();
    public DbSet<UnitFeedback> UnitFeedback => Set<UnitFeedback>();

    protected override void OnModelCreating(ModelBuilder modelBuilder)
    {
        modelBuilder.Entity<Department>()
            .HasIndex(d => d.Name)
            .IsUnique();

        modelBuilder.Entity<Unit>()
            .HasIndex(u => new { u.DepartmentId, u.Name })
            .IsUnique();

        modelBuilder.Entity<User>()
            .HasIndex(u => u.StaffId)
            .IsUnique();

        modelBuilder.Entity<User>()
            .HasIndex(u => u.Email)
            .IsUnique();

        modelBuilder.Entity<DeptSurveyAssignment>()
            .HasIndex(a => new { a.SurveyId, a.RaterDepartmentId, a.RatedDepartmentId })
            .IsUnique();

        modelBuilder.Entity<DeptSurveyAssignment>()
            .HasOne(a => a.RaterDepartment)
            .WithMany()
            .HasForeignKey(a => a.RaterDepartmentId)
            .OnDelete(DeleteBehavior.Restrict);

        modelBuilder.Entity<DeptSurveyAssignment>()
            .HasOne(a => a.RatedDepartment)
            .WithMany()
            .HasForeignKey(a => a.RatedDepartmentId)
            .OnDelete(DeleteBehavior.Restrict);

        modelBuilder.Entity<Submission>()
            .HasIndex(s => new { s.SurveyId, s.RaterId, s.DepartmentId })
            .IsUnique();

        modelBuilder.Entity<Submission>()
            .HasOne(s => s.Rater)
            .WithMany()
            .HasForeignKey(s => s.RaterId)
            .OnDelete(DeleteBehavior.Restrict);

        modelBuilder.Entity<Submission>()
            .HasOne(s => s.Department)
            .WithMany()
            .HasForeignKey(s => s.DepartmentId)
            .OnDelete(DeleteBehavior.Restrict);

        modelBuilder.Entity<Answer>()
            .HasIndex(a => new { a.SubmissionId, a.UnitId, a.QuestionId })
            .IsUnique();

        modelBuilder.Entity<Answer>()
            .ToTable(t => t.HasCheckConstraint("CK_Answer_Rating", "Rating BETWEEN 1 AND 5"));

        modelBuilder.Entity<Answer>()
            .HasOne(a => a.Question)
            .WithMany()
            .HasForeignKey(a => a.QuestionId)
            .OnDelete(DeleteBehavior.NoAction);

        modelBuilder.Entity<UnitFeedback>()
            .HasIndex(f => new { f.SubmissionId, f.UnitId })
            .IsUnique();
    }
}
