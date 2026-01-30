# Configurar User Secrets (rodar local sem Docker)

Execute estes comandos **uma vez por máquina** no terminal, na raiz do repositório (`c:\Projeto Expense Management`).

## API (ExpenseManagement)

```powershell
cd "c:\Projeto Expense Management"

dotnet user-secrets set "ConnectionStrings__DefaultConnection" "Server=RAFAELVN;Database=Expense;Trusted_Connection=True;TrustServerCertificate=True;" --project ExpenseManagement/ExpenseApi.csproj

dotnet user-secrets set "Jwt__Key" "ExpenseApiJwtKeyLocalDevMin32Caracteres!!" --project ExpenseManagement/ExpenseApi.csproj
```

## Web (ExpenseWeb)

```powershell
dotnet user-secrets set "ServiceUri__ExpenseApi" "https://localhost:7298" --project ExpenseWeb/ExpenseWeb.csproj

dotnet user-secrets set "Jwt__Key" "ExpenseApiJwtKeyLocalDevMin32Caracteres!!" --project ExpenseWeb/ExpenseWeb.csproj
```

A chave JWT deve ser a **mesma** na API e no Web (mínimo 32 caracteres). Troque `ExpenseApiJwtKeyLocalDevMin32Caracteres!!` se quiser outra.

Depois disso, rode a API e o Web (F5 ou `dotnet run`). Em **Development**, a API e o Web carregam os User Secrets automaticamente.

**Migrações EF:** ao rodar `dotnet ef database update`, defina a connection string na variável de ambiente antes:
```powershell
$env:ConnectionStrings__DefaultConnection = "Server=RAFAELVN;Database=Expense;Trusted_Connection=True;TrustServerCertificate=True;"
dotnet ef database update --project ExpenseManagement/ExpenseApi.csproj
```
