# Render Deploy - Expense Management

## 🆓 Deploy GRATUITO (90 dias) + $7/mês depois

### 1️⃣ Criar PostgreSQL Database

1. Acesse https://dashboard.render.com
2. **New +** → **PostgreSQL**
3. Configure:
   - **Name:** `expense-db`
   - **Database:** `expense`
   - **User:** `expense_user` (gerado automaticamente)
   - **Region:** escolha o mais próximo
   - **Plan:** **Free** (90 dias grátis, depois $7/mês)
4. Clique em **Create Database**
5. ⚠️ **COPIE** a **Internal Database URL** (formato: `postgresql://user:pass@host/db`)

---

### 2️⃣ Deploy da API (ExpenseApi)

1. **New +** → **Web Service**
2. Conecte seu repositório GitHub
3. Configure:

**Basics:**
- **Name:** `expense-api`
- **Region:** mesmo da database
- **Branch:** `main` ou `render-postgres`
- **Root Directory:** deixe vazio
- **Runtime:** `Docker`
- **Dockerfile Path:** `ExpenseManagement/Dockerfile`

**Plan:**
- **Instance Type:** **Free** (hiberna após 15 min)

**Environment Variables:**
- **Key:** `ConnectionStrings__DefaultConnection` (com **dois underscores** `__`)
- **Value:** cole a **Internal Database URL** do PostgreSQL (ex: `postgresql://user:senha@dpg-xxx-a.oregon-postgres.render.com/expense`)

Não use aspas nem espaços. A URL vem no dashboard do PostgreSQL → **Info** → **Internal Database URL** (botão "Copy").

Exemplo de variáveis:
```
ConnectionStrings__DefaultConnection=postgresql://expense_user:xxxx@dpg-xxx.oregon-postgres.render.com/expense
Jwt__Key=UmaChaveSecretaComPeloMenos32Caracteres!!
Jwt__Issuer=ExpenseApi
Jwt__Audience=ExpenseWeb
Jwt__ExpireHours=2
Jwt__RefreshTokenDays=7
ASPNETCORE_ENVIRONMENT=Production
Cors__AllowedOrigins__0=https://expense-web.onrender.com
```

**⚠️ IMPORTANTE:**
- Nome da variável: exatamente `ConnectionStrings__DefaultConnection` (dois underscores).
- Valor: a Internal Database URL completa, sem aspas e sem espaços no início/fim.
- Se no Render você vincular o PostgreSQL ao Web Service, pode usar a variável `DATABASE_URL` (a API usa como fallback).

4. Clique em **Create Web Service**

---

### 3️⃣ Deploy do Web (ExpenseWeb)

1. **New +** → **Web Service**
2. Selecione o mesmo repositório
3. Configure:

**Basics:**
- **Name:** `expense-web`
- **Region:** mesmo da API
- **Branch:** `main` ou `render-postgres`
- **Root Directory:** deixe vazio
- **Runtime:** `Docker`
- **Dockerfile Path:** `ExpenseWeb/Dockerfile`

**Plan:**
- **Instance Type:** **Free**

**Environment Variables:**
```
ServiceUri__ExpenseApi=https://expense-api.onrender.com
ConnectionStrings__DefaultConnection=[MESMA INTERNAL DATABASE URL DO POSTGRESQL]
Jwt__Key=UmaChaveSecretaComPeloMenos32Caracteres!!
Jwt__Issuer=ExpenseApi
Jwt__Audience=ExpenseWeb
ASPNETCORE_ENVIRONMENT=Production
```

**⚠️ IMPORTANTE:**
- **ServiceUri__ExpenseApi:** use a **URL pública** da API: `https://expense-api-xxx.onrender.com` (substitua pela sua URL real). A URL interna só funciona se a expense-api estiver no plano **Starter** ou superior.
- **ConnectionStrings__DefaultConnection:** use a **mesma Internal Database URL** do PostgreSQL (a mesma da API). Isso faz as chaves de sessão ficarem no banco e evita o erro "The key was not found in the key ring" após restart/hibernação.

4. Clique em **Create Web Service**

---

### 4️⃣ Atualizar CORS da API

Depois que o ExpenseWeb subir, você terá a URL dele (ex: `https://expense-web.onrender.com`).

1. Volte no **expense-api** dashboard
2. Vá em **Environment**
3. Atualize as variáveis de CORS:
   ```
   Cors__AllowedOrigins__0=https://expense-web.onrender.com
   ```
4. Clique em **Save Changes** (vai fazer redeploy automático)

---

