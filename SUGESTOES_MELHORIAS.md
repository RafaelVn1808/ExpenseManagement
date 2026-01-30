# Sugestões de Melhorias - Expense Management

## Já implementado nesta sessão
- **Permissões:** Lista de usuários sem cache (novo usuário aparece ao recarregar).
- **Categorias:** Qualquer usuário autenticado pode criar categoria (API e Web).
- **Criar categoria na Nova Despesa:** Botão "+" ao lado do select de categoria abre modal para criar categoria sem sair da tela.
- **Dashboard:** Página inicial com cards de acesso rápido (Despesas, Nova Despesa, Categorias) e link para Permissões (Admin).
- **Dívida parcelada no filtro:** Despesa com início em jan e 3 parcelas passa a aparecer ao filtrar jan, fev e mar; rótulo da parcela (1/3, 2/3, 3/3) é calculado pelo mês filtrado.
- **Upload e visualização:** Fluxo de upload (nota fiscal/comprovante) e exibição na lista com modal para ampliar já existia; conferido e mantido.

---

## Melhorias sugeridas

### 1. **Relatórios e gráficos**
- Gráfico de despesas por categoria (pizza ou barras) no dashboard ou em página dedicada.
- Gráfico de evolução mensal (linha ou barras) para ver tendência.
- Exportar lista de despesas para Excel/CSV.

### 2. **Notificações e lembretes**
- Avisos de vencimento (ex.: despesas que vencem em X dias).
- Opção de e-mail ao cadastrar usuário (envio de confirmação ou lembrete de vencimento).

### 3. **Filtros e busca**
- Filtro por categoria na lista de despesas (além de mês/ano).
- Busca por nome da despesa.
- Filtro por status (Pendente, Pago, etc.) na própria tela de listagem.

### 4. **Usabilidade**
- Confirmação antes de excluir despesa/categoria (modal “Tem certeza?”).
- Mensagem de sucesso ao criar categoria pelo modal (toast ou alerta discreto).
- Paginação com tamanho de página configurável (10, 20, 50) já existente; manter e, se quiser, salvar preferência no navegador.

### 5. **Segurança e auditoria**
- Log de alterações (quem criou/editou despesa e quando).
- Troca de senha e “Esqueci minha senha” (fluxo de recuperação por e-mail).

### 6. **Performance**
- Cache de categorias (pouco alteradas) para reduzir chamadas à API.
- Lazy loading ou infinite scroll na lista de despesas em cenários com muitos registros.

### 7. **Mobile**
- Layout responsivo já ajuda; considerar PWA (instalável no celular) ou app nativo se o uso mobile crescer.

### 8. **Dívida parcelada**
- Se `DateDiffMonth` não existir na sua versão do EF Core para SQL Server, trocar o filtro por:
  - coluna calculada `EndDate = StartDate + (Installments-1) meses` no banco e filtrar por `StartDate <= To && EndDate >= From`, ou
  - pequeno trecho de SQL raw na query para usar `DATEADD(month, Installments-1, StartDate)`.

---

## Observações técnicas
- **Imagens:** Garantir que a API use `PublicBaseUrl` (ou equivalente) para devolver URL absoluta das imagens; assim a visualização na Web funciona mesmo com API em outro host.
- **Admin/usuários:** Se um usuário recém-criado ainda não aparecer na lista, recarregar a página (F5); o endpoint já está com `[ResponseCache(NoStore = true)]` para evitar cache.
