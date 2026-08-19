using Microsoft.EntityFrameworkCore;
using SurveyPortal.Api.Models;

namespace SurveyPortal.Api.Data;

public class SurveyPortalContext(DbContextOptions<SurveyPortalContext> options) : DbContext(options)
{
    public DbSet<Department> Departments => Set<Department>();
    public DbSet<Unit> Units => Set<Unit>();
   public DbSet<Question> Questions => Set<Question>();
    public DbSet<Staff> Staff => Set<Staff>();
    public DbSet<EvaluationCycle> EvaluationCycles => Set<EvaluationCycle>();
    public DbSet<Submission> Submissions => Set<Submission>();
    public DbSet<Answer> Answers => Set<Answer>();
    public DbSet<CompletionRecord> CompletionRecords => Set<CompletionRecord>();
    public DbSet<Feedback> Feedback => Set<Feedback>();
}


