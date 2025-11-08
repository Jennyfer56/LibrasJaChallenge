using LibrasJa.Infrastructure.Data;
using LibrasJa.Infrastructure.Repositories;
using LibrasJ·Challenge.DTOs;
using Microsoft.EntityFrameworkCore;
using LibrasJa.Domain.Entities;
using System.Collections.Generic;

var builder = WebApplication.CreateBuilder(args);

// =============== DB =================
builder.Services.AddDbContext<AppDbContext>(options =>
    options.UseOracle(builder.Configuration.GetConnectionString("Oracle")));

// =============== DI =================
builder.Services.AddScoped<IUserRepository, UserRepository>();
builder.Services.AddScoped<IInterpreterProfileRepository, InterpreterProfileRepository>();

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

app.UseHttpsRedirection();


// =====================================================
//                      USERS
// =====================================================

// GET /api/users
app.MapGet("/api/users", async (IUserRepository repo) =>
{
    var users = await repo.GetAllAsync();
    return Results.Ok(users);
})
.WithTags("Users");

// GET /api/users/{id}
app.MapGet("/api/users/{id:int}", async (int id, IUserRepository repo) =>
{
    var user = await repo.GetByIdAsync(id);
    if (user is null) return Results.NotFound();

    var response = new
    {
        data = user,
        links = new[]
        {
            new { rel = "self", method = "GET", href = $"/api/users/{id}" },
            new { rel = "update", method = "PUT", href = $"/api/users/{id}" },
            new { rel = "delete", method = "DELETE", href = $"/api/users/{id}" }
        }
    };

    return Results.Ok(response);
})
.WithTags("Users");

// POST /api/users
app.MapPost("/api/users", async (CreateUserDto dto, IUserRepository repo) =>
{
    var user = new User
    {
        Nome = dto.Nome,
        Email = dto.Email,
        Tipo = dto.Tipo
    };

    await repo.AddAsync(user);
    return Results.Created($"/api/users/{user.Id}", user);
})
.WithTags("Users");

// PUT /api/users/{id}
app.MapPut("/api/users/{id:int}", async (int id, CreateUserDto dto, IUserRepository repo) =>
{
    var user = await repo.GetByIdAsync(id);
    if (user is null) return Results.NotFound();

    user.Nome = dto.Nome;
    user.Email = dto.Email;
    user.Tipo = dto.Tipo;

    await repo.UpdateAsync(user);
    return Results.NoContent();
})
.WithTags("Users");

// DELETE /api/users/{id}
app.MapDelete("/api/users/{id:int}", async (int id, IUserRepository repo) =>
{
    var user = await repo.GetByIdAsync(id);
    if (user is null) return Results.NotFound();

    // seu repo espera User, n„o int
    await repo.DeleteAsync(user);
    return Results.NoContent();
})
.WithTags("Users");

// SEARCH /api/users/search
app.MapGet("/api/users/search", async ([AsParameters] SearchParams qp, IUserRepository repo) =>
{
    // tipar explicitamente resolve o CS8917
    List<User> users = (await repo.GetAllAsync()).ToList();

    // filtro
    if (!string.IsNullOrWhiteSpace(qp.Search))
    {
        users = users
            .Where(u =>
                u.Nome.Contains(qp.Search, StringComparison.OrdinalIgnoreCase) ||
                u.Email.Contains(qp.Search, StringComparison.OrdinalIgnoreCase))
            .ToList();
    }

    // ordenaÁ„o
    users = qp.OrderBy?.ToLower() switch
    {
        "email" => (qp.OrderDir == "desc"
            ? users.OrderByDescending(u => u.Email)
            : users.OrderBy(u => u.Email)).ToList(),
        _ => (qp.OrderDir == "desc"
            ? users.OrderByDescending(u => u.Nome)
            : users.OrderBy(u => u.Nome)).ToList()
    };

    var total = users.Count;

    var items = users
        .Skip((qp.Page - 1) * qp.PageSize)
        .Take(qp.PageSize)
        .ToList();

    return Results.Ok(new
    {
        total,
        page = qp.Page,
        pageSize = qp.PageSize,
        data = items
    });
})
.WithTags("Users");


