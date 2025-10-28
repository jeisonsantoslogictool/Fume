-- Script para crear las tablas SubCategories y ProductSubCategories
-- Ejecuta esto en SQL Server Management Studio o Azure Data Studio

USE [Fume]

-- Crear tabla SubCategories si no existe
IF NOT EXISTS (SELECT 1 FROM INFORMATION_SCHEMA.TABLES WHERE TABLE_NAME = 'SubCategories')
BEGIN
    CREATE TABLE [SubCategories] (
        [Id] int NOT NULL IDENTITY,
        [Name] nvarchar(100) NOT NULL,
        [CategoryId] int NOT NULL,
        [Image] varbinary(max) NULL,
        CONSTRAINT [PK_SubCategories] PRIMARY KEY ([Id]),
        CONSTRAINT [FK_SubCategories_categories_CategoryId] FOREIGN KEY ([CategoryId]) REFERENCES [categories] ([Id]) ON DELETE CASCADE
    );

    CREATE UNIQUE INDEX [IX_SubCategories_CategoryId_Name] ON [SubCategories] ([CategoryId], [Name]);

    PRINT 'Created table: SubCategories';
END
ELSE
BEGIN
    PRINT 'Table SubCategories already exists';
END

-- Crear tabla ProductSubCategories si no existe
IF NOT EXISTS (SELECT 1 FROM INFORMATION_SCHEMA.TABLES WHERE TABLE_NAME = 'ProductSubCategories')
BEGIN
    CREATE TABLE [ProductSubCategories] (
        [Id] int NOT NULL IDENTITY,
        [ProductId] int NOT NULL,
        [SubCategoryId] int NOT NULL,
        CONSTRAINT [PK_ProductSubCategories] PRIMARY KEY ([Id]),
        CONSTRAINT [FK_ProductSubCategories_Products_ProductId] FOREIGN KEY ([ProductId]) REFERENCES [Products] ([Id]) ON DELETE CASCADE,
        CONSTRAINT [FK_ProductSubCategories_SubCategories_SubCategoryId] FOREIGN KEY ([SubCategoryId]) REFERENCES [SubCategories] ([Id]) ON DELETE CASCADE
    );

    CREATE INDEX [IX_ProductSubCategories_ProductId] ON [ProductSubCategories] ([ProductId]);
    CREATE INDEX [IX_ProductSubCategories_SubCategoryId] ON [ProductSubCategories] ([SubCategoryId]);

    PRINT 'Created table: ProductSubCategories';
END
ELSE
BEGIN
    PRINT 'Table ProductSubCategories already exists';
END

-- Verificar que las tablas se crearon
PRINT ''
PRINT 'Tablas en la base de datos:'
SELECT TABLE_NAME FROM INFORMATION_SCHEMA.TABLES WHERE TABLE_NAME IN ('SubCategories', 'ProductSubCategories') ORDER BY TABLE_NAME;
