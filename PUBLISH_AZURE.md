# Publicar no Azure e aplicar alterações no banco

## 1. Você já pode publicar no Azure

Sim. Basta configurar no portal do Azure as **Application Settings** (ou variáveis de ambiente) com a connection string e as chaves JWT. O código já está preparado para produção.

---

## 2. Configuração no Azure

### API (App Service ou Container)

Defina estas **Application Settings** (no portal: App Service → Configuration → Application settings):

| Nome | Valor | Obrigatório |
|------|--------|-------------|
| `ConnectionStrings__DefaultConnection` | Connection string do banco SQL no Azure | Sim |
| `Jwt__Key` | Chave secreta longa (ex.: 32+ caracteres) | Sim |
| `Jwt__Issuer` | Ex.: `ExpenseApi` | Sim |
| `Jwt__Audience` | Ex.: `ExpenseWeb` | Sim |
| `Jwt__ExpireHours` | Ex.: `2` | Opcional |
| `Jwt__RefreshTokenDays` | Ex.: `7` | Opcional |
| `ASPNETCORE_ENVIRONMENT` | `Production` | Recomendado |

**Connection string** do Azure SQL costuma ser algo como:

```
Server=tcp:SEU-SERVIDOR.database.windows.net,1433;Initial Catalog=Expense;Persist Security Info=False;User ID=USUARIO;Password=SENHA;MultipleActiveResultSets=False;Encrypt=True;TrustServerCertificate=False;Connection Timeout=30;
```

### Web (App Service ou Container)

| Nome | Valor |
|------|--------|
| `ServiceUri__ExpenseApi` | URL pública da API (ex.: `https://sua-api.azurewebsites.net`) |
| `Jwt__Key` | **A mesma** chave usada na API |
| `Jwt__Issuer` | Mesmo da API |
| `Jwt__Audience` | Mesmo da API |
| `ASPNETCORE_ENVIRONMENT` | `Production` |

---

## 3. Como as alterações do banco vão para o Azure

As migrations (incluindo **AddExpenseAuditFields** – colunas `CreatedAt` e `UpdatedAt`) precisam ser aplicadas **no banco do Azure**. Duas formas comuns:

### Opção A: Rodar as migrations da sua máquina apontando para o banco do Azure

1. No Azure, crie o banco SQL (ou use um já existente) e pegue a **connection string**.
2. Na sua máquina, defina temporariamente a connection string do Azure e rode os updates:

**PowerShell (Windows):**

```powershell
cd "c:\Projeto Expense Management"

# Usar a connection string do banco no Azure
$env:ConnectionStrings__DefaultConnection = "Server=tcp:SEU-SERVIDOR.database.windows.net,1433;Initial Catalog=Expense;User ID=...;Password=...;Encrypt=True;TrustServerCertificate=False;..."

# Aplicar migrations do AppDbContext (Expenses, Categories, CreatedAt, UpdatedAt)
dotnet ef database update --context AppDbContext --project ExpenseManagement

# Aplicar migrations do ApplicationDbContext (Identity, RefreshTokens)
dotnet ef database update --context ApplicationDbContext --project ExpenseManagement
```

**Ou** coloque a connection string do Azure em um arquivo (ex.: `appsettings.Azure.json`) **só na sua máquina** e não faça commit. No `DesignTimeAppDbContextFactory` e `DesignTimeApplicationDbContextFactory` já existe suporte a appsettings; você pode adicionar:

```json
// appsettings.Azure.json (local, não commitar)
{
  "ConnectionStrings": {
    "DefaultConnection": "Server=tcp:....;Initial Catalog=Expense;..."
  }
}
```

E carregar esse arquivo na factory (ou usar variável de ambiente como acima).

### Opção B: Rodar migrations no deploy (container ou pipeline)

No **Dockerfile** da API você pode fazer o container rodar as migrations na subida e depois iniciar a API. Exemplo de script de entrada:

```dockerfile
# Exemplo: script entrypoint que roda migrations e depois inicia a API
# dotnet ef database update ... (requer SDK no container)
```

Ou no **Azure DevOps / GitHub Actions**: num passo do pipeline, rodar `dotnet ef database update` usando a connection string do Azure (guardada em secrets). Assim, cada deploy aplica as migrations pendentes no banco de produção.

---

## 4. Resumo

| O que | Onde |
|-------|------|
| Publicar a API e a Web | Azure App Service ou Azure Container Apps, com as Application Settings acima |
| Aplicar alterações no banco (CreatedAt, UpdatedAt, etc.) | Rodar `dotnet ef database update` para **AppDbContext** e **ApplicationDbContext** **contra a connection string do Azure** (Opção A ou B) |

Depois de aplicar as duas migrations (`AppDbContext` e `ApplicationDbContext`) uma vez no banco do Azure, os próximos deploys da aplicação **não** precisam rodar migration de novo, a menos que você crie novas migrations no projeto.

---

## 5. Troubleshooting: 500.30 (API) e 400 (Web)

### API retorna 500.30 (Internal Server Error) / Error Code 0x8007023e

Significa que a **aplicação não chegou a subir**. As causas mais comuns são:

1. **Configuração faltando no App Service da API**
   - No portal: App Service da API → **Configuration** → **Application settings**
   - Confirme que existem **exatamente** (com dois underscores):
     - `ConnectionStrings__DefaultConnection` → connection string do Azure SQL
     - `Jwt__Key` → chave com **pelo menos 32 caracteres**
     - `Jwt__Issuer` → ex.: `ExpenseApi`
     - `Jwt__Audience` → ex.: `ExpenseWeb`
   - Se algum estiver faltando ou vazio, a API lança exceção na inicialização e o IIS retorna 500.30.

2. **Ver o erro real no Azure**
   - App Service da API → **Monitoring** → **App Service logs**
   - Ative **Application Logging** = **File System**, Level = **Information**
   - Em **Log stream**, ao acessar a API de novo, deve aparecer algo como:  
     `[STARTUP ERROR] InvalidOperationException: ConnectionString 'DefaultConnection' não configurada...`  
     Assim você sabe qual configuração corrigir.

3. **Runtime da API**
   - **Configuration** → **General settings** → **Stack settings** = **.NET 8**.

### Web retorna 400 (Bad Request)

1. **URL da API na Web**
   - No App Service da **Web**: Application Setting `ServiceUri__ExpenseApi` deve ser a URL **pública** da API, **sem barra no final**, ex.:  
     `https://exepense-api-rafaelvn-bfhageg6bngyeyfq.canadacentral-01.azurewebsites.net`

2. **Acesso à Web**
   - Use a URL do App Service da Web (ex.: `https://exepense-web-rafaelvn....azurewebsites.net`) e abra a raiz `/` ou `/Account/Login`. Evite acessar direto `/expense` antes de logar, se a rota exigir autenticação.

3. **CORS na API**
   - No App Service da **API**, em Application Settings, inclua a URL da Web em CORS se necessário, ou use em `appsettings.Production.json` (ou variável):  
     `Cors__AllowedOrigins` = `https://sua-web.azurewebsites.net` (ou múltiplas origens separadas por vírgula, conforme sua configuração).
