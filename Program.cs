using LibrasJa.Infrastructure.Data;
using LibrasJa.Infrastructure.Repositories;
using LibrasJa.Application.Interfaces;
using LibrasJ·Challenge.DTOs;
using Microsoft.EntityFrameworkCore;
using LibrasJa.Domain.Entities;
using Serilog;
using Microsoft.AspNetCore.Diagnostics.HealthChecks;
using HealthChecks.UI.Client;
using OpenTelemetry.Resources;
using OpenTelemetry.Trace;
using OpenTelemetry.Metrics;

// =============== SERILOG =================
Log.Logger = new LoggerConfiguration()
    .MinimumLevel.Information()
    .WriteTo.Console()
    .WriteTo.File("logs/librasj·-.txt", rollingInterval: RollingInterval.Day)
    .Enrich.FromLogContext()
    .CreateLogger();

var builder = WebApplication.CreateBuilder(args);
builder.Host.UseSerilog();

// =============== DB =================
builder.Services.AddDbContext<AppDbContext>(options =>
    options.UseOracle(builder.Configuration.GetConnectionString("Oracle")));

// =============== DI =================
builder.Services.AddScoped<IUserRepository, UserRepository>();
builder.Services.AddScoped<IInterpreterProfileRepository, InterpreterProfileRepository>();

// =============== HEALTH CHECKS =================
builder.Services.AddHealthChecks()
    .AddDbContextCheck<AppDbContext>("oracle-db")
    .AddCheck("api-self", () => Microsoft.Extensions.Diagnostics.HealthChecks.HealthCheckResult.Healthy("API funcionando corretamente"));

// =============== OPENTELEMETRY =================
builder.Services.AddOpenTelemetry()
    .ConfigureResource(r => r.AddService("LibrasJa.API"))
    .WithTracing(tracing => tracing
        .AddAspNetCoreInstrumentation()
        .AddConsoleExporter())
    .WithMetrics(metrics => metrics
        .AddAspNetCoreInstrumentation()
        .AddConsoleExporter());

builder.Services.AddEndpointsApiExplorer();
builder.Services.AddSwaggerGen();

var app = builder.Build();

// =============== ERRO GLOBAL =================
app.UseExceptionHandler(errorApp =>
{
    errorApp.Run(async context =>
    {
        context.Response.StatusCode = 500;
        context.Response.ContentType = "application/json";
        await context.Response.WriteAsJsonAsync(new { message = "Erro interno. Tente novamente." });
    });
});

// =============== SWAGGER =================
if (app.Environment.IsDevelopment())
{
    app.UseSwagger();
    app.UseSwaggerUI();
}

// =============== HEALTH ENDPOINT =================
app.MapHealthChecks("/health", new HealthCheckOptions
{
    ResponseWriter = UIResponseWriter.WriteHealthCheckUIResponse
});

app.UseHttpsRedirection();
app.UseSerilogRequestLogging();

// =====================================================
//                      USERS
// =====================================================
app.MapGet("/api/users", async (IUserRepository repo, ILogger<Program> logger) =>
{
    logger.LogInformation("Listando todos os usu·rios");
    return Results.Ok(await repo.GetAllAsync());
}).WithTags("Users");

app.MapGet("/api/users/{id:int}", async (int id, IUserRepository repo, ILogger<Program> logger) =>
{
    logger.LogInformation("Buscando usu·rio {UserId}", id);
    var user = await repo.GetByIdAsync(id);
    if (user is null)
    {
        logger.LogWarning("Usu·rio {UserId} n„o encontrado", id);
        return Results.NotFound();
    }
    return Results.Ok(new {
        data = user,
        links = new[] {
            new { rel = "self", method = "GET", href = $"/api/users/{id}" },
            new { rel = "update", method = "PUT", href = $"/api/users/{id}" },
            new { rel = "delete", method = "DELETE", href = $"/api/users/{id}" }
        }
    });
}).WithTags("Users");

app.MapPost("/api/users", async (CreateUserDto dto, IUserRepository repo, ILogger<Program> logger) =>
{
    logger.LogInformation("Criando usu·rio {Nome} do tipo {Tipo}", dto.Nome, dto.Tipo);
    var user = new User { Nome = dto.Nome, Email = dto.Email, Tipo = dto.Tipo };
    await repo.AddAsync(user);
    return Results.Created($"/api/users/{user.Id}", user);
}).WithTags("Users");

app.MapPut("/api/users/{id:int}", async (int id, CreateUserDto dto, IUserRepository repo, ILogger<Program> logger) =>
{
    var user = await repo.GetByIdAsync(id);
    if (user is null)
    {
        logger.LogWarning("Tentativa de atualizar usu·rio inexistente {UserId}", id);
        return Results.NotFound();
    }
    user.Nome = dto.Nome; user.Email = dto.Email; user.Tipo = dto.Tipo;
    await repo.UpdateAsync(user);
    logger.LogInformation("Usu·rio {UserId} atualizado", id);
    return Results.NoContent();
}).WithTags("Users");

