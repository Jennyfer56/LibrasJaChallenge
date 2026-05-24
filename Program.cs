using LibrasJa.Infrastructure.Data;
using LibrasJa.Infrastructure.Repositories;
using LibrasJa.Infrastructure.Mongo;
using LibrasJa.Application.Interfaces;
using LibrasJáChallenge.DTOs;
using Microsoft.EntityFrameworkCore;
using LibrasJa.Domain.Entities;
using Serilog;
using Microsoft.AspNetCore.Diagnostics.HealthChecks;
using HealthChecks.UI.Client;
using OpenTelemetry.Resources;
using OpenTelemetry.Trace;
using OpenTelemetry.Metrics;
using Microsoft.AspNetCore.Authentication.JwtBearer;
using Microsoft.AspNetCore.Authorization;
using Microsoft.IdentityModel.Tokens;
using Microsoft.OpenApi.Models;
using System.Text;
using System.Text.Json;
using LibrasJaChallenge.Auth;
using MongoDB.Driver;

// =============== SERILOG =================
Log.Logger = new LoggerConfiguration()
    .MinimumLevel.Information()
    .WriteTo.Console()
    .WriteTo.File("logs/librasja-.txt", rollingInterval: RollingInterval.Day)
    .Enrich.FromLogContext()
    .CreateLogger();

var builder = WebApplication.CreateBuilder(args);
builder.Host.UseSerilog();

// =============== DB (Oracle) =================
builder.Services.AddDbContext<AppDbContext>(options =>
    options.UseOracle(builder.Configuration.GetConnectionString("Oracle")));

// =============== DI (Repositories) =================
builder.Services.AddScoped<IUserRepository, UserRepository>();
builder.Services.AddScoped<IInterpreterProfileRepository, InterpreterProfileRepository>();

// =============== MONGODB =================
var mongoConn = builder.Configuration.GetConnectionString("MongoDb")!;
var mongoDb   = builder.Configuration["MongoDb:DatabaseName"]   ?? "librasja_audit";
var mongoColl = builder.Configuration["MongoDb:CollectionName"] ?? "audit_logs";
builder.Services.AddSingleton<IMongoClient>(_ => new MongoClient(mongoConn));
builder.Services.AddScoped<IAuditLogRepository>(sp =>
    new MongoAuditLogRepository(sp.GetRequiredService<IMongoClient>(), mongoDb, mongoColl));

// =============== HEALTH CHECKS =================
builder.Services.AddHealthChecks()
    .AddDbContextCheck<AppDbContext>("oracle-db")
    .AddCheck("api-self", () => Microsoft.Extensions.Diagnostics.HealthChecks.HealthCheckResult.Healthy("API funcionando corretamente"))
    .AddMongoDb(sp => sp.GetRequiredService<IMongoClient>(), name: "mongodb");

// =============== OPENTELEMETRY =================
builder.Services.AddOpenTelemetry()
    .ConfigureResource(r => r.AddService("LibrasJa.API"))
    .WithTracing(tracing => tracing
        .AddAspNetCoreInstrumentation()
        .AddConsoleExporter())
    .WithMetrics(metrics => metrics
        .AddAspNetCoreInstrumentation()
        .AddConsoleExporter());

// =============== JWT =================
var jwtKey = builder.Configuration["Jwt:Key"]!;
builder.Services.AddSingleton<JwtTokenService>();
builder.Services.AddAuthentication(JwtBearerDefaults.AuthenticationScheme)
    .AddJwtBearer(options =>
    {
        options.TokenValidationParameters = new TokenValidationParameters
        {
            ValidateIssuer           = true,
            ValidateAudience         = true,
            ValidateLifetime         = true,
            ValidateIssuerSigningKey = true,
            ValidIssuer              = builder.Configuration["Jwt:Issuer"],
            ValidAudience            = builder.Configuration["Jwt:Audience"],
            IssuerSigningKey         = new SymmetricSecurityKey(Encoding.UTF8.GetBytes(jwtKey))
        };
    });
builder.Services.AddAuthorization();

// =============== SWAGGER COM JWT =================
builder.Services.AddEndpointsApiExplorer();
builder.Services.AddSwaggerGen(c =>
{
    c.SwaggerDoc("v1", new OpenApiInfo { Title = "LibrasJa API", Version = "v1" });
    c.AddSecurityDefinition("Bearer", new OpenApiSecurityScheme
    {
        Name         = "Authorization",
        Type         = SecuritySchemeType.Http,
        Scheme       = "bearer",
        BearerFormat = "JWT",
        In           = ParameterLocation.Header,
        Description  = "Cole o token JWT (sem o prefixo Bearer)."
    });
    c.AddSecurityRequirement(new OpenApiSecurityRequirement
    {
        {
            new OpenApiSecurityScheme
            {
                Reference = new OpenApiReference { Type = ReferenceType.SecurityScheme, Id = "Bearer" }
            },
            Array.Empty<string>()
        }
    });
});

var app = builder.Build();

