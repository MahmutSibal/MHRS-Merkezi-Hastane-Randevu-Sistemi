using System.Net;
using FluentValidation;
using WebAppointmentApi.Application.Common.Exceptions;

namespace WebAppointmentApi.WebApi.Middleware;

public sealed class ExceptionHandlingMiddleware : IMiddleware
{
    private readonly ILogger<ExceptionHandlingMiddleware> _logger;

    public ExceptionHandlingMiddleware(ILogger<ExceptionHandlingMiddleware> logger)
    {
        _logger = logger;
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

            var status = ex switch
            {
                ValidationException => (int)HttpStatusCode.BadRequest,
                UnauthorizedException => (int)HttpStatusCode.Unauthorized,
                ForbiddenException => (int)HttpStatusCode.Forbidden,
                NotFoundException => (int)HttpStatusCode.NotFound,
                ConflictException => (int)HttpStatusCode.Conflict,
                TooManyRequestsException => StatusCodes.Status429TooManyRequests,
                EmailVerificationRequiredException => (int)HttpStatusCode.Forbidden,
                _ => (int)HttpStatusCode.InternalServerError
            };

            context.Response.StatusCode = status;
            context.Response.ContentType = "text/plain; charset=utf-8";

            // Son kullanıcı mesajlarını daima kullanıcı dostu ve kod/numara içermeyen şekilde ver.
            string userMessage;
            if (ex is ValidationException vex)
            {
                // İlk hata mesajını yüzeye çıkar
                var first = vex.Errors?.FirstOrDefault();
                userMessage = first?.ErrorMessage ?? "Geçersiz giriş. Lütfen bilgileri kontrol edin.";
            }
            else if (ex is UnauthorizedException)
            {
                userMessage = string.IsNullOrWhiteSpace(ex.Message) ? "Oturum açmanız gerekiyor." : ex.Message;
            }
            else if (ex is ForbiddenException)
            {
                userMessage = string.IsNullOrWhiteSpace(ex.Message) ? "Bu işlemi yapma izniniz yok." : ex.Message;
            }
            else if (ex is NotFoundException)
            {
                userMessage = string.IsNullOrWhiteSpace(ex.Message) ? "Aradığınız kayıt bulunamadı." : ex.Message;
            }
            else if (ex is ConflictException)
            {
                userMessage = string.IsNullOrWhiteSpace(ex.Message) ? "İstek mevcut durumla çakışıyor." : ex.Message;
            }
            else if (ex is TooManyRequestsException)
            {
                userMessage = string.IsNullOrWhiteSpace(ex.Message) ? "Çok fazla istek. Lütfen daha sonra tekrar deneyin." : ex.Message;
            }
            else if (ex is EmailVerificationRequiredException)
            {
                // Sabit bir kod: frontend bu metni "EMAIL_NOT_VERIFIED" olarak tanıyıp doğrulama ekranına geçiyor.
                userMessage = ex.Message;
            }
            else
            {
                userMessage = "Beklenmeyen bir hata oluştu.";
            }

            await context.Response.WriteAsync(userMessage);
        }
    }
}
