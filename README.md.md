README - LibrasJáChallenge

LibrasJáChallenge - Challenge FIAP .NET



API desenvolvida como parte do Challenge FIAP — Advanced Business Development with .NET.

O projeto segue os princípios de Clean Architecture e boas práticas de desenvolvimento com .NET 8 + Oracle + Swagger, simulando um sistema para conexão entre usuários surdos e intérpretes de Libras.



---------------------------------------------

&nbsp;Objetivo do Projeto

Criar uma API REST que permita o cadastro, listagem, atualização e exclusão de usuários, bem como a busca e associação de intérpretes com base em especialidades e disponibilidade.



---------------------------------------------

&nbsp;Escopo

A API implementa as seguintes funcionalidades principais:

\- Usuários (Users)

&nbsp; - GET /api/users — Lista usuários com paginação e filtros.

&nbsp; - GET /api/users/{id} — Retorna um usuário específico.

&nbsp; - POST /api/users — Cadastra um novo usuário.

&nbsp; - PUT /api/users/{id} — Atualiza os dados de um usuário.

&nbsp; - DELETE /api/users/{id} — Remove um usuário.



\- Intérpretes (Interpreters)

&nbsp; - POST /api/interpreters — Cria um perfil de intérprete vinculado a um usuário do tipo INTERPRETE.

&nbsp; - GET /api/interpreters/search — Busca intérpretes por especialidade e dia de disponibilidade (com paginação e ordenação).



---------------------------------------------

&nbsp;Requisitos Funcionais e Não Funcionais

Funcionais:

\- CRUD completo de usuários.

\- Criação e consulta de perfis de intérpretes.

\- Filtros por especialidade e disponibilidade.

\- Respostas JSON padronizadas.

\- Documentação Swagger.



Não Funcionais:

\- Persistência Oracle com EF Core.

\- Arquitetura em camadas (Clean Architecture).

\- Paginação e HATEOAS.

\- Código desacoplado e modular.



---------------------------------------------

&nbsp;Arquitetura e Estrutura

LibrasJáChallenge/

├── LibrasJa.Domain/ → Entidades e regras de negócio

│   └── Entities/

│       ├── User.cs

│       └── InterpreterProfile.cs

├── LibrasJa.Infrastructure/ → Banco e EF Core

│   └── Data/AppDbContext.cs

├── LibrasJa.Application/ → DTOs e casos de uso

└── LibrasJáChallenge/ → API

&nbsp;   ├── Program.cs

&nbsp;   ├── appsettings.json

&nbsp;   └── launchSettings.json



---------------------------------------------

&nbsp;Modelagem das Entidades

User:

\- Id

\- Nome

\- Email

\- Tipo (INTERPRETE / SURDO)

\- CreatedAt



InterpreterProfile:

\- Id

\- UserId

\- Especialidades

\- DescricaoCurta

\- Disponivel



---------------------------------------------

&nbsp;Banco de Dados

Usa Oracle + EF Core.



Para atualizar:

dotnet ef database update



---------------------------------------------

&nbsp;Como Executar

1\. Clonar o repositório

git clone https://github.com/seuusuario/LibrasJaChallenge.git

cd LibrasJaChallenge



2\. Restaurar dependências

dotnet restore



3\. Configurar conexão Oracle em appsettings.json



4\. Criar tabelas

dotnet ef database update



5\. Executar

dotnet run

Acesse: https://localhost:7178/swagger



---------------------------------------------

&nbsp;Exemplos de Testes

POST /api/users

{

&nbsp; "nome": "Carlos Andrade",

&nbsp; "email": "carlos@teste.com",

&nbsp; "tipo": "INTERPRETE"

}



GET /api/users

GET /api/users/3

PUT /api/users/3

DELETE /api/users/3



POST /api/interpreters

{

&nbsp; "userId": 1,

&nbsp; "especialidades": "Saúde, Jurídico",

&nbsp; "descricaoCurta": "Intérprete experiente",

&nbsp; "disponivel": "SEG,TER,QUA"

}



GET /api/interpreters/search?especialidade=Saúde\&dia=SEG



---------------------------------------------

&nbsp;Prints 

\- Swagger com endpoints

\- POST /api/users com sucesso

\- GET /api/users

\- POST /api/interpreters

\- GET /api/interpreters/search



---------------------------------------------

&nbsp;Tecnologias

\- .NET 8

\- EF Core (Oracle)

\- Swagger UI

\- C# 12

\- IIS Express



---------------------------------------------

GRUPO



Ivanildo Alfredo da Silva Filho (RM

JENNYFER LEE (RM561020)

LETÍCIA SOUSA PRADO SILVA



FIAP — Análise e Desenvolvimento de Sistemas

Challenge: Advanced Business Development with .NET — Sprint 1







