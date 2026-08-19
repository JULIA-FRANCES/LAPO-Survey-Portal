using SurveyPortal.Api.Data;
using SurveyPortal.Api.Endpoints;

var builder = WebApplication.CreateBuilder(args);

builder.AddSurveyPortalDb();

var app = builder.Build();

if (app.Environment.IsDevelopment())
{
    app.MapOpenApi();
}

app.UseHttpsRedirection();

app.MapDepartmentsEndpoints();
app.MapQuestionsEndpoints();
app.MapEvaluationCyclesEndpoints();
app.MapSubmissionsEndpoints();

app.MigrateDb();
app.SeedData();

app.Run();