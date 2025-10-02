using Microsoft.EntityFrameworkCore;
using FluentValidation;
using FormDesigner.API.Data;
using FormDesigner.API.Data.Repositories;
using FormDesigner.API.Services;
using FormDesigner.API.Validators;
using FormDesigner.API.Models.DTOs;
using System.Text.Json;
using System.Text.Json.Serialization;

var builder = WebApplication.CreateBuilder(args);

// Add services to the container
builder.Services.AddControllers()
    .AddJsonOptions(options =>
    {
        // Use snake_case naming for JSON serialization (DSL v0.1 compliance)
        options.JsonSerializerOptions.PropertyNamingPolicy = JsonNamingPolicy.SnakeCaseLower;
        options.JsonSerializerOptions.DefaultIgnoreCondition = JsonIgnoreCondition.WhenWritingNull;
        options.JsonSerializerOptions.WriteIndented = false;
    });

// Configure Database
builder.Services.AddDbContext<ApplicationDbContext>(options =>
{
    var connectionString = builder.Configuration.GetConnectionString("FormDesignerDb");
    options.UseNpgsql(connectionString);

    // Enable sensitive data logging in development
    if (builder.Environment.IsDevelopment())
    {
        options.EnableSensitiveDataLogging();
    }
});

// Register Repositories
builder.Services.AddScoped<IFormRepository, FormRepository>();
builder.Services.AddScoped<IFormInstanceRepository, FormInstanceRepository>();

// Register Services
builder.Services.AddScoped<IFormBuilderService, FormBuilderService>();
builder.Services.AddScoped<IYamlExportService, YamlExportService>();
builder.Services.AddScoped<IYamlImportService, YamlImportService>();
builder.Services.AddScoped<ISqlGeneratorService, SqlGeneratorService>();
builder.Services.AddScoped<IRuntimeService, RuntimeService>();

// Register Validators
builder.Services.AddScoped<IValidator<FormDefinitionRoot>, FormDefinitionValidator>();

// Add FluentValidation
builder.Services.AddValidatorsFromAssemblyContaining<FormDefinitionValidator>();

// Configure CORS
builder.Services.AddCors(options =>
{
    options.AddDefaultPolicy(policy =>
    {
        policy.AllowAnyOrigin()
              .AllowAnyMethod()
              .AllowAnyHeader();
    });
});

// Configure Swagger/OpenAPI (disabled for .NET 10 preview compatibility)
// builder.Services.AddEndpointsApiExplorer();
// builder.Services.AddSwaggerGen();

var app = builder.Build();

// Configure the HTTP request pipeline
// Swagger disabled for .NET 10 preview compatibility
// if (app.Environment.IsDevelopment())
// {
//     app.UseSwagger();
//     app.UseSwaggerUI(options =>
//     {
//         options.SwaggerEndpoint("/swagger/v1/swagger.json", "Form Designer API v1");
//         options.RoutePrefix = "swagger";
//     });
// }

// Enable CORS
app.UseCors();

// Enable HTTPS redirection
app.UseHttpsRedirection();

// Serve static files from wwwroot
app.UseStaticFiles();

// Enable routing
app.UseRouting();

// Map controllers
app.MapControllers();

// Default route to serve index.html
app.MapGet("/", () => Results.Redirect("/index.html"));

app.Run();

// Make Program class accessible for integration tests
public partial class Program { }
