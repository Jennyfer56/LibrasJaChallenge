
 LibrasJáChallenge

 Objetivo do Projeto
O **LibrasJá** é uma aplicação web desenvolvida em **.NET 8 com Oracle Database** que tem como objetivo conectar **pessoas surdas** a **intérpretes de Libras** de forma simples e direta.

A plataforma permite o cadastro e gerenciamento de usuários (surdo ou intérprete) e a busca por intérpretes com filtros de **especialidade** e **disponibilidade**, promovendo inclusão e acessibilidade digital.

---

 Escopo
O sistema oferece as seguintes funcionalidades principais:

- Cadastro, listagem, edição e exclusão de usuários.
- Definição de tipo de usuário (SURDO ou INTERPRETE).
- Criação de perfis de intérprete (com especialidades, descrição e disponibilidade).
- Busca paginada e filtrada de intérpretes.
- Exposição da API via **Swagger**.
- Persistência de dados no banco Oracle através do **Entity Framework Core**.

---

 Arquitetura (Clean Architecture)

A aplicação foi estruturada em **camadas** de forma a garantir separação de responsabilidades e um código limpo e desacoplado.

```

LibrasJa.Domain         → Entidades e modelos de domínio
LibrasJa.Infrastructure → Contexto de dados (EF Core + Oracle), Migrations e integração
LibrasJa.Application    → Regras de negócio, DTOs e serviços
LibrasJáChallenge       → Camada principal (API Minimal + Swagger)

```

---

Entidades

 User
| Campo | Tipo | Descrição |
|-------|------|------------|
| Id | int | Identificador do usuário |
| Nome | string | Nome completo |
| Email | string | E-mail único |
| Tipo | string | Tipo de usuário (`SURDO` ou `INTERPRETE`) |
| CreatedAt | DateTime | Data de criação |

 InterpreterProfile
| Campo | Tipo | Descrição |
|-------|------|------------|
| Id | int | Identificador do perfil |
| UserId | int | Relacionamento com User |
| Especialidades | string | Áreas de atuação do intérprete |
| DescricaoCurta | string | Resumo sobre o intérprete |
| Disponivel | string | Dias disponíveis para atendimento |

Relação **1:1** → Cada intérprete tem um único perfil vinculado a um `User`.

---

 Endpoints da API

| Método | Endpoint | Descrição |
|--------|-----------|------------|
| `GET` | `/api/users` | Lista usuários (com filtros e paginação) |
| `GET` | `/api/users/{id}` | Retorna um usuário específico |
| `POST` | `/api/users` | Cria um novo usuário |
| `PUT` | `/api/users/{id}` | Atualiza um usuário existente |
| `DELETE` | `/api/users/{id}` | Exclui um usuário |
| `GET` | `/api/interpreters/search` | Busca intérpretes por especialidade e dia disponível |

---

 Paginação e Filtros

A API utiliza paginação padrão:
```

?page=1&pageSize=10

```

E filtros opcionais:
```

/api/users?search=maria&tipo=INTERPRETE
/api/interpreters/search?especialidade=MEDICA&dia=SEGUNDA

````

---

 Como Executar o Projeto

 1. Clonar o repositório
```bash
git clone https://github.com/Jennyfer56/LibrasJaChallenge.git
cd LibrasJaChallenge
````
 2. Restaurar dependências

```bash
dotnet restore
```

 3. Aplicar as migrations (criar o banco)

```bash
dotnet ef database update
```

 4. Executar a aplicação

```bash
dotnet run
```

Após iniciar, acesse no navegador:

```
https://localhost:7178/swagger
```

---

 Tecnologias Utilizadas

* **.NET 8**
* **Entity Framework Core (Oracle)**
* **Swagger UI**
* **LINQ + Minimal API**
* **C#**
* **Clean Architecture**

---

 Prints do Projeto (Swagger)


* ✅ POST /api/users → cadastro realizado com sucesso (HTTP 201)
* ✅ GET /api/users → listagem paginada
* ✅ PUT /api/users/{id} → atualização confirmada
* ✅ DELETE /api/users/{id} → exclusão OK
* ✅ GET /api/interpreters/search → retorno filtrado

---

 Aprendizados

Durante o desenvolvimento do **LibrasJá**, foi possível praticar:

* Criação de APIs REST com **.NET 8 Minimal API**
* Mapeamento ORM com **Entity Framework Core (Oracle)**
* Aplicação de **padrões de arquitetura limpa (Clean Architecture)**
* Utilização de **Swagger** para documentação e testes
* Controle de versão com **Git + GitHub**

---

 Grupo

Ivanildo Alfredo da Silva Filho - RM560049
Jennyfer Lee - RM561020
Letícia Sousa Prado Silva - RM559258

Curso: *Análise e Desenvolvimento de Sistemas – FIAP*
Repositório GitHub: [https://github.com/Jennyfer56/LibrasJaChallenge](https://github.com/Jennyfer56/LibrasJaChallenge)

---