// =============== ERRO GLOBAL =================
app.UseMiddleware<LibrasJaChallenge.Middleware.GlobalExceptionHandlerMiddleware>();

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
app.UseAuthentication();
app.UseAuthorization();
app.UseSerilogRequestLogging();

// =====================================================
//                      AUTH
// =====================================================
app.MapPost("/api/auth/login", (LoginDto dto, JwtTokenService jwt) =>
{
    if (string.IsNullOrWhiteSpace(dto.Username) || dto.Password != "1234")
        return Results.Unauthorized();

    var role  = dto.Username.Equals("admin", StringComparison.OrdinalIgnoreCase) ? "admin" : "user";
    var token = jwt.GenerateToken(dto.Username, role);
    return Results.Ok(new { token, expiresInMinutes = 120, role });
}).WithTags("Auth").AllowAnonymous();

// =====================================================
//                      USERS
// =====================================================
app.MapGet("/api/users", async (IUserRepository repo, ILogger<Program> logger) =>
{
    logger.LogInformation("Listando todos os usuarios");
    return Results.Ok(await repo.GetAllAsync());
}).WithTags("Users").AllowAnonymous();

app.MapGet("/api/users/{id:int}", async (int id, IUserRepository repo, ILogger<Program> logger) =>
{
    logger.LogInformation("Buscando usuario {UserId}", id);
    var user = await repo.GetByIdAsync(id);
    if (user is null)
    {
        logger.LogWarning("Usuario {UserId} nao encontrado", id);
        return Results.NotFound();
    }
    return Results.Ok(new {
        data = user,
        links = new[] {
            new { rel = "self",   method = "GET",    href = $"/api/users/{id}" },
            new { rel = "update", method = "PUT",    href = $"/api/users/{id}" },
            new { rel = "delete", method = "DELETE", href = $"/api/users/{id}" }
        }
    });
}).WithTags("Users").AllowAnonymous();

app.MapPost("/api/users", [Authorize] async (CreateUserDto dto, IUserRepository repo, IAuditLogRepository audit, HttpContext ctx, ILogger<Program> logger) =>
{
    logger.LogInformation("Criando usuario {Nome} do tipo {Tipo}", dto.Nome, dto.Tipo);
    var user = new User { Nome = dto.Nome, Email = dto.Email, Tipo = dto.Tipo };
    await repo.AddAsync(user);

    await audit.AddAsync(new AuditLog {
        Action  = "POST /api/users",
        Entity  = "User",
        User    = ctx.User.Identity?.Name ?? "anonymous",
        Payload = JsonSerializer.Serialize(dto)
    });

    return Results.Created($"/api/users/{user.Id}", user);
}).WithTags("Users");

app.MapPut("/api/users/{id:int}", [Authorize] async (int id, CreateUserDto dto, IUserRepository repo, IAuditLogRepository audit, HttpContext ctx, ILogger<Program> logger) =>
{
    var user = await repo.GetByIdAsync(id);
    if (user is null)
    {
        logger.LogWarning("Tentativa de atualizar usuario inexistente {UserId}", id);
        return Results.NotFound();
    }
    user.Nome = dto.Nome; user.Email = dto.Email; user.Tipo = dto.Tipo;
    await repo.UpdateAsync(user);

    await audit.AddAsync(new AuditLog {
        Action  = $"PUT /api/users/{id}",
        Entity  = "User",
        User    = ctx.User.Identity?.Name ?? "anonymous",
        Payload = JsonSerializer.Serialize(dto)
    });

    logger.LogInformation("Usuario {UserId} atualizado", id);
    return Results.NoContent();
}).WithTags("Users");

app.MapDelete("/api/users/{id:int}", [Authorize(Roles = "admin")] async (int id, IUserRepository repo, IAuditLogRepository audit, HttpContext ctx, ILogger<Program> logger) =>
{
    var user = await repo.GetByIdAsync(id);
    if (user is null)
    {
        logger.LogWarning("Tentativa de deletar usuario inexistente {UserId}", id);
        return Results.NotFound();
    }
    await repo.DeleteAsync(user);

    await audit.AddAsync(new AuditLog {
        Action  = $"DELETE /api/users/{id}",
        Entity  = "User",
        User    = ctx.User.Identity?.Name ?? "anonymous",
        Payload = $"{{\"id\":{id}}}"
    });

    logger.LogInformation("Usuario {UserId} deletado", id);
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
        _       => (qp.OrderDir == "desc" ? users.OrderByDescending(u => u.Nome)  : users.OrderBy(u => u.Nome)).ToList()
    };
    var total = users.Count;
    var items = users.Skip((qp.Page - 1) * qp.PageSize).Take(qp.PageSize).ToList();
    return Results.Ok(new { total, page = qp.Page, pageSize = qp.PageSize, data = items });
}).WithTags("Users").AllowAnonymous();

// =====================================================
//                  INTERPRETERS
// =====================================================
app.MapGet("/api/interpreters", async (IInterpreterProfileRepository repo) =>
    Results.Ok(await repo.GetAllAsync())).WithTags("Interpreters").AllowAnonymous();

