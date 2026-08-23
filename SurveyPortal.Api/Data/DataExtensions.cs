using Microsoft.EntityFrameworkCore;


namespace SurveyPortal.Api.Data;

public static class DataExtensions
{
    public static void AddSurveyPortalDb(this WebApplicationBuilder builder)
    {
        var connString = builder.Configuration.GetConnectionString("SurveyPortalDb")
            ?? throw new InvalidOperationException("Connection string 'SurveyPortalDb' is not configured.");

        if (Uri.TryCreate(connString, UriKind.Absolute, out var uri) && uri.Scheme == "mysql")
        {
            var credentials = uri.UserInfo.Split(':', 2);
            var connectionBuilder = new MySqlConnector.MySqlConnectionStringBuilder
            {
                Server = uri.Host,
                Port = uri.IsDefaultPort ? 3306u : (uint)uri.Port,
                Database = uri.AbsolutePath.Trim('/'),
                UserID = Uri.UnescapeDataString(credentials[0]),
                Password = credentials.Length > 1 ? Uri.UnescapeDataString(credentials[1]) : string.Empty,
                SslMode = uri.Query.Contains("ssl-mode=VERIFY_IDENTITY", StringComparison.OrdinalIgnoreCase)
                    ? MySqlConnector.MySqlSslMode.VerifyFull
                    : MySqlConnector.MySqlSslMode.Required
            };

            connString = connectionBuilder.ConnectionString;
        }

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
