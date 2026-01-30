-- Execute este script no banco do Azure (Query Editor no portal ou SSMS)
-- se a migration AddExpenseAuditFields ainda não foi aplicada.
-- Erro: Invalid column name 'CreatedAt'. Invalid column name 'UpdatedAt'.

-- 1) Adicionar as colunas na tabela Expenses
IF NOT EXISTS (SELECT 1 FROM sys.columns WHERE object_id = OBJECT_ID('Expenses') AND name = 'CreatedAt')
BEGIN
    ALTER TABLE [Expenses] ADD [CreatedAt] datetime2 NOT NULL DEFAULT (GETUTCDATE());
END

IF NOT EXISTS (SELECT 1 FROM sys.columns WHERE object_id = OBJECT_ID('Expenses') AND name = 'UpdatedAt')
BEGIN
    ALTER TABLE [Expenses] ADD [UpdatedAt] datetime2 NULL;
END

-- 2) Registrar a migration no histórico do EF (evita reaplicar depois)
IF NOT EXISTS (SELECT 1 FROM [__EFMigrationsHistory] WHERE [MigrationId] = N'20260126000000_AddExpenseAuditFields')
BEGIN
    INSERT INTO [__EFMigrationsHistory] ([MigrationId], [ProductVersion])
    VALUES (N'20260126000000_AddExpenseAuditFields', N'8.0.11');
END