app.MapGet("/api/interpreters/{id:int}", async (int id, IInterpreterProfileRepository repo) =>
{
    var item = await repo.GetByIdAsync(id);
    if (item is null) return Results.NotFound();
    return Results.Ok(new {
        data = item,
        links = new[] {
            new { rel = "self",   method = "GET",    href = $"/api/interpreters/{id}" },
            new { rel = "update", method = "PUT",    href = $"/api/interpreters/{id}" },
            new { rel = "delete", method = "DELETE", href = $"/api/interpreters/{id}" }
        }
    });
}).WithTags("Interpreters").AllowAnonymous();

app.MapPost("/api/interpreters", [Authorize] async (CreateInterpreterDto dto, IInterpreterProfileRepository repo, IAuditLogRepository audit, HttpContext ctx) =>
{
    var entity = new InterpreterProfile {
        UserId         = dto.UserId,
        Especialidades = dto.Especialidades,
        DescricaoCurta = dto.DescricaoCurta,
        Disponivel     = dto.Disponivel
    };
    await repo.AddAsync(entity);

    await audit.AddAsync(new AuditLog {
        Action  = "POST /api/interpreters",
        Entity  = "InterpreterProfile",
        User    = ctx.User.Identity?.Name ?? "anonymous",
        Payload = JsonSerializer.Serialize(dto)
    });

    return Results.Created($"/api/interpreters/{entity.Id}", entity);
}).WithTags("Interpreters");

app.MapPut("/api/interpreters/{id:int}", [Authorize] async (int id, UpdateInterpreterDto dto, IInterpreterProfileRepository repo, IAuditLogRepository audit, HttpContext ctx) =>
{
    var entity = await repo.GetByIdAsync(id);
    if (entity is null) return Results.NotFound();
    entity.Especialidades = dto.Especialidades;
    entity.DescricaoCurta = dto.DescricaoCurta;
    entity.Disponivel     = dto.Disponivel;
    await repo.UpdateAsync(entity);

    await audit.AddAsync(new AuditLog {
        Action  = $"PUT /api/interpreters/{id}",
        Entity  = "InterpreterProfile",
        User    = ctx.User.Identity?.Name ?? "anonymous",
        Payload = JsonSerializer.Serialize(dto)
    });

    return Results.NoContent();
}).WithTags("Interpreters");

app.MapDelete("/api/interpreters/{id:int}", [Authorize(Roles = "admin")] async (int id, IInterpreterProfileRepository repo, IAuditLogRepository audit, HttpContext ctx) =>
{
    await repo.DeleteAsync(id);

    await audit.AddAsync(new AuditLog {
        Action  = $"DELETE /api/interpreters/{id}",
        Entity  = "InterpreterProfile",
        User    = ctx.User.Identity?.Name ?? "anonymous",
        Payload = $"{{\"id\":{id}}}"
    });

    return Results.NoContent();
}).WithTags("Interpreters");

app.MapGet("/api/interpreters/search", async ([AsParameters] SearchParams qp, IInterpreterProfileRepository repo) =>
{
    var list = await repo.GetAllAsync();
    if (!string.IsNullOrWhiteSpace(qp.Search))
        list = list.Where(i =>
            (i.DescricaoCurta ?? "").Contains(qp.Search, StringComparison.OrdinalIgnoreCase) ||
            (i.Especialidades ?? "").Contains(qp.Search, StringComparison.OrdinalIgnoreCase) ||
            (i.User?.Nome      ?? "").Contains(qp.Search, StringComparison.OrdinalIgnoreCase)).ToList();
    list = qp.OrderBy?.ToLower() switch {
        "user" => (qp.OrderDir == "desc" ? list.OrderByDescending(i => i.User!.Nome) : list.OrderBy(i => i.User!.Nome)).ToList(),
        _      => list.OrderBy(i => i.Id).ToList()
    };
    var total = list.Count;
    var items = list.Skip((qp.Page - 1) * qp.PageSize).Take(qp.PageSize).ToList();
    return Results.Ok(new { total, page = qp.Page, pageSize = qp.PageSize, data = items });
}).WithTags("Interpreters").AllowAnonymous();

// =====================================================
//                  AUDIT LOGS (MongoDB)
// =====================================================
app.MapGet("/api/audit-logs", [Authorize] async (IAuditLogRepository audit) =>
{
    var logs = await audit.GetAllAsync();
    return Results.Ok(logs);
}).WithTags("AuditLogs");

app.MapGet("/api/audit-logs/{entity}", [Authorize] async (string entity, IAuditLogRepository audit) =>
{
    var logs = await audit.GetByEntityAsync(entity);
    return Results.Ok(logs);
}).WithTags("AuditLogs");

app.Run();

public partial class Program { }

public record SearchParams(
    string? Search,
    int Page = 1,
    int PageSize = 10,
    string? OrderBy = null,
    string OrderDir = "asc");
