using LibrasJa.Domain.Entities;
using LibrasJa.Infrastructure.Data;
using Microsoft.EntityFrameworkCore;

var builder = WebApplication.CreateBuilder(args);

// EF Core + Oracle
builder.Services.AddDbContext<AppDbContext>(opt =>
    opt.UseOracle(builder.Configuration.GetConnectionString("DefaultConnection")));

builder.Services.AddEndpointsApiExplorer();
builder.Services.AddSwaggerGen();

var app = builder.Build();
app.UseSwagger();
app.UseSwaggerUI();
app.UseHttpsRedirection();

// ------- USERS -------
app.MapGet("/api/users", async (
    AppDbContext db, int page = 1, int pageSize = 10,
    string? search = null, string? tipo = null, string? orderBy = "nome") =>
{
    var q = db.Users.AsQueryable();

    if (!string.IsNullOrWhiteSpace(search))
        q = q.Where(u => u.Nome.Contains(search) || u.Email.Contains(search));

    if (!string.IsNullOrWhiteSpace(tipo))
        q = q.Where(u => u.Tipo == tipo);

    q = orderBy?.ToLower() switch
    {
        "email" => q.OrderBy(u => u.Email),
        "createdat" => q.OrderByDescending(u => u.CreatedAt),
        _ => q.OrderBy(u => u.Nome)
    };

    var total = await q.CountAsync();
    var items = await q.Skip((page - 1) * pageSize).Take(pageSize).ToListAsync();

    var baseUrl = "/api/users";
    var links = new
    {
        self = $"{baseUrl}?page={page}&pageSize={pageSize}&search={search}&tipo={tipo}&orderBy={orderBy}",
        next = (page * pageSize < total) ? $"{baseUrl}?page={page + 1}&pageSize={pageSize}&search={search}&tipo={tipo}&orderBy={orderBy}" : null,
        prev = (page > 1) ? $"{baseUrl}?page={page - 1}&pageSize={pageSize}&search={search}&tipo={tipo}&orderBy={orderBy}" : null
    };

    return Results.Ok(new { total, page, pageSize, items, links });
});

app.MapGet("/api/users/{id:int}", async (AppDbContext db, int id) =>
    await db.Users.FindAsync(id) is { } u ? Results.Ok(u) : Results.NotFound());

app.MapPost("/api/users", async (AppDbContext db, UserDto dto) =>
{
    if (string.IsNullOrWhiteSpace(dto.Nome) || string.IsNullOrWhiteSpace(dto.Email) || string.IsNullOrWhiteSpace(dto.Tipo))
        return Results.BadRequest("Nome, Email e Tipo são obrigatórios.");

    var user = new User { Nome = dto.Nome, Email = dto.Email, Tipo = dto.Tipo.ToUpper() };
    db.Users.Add(user);
    await db.SaveChangesAsync();
    return Results.Created($"/api/users/{user.Id}", user);
});

app.MapPut("/api/users/{id:int}", async (AppDbContext db, int id, UserDto dto) =>
{
    var user = await db.Users.FindAsync(id);
    if (user is null) return Results.NotFound();

    user.Nome = dto.Nome;
    user.Email = dto.Email;
    user.Tipo = dto.Tipo.ToUpper();
    await db.SaveChangesAsync();
    return Results.NoContent();
});

app.MapDelete("/api/users/{id:int}", async (AppDbContext db, int id) =>
{
    var user = await db.Users.FindAsync(id);
    if (user is null) return Results.NotFound();
    db.Users.Remove(user);
    await db.SaveChangesAsync();
    return Results.NoContent();
});

// ------- INTERPRETERS (join + filtros) -------
app.MapGet("/api/interpreters/search", async (
    AppDbContext db, int page = 1, int pageSize = 10,
    string? especialidade = null, string? dia = null, string? orderBy = "nome") =>
{
    var q = db.InterpreterProfiles.Include(i => i.User).AsQueryable();

    if (!string.IsNullOrWhiteSpace(especialidade))
        q = q.Where(i => i.Especialidades.Contains(especialidade));

    if (!string.IsNullOrWhiteSpace(dia))
        q = q.Where(i => i.Disponivel != null && i.Disponivel.Contains(dia.ToUpper()));

    q = orderBy?.ToLower() switch
    {
        "especialidades" => q.OrderBy(i => i.Especialidades),
        _ => q.OrderBy(i => i.User.Nome)
    };

    var total = await q.CountAsync();
    var items = await q.Skip((page - 1) * pageSize).Take(pageSize)
        .Select(i => new { i.Id, i.UserId, Nome = i.User.Nome, Email = i.User.Email, i.Especialidades, i.Disponivel })
        .ToListAsync();

    var baseUrl = "/api/interpreters/search";
    var links = new
    {
        self = $"{baseUrl}?page={page}&pageSize={pageSize}&especialidade={especialidade}&dia={dia}&orderBy={orderBy}",
        next = (page * pageSize < total) ? $"{baseUrl}?page={page + 1}&pageSize={pageSize}&especialidade={especialidade}&dia={dia}&orderBy={orderBy}" : null,
        prev = (page > 1) ? $"{baseUrl}?page={page - 1}&pageSize={pageSize}&especialidade={especialidade}&dia={dia}&orderBy={orderBy}" : null
    };

    return Results.Ok(new { total, page, pageSize, items, links });
});

app.Run();

// DTO
public record UserDto(string Nome, string Email, string Tipo);
