# Instrucciones para Solucionar el Problema de Migraciones

## Problema
Las tablas `categories`, `Products` y `ProductCategories` ya existen en la base de datos, pero las migraciones no están registradas en `__EFMigrationsHistory`.

## Solución: Marcar las migraciones como aplicadas manualmente

### Paso 1: Ejecutar este script SQL

Abre **SQL Server Management Studio** o **Azure Data Studio**, conéctate a tu base de datos **Fume** y ejecuta:

```sql
-- Verificar migraciones actuales
SELECT * FROM [__EFMigrationsHistory];

-- Marcar las migraciones antiguas como aplicadas
-- (Solo si no aparecen en la consulta anterior)
INSERT INTO [__EFMigrationsHistory] ([MigrationId], [ProductVersion])
VALUES ('20231218145955_AddMigration', '7.0.0');

INSERT INTO [__EFMigrationsHistory] ([MigrationId], [ProductVersion])
VALUES ('20231220135840_AddProductsTables', '7.0.0');

-- Verificar que se agregaron
SELECT * FROM [__EFMigrationsHistory];
```

### Paso 2: Verificar si la columna Image existe en categories

```sql
-- Verificar columnas de la tabla categories
SELECT COLUMN_NAME, DATA_TYPE, IS_NULLABLE
FROM INFORMATION_SCHEMA.COLUMNS
WHERE TABLE_NAME = 'categories';
```

**Si la columna `Image` NO aparece**, ejecuta:

```sql
ALTER TABLE categories
ADD Image varbinary(max) NULL;
```

### Paso 3: Recrear la migración de subcategorías

Después de ejecutar el script SQL, regresa a la terminal y ejecuta:

```bash
cd fume.api
dotnet ef migrations add AddSubCategoriesAndProductSubCategories
dotnet ef database update
```

---

## Alternativa: Si prefieres una solución automática

Ejecuta este comando que intentará aplicar las migraciones de forma segura:

```bash
cd fume.api
dotnet ef database update --verbose
```

Si da error, entonces sigue con la **Solución Manual** (Paso 1-3).
