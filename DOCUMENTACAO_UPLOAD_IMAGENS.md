# 📸 Documentação Completa: Upload de Imagens

## 📋 Índice
1. [Visão Geral](#visão-geral)
2. [Arquitetura da Solução](#arquitetura-da-solução)
3. [Componentes Implementados](#componentes-implementados)
4. [Fluxo Completo](#fluxo-completo)
5. [Detalhamento Técnico](#detalhamento-técnico)
6. [Segurança e Validações](#segurança-e-validações)

---

## 🎯 Visão Geral

O sistema de upload de imagens permite que usuários anexem:
- **Imagem da Nota Fiscal** (`NoteImageFile`)
- **Imagem do Comprovante** (`ProofImageFile`)

As imagens são salvas no servidor e suas URLs são armazenadas no banco de dados.

---

## 🏗️ Arquitetura da Solução

```
┌─────────────────┐
│   View (HTML)    │  ← Usuário seleciona arquivo
└────────┬─────────┘
         │
         ▼
┌─────────────────┐
│  Controller     │  ← Recebe IFormFile
└────────┬─────────┘
         │
         ▼
┌─────────────────┐
│ ImageUpload     │  ← Valida e salva arquivo
│ Service         │
└────────┬─────────┘
         │
         ▼
┌─────────────────┐
│ wwwroot/uploads │  ← Arquivo salvo no disco
│ /expenses/      │
└─────────────────┘
         │
         ▼
┌─────────────────┐
│ ExpenseService  │  ← Envia URL para API
└────────┬─────────┘
         │
         ▼
┌─────────────────┐
│  API (Backend)  │  ← Salva URL no banco
└─────────────────┘
```

---

## 🔧 Componentes Implementados

### 1. **ImageUploadService.cs** - Serviço de Upload

**Localização:** `ExpenseWeb/Services/ImageUploadService.cs`

#### Responsabilidades:
- ✅ Validar tamanho do arquivo (máx. 5MB)
- ✅ Validar extensões permitidas
- ✅ Criar diretório se não existir
- ✅ Gerar nomes únicos para arquivos
- ✅ Salvar arquivo no disco
- ✅ Deletar arquivos antigos

#### Código Detalhado:

```csharp
public class ImageUploadService
{
    // 📁 CAMINHO ONDE AS IMAGENS SERÃO SALVAS
    private const string UploadFolder = "uploads/expenses";
    
    // 📏 TAMANHO MÁXIMO: 5MB
    private const long MaxFileSize = 5 * 1024 * 1024; // 5 * 1024 * 1024 bytes
    
    // ✅ EXTENSÕES PERMITIDAS
    private readonly string[] AllowedExtensions = { 
        ".jpg", ".jpeg", ".png", ".gif", ".webp" 
    };
    
    private readonly IWebHostEnvironment _environment;
    
    // Construtor recebe IWebHostEnvironment para acessar wwwroot
    public ImageUploadService(IWebHostEnvironment environment)
    {
        _environment = environment;
    }
}
```

#### Método `UploadImageAsync` - Passo a Passo:

```csharp
public async Task<string?> UploadImageAsync(IFormFile? file, string prefix = "img")
{
    // 1️⃣ VERIFICAÇÃO INICIAL
    if (file == null || file.Length == 0)
        return null; // Nenhum arquivo enviado
    
    // 2️⃣ VALIDAÇÃO DE TAMANHO
    if (file.Length > MaxFileSize)
        throw new InvalidOperationException(
            $"O arquivo excede o tamanho máximo de {MaxFileSize / 1024 / 1024}MB."
        );
    
    // 3️⃣ VALIDAÇÃO DE EXTENSÃO
    var extension = Path.GetExtension(file.FileName).ToLowerInvariant();
    // Exemplo: "foto.jpg" → ".jpg"
    
    if (!AllowedExtensions.Contains(extension))
        throw new InvalidOperationException(
            $"Extensão não permitida. Use: {string.Join(", ", AllowedExtensions)}"
        );
    
    // 4️⃣ CRIAR DIRETÓRIO SE NÃO EXISTIR
    var uploadPath = Path.Combine(_environment.WebRootPath, UploadFolder);
    // Exemplo: C:\Projeto\ExpenseWeb\wwwroot\uploads\expenses
    
    if (!Directory.Exists(uploadPath))
        Directory.CreateDirectory(uploadPath);
    
    // 5️⃣ GERAR NOME ÚNICO
    var fileName = $"{prefix}_{Guid.NewGuid()}{extension}";
    // Exemplo: "note_a1b2c3d4-e5f6-7890-abcd-ef1234567890.jpg"
    //          "proof_f9e8d7c6-b5a4-3210-9876-543210fedcba.png"
    
    var filePath = Path.Combine(uploadPath, fileName);
    
    // 6️⃣ SALVAR ARQUIVO NO DISCO
    using (var stream = new FileStream(filePath, FileMode.Create))
    {
        await file.CopyToAsync(stream);
    }
    
    // 7️⃣ RETORNAR URL RELATIVA
    return $"/{UploadFolder}/{fileName}";
    // Exemplo: "/uploads/expenses/note_a1b2c3d4-e5f6-7890-abcd-ef1234567890.jpg"
}
```

#### Método `DeleteImage` - Para Remover Arquivos:

```csharp
public void DeleteImage(string? imageUrl)
{
    if (string.IsNullOrEmpty(imageUrl))
        return; // Nada para deletar
    
    try
    {
        // Converter URL relativa em caminho físico
        var filePath = Path.Combine(_environment.WebRootPath, imageUrl.TrimStart('/'));
        // "/uploads/expenses/note_xxx.jpg" → 
        // "C:\Projeto\ExpenseWeb\wwwroot\uploads\expenses\note_xxx.jpg"
        
        if (File.Exists(filePath))
        {
            File.Delete(filePath);
        }
    }
    catch
    {
        // Ignorar erros (arquivo pode não existir)
    }
}
```

---

### 2. **ExpenseViewModel.cs** - Modelo com Propriedades de Upload

**Localização:** `ExpenseWeb/Models/ExpenseViewModel.cs`

#### Propriedades Adicionadas:

```csharp
public class ExpenseViewModel
{
    // ... outras propriedades ...
    
    // 📸 URLs DAS IMAGENS (salvas no banco)
    public string? NoteImageUrl { get; set; }      // Ex: "/uploads/expenses/note_xxx.jpg"
    public string? ProofImageUrl { get; set; }     // Ex: "/uploads/expenses/proof_xxx.png"
    
    // 📁 ARQUIVOS PARA UPLOAD (não são salvos no banco)
    [Display(Name = "Imagem da Nota Fiscal")]
    public IFormFile? NoteImageFile { get; set; }  // Arquivo selecionado pelo usuário
    
    [Display(Name = "Imagem do Comprovante")]
    public IFormFile? ProofImageFile { get; set; }  // Arquivo selecionado pelo usuário
}
```

#### Por que duas propriedades?

- **`NoteImageFile` (IFormFile)**: Usado apenas no formulário para receber o arquivo do usuário
- **`NoteImageUrl` (string)**: URL da imagem salva, enviada para a API e armazenada no banco

**Fluxo:**
```
IFormFile → Upload → Salva no disco → Retorna URL → Salva URL no banco
```

---

### 3. **ExpenseController.cs** - Processamento no Controller

**Localização:** `ExpenseWeb/Controllers/ExpenseController.cs`

#### Injeção de Dependência:

```csharp
public class ExpenseController : Controller
{
    private readonly ImageUploadService _imageUploadService;
    
    public ExpenseController(
        IExpenseService expenseService,
        ICategoryService categoryService,
        ImageUploadService imageUploadService)  // ← Injetado
    {
        _imageUploadService = imageUploadService;
        // ...
    }
}
```

#### Método `CreateExpense` (POST) - Processamento:

```csharp
[HttpPost]
[ValidateAntiForgeryToken]
public async Task<IActionResult> CreateExpense(ExpenseViewModel expense)
{
    // 1️⃣ VALIDAÇÃO DO MODELO
    if (!ModelState.IsValid)
    {
        // Recarregar categorias e retornar view com erros
        return View(expense);
    }
    
    // 2️⃣ PROCESSAR UPLOAD DE IMAGENS
    try
    {
        // Se o usuário selecionou uma imagem da nota fiscal
        if (expense.NoteImageFile != null)
        {
            // Upload retorna a URL: "/uploads/expenses/note_xxx.jpg"
            expense.NoteImageUrl = await _imageUploadService.UploadImageAsync(
                expense.NoteImageFile, 
                "note"  // Prefixo para identificar tipo de imagem
            );
        }
        
        // Se o usuário selecionou uma imagem do comprovante
        if (expense.ProofImageFile != null)
        {
            expense.ProofImageUrl = await _imageUploadService.UploadImageAsync(
                expense.ProofImageFile, 
                "proof"  // Prefixo diferente
            );
        }
    }
    catch (Exception ex)
    {
        // Se houver erro no upload (tamanho, extensão, etc.)
        ModelState.AddModelError("", ex.Message);
        return View(expense); // Retornar para o usuário corrigir
    }
    
    // 3️⃣ ENVIAR PARA API (com as URLs das imagens)
    var result = await _expenseService.CreateExpense(expense);
    
    // 4️⃣ REDIRECIONAR
    return RedirectToAction(nameof(Index));
}
```

#### Método `UpdateExpense` (POST) - Atualização com Substituição:

```csharp
[HttpPost]
public async Task<IActionResult> UpdateExpense(ExpenseViewModel expense)
{
    if (!ModelState.IsValid)
        return View(expense);
    
    // 1️⃣ BUSCAR DESPESA EXISTENTE (para manter URLs antigas)
    var existingExpense = await _expenseService.GetExpenseById(expense.ExpenseId);
    if (existingExpense != null)
    {
        // Manter URLs antigas se não houver novo upload
        expense.NoteImageUrl = existingExpense.NoteImageUrl;
        expense.ProofImageUrl = existingExpense.ProofImageUrl;
    }
    
    // 2️⃣ PROCESSAR NOVOS UPLOADS
    try
    {
        if (expense.NoteImageFile != null)
        {
            // Deletar imagem antiga se existir
            if (!string.IsNullOrEmpty(existingExpense?.NoteImageUrl))
            {
                _imageUploadService.DeleteImage(existingExpense.NoteImageUrl);
            }
            
            // Upload da nova imagem
            expense.NoteImageUrl = await _imageUploadService.UploadImageAsync(
                expense.NoteImageFile, 
                "note"
            );
        }
        
        // Mesmo processo para ProofImageFile...
    }
    catch (Exception ex)
    {
        ModelState.AddModelError("", ex.Message);
        return View(expense);
    }
    
    // 3️⃣ ATUALIZAR NA API
    var updatedExpense = await _expenseService.UpdateExpense(expense);
    
    return RedirectToAction(nameof(Index));
}
```

---

### 4. **CreateExpense.cshtml** - View com Campos de Upload

**Localização:** `ExpenseWeb/Views/Expense/CreateExpense.cshtml`

#### Formulário com `enctype`:

```html
<!-- ⚠️ IMPORTANTE: enctype="multipart/form-data" é obrigatório para upload -->
<form asp-action="CreateExpense" enctype="multipart/form-data">
```

**Por quê?**
- Sem `enctype="multipart/form-data"`, o arquivo não é enviado
- Permite enviar dados binários (imagens) junto com dados de formulário

#### Campo de Upload - Nota Fiscal:

```html
<div class="col-md-6 mb-3">
    <!-- Label -->
    <label asp-for="NoteImageFile" class="form-label">
        <i class="bi bi-receipt me-1"></i>Imagem da Nota Fiscal
    </label>
    
    <!-- Input File -->
    <div class="input-group">
        <span class="input-group-text">
            <i class="bi bi-image text-muted"></i>
        </span>
        <input asp-for="NoteImageFile" 
               type="file" 
               class="form-control" 
               accept="image/*"  <!-- Aceita apenas imagens -->
               onchange="previewImage(this, 'notePreview')" />  <!-- Preview em JS -->
    </div>
    
    <!-- Mensagem de ajuda -->
    <small class="form-text text-muted">
        Formatos aceitos: JPG, PNG, GIF, WEBP (máx. 5MB)
    </small>
    
    <!-- Preview da imagem (inicialmente oculto) -->
    <div id="notePreview" class="mt-2" style="display: none;">
        <img id="notePreviewImg" src="" alt="Preview" 
             class="img-thumbnail" 
             style="max-height: 150px; max-width: 100%;" />
        <button type="button" 
                class="btn btn-sm btn-outline-danger mt-2" 
                onclick="removeImage('notePreview', 'NoteImageFile')">
            <i class="bi bi-x-circle me-1"></i> Remover
        </button>
    </div>
</div>
```

#### JavaScript para Preview:

```javascript
function previewImage(input, previewId) {
    if (input.files && input.files[0]) {
        var reader = new FileReader();  // API do navegador para ler arquivos
        
        var previewDiv = document.getElementById(previewId);
        var previewImg = document.getElementById(previewId + 'Img');
        
        // Quando o arquivo for lido
        reader.onload = function(e) {
            previewImg.src = e.target.result;  // Define src da imagem
            previewDiv.style.display = 'block'; // Mostra o preview
        };
        
        // Lê o arquivo como Data URL (base64)
        reader.readAsDataURL(input.files[0]);
    }
}

function removeImage(previewId, inputId) {
    // Esconde o preview
    document.getElementById(previewId).style.display = 'none';
    
    // Limpa a imagem
    document.getElementById(previewId + 'Img').src = '';
    
    // Limpa o input file
    document.getElementById(inputId).value = '';
}
```

**Como funciona o Preview:**
1. Usuário seleciona arquivo → `onchange` dispara
2. `FileReader` lê o arquivo como base64
3. Imagem é exibida no `<img>` usando `data:image/jpeg;base64,...`
4. Preview aparece antes do envio do formulário

---

### 5. **UpdateExpense.cshtml** - View de Edição

**Localização:** `ExpenseWeb/Views/Expense/UpdateExpense.cshtml`

#### Exibição de Imagem Existente:

```html
@if (!string.IsNullOrEmpty(Model.NoteImageUrl))
{
    <div class="mb-2">
        <p class="text-muted small mb-1">Imagem atual:</p>
        <!-- Exibe a imagem que já está salva -->
        <img src="@Model.NoteImageUrl" 
             alt="Nota Fiscal" 
             class="img-thumbnail" 
             style="max-height: 150px; max-width: 100%;" />
    </div>
}
```

**Comportamento:**
- Se já existe imagem → mostra a imagem atual
- Usuário pode selecionar nova imagem → substitui a antiga
- Se não selecionar nada → mantém a imagem atual

---

### 6. **Program.cs** - Registro do Serviço

**Localização:** `ExpenseWeb/Program.cs`

```csharp
// 🔧 SERVICES
builder.Services.AddScoped<ImageUploadService>();
```

**Por que Scoped?**
- Uma instância por requisição HTTP
- Permite acesso ao `IWebHostEnvironment` corretamente
- Mais eficiente que Transient para este caso

---

## 🔄 Fluxo Completo

### Cenário: Criar Despesa com Imagem

```
1. USUÁRIO
   └─> Acessa /Expense/CreateExpense
   └─> Preenche formulário
   └─> Seleciona arquivo de imagem
   └─> Visualiza preview
   └─> Clica em "Cadastrar"

2. NAVEGADOR
   └─> Envia POST com multipart/form-data
   └─> Inclui arquivo binário no request

3. CONTROLLER (ExpenseController)
   └─> Recebe ExpenseViewModel
   └─> ExpenseViewModel.NoteImageFile contém o arquivo
   └─> Chama ImageUploadService.UploadImageAsync()

4. IMAGE UPLOAD SERVICE
   └─> Valida tamanho (≤ 5MB)
   └─> Valida extensão (.jpg, .png, etc.)
   └─> Cria diretório se necessário
   └─> Gera nome único: "note_guid.jpg"
   └─> Salva arquivo em: wwwroot/uploads/expenses/
   └─> Retorna URL: "/uploads/expenses/note_guid.jpg"

5. CONTROLLER (continuação)
   └─> Atribui URL: expense.NoteImageUrl = "/uploads/expenses/note_guid.jpg"
   └─> Chama ExpenseService.CreateExpense(expense)

6. EXPENSE SERVICE
   └─> Serializa ExpenseViewModel para JSON
   └─> Envia para API: POST /api/expense/
   └─> JSON inclui: { "noteImageUrl": "/uploads/expenses/note_guid.jpg" }

7. API (ExpenseManagement)
   └─> Recebe ExpenseDTO
   └─> Salva no banco de dados
   └─> Campo NoteImageUrl armazenado

8. RESULTADO
   └─> Imagem salva em: wwwroot/uploads/expenses/note_guid.jpg
   └─> URL salva no banco: "/uploads/expenses/note_guid.jpg"
   └─> Imagem acessível via: https://localhost:7000/uploads/expenses/note_guid.jpg
```

---

## 🔒 Segurança e Validações

### 1. Validação de Tamanho

```csharp
if (file.Length > MaxFileSize)  // 5MB
    throw new InvalidOperationException("Arquivo muito grande");
```

**Por quê?**
- Previne ataques de DoS (negação de serviço)
- Evita sobrecarga do servidor
- Melhora performance

### 2. Validação de Extensão

```csharp
var extension = Path.GetExtension(file.FileName).ToLowerInvariant();
if (!AllowedExtensions.Contains(extension))
    throw new InvalidOperationException("Extensão não permitida");
```

**Por quê?**
- Previne upload de arquivos maliciosos (.exe, .bat, etc.)
- Garante que apenas imagens sejam aceitas
- Protege contra execução de código

### 3. Nomes Únicos com GUID

```csharp
var fileName = $"{prefix}_{Guid.NewGuid()}{extension}";
// Exemplo: "note_a1b2c3d4-e5f6-7890-abcd-ef1234567890.jpg"
```

**Por quê?**
- Evita sobrescrita de arquivos
- Previne conflitos de nomes
- Facilita rastreamento

### 4. Caminho Seguro

```csharp
var uploadPath = Path.Combine(_environment.WebRootPath, UploadFolder);
```

**Por quê?**
- Usa `Path.Combine` para evitar path traversal attacks
- Limita uploads à pasta `wwwroot/uploads/expenses/`
- Não permite acesso a outras pastas do sistema

---

## 📁 Estrutura de Arquivos

```
ExpenseWeb/
├── wwwroot/
│   └── uploads/
│       └── expenses/
│           ├── note_a1b2c3d4-e5f6-7890-abcd-ef1234567890.jpg
│           ├── note_f9e8d7c6-b5a4-3210-9876-543210fedcba.png
│           ├── proof_12345678-1234-1234-1234-123456789abc.jpg
│           └── ...
├── Services/
│   └── ImageUploadService.cs  ← Serviço de upload
├── Models/
│   └── ExpenseViewModel.cs    ← Modelo com IFormFile
├── Controllers/
│   └── ExpenseController.cs    ← Processa upload
└── Views/
    └── Expense/
        ├── CreateExpense.cshtml  ← Formulário de upload
        └── UpdateExpense.cshtml  ← Edição com preview
```

---

## 🎨 Funcionalidades Visuais

### Preview de Imagem

**Antes do envio:**
- Usuário seleciona arquivo
- JavaScript lê o arquivo
- Exibe preview usando `FileReader`
- Permite remover antes de enviar

**Código JavaScript:**
```javascript
reader.readAsDataURL(input.files[0]);
// Converte arquivo para: data:image/jpeg;base64,/9j/4AAQSkZJRg...
```

### Exibição de Imagem Existente (Edição)

**Ao editar:**
- Se já existe imagem → mostra thumbnail
- Usuário pode substituir
- Se não selecionar nova → mantém a antiga

---

## 🔍 Detalhes Técnicos Importantes

### 1. IFormFile vs IFormFileCollection

```csharp
// ✅ CORRETO: Um arquivo por propriedade
public IFormFile? NoteImageFile { get; set; }

// ❌ ERRADO: Para múltiplos arquivos
public IFormFileCollection? Files { get; set; }
```

### 2. Caminho Físico vs URL

```csharp
// CAMINHO FÍSICO (no servidor)
var physicalPath = Path.Combine(_environment.WebRootPath, "uploads/expenses/file.jpg");
// C:\Projeto\ExpenseWeb\wwwroot\uploads\expenses\file.jpg

// URL RELATIVA (para o navegador)
var url = "/uploads/expenses/file.jpg";
// https://localhost:7000/uploads/expenses/file.jpg
```

### 3. Async/Await

```csharp
// ✅ CORRETO: Upload assíncrono
await file.CopyToAsync(stream);

// ❌ ERRADO: Bloqueia thread
file.CopyTo(stream);
```

### 4. Using Statement

```csharp
using (var stream = new FileStream(filePath, FileMode.Create))
{
    await file.CopyToAsync(stream);
} // Stream é fechado automaticamente aqui
```

**Por quê?**
- Garante que o arquivo seja fechado
- Libera recursos do sistema
- Previne locks de arquivo

---

## 🧪 Exemplo Prático Completo

### Cenário: Usuário faz upload de nota fiscal

**1. Usuário seleciona arquivo:**
```
Arquivo: "nota_fiscal_compra.jpg"
Tamanho: 2.5 MB
Tipo: image/jpeg
```

**2. JavaScript faz preview:**
```javascript
FileReader lê arquivo → Converte para base64 → Exibe no <img>
```

**3. Usuário clica em "Cadastrar":**
```
POST /Expense/CreateExpense
Content-Type: multipart/form-data

--boundary
Content-Disposition: form-data; name="Name"
Supermercado

--boundary
Content-Disposition: form-data; name="Amount"
150.00

--boundary
Content-Disposition: form-data; name="NoteImageFile"; filename="nota_fiscal_compra.jpg"
Content-Type: image/jpeg

[binary data da imagem...]
```

**4. Controller processa:**
```csharp
expense.NoteImageFile.Length = 2.5 MB ✅
extension = ".jpg" ✅
fileName = "note_a1b2c3d4-e5f6-7890-abcd-ef1234567890.jpg"
filePath = "C:\...\wwwroot\uploads\expenses\note_xxx.jpg"
Salva arquivo ✅
expense.NoteImageUrl = "/uploads/expenses/note_xxx.jpg"
```

**5. Envia para API:**
```json
{
  "name": "Supermercado",
  "amount": 150.00,
  "noteImageUrl": "/uploads/expenses/note_xxx.jpg",
  ...
}
```

**6. API salva no banco:**
```sql
INSERT INTO Expenses (Name, TotalAmount, NoteImageUrl, ...)
VALUES ('Supermercado', 150.00, '/uploads/expenses/note_xxx.jpg', ...)
```

**7. Imagem acessível:**
```
URL: https://localhost:7000/uploads/expenses/note_xxx.jpg
```

---

## ⚠️ Pontos de Atenção

### 1. enctype="multipart/form-data"

**OBRIGATÓRIO** no formulário:
```html
<form enctype="multipart/form-data">
```

Sem isso, o arquivo não é enviado!

### 2. Tamanho Máximo

**Configurado:** 5MB
**Pode ser ajustado** em `ImageUploadService.cs`:
```csharp
private const long MaxFileSize = 10 * 1024 * 1024; // 10MB
```

### 3. Extensões Permitidas

**Atualmente:** `.jpg`, `.jpeg`, `.png`, `.gif`, `.webp`

**Para adicionar mais:**
```csharp
private readonly string[] AllowedExtensions = { 
    ".jpg", ".jpeg", ".png", ".gif", ".webp", ".bmp" 
};
```

### 4. Pasta de Upload

**Localização:** `wwwroot/uploads/expenses/`

**Acessível via:** `https://localhost:7000/uploads/expenses/arquivo.jpg`

**Importante:** A pasta `wwwroot` é servida estaticamente pelo ASP.NET Core

---

## 🎯 Resumo

### O que foi implementado:

1. ✅ **Serviço de Upload** (`ImageUploadService`)
   - Validações de segurança
   - Geração de nomes únicos
   - Salvamento em disco

2. ✅ **Modelo Atualizado** (`ExpenseViewModel`)
   - Propriedades `IFormFile` para upload
   - Propriedades `string` para URLs

3. ✅ **Controller Atualizado** (`ExpenseController`)
   - Processamento de upload
   - Tratamento de erros
   - Substituição de imagens antigas

4. ✅ **Views Atualizadas**
   - Campos de upload
   - Preview de imagens
   - Exibição de imagens existentes

5. ✅ **JavaScript**
   - Preview antes do envio
   - Remoção de preview

### Fluxo Final:

```
Usuário → Seleciona arquivo → Preview → Envia
    ↓
Controller → Valida → Upload Service → Salva no disco
    ↓
Retorna URL → Envia para API → Salva no banco
    ↓
Imagem acessível via URL
```

---

## 📚 Referências

- **IFormFile**: Interface do ASP.NET Core para arquivos
- **FileStream**: Classe .NET para manipular arquivos
- **FileReader API**: API do navegador para ler arquivos
- **multipart/form-data**: Tipo MIME para upload de arquivos

---

**Implementação completa e funcional!** 🎉
