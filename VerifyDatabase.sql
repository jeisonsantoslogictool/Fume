-- Script para verificar el estado de la base de datos
-- Ejecuta este script en SQL Server Management Studio o Azure Data Studio

-- 1. Verificar si existe la columna Image en categories
SELECT
    COLUMN_NAME,
    DATA_TYPE,
    IS_NULLABLE
FROM INFORMATION_SCHEMA.COLUMNS
WHERE TABLE_NAME = 'categories';

-- 2. Verificar qué tablas existen
SELECT TABLE_NAME
FROM INFORMATION_SCHEMA.TABLES
WHERE TABLE_TYPE = 'BASE TABLE'
ORDER BY TABLE_NAME;

-- 3. Verificar migraciones aplicadas
SELECT * FROM [__EFMigrationsHistory];
