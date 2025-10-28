-- Script para registrar todas las migraciones ya aplicadas en la base de datos Fume
-- Ejecuta esto en SQL Server Management Studio o Azure Data Studio

USE [Fume]

-- Registrar migraciones existentes en __EFMigrationsHistory
IF NOT EXISTS (SELECT 1 FROM [__EFMigrationsHistory] WHERE MigrationId = '20231204131322_InitialDb')
BEGIN
    INSERT INTO [__EFMigrationsHistory] ([MigrationId], [ProductVersion])
    VALUES ('20231204131322_InitialDb', '7.0.0');
    PRINT 'Registered: 20231204131322_InitialDb';
END

IF NOT EXISTS (SELECT 1 FROM [__EFMigrationsHistory] WHERE MigrationId = '20231207131956_AddStatesAndCitiesTable')
BEGIN
    INSERT INTO [__EFMigrationsHistory] ([MigrationId], [ProductVersion])
    VALUES ('20231207131956_AddStatesAndCitiesTable', '7.0.0');
    PRINT 'Registered: 20231207131956_AddStatesAndCitiesTable';
END

IF NOT EXISTS (SELECT 1 FROM [__EFMigrationsHistory] WHERE MigrationId = '20231208204026_AddUserTables')
BEGIN
    INSERT INTO [__EFMigrationsHistory] ([MigrationId], [ProductVersion])
    VALUES ('20231208204026_AddUserTables', '7.0.0');
    PRINT 'Registered: 20231208204026_AddUserTables';
END

IF NOT EXISTS (SELECT 1 FROM [__EFMigrationsHistory] WHERE MigrationId = '20231218145955_AddMigration')
BEGIN
    INSERT INTO [__EFMigrationsHistory] ([MigrationId], [ProductVersion])
    VALUES ('20231218145955_AddMigration', '7.0.0');
    PRINT 'Registered: 20231218145955_AddMigration';
END

IF NOT EXISTS (SELECT 1 FROM [__EFMigrationsHistory] WHERE MigrationId = '20231220135840_AddProductsTables')
BEGIN
    INSERT INTO [__EFMigrationsHistory] ([MigrationId], [ProductVersion])
    VALUES ('20231220135840_AddProductsTables', '7.0.0');
    PRINT 'Registered: 20231220135840_AddProductsTables';
END

IF NOT EXISTS (SELECT 1 FROM [__EFMigrationsHistory] WHERE MigrationId = '20251024235540_AddSubCategoriesAndProductSubCategories')
BEGIN
    INSERT INTO [__EFMigrationsHistory] ([MigrationId], [ProductVersion])
    VALUES ('20251024235540_AddSubCategoriesAndProductSubCategories', '7.0.0');
    PRINT 'Registered: 20251024235540_AddSubCategoriesAndProductSubCategories';
END

-- Verificar que todas las migraciones están registradas
PRINT ''
PRINT 'Migraciones registradas:'
SELECT [MigrationId], [ProductVersion] FROM [__EFMigrationsHistory] ORDER BY MigrationId;
