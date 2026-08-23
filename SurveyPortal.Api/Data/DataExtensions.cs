using Microsoft.EntityFrameworkCore;


namespace SurveyPortal.Api.Data;

public static class DataExtensions
{
    public static void AddSurveyPortalDb(this WebApplicationBuilder builder)
    {
        var connString = builder.Configuration.GetConnectionString("SurveyPortalDb")
            ?? throw new InvalidOperationException("Connection string 'SurveyPortalDb' is not configured.");
        builder.Services.AddDbContext<SurveyPortalContext>(options =>
            options.UseMySql(connString, new MySqlServerVersion(new Version(8, 0, 0))));
    }

    public static void MigrateDb(this WebApplication app)
    {
        using var scope = app.Services.CreateScope();
        var dbContext = scope.ServiceProvider.GetRequiredService<SurveyPortalContext>();
        dbContext.Database.Migrate();
    }
}
