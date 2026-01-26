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
- .NET 10, ASP.NET Core Web API, ASP.NET Core MVC
- EF Core, AutoMapper, ASP.NET Identity
- SQL Server
- xUnit, Moq
- Docker, Docker Compose

## Requisitos
- .NET SDK 10
- SQL Server

## Configuração
1. Ajuste a string de conexão em `ExpenseManagement/appsettings.json`.
2. Ajuste a URL da API em `ExpenseWeb/appsettings.json` (`ServiceUri:ExpenseApi`).
3. Configure a chave JWT em `ExpenseManagement/appsettings.json` e `ExpenseWeb/appsettings.json`.

## Execução
API:
```
dotnet run --project ExpenseManagement/ExpenseApi.csproj
```

Web:
```
dotnet run --project ExpenseWeb/ExpenseWeb.csproj
```

## Migrações
```
dotnet ef database update --project ExpenseManagement/ExpenseApi.csproj
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
