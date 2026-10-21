using System.Text.Json;
using Microsoft.AspNetCore.Diagnostics;
using Microsoft.AspNetCore.Mvc;

namespace WorkRequestTracker.Api.Middleware;

public static class GlobalExceptionHandler
{
    public static void UseGlobalExceptionHandler(this WebApplication app)
    {
        app.UseExceptionHandler(errApp =>
        {
            errApp.Run(async context =>
            {
                context.Response.StatusCode = StatusCodes.Status500InternalServerError;
                context.Response.ContentType = "application/problem+json";

                var feature = context.Features.Get<IExceptionHandlerFeature>();
                var exception = feature?.Error;

                var env = context.RequestServices.GetRequiredService<IWebHostEnvironment>();
                var logger = context.RequestServices.GetRequiredService<ILogger<Program>>();
                logger.LogError(exception, "Unhandled exception.");

                var problem = new ProblemDetails
                {
                    Status = StatusCodes.Status500InternalServerError,
                    Title = "An unexpected error occurred.",
                    Detail = env.IsDevelopment() ? exception?.Message : "Please try again later.",
                    Instance = context.Request.Path,
                };

                await context.Response.WriteAsync(JsonSerializer.Serialize(problem));
            });
        });
    }
}
