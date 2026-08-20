using Microsoft.EntityFrameworkCore;


namespace SurveyPortal.Api.Data;

public static class DataExtensions
{
    public static void AddSurveyPortalDb(this WebApplicationBuilder builder)
    {
        var connString = builder.Configuration.GetConnectionString("SurveyPortalDb")
            ?? "Data Source=SurveyPortal.db";
        builder.Services.AddSqlite<SurveyPortalContext>(connString);
    }

    public static void MigrateDb(this WebApplication app)
    {
        using var scope = app.Services.CreateScope();
        var dbContext = scope.ServiceProvider.GetRequiredService<SurveyPortalContext>();
        dbContext.Database.Migrate();
    }
}
