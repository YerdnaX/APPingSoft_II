/*
    Proyecto: APPingSoft_II
    Archivo: HotfixFlujoAcademico.sql
    Objetivo: Aplicar correcciones de integridad del flujo academico en bases existentes
    Motor: SQL Server (T-SQL)
*/

SET NOCOUNT ON;

IF DB_ID(N'APPingSoftII') IS NULL
BEGIN
    THROW 50000, N'La base APPingSoftII no existe. Ejecute primero creadorbasedatos.sql.', 1;
END;

USE APPingSoftII;
GO

IF NOT EXISTS
(
    SELECT 1
    FROM sys.indexes
    WHERE name = N'IX_Inscripciones_ProgramaId_Estado'
      AND object_id = OBJECT_ID(N'dbo.Inscripciones')
)
BEGIN
    CREATE INDEX IX_Inscripciones_ProgramaId_Estado
        ON dbo.Inscripciones(ProgramaId, Estado);
END
GO

CREATE OR ALTER TRIGGER dbo.trg_ResultadosEvaluacion_Validaciones
ON dbo.ResultadosEvaluacion
AFTER INSERT, UPDATE
AS
BEGIN
    SET NOCOUNT ON;

    IF EXISTS
    (
        SELECT 1
        FROM inserted i
        INNER JOIN dbo.Evaluaciones e
            ON e.EvaluacionId = i.EvaluacionId
        WHERE i.NotaFinal > e.PuntosMax
    )
    BEGIN
        RAISERROR(N'La nota final no puede ser mayor a los puntos maximos de la evaluacion.', 16, 1);
        ROLLBACK TRANSACTION;
        RETURN;
    END

    IF EXISTS
    (
        SELECT 1
        FROM inserted i
        INNER JOIN dbo.Inscripciones ins
            ON ins.InscripcionId = i.InscripcionId
        INNER JOIN dbo.Programas p
            ON p.ProgramaId = ins.ProgramaId
        INNER JOIN dbo.Evaluaciones e
            ON e.EvaluacionId = i.EvaluacionId
        WHERE p.CursoId <> e.CursoId
    )
    BEGIN
        RAISERROR(N'No se puede registrar resultado: la inscripcion no pertenece al curso de la evaluacion.', 16, 1);
        ROLLBACK TRANSACTION;
        RETURN;
    END

    IF EXISTS
    (
        SELECT 1
        FROM inserted i
        INNER JOIN dbo.Inscripciones ins
            ON ins.InscripcionId = i.InscripcionId
        WHERE ins.Estado NOT IN (N'Activa', N'Finalizada')
    )
    BEGIN
        RAISERROR(N'No se puede registrar resultado: la inscripcion debe estar Activa o Finalizada.', 16, 1);
        ROLLBACK TRANSACTION;
        RETURN;
    END

    IF EXISTS
    (
        SELECT 1
        FROM inserted i
        INNER JOIN dbo.Inscripciones ins
            ON ins.InscripcionId = i.InscripcionId
        INNER JOIN dbo.Programas p
            ON p.ProgramaId = ins.ProgramaId
        WHERE p.Estado = N'Inactivo'
    )
    BEGIN
        RAISERROR(N'No se puede registrar resultado en un programa inactivo.', 16, 1);
        ROLLBACK TRANSACTION;
        RETURN;
    END
END
GO

PRINT N'Hotfix de flujo academico aplicado correctamente.';
GO
