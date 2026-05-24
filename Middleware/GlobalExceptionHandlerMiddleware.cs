using Microsoft.AspNetCore.Mvc;
using System.Net;
using System.Text.Json;

namespace LibrasJaChallenge.Middleware;

public class GlobalExceptionHandlerMiddleware
{
    private readonly RequestDelegate _next;
    private readonly ILogger<GlobalExceptionHandlerMiddleware> _logger;

    public GlobalExceptionHandlerMiddleware(RequestDelegate next, ILogger<GlobalExceptionHandlerMiddleware> logger)
    {
        _next = next;
        _logger = logger;
    }

    public async Task InvokeAsync(HttpContext context)
    {
        try
        {
            await _next(context);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Erro não tratado: {Mensagem}", ex.Message);
            await HandleExceptionAsync(context, ex);
        }
    }

    private static Task HandleExceptionAsync(HttpContext context, Exception ex)
    {
        var (status, title) = ex switch
        {
            ArgumentException        => (HttpStatusCode.BadRequest,   "Requisição inválida"),
            KeyNotFoundException     => (HttpStatusCode.NotFound,     "Recurso não encontrado"),
            UnauthorizedAccessException => (HttpStatusCode.Unauthorized, "Não autorizado"),
            InvalidOperationException => (HttpStatusCode.Conflict,    "Operação inválida"),
            _                        => (HttpStatusCode.InternalServerError, "Erro interno do servidor")
        };

        var problem = new ProblemDetails
        {
            Status   = (int)status,
            Title    = title,
            Detail   = ex.Message,
            Instance = context.Request.Path,
            Type     = $"https://httpstatuses.com/{(int)status}"
        };
        problem.Extensions["traceId"] = context.TraceIdentifier;

        context.Response.StatusCode  = (int)status;
        context.Response.ContentType = "application/problem+json";

        var json = JsonSerializer.Serialize(problem, new JsonSerializerOptions
        {
            PropertyNamingPolicy = JsonNamingPolicy.CamelCase
        });
        return context.Response.WriteAsync(json);
    }
}
