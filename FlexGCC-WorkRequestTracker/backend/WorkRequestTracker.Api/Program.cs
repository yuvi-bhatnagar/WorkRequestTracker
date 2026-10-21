using System.Text.Json.Serialization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using WorkRequestTracker.Api.Data;
using WorkRequestTracker.Api.Middleware;
using WorkRequestTracker.Api.Services;

var builder = WebApplication.CreateBuilder(args);

// --- Database ---
var dbPath = Path.Combine(
    builder.Environment.ContentRootPath,
    builder.Configuration.GetValue<string>("Database:Path") ?? "workrequests.db");

builder.Services.AddDbContext<AppDbContext>(options =>
    options.UseSqlite($"Data Source={dbPath}"));

// --- Services ---
builder.Services.AddScoped<IWorkRequestService, WorkRequestService>();

// --- Controllers ---
builder.Services.AddControllers()
    .ConfigureApiBehaviorOptions(options =>
    {
        options.InvalidModelStateResponseFactory = context =>
        {
            var problem = new ValidationProblemDetails(context.ModelState)
            {
                Status = StatusCodes.Status400BadRequest,
                Title = "One or more validation errors occurred.",
                Instance = context.HttpContext.Request.Path,
            };

            return new BadRequestObjectResult(problem);
        };
    })
    .AddJsonOptions(opts =>
    {
        opts.JsonSerializerOptions.Converters.Add(new JsonStringEnumConverter());
        opts.JsonSerializerOptions.PropertyNamingPolicy = System.Text.Json.JsonNamingPolicy.CamelCase;
    });

// --- CORS ---
var allowedOrigins = builder.Configuration
    .GetSection("Cors:AllowedOrigins")
    .Get<string[]>() ?? new[] { "http://localhost:5173" };

builder.Services.AddCors(options =>
    options.AddPolicy("FrontendPolicy", policy =>
        policy.WithOrigins(allowedOrigins)
              .AllowAnyHeader()
              .AllowAnyMethod()));

// --- Swagger ---
builder.Services.AddEndpointsApiExplorer();
builder.Services.AddSwaggerGen(c =>
{
    c.SwaggerDoc("v1", new()
    {
        Title = "Work Request Tracker API",
        Version = "v1",
        Description = "API for managing work requests and notes.",
    });
});

var app = builder.Build();

// --- Exception handling ---
app.UseGlobalExceptionHandler();

// --- Swagger UI (always on for assignment review) ---
app.UseSwagger();
app.UseSwaggerUI(c =>
{
    c.SwaggerEndpoint("/swagger/v1/swagger.json", "Work Request Tracker API v1");
    c.RoutePrefix = "swagger";
});

app.UseHttpsRedirection();
app.UseCors("FrontendPolicy");
app.MapControllers();

// --- Database initialization ---
using (var scope = app.Services.CreateScope())
{
    var db = scope.ServiceProvider.GetRequiredService<AppDbContext>();
    await DbInitializer.InitializeAsync(db);
}

app.Run();

// Make Program partial for integration testing
public partial class Program { }
