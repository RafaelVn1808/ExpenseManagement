# Expense Management

Projeto fullstack para gestão de despesas com API ASP.NET Core (`ExpenseManagement`) e Web MVC (`ExpenseWeb`).

## Visão geral
Aplicação para controle de despesas com autenticação, autorização por roles, CRUD avançado, filtros/paginação e upload de comprovantes. Pensada para simular um cenário corporativo real.

## Funcionalidades
- Autenticação e renovação de token (JWT + refresh)
- CRUD completo de despesas
- Filtros, paginação e ordenação
- Upload de comprovantes (imagens)
- Gestão de categorias
- Controle de acesso por roles (Admin/User)

## Stack
- .NET 8, ASP.NET Core Web API, ASP.NET Core MVC
- EF Core, AutoMapper, ASP.NET Identity
- PostgreSQL (Npgsql)
- xUnit, Moq
- Docker, Docker Compose

## Requisitos
- .NET SDK 8
- PostgreSQL (ou Docker)

## Configuração
1. Copie `.env.example` para `.env` e preencha os valores (obrigatório: `JWT_KEY` com mínimo 32 caracteres).
2. Para execução local sem Docker: ajuste a connection string em `ExpenseManagement/appsettings.json` (formato PostgreSQL).
3. A URL da API em `ExpenseWeb` é configurada via `ServiceUri__ExpenseApi` (no Docker usa `http://expense-api:8080`).

## Execução com Docker (recomendado)
```bash
docker compose up -d
```
- API: http://localhost:8080
- Web: http://localhost:8081
- PostgreSQL: localhost:5432

> **Nota:** Se aparecer erro "email must be verified" ao baixar a imagem do PostgreSQL:
> 1. Verifique o e-mail da sua conta em [hub.docker.com](https://hub.docker.com), ou
> 2. Execute `docker logout` e tente novamente (o pull anônimo pode funcionar para imagens públicas).

## Execução local
API:
```
dotnet run --project ExpenseManagement/ExpenseApi.csproj
```

Web:
```
dotnet run --project ExpenseWeb/ExpenseWeb.csproj
```

## Migrações
As migrations são aplicadas automaticamente na inicialização da API. Para rodar manualmente:
```
dotnet ef database update --context AppDbContext --project ExpenseManagement
dotnet ef database update --context ApplicationDbContext --project ExpenseManagement
```

## Autenticação
- `POST /api/auth/register` registra usuário.
- `POST /api/auth/login` retorna `token`, `refreshToken` e `expiresAt`.
- `POST /api/auth/refresh` renova o token.

## Endpoints principais
- `GET /api/expense` (paginado e filtrável)
  - Query params: `page`, `pageSize`, `from`, `to`, `categoryId`, `status`, `search`, `sortBy`, `sortDir`
- `POST /api/expense`
- `PUT /api/expense/{id}`
- `DELETE /api/expense/{id}`
- `POST /api/expense/upload`
- `GET /api/category`

## Upload de imagens
O upload é feito via API e salvo em `wwwroot/uploads/expenses`. A URL retornada é armazenada na despesa.

## Observações
- O Swagger/OpenAPI fica exposto apenas em desenvolvimento.
- Em produção, o endpoint OpenAPI exige autenticação.
