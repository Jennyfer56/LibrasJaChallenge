using LibrasJa.Infrastructure.Data;
using LibrasJa.Infrastructure.Repositories;
using LibrasJa.Domain.Entities;
using LibrasJaChallenge.DTOs;
using Microsoft.EntityFrameworkCore;

var builder = WebApplication.CreateBuilder(args);

// ==========================================================
//                    CONFIGURAÇÕES
// ==========================================================

// Banco de Dados (Oracle - connection string do appsettings.json)
builder.Services.AddDbContext<AppDbContext>(options =>
    options.UseOracle(builder.Configuration.GetConnectionString("Oracle")));

// Injeção de Dependência (Repositórios)
builder.Services.AddScoped<IUserRepository, UserRepository>();
builder.Services.AddScoped<IInterpreterProfileRepository, InterpreterProfileRepository>();

// Swagger
builder.Services.AddEndpointsApiExplorer();
builder.Services.AddSwaggerGen();

var app = builder.Build();

// ==========================================================
//                MIDDLEWARE DE TRATAMENTO DE ERROS
// ==========================================================
app.UseExceptionHandler(errorApp =>
{
    errorApp.Run(async context =>
    {
        context.Response.StatusCode = 500;
        context.Response.ContentType = "application/json";
        await context.Response.WriteAsJsonAsync(new { message = "Erro interno. Tente novamente." });
    });
});

// ==========================================================
//                       SWAGGER
// ==========================================================
if (app.Environment.IsDevelopment())
{
    app.UseSwagger();
    app.UseSwaggerUI();
}

app.UseHttpsRedirection();

// ==========================================================
//                       USERS
// ==========================================================

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
    return user is null ? Results.NotFound() : Results.Ok(user);
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

    // Repositório retorna Task, então não há retorno
    await repo.AddAsync(user);

    // Devolve o próprio objeto criado
    return Results.Created($"/api/users/{user.Id}", user);
})
.WithTags("Users");

// PUT /api/users/{id}
app.MapPut("/api/users/{id:int}", async (int id, CreateUserDto dto, IUserRepository repo) =>
{
    var user = await repo.GetByIdAsync(id);
    if (user is null)
        return Results.NotFound();

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
    if (user is null)
        return Results.NotFound();

    await repo.DeleteAsync(user);
    return Results.NoContent();
})
.WithTags("Users");

// ==========================================================
//                  INTERPRETERS
// ==========================================================

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
    return item is null ? Results.NotFound() : Results.Ok(item);
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
    if (entity is null)
        return Results.NotFound();

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

// ==========================================================
//                       RUN
// ==========================================================
app.Run();
