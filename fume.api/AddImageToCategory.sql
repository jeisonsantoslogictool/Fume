BEGIN TRANSACTION;
GO

ALTER TABLE [categories] ADD [Image] varbinary(max) NULL;
GO

INSERT INTO [__EFMigrationsHistory] ([MigrationId], [ProductVersion])
VALUES (N'20251023223904_AddImageToCategory', N'7.0.14');
GO

COMMIT;
GO

