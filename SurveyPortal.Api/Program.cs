using SurveyPortal.Api.Data;
using SurveyPortal.Api.Repositories;
using SurveyPortal.Api.Repositories.Interface;

var builder = WebApplication.CreateBuilder(args);

builder.AddSurveyPortalDb();

builder.Services.AddControllers();
builder.Services.AddEndpointsApiExplorer();
builder.Services.AddSwaggerGen();

builder.Services.AddCors(options =>
{
    options.AddPolicy("AllowFrontend", policy =>
    {
        policy.WithOrigins("http://localhost:5173")
              .AllowAnyHeader()
              .AllowAnyMethod();
    });
});

builder.Services.AddScoped<ISurveyRepository, SurveyRepository>();
builder.Services.AddScoped<IQuestionRepository, QuestionRepository>();
builder.Services.AddScoped<IDeptSurveyAssignmentRepository, DeptSurveyAssignmentRepository>();
builder.Services.AddScoped<IUserRepository, UserRepository>();
builder.Services.AddScoped<IDepartmentRepository, DepartmentRepository>();
builder.Services.AddScoped<ISubmissionRepository, SubmissionRepository>();

var app = builder.Build();

app.UseSwagger();

app.UseSwaggerUI(options =>
{
    options.SwaggerEndpoint(
        "/swagger/v1/swagger.json",
        "SurveyPortal API v1"
    );
});

app.UseHttpsRedirection();

app.UseCors("AllowFrontend");

app.MapControllers();

app.MigrateDb();
app.SeedData();

app.Run();