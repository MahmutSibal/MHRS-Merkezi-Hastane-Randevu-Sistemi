using System.Net;
using System.Diagnostics;
using FluentValidation;
using Microsoft.AspNetCore.Mvc;
using WebAppointmentApi.Application.Common.Exceptions;

namespace WebAppointmentApi.WebApi.Middleware;

public sealed class ExceptionHandlingMiddleware : IMiddleware
{
    private readonly ILogger<ExceptionHandlingMiddleware> _logger;
    private readonly IWebHostEnvironment _env;

    public ExceptionHandlingMiddleware(ILogger<ExceptionHandlingMiddleware> logger, IWebHostEnvironment env)
    {
        _logger = logger;
        _env = env;
    }

    public async Task InvokeAsync(HttpContext context, RequestDelegate next)
    {
        try
        {
            await next(context);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Unhandled exception");

            var (status, title) = ex switch
            {
                ValidationException => ((int)HttpStatusCode.BadRequest, "Validation error"),
                UnauthorizedException => ((int)HttpStatusCode.Unauthorized, "Unauthorized"),
                ForbiddenException => ((int)HttpStatusCode.Forbidden, "Forbidden"),
                NotFoundException => ((int)HttpStatusCode.NotFound, "Not found"),
                ConflictException => ((int)HttpStatusCode.Conflict, "Conflict"),
                _ => ((int)HttpStatusCode.InternalServerError, "Server error")
            };

            context.Response.StatusCode = status;
            context.Response.ContentType = "application/problem+json";

            var details = new ProblemDetails
            {
                Status = status,
                Title = title,
                Detail = status >= 500
                    ? (_env.IsDevelopment() ? ex.ToString() : "An unexpected error occurred.")
                    : ex.Message,
                Instance = context.Request.Path,
            };

            details.Extensions["traceId"] = Activity.Current?.Id ?? context.TraceIdentifier;

            await context.Response.WriteAsJsonAsync(details);
        }
    }
}
