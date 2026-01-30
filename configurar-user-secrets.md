# Configurar User Secrets (rodar local sem Docker)

Execute estes comandos **uma vez por máquina** no terminal, na raiz do repositório (`c:\Projeto Expense Management`).

## API (ExpenseManagement)

```powershell
cd "c:\Projeto Expense Management"

dotnet user-secrets set "ConnectionStrings__DefaultConnection" "Server=RAFAELVN;Database=Expense;Trusted_Connection=True;TrustServerCertificate=True;" --project ExpenseManagement/ExpenseApi.csproj

dotnet user-secrets set "Jwt__Key" "ExpenseApiJwtKeyLocalDevMin32Caracteres!!" --project ExpenseManagement/ExpenseApi.csproj
```

## Web (ExpenseWeb)

O Web **precisa** da mesma chave JWT (e Issuer/Audience) que a API, pois valida o token da sessão e envia o Bearer nas chamadas à API. Sem isso, /Admin e criar categoria retornam 401.

```powershell
dotnet user-secrets set "ServiceUri__ExpenseApi" "https://localhost:7298" --project ExpenseWeb/ExpenseWeb.csproj

dotnet user-secrets set "Jwt__Key" "ExpenseApiJwtKeyLocalDevMin32Caracteres!!" --project ExpenseWeb/ExpenseWeb.csproj

dotnet user-secrets set "Jwt__Issuer" "ExpenseApi" --project ExpenseWeb/ExpenseWeb.csproj

dotnet user-secrets set "Jwt__Audience" "ExpenseWeb" --project ExpenseWeb/ExpenseWeb.csproj
```

A chave JWT deve ser a **mesma** na API e no Web (mínimo 32 caracteres). Troque `ExpenseApiJwtKeyLocalDevMin32Caracteres!!` se quiser outra.

Depois disso, rode a API e o Web (F5 ou `dotnet run`). Em **Development**, a API e o Web carregam os User Secrets automaticamente.

**Migrações EF:** ao rodar `dotnet ef database update`, defina a connection string na variável de ambiente antes:
```powershell
$env:ConnectionStrings__DefaultConnection = "Server=RAFAELVN;Database=Expense;Trusted_Connection=True;TrustServerCertificate=True;"
dotnet ef database update --project ExpenseManagement/ExpenseApi.csproj
```

---

## Segurança JWT e checklist

- **API e Web precisam da mesma chave JWT** (`Jwt:Key`), do mesmo `Jwt:Issuer` e `Jwt:Audience`. Caso contrário o token é rejeitado (401).
- Em Development, use **User Secrets** no Web para `Jwt__Key`, `Jwt__Issuer` e `Jwt__Audience` (iguais à API). O JWT não fica em appsettings para não commitar segredos.
- Se **Permissões (/Admin)** ou **criar categoria** continuarem falhando:
  1. Confirme que a API está rodando na URL configurada em `ServiceUri:ExpenseApi` (ex.: https://localhost:7298).
  2. Faça login de novo (para gerar um token novo) e teste outra vez.
  3. Rode as migrações da API para garantir que as tabelas do Identity (e RefreshToken) existem:
     ```powershell
     $env:ConnectionStrings__DefaultConnection = "Server=SEU_SERVIDOR;Database=Expense;Trusted_Connection=True;TrustServerCertificate=True;"
     dotnet ef database update --project ExpenseManagement/ExpenseApi.csproj
     ```
  4. Para ter um admin inicial em Development, configure (API) `Admin:Email` e `Admin:Password` em appsettings.Development.json ou User Secrets; o `IdentitySeed` cria o usuário na primeira execução.
