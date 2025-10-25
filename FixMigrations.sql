-- Script de solución para el problema de migraciones
-- Ejecuta este script en SQL Server Management Studio o Azure Data Studio

-- PASO 1: Verificar si la columna Image existe en categories
-- Si devuelve 1, significa que ya existe y NO necesita ser agregada
SELECT CASE WHEN EXISTS (
    SELECT 1
    FROM INFORMATION_SCHEMA.COLUMNS
    WHERE TABLE_NAME = 'categories' AND COLUMN_NAME = 'Image'
) THEN 1 ELSE 0 END AS ImageColumnExists;

-- PASO 2: Si la columna Image NO existe (resultado = 0), ejecuta esto:
-- (Si existe, comenta esta sección)
/*
ALTER TABLE categories
ADD Image varbinary(max) NULL;
*/

-- PASO 3: Marcar las migraciones antiguas como aplicadas
-- Solo ejecuta esto si las tablas Products, categories y ProductCategories YA EXISTEN
IF NOT EXISTS (SELECT 1 FROM [__EFMigrationsHistory] WHERE MigrationId = '20231218145955_AddMigration')
BEGIN
    INSERT INTO [__EFMigrationsHistory] ([MigrationId], [ProductVersion])
    VALUES ('20231218145955_AddMigration', '7.0.0');
END

IF NOT EXISTS (SELECT 1 FROM [__EFMigrationsHistory] WHERE MigrationId = '20231220135840_AddProductsTables')
BEGIN
    INSERT INTO [__EFMigrationsHistory] ([MigrationId], [ProductVersion])
    VALUES ('20231220135840_AddProductsTables', '7.0.0');
END

-- PASO 4: Verificar las migraciones registradas
SELECT * FROM [__EFMigrationsHistory] ORDER BY MigrationId;