// =====================================================
//                  INTERPRETERS
// =====================================================

// GET /api/interpreters
app.MapGet("/api/interpreters", async (IInterpreterProfileRepository repo) =>
{
    var list = await repo.GetAllAsync();
    return Results.Ok(list);
})
.WithTags("Interpreters");

// GET /api/interpreters/{id}
app.MapGet("/api/interpreters/{id:int}", async (int id, IInterpreterProfileRepository repo) =>
{
    var item = await repo.GetByIdAsync(id);
    if (item is null) return Results.NotFound();

    var response = new
    {
        data = item,
        links = new[]
        {
            new { rel = "self", method = "GET", href = $"/api/interpreters/{id}" },
            new { rel = "update", method = "PUT", href = $"/api/interpreters/{id}" },
            new { rel = "delete", method = "DELETE", href = $"/api/interpreters/{id}" }
        }
    };

    return Results.Ok(response);
})
.WithTags("Interpreters");

// POST /api/interpreters
app.MapPost("/api/interpreters", async (CreateInterpreterDto dto, IInterpreterProfileRepository repo) =>
{
    var entity = new InterpreterProfile
    {
        UserId = dto.UserId,
        Especialidades = dto.Especialidades,
        DescricaoCurta = dto.DescricaoCurta,
        Disponivel = dto.Disponivel
    };

    await repo.AddAsync(entity);
    return Results.Created($"/api/interpreters/{entity.Id}", entity);
})
.WithTags("Interpreters");

// PUT /api/interpreters/{id}
app.MapPut("/api/interpreters/{id:int}", async (int id, UpdateInterpreterDto dto, IInterpreterProfileRepository repo) =>
{
    var entity = await repo.GetByIdAsync(id);
    if (entity is null) return Results.NotFound();

    entity.Especialidades = dto.Especialidades;
    entity.DescricaoCurta = dto.DescricaoCurta;
    entity.Disponivel = dto.Disponivel;

    await repo.UpdateAsync(entity);
    return Results.NoContent();
})
.WithTags("Interpreters");

// DELETE /api/interpreters/{id}
app.MapDelete("/api/interpreters/{id:int}", async (int id, IInterpreterProfileRepository repo) =>
{
    await repo.DeleteAsync(id);
    return Results.NoContent();
})
.WithTags("Interpreters");

// SEARCH /api/interpreters/search
app.MapGet("/api/interpreters/search", async ([AsParameters] SearchParams qp, IInterpreterProfileRepository repo) =>
{
    List<InterpreterProfile> list = await repo.GetAllAsync();

    if (!string.IsNullOrWhiteSpace(qp.Search))
    {
        list = list
            .Where(i =>
                (i.DescricaoCurta ?? "").Contains(qp.Search, StringComparison.OrdinalIgnoreCase) ||
                (i.Especialidades ?? "").Contains(qp.Search, StringComparison.OrdinalIgnoreCase) ||
                (i.User?.Nome ?? "").Contains(qp.Search, StringComparison.OrdinalIgnoreCase))
            .ToList();
    }

    list = qp.OrderBy?.ToLower() switch
    {
        "user" => (qp.OrderDir == "desc"
            ? list.OrderByDescending(i => i.User!.Nome)
            : list.OrderBy(i => i.User!.Nome)).ToList(),
        _ => list.OrderBy(i => i.Id).ToList()
    };

    var total = list.Count;

    var items = list
        .Skip((qp.Page - 1) * qp.PageSize)
        .Take(qp.PageSize)
        .ToList();

    return Results.Ok(new
    {
        total,
        page = qp.Page,
        pageSize = qp.PageSize,
        data = items
    });
})
.WithTags("Interpreters");

app.Run();

public record SearchParams(
    string? Search,
    int Page = 1,
    int PageSize = 10,
    string? OrderBy = null,
    string OrderDir = "asc");
