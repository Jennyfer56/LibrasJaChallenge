# LibrasJá Challenge — API .NET 8

Plataforma que conecta pessoas surdas a intérpretes de Libras.

## Integrantes
- Ivanildo Alfredo da Silva Filho — RM560049
- Jennyfer Lee — RM561020
- Letícia Sousa Prado Silva — RM559258

## Tecnologias
- .NET 8 / Minimal API
- Entity Framework Core + Oracle
- Serilog (logging estruturado)
- OpenTelemetry (tracing e métricas)
- Health Checks
- xUnit + Moq (testes)
- Swagger

## Arquitetura (Clean Architecture)
LibrasJa.Domain          ? Entidades
LibrasJa.Application     ? Interfaces e regras de negócio
LibrasJa.Infrastructure  ? DbContext, Repositories (Oracle EF Core)
LibrasJáChallenge        ? API Minimal + Swagger
LibrasJa.Tests.Unit      ? Testes unitários (xUnit + Moq)
LibrasJa.Tests.Integration ? Testes de integração (WebApplicationFactory)

## Endpoints
| Método | Rota | Descrição |
|--------|------|-----------|
| GET | /api/users | Lista usuários |
| GET | /api/users/{id} | Busca usuário por ID |
| POST | /api/users | Cria usuário |
| PUT | /api/users/{id} | Atualiza usuário |
| DELETE | /api/users/{id} | Remove usuário |
| GET | /api/users/search | Busca com filtros e paginação |
| GET | /api/interpreters | Lista intérpretes |
| GET | /api/interpreters/{id} | Busca intérprete por ID |
| POST | /api/interpreters | Cria perfil de intérprete |
| PUT | /api/interpreters/{id} | Atualiza intérprete |
| DELETE | /api/interpreters/{id} | Remove intérprete |
| GET | /api/interpreters/search | Busca intérpretes com filtros |
| GET | /health | Health Check da API e banco |

## Health Check
Acesse `/health` para verificar:
- Status da API
- Conectividade com o banco Oracle

Resposta de exemplo:
```json
{
  "status": "Healthy",
  "entries": {
    "oracle-db": { "status": "Healthy" },
    "api-self": { "status": "Healthy" }
  }
}
```

## Como executar
```bash
git clone https://github.com/Jennyfer56/LibrasJaChallenge.git
cd LibrasJaChallenge
dotnet restore
dotnet run
```
Acesse: https://localhost:7178/swagger

## Executar Testes
```bash
# Testes unitários
cd LibrasJa.Tests.Unit
dotnet test

# Testes de integração
cd LibrasJa.Tests.Integration
dotnet test
```

## Sprint 3 — Novidades
- Health Checks configurados (API + Oracle DB) em `/health`
- Logging estruturado com Serilog (console + arquivo em `logs/`)
- Distributed Tracing com OpenTelemetry (métricas de tempo de resposta)
- 11 testes unitários (xUnit + Moq) — camadas Domain e Application
- 5 testes de integração (WebApplicationFactory) — endpoints e health