## 🎯 URLs finais

Após o deploy:
- **API:** `https://expense-api.onrender.com`
- **Web:** `https://expense-web.onrender.com`
- **Swagger:** `https://expense-api.onrender.com/swagger`

---

## ⚠️ Limitações do Free Tier

### Hibernação (spin down)
- Apps gratuitos **hibernam** após 15 min sem uso
- **Primeira requisição** após hibernar leva ~30-60s
- Depois funciona normal

### Solução para "acordar" automático:
Use um serviço gratuito como **UptimeRobot** ou **Cron-job.org**:
1. Cadastre-se em https://uptimerobot.com (grátis)
2. Adicione monitor HTTP para `https://expense-api.onrender.com/health`
3. Intervalo: 5 minutos
4. Isso mantém a API "acordada" durante o dia

---

## 💰 Custos após 90 dias

| Serviço | Custo |
|---------|-------|
| PostgreSQL | $7/mês (obrigatório) |
| ExpenseApi (free tier) | $0 (com hibernação) |
| ExpenseWeb (free tier) | $0 (com hibernação) |
| **TOTAL** | **$7/mês** |

---

## 🚀 Alternativa: Sem hibernação

Para API sempre disponível (produção real):

1. **expense-api**: Instance Type = **Starter** ($7/mês)
2. **expense-web**: Free tier (ok hibernar)
3. **PostgreSQL**: $7/mês

**Total:** $14/mês

---

## 📝 Checklist

- [ ] Criar conta no Render
- [ ] Push do código para GitHub (branch `render-postgres`)
- [ ] Criar PostgreSQL database
- [ ] Copiar Internal Database URL
- [ ] Deploy expense-api (com connection string)
- [ ] Deploy expense-web (com API URL)
- [ ] Atualizar CORS na API
- [ ] Testar: registrar usuário, criar despesa
- [ ] (Opcional) Configurar UptimeRobot para evitar hibernação

---

## 🔧 Troubleshooting

### "Format of the initialization string does not conform to specification"
- A connection string está vazia ou com nome errado.
- No **expense-api** → **Environment**:
  1. Adicione a variável **Key:** `ConnectionStrings__DefaultConnection` (com **dois** underscores `__`).
  2. **Value:** cole a **Internal Database URL** do PostgreSQL (dashboard do banco → **Info** → **Internal Database URL** → Copy).
  3. Sem aspas, sem espaços no início/fim.
- **Alternativa:** Vincule o PostgreSQL ao Web Service (dashboard da API → **Environment** → **Add from Render** → escolha o PostgreSQL). O Render injeta `DATABASE_URL` e a API usa como fallback.

### Erro nas migrations
- Render aplica migrations automaticamente no startup
- Se falhar, veja logs: Dashboard → Logs

### CORS error
- Verifique se adicionou a URL do ExpenseWeb no CORS da API
- Use a URL HTTPS completa

### "The key was not found in the key ring" (session cookie)
- As chaves de sessão estavam no disco e se perdem a cada restart no Render.
- **Solução:** No **expense-web** → **Environment**, adicione `ConnectionStrings__DefaultConnection` com a **mesma Internal Database URL** do PostgreSQL (a mesma da API). O Web passa a guardar as chaves no banco e o login continua válido após hibernação/restart.

### Timeout no primeiro acesso
- Normal no free tier - aguarde ~30s
- Configure UptimeRobot para manter ativo

### 429 (Too Many Requests) ao fazer login
- No tier gratuito, o Render aplica **rate limit** quando um serviço chama outro pela **URL pública** (internet). O ExpenseWeb chamando a API por `https://expense-api-xxx.onrender.com` pode receber 429.
- **Importante:** Serviços **free** no Render **não podem receber** conexões na rede privada. A expense-api (free) não aceita requisições em `http://expense-api-xxx:8080` — por isso aparece "Serviço temporariamente indisponível" ao usar URL interna com ambos no free tier.
- **Soluções:**
  1. **Manter URL pública** (`https://expense-api-qvo6.onrender.com`) — o login funciona quando não há rate limit; em picos pode dar 429. Aguarde alguns minutos e tente de novo.
  2. **Upgrade da expense-api para Starter** ($7/mês) — a API passa a aceitar rede privada; então use a URL interna (`http://expense-api-qvo6:8080`) em `ServiceUri__ExpenseApi` e o 429 deixa de ocorrer.
  3. Configure **UptimeRobot** no endpoint `/health` da API (intervalo 5 min) para manter o serviço ativo e reduzir variações.
