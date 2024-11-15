using Microsoft.AspNetCore.Http;
using Microsoft.Extensions.Logging;
using System;
using System.Net;
using System.Threading.Tasks;

public class ErrorHandlingMiddleware
{
    private readonly RequestDelegate _next;
    private readonly ILogger<ErrorHandlingMiddleware> _logger;

    public ErrorHandlingMiddleware(RequestDelegate next, ILogger<ErrorHandlingMiddleware> logger)
    {
        _next = next ?? throw new ArgumentNullException(nameof(next));
        _logger = logger ?? throw new ArgumentNullException(nameof(logger));
    }

    public async Task InvokeAsync(HttpContext httpContext)
    {
        try
        {
            await _next(httpContext);  // Выполняет следующий этап в пайплайне
        }
        catch (Exception ex)
        {
            await HandleExceptionAsync(httpContext, ex);  // Обработка ошибки
        }
    }

    private Task HandleExceptionAsync(HttpContext context, Exception exception)
    {
        // Логируем ошибку
        _logger.LogError(exception, "An unhandled exception occurred");

        // Отправляем ответ с ошибкой
        context.Response.StatusCode = (int)HttpStatusCode.InternalServerError;  // 500
        context.Response.ContentType = "application/json";

        var response = new
        {
            message = "An unexpected error occurred. Please try again later.",
            detail = exception.Message
        };

        return context.Response.WriteAsJsonAsync(response);
    }
}
