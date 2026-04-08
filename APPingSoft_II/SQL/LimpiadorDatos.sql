/*
    Proyecto: APPingSoft_II
    Archivo: LimpiadorDatos.sql
    Objetivo: Limpiar el contenido de todas las tablas (sin eliminar estructura)
    Motor: SQL Server (T-SQL)
*/

SET NOCOUNT ON;

IF DB_ID(N'APPingSoftII') IS NULL
BEGIN
    THROW 50000, N'La base APPingSoftII no existe. Ejecute primero creadorbasedatos.sql.', 1;
END;

USE APPingSoftII;

BEGIN TRY
    BEGIN TRANSACTION;

    DECLARE @SQL NVARCHAR(MAX) = N'';

    /* Desactiva constraints para permitir borrado masivo sin conflictos FK */
    SELECT
        @SQL += N'ALTER TABLE '
             + QUOTENAME(s.name) + N'.' + QUOTENAME(t.name)
             + N' NOCHECK CONSTRAINT ALL;'
             + CHAR(13) + CHAR(10)
    FROM sys.tables t
    INNER JOIN sys.schemas s
        ON s.schema_id = t.schema_id
    WHERE t.is_ms_shipped = 0;

    IF @SQL <> N''
        EXEC sys.sp_executesql @SQL;

    /* Desactiva triggers para evitar validaciones durante la limpieza */
    SET @SQL = N'';

    SELECT
        @SQL += N'DISABLE TRIGGER ALL ON '
             + QUOTENAME(s.name) + N'.' + QUOTENAME(t.name)
             + N';'
             + CHAR(13) + CHAR(10)
    FROM sys.tables t
    INNER JOIN sys.schemas s
        ON s.schema_id = t.schema_id
    WHERE t.is_ms_shipped = 0;

    IF @SQL <> N''
        EXEC sys.sp_executesql @SQL;

    /* Borra datos de todas las tablas de usuario */
    SET @SQL = N'';

    SELECT
        @SQL += N'DELETE FROM '
             + QUOTENAME(s.name) + N'.' + QUOTENAME(t.name)
             + N';'
             + CHAR(13) + CHAR(10)
    FROM sys.tables t
    INNER JOIN sys.schemas s
        ON s.schema_id = t.schema_id
    WHERE t.is_ms_shipped = 0
    ORDER BY t.object_id DESC;

    IF @SQL <> N''
        EXEC sys.sp_executesql @SQL;

    /* Reinicia columnas IDENTITY */
    SET @SQL = N'';

    SELECT
        @SQL += N'DBCC CHECKIDENT ('''
             + QUOTENAME(s.name) + N'.' + QUOTENAME(t.name)
             + N''', RESEED, 0) WITH NO_INFOMSGS;'
             + CHAR(13) + CHAR(10)
    FROM sys.tables t
    INNER JOIN sys.schemas s
        ON s.schema_id = t.schema_id
    WHERE t.is_ms_shipped = 0
      AND EXISTS
      (
          SELECT 1
          FROM sys.identity_columns ic
          WHERE ic.object_id = t.object_id
      );

    IF @SQL <> N''
        EXEC sys.sp_executesql @SQL;

    /* Reactiva triggers */
    SET @SQL = N'';

    SELECT
        @SQL += N'ENABLE TRIGGER ALL ON '
             + QUOTENAME(s.name) + N'.' + QUOTENAME(t.name)
             + N';'
             + CHAR(13) + CHAR(10)
    FROM sys.tables t
    INNER JOIN sys.schemas s
        ON s.schema_id = t.schema_id
    WHERE t.is_ms_shipped = 0;

    IF @SQL <> N''
        EXEC sys.sp_executesql @SQL;

    /* Reactiva y valida constraints */
    SET @SQL = N'';

    SELECT
        @SQL += N'ALTER TABLE '
             + QUOTENAME(s.name) + N'.' + QUOTENAME(t.name)
             + N' WITH CHECK CHECK CONSTRAINT ALL;'
             + CHAR(13) + CHAR(10)
    FROM sys.tables t
    INNER JOIN sys.schemas s
        ON s.schema_id = t.schema_id
    WHERE t.is_ms_shipped = 0;

    IF @SQL <> N''
        EXEC sys.sp_executesql @SQL;

    COMMIT TRANSACTION;
END TRY
BEGIN CATCH
    IF @@TRANCOUNT > 0
        ROLLBACK TRANSACTION;

    THROW;
END CATCH;

PRINT N'Contenido de todas las tablas limpiado correctamente en APPingSoftII.';