app.MapDelete("/api/users/{id:int}", async (int id, IUserRepository repo, ILogger<Program> logger) =>
{
    var user = await repo.GetByIdAsync(id);
    if (user is null)
    {
        logger.LogWarning("Tentativa de deletar usu·rio inexistente {UserId}", id);
        return Results.NotFound();
    }
    await repo.DeleteAsync(user);
    logger.LogInformation("Usu·rio {UserId} deletado", id);
    return Results.NoContent();
}).WithTags("Users");

app.MapGet("/api/users/search", async ([AsParameters] SearchParams qp, IUserRepository repo) =>
{
    var users = (await repo.GetAllAsync()).ToList();
    if (!string.IsNullOrWhiteSpace(qp.Search))
        users = users.Where(u =>
            u.Nome.Contains(qp.Search, StringComparison.OrdinalIgnoreCase) ||
            u.Email.Contains(qp.Search, StringComparison.OrdinalIgnoreCase)).ToList();
    users = qp.OrderBy?.ToLower() switch {
        "email" => (qp.OrderDir == "desc" ? users.OrderByDescending(u => u.Email) : users.OrderBy(u => u.Email)).ToList(),
        _ => (qp.OrderDir == "desc" ? users.OrderByDescending(u => u.Nome) : users.OrderBy(u => u.Nome)).ToList()
    };
    var total = users.Count;
    var items = users.Skip((qp.Page - 1) * qp.PageSize).Take(qp.PageSize).ToList();
    return Results.Ok(new { total, page = qp.Page, pageSize = qp.PageSize, data = items });
}).WithTags("Users");

// =====================================================
//                  INTERPRETERS
// =====================================================
app.MapGet("/api/interpreters", async (IInterpreterProfileRepository repo) =>
    Results.Ok(await repo.GetAllAsync())).WithTags("Interpreters");

app.MapGet("/api/interpreters/{id:int}", async (int id, IInterpreterProfileRepository repo) =>
{
    var item = await repo.GetByIdAsync(id);
    if (item is null) return Results.NotFound();
    return Results.Ok(new {
        data = item,
        links = new[] {
            new { rel = "self", method = "GET", href = $"/api/interpreters/{id}" },
            new { rel = "update", method = "PUT", href = $"/api/interpreters/{id}" },
            new { rel = "delete", method = "DELETE", href = $"/api/interpreters/{id}" }
        }
    });
}).WithTags("Interpreters");

app.MapPost("/api/interpreters", async (CreateInterpreterDto dto, IInterpreterProfileRepository repo) =>
{
    var entity = new InterpreterProfile {
        UserId = dto.UserId, Especialidades = dto.Especialidades,
        DescricaoCurta = dto.DescricaoCurta, Disponivel = dto.Disponivel
    };
    await repo.AddAsync(entity);
    return Results.Created($"/api/interpreters/{entity.Id}", entity);
}).WithTags("Interpreters");

app.MapPut("/api/interpreters/{id:int}", async (int id, UpdateInterpreterDto dto, IInterpreterProfileRepository repo) =>
{
    var entity = await repo.GetByIdAsync(id);
    if (entity is null) return Results.NotFound();
    entity.Especialidades = dto.Especialidades;
    entity.DescricaoCurta = dto.DescricaoCurta;
    entity.Disponivel = dto.Disponivel;
    await repo.UpdateAsync(entity);
    return Results.NoContent();
}).WithTags("Interpreters");

app.MapDelete("/api/interpreters/{id:int}", async (int id, IInterpreterProfileRepository repo) =>
{
    await repo.DeleteAsync(id);
    return Results.NoContent();
}).WithTags("Interpreters");

app.MapGet("/api/interpreters/search", async ([AsParameters] SearchParams qp, IInterpreterProfileRepository repo) =>
{
    var list = await repo.GetAllAsync();
    if (!string.IsNullOrWhiteSpace(qp.Search))
        list = list.Where(i =>
            (i.DescricaoCurta ?? "").Contains(qp.Search, StringComparison.OrdinalIgnoreCase) ||
            (i.Especialidades ?? "").Contains(qp.Search, StringComparison.OrdinalIgnoreCase) ||
            (i.User?.Nome ?? "").Contains(qp.Search, StringComparison.OrdinalIgnoreCase)).ToList();
    list = qp.OrderBy?.ToLower() switch {
        "user" => (qp.OrderDir == "desc" ? list.OrderByDescending(i => i.User!.Nome) : list.OrderBy(i => i.User!.Nome)).ToList(),
        _ => list.OrderBy(i => i.Id).ToList()
    };
    var total = list.Count;
    var items = list.Skip((qp.Page - 1) * qp.PageSize).Take(qp.PageSize).ToList();
    return Results.Ok(new { total, page = qp.Page, pageSize = qp.PageSize, data = items });
}).WithTags("Interpreters");

app.Run();

public partial class Program { }

public record SearchParams(
    string? Search,
    int Page = 1,
    int PageSize = 10,
    string? OrderBy = null,
    string OrderDir = "asc");
