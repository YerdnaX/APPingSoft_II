/*
    Proyecto: APPingSoft_II
    Archivo: creadorbasedatos.sql
    Motor: SQL Server (T-SQL)
*/

SET NOCOUNT ON;

IF DB_ID(N'APPingSoftII') IS NULL
BEGIN
    CREATE DATABASE APPingSoftII;
END
GO

USE APPingSoftII;
GO

/* Limpiar objetos programables para permitir re-ejecucion del script */
IF OBJECT_ID(N'dbo.trg_ResultadoCriterio_Validaciones', N'TR') IS NOT NULL
    DROP TRIGGER dbo.trg_ResultadoCriterio_Validaciones;
GO
IF OBJECT_ID(N'dbo.trg_ResultadosEvaluacion_Validaciones', N'TR') IS NOT NULL
    DROP TRIGGER dbo.trg_ResultadosEvaluacion_Validaciones;
GO
IF OBJECT_ID(N'dbo.trg_CriteriosEvaluacion_PonderacionTotal', N'TR') IS NOT NULL
    DROP TRIGGER dbo.trg_CriteriosEvaluacion_PonderacionTotal;
GO

/* ==============================
   TABLAS MAESTRAS
   ============================== */

IF OBJECT_ID(N'dbo.Usuarios', N'U') IS NULL
BEGIN
    CREATE TABLE dbo.Usuarios
    (
        UsuarioId INT IDENTITY(1,1) NOT NULL
            CONSTRAINT PK_Usuarios PRIMARY KEY,
        NombreCompleto NVARCHAR(120) NOT NULL,
        CorreoElectronico NVARCHAR(150) NOT NULL,
        Contrasena NVARCHAR(255) NOT NULL,
        Rol NVARCHAR(30) NOT NULL
            CONSTRAINT DF_Usuarios_Rol DEFAULT N'Administrador',
        Estado NVARCHAR(20) NOT NULL
            CONSTRAINT DF_Usuarios_Estado DEFAULT N'Activo',
        FechaCreacion DATETIME2(0) NOT NULL
            CONSTRAINT DF_Usuarios_FechaCreacion DEFAULT SYSDATETIME(),
        CONSTRAINT UQ_Usuarios_Correo UNIQUE (CorreoElectronico),
        CONSTRAINT CK_Usuarios_Rol CHECK (Rol IN (N'Administrador', N'Coordinador', N'Instructor')),
        CONSTRAINT CK_Usuarios_Estado CHECK (Estado IN (N'Activo', N'Inactivo'))
    );
END
GO

IF OBJECT_ID(N'dbo.Instructores', N'U') IS NULL
BEGIN
    CREATE TABLE dbo.Instructores
    (
        InstructorId INT IDENTITY(1,1) NOT NULL
            CONSTRAINT PK_Instructores PRIMARY KEY,
        UsuarioId INT NULL,
        NombreCompleto NVARCHAR(120) NOT NULL,
        CorreoElectronico NVARCHAR(150) NOT NULL,
        Especialidad NVARCHAR(120) NULL,
        Estado NVARCHAR(20) NOT NULL
            CONSTRAINT DF_Instructores_Estado DEFAULT N'Activo',
        FechaRegistro DATETIME2(0) NOT NULL
            CONSTRAINT DF_Instructores_FechaRegistro DEFAULT SYSDATETIME(),
        CONSTRAINT UQ_Instructores_Correo UNIQUE (CorreoElectronico),
        CONSTRAINT UQ_Instructores_Usuario UNIQUE (UsuarioId),
        CONSTRAINT CK_Instructores_Estado CHECK (Estado IN (N'Activo', N'Inactivo')),
        CONSTRAINT FK_Instructores_Usuarios
            FOREIGN KEY (UsuarioId) REFERENCES dbo.Usuarios(UsuarioId)
    );
END
GO

IF OBJECT_ID(N'dbo.Cursos', N'U') IS NULL
BEGIN
    CREATE TABLE dbo.Cursos
    (
        CursoId INT IDENTITY(1,1) NOT NULL
            CONSTRAINT PK_Cursos PRIMARY KEY,
        Codigo NVARCHAR(20) NOT NULL,
        Nombre NVARCHAR(120) NOT NULL,
        Descripcion NVARCHAR(500) NULL,
        DuracionHoras INT NOT NULL
            CONSTRAINT DF_Cursos_DuracionHoras DEFAULT (40),
        Estado NVARCHAR(20) NOT NULL
            CONSTRAINT DF_Cursos_Estado DEFAULT N'Activo',
        FechaCreacion DATETIME2(0) NOT NULL
            CONSTRAINT DF_Cursos_FechaCreacion DEFAULT SYSDATETIME(),
        CONSTRAINT UQ_Cursos_Codigo UNIQUE (Codigo),
        CONSTRAINT CK_Cursos_DuracionHoras CHECK (DuracionHoras > 0),
        CONSTRAINT CK_Cursos_Estado CHECK (Estado IN (N'Activo', N'Inactivo'))
    );
END
GO

IF OBJECT_ID(N'dbo.Programas', N'U') IS NULL
BEGIN
    CREATE TABLE dbo.Programas
    (
        ProgramaId INT IDENTITY(1,1) NOT NULL
            CONSTRAINT PK_Programas PRIMARY KEY,
        Nombre NVARCHAR(120) NOT NULL,
        FechaInicio DATE NOT NULL,
        FechaFin DATE NOT NULL,
        Estado NVARCHAR(20) NOT NULL
            CONSTRAINT DF_Programas_Estado DEFAULT N'Activo',
        InstructorId INT NOT NULL,
        CursoId INT NOT NULL,
        FechaCreacion DATETIME2(0) NOT NULL
            CONSTRAINT DF_Programas_FechaCreacion DEFAULT SYSDATETIME(),
        CONSTRAINT UQ_Programas_Nombre_FechaInicio UNIQUE (Nombre, FechaInicio),
        CONSTRAINT CK_Programas_Fechas CHECK (FechaFin >= FechaInicio),
        CONSTRAINT CK_Programas_Estado CHECK (Estado IN (N'Activo', N'Inactivo', N'Finalizado')),
        CONSTRAINT FK_Programas_Instructores
            FOREIGN KEY (InstructorId) REFERENCES dbo.Instructores(InstructorId),
        CONSTRAINT FK_Programas_Cursos
            FOREIGN KEY (CursoId) REFERENCES dbo.Cursos(CursoId)
    );
END
GO

IF OBJECT_ID(N'dbo.Participantes', N'U') IS NULL
BEGIN
    CREATE TABLE dbo.Participantes
    (
        ParticipanteId INT IDENTITY(1,1) NOT NULL
            CONSTRAINT PK_Participantes PRIMARY KEY,
        NombreCompleto NVARCHAR(120) NOT NULL,
        CorreoElectronico NVARCHAR(150) NOT NULL,
        Telefono NVARCHAR(25) NULL,
        Estado NVARCHAR(20) NOT NULL
            CONSTRAINT DF_Participantes_Estado DEFAULT N'Activo',
        FechaRegistro DATETIME2(0) NOT NULL
            CONSTRAINT DF_Participantes_FechaRegistro DEFAULT SYSDATETIME(),
        CONSTRAINT UQ_Participantes_Correo UNIQUE (CorreoElectronico),
        CONSTRAINT CK_Participantes_Estado CHECK (Estado IN (N'Activo', N'Inactivo'))
    );
END
GO

IF OBJECT_ID(N'dbo.Inscripciones', N'U') IS NULL
BEGIN
    CREATE TABLE dbo.Inscripciones
    (
        InscripcionId INT IDENTITY(1,1) NOT NULL
            CONSTRAINT PK_Inscripciones PRIMARY KEY,
        ProgramaId INT NOT NULL,
        ParticipanteId INT NOT NULL,
        FechaInscripcion DATE NOT NULL
            CONSTRAINT DF_Inscripciones_FechaInscripcion DEFAULT (CAST(GETDATE() AS DATE)),
        Estado NVARCHAR(20) NOT NULL
            CONSTRAINT DF_Inscripciones_Estado DEFAULT N'Activa',
        CONSTRAINT UQ_Inscripciones_Programa_Participante UNIQUE (ProgramaId, ParticipanteId),
        CONSTRAINT CK_Inscripciones_Estado CHECK (Estado IN (N'Activa', N'Retirada', N'Finalizada')),
        CONSTRAINT FK_Inscripciones_Programas
            FOREIGN KEY (ProgramaId) REFERENCES dbo.Programas(ProgramaId),
        CONSTRAINT FK_Inscripciones_Participantes
            FOREIGN KEY (ParticipanteId) REFERENCES dbo.Participantes(ParticipanteId)
    );
END
GO

/* ==============================
   TABLAS TRANSACCIONALES
   ============================== */

IF OBJECT_ID(N'dbo.Evaluaciones', N'U') IS NULL
BEGIN
    CREATE TABLE dbo.Evaluaciones
    (
        EvaluacionId INT IDENTITY(1,1) NOT NULL
            CONSTRAINT PK_Evaluaciones PRIMARY KEY,
        CursoId INT NOT NULL,
        Titulo NVARCHAR(150) NOT NULL,
        Tipo NVARCHAR(30) NOT NULL,
        Momento NVARCHAR(100) NOT NULL,
        FechaApertura DATE NOT NULL,
        FechaCierre DATE NOT NULL,
        PuntosMax DECIMAL(8,2) NOT NULL,
        Estado NVARCHAR(20) NOT NULL
            CONSTRAINT DF_Evaluaciones_Estado DEFAULT N'Pendiente',
        FechaCreacion DATETIME2(0) NOT NULL
            CONSTRAINT DF_Evaluaciones_FechaCreacion DEFAULT SYSDATETIME(),
        CONSTRAINT UQ_Evaluaciones_Curso_Titulo_Fecha UNIQUE (CursoId, Titulo, FechaApertura),
        CONSTRAINT CK_Evaluaciones_Tipo CHECK (Tipo IN (N'Diagnóstica', N'Formativa', N'Sumativa')),
        CONSTRAINT CK_Evaluaciones_Estado CHECK (Estado IN (N'Activo', N'Cerrado', N'Pendiente')),
        CONSTRAINT CK_Evaluaciones_Fechas CHECK (FechaCierre >= FechaApertura),
        CONSTRAINT CK_Evaluaciones_PuntosMax CHECK (PuntosMax > 0),
        CONSTRAINT FK_Evaluaciones_Cursos
            FOREIGN KEY (CursoId) REFERENCES dbo.Cursos(CursoId)
    );
END
GO

IF OBJECT_ID(N'dbo.CriteriosEvaluacion', N'U') IS NULL
BEGIN
    CREATE TABLE dbo.CriteriosEvaluacion
    (
        CriterioId INT IDENTITY(1,1) NOT NULL
            CONSTRAINT PK_CriteriosEvaluacion PRIMARY KEY,
        EvaluacionId INT NOT NULL,
        NombreCriterio NVARCHAR(150) NOT NULL,
        Ponderacion DECIMAL(5,2) NOT NULL,
        PuntosMaxCriterio DECIMAL(8,2) NOT NULL,
        CONSTRAINT UQ_Criterios_Evaluacion_Nombre UNIQUE (EvaluacionId, NombreCriterio),
        CONSTRAINT CK_Criterios_Ponderacion CHECK (Ponderacion > 0 AND Ponderacion <= 100),
        CONSTRAINT CK_Criterios_PuntosMax CHECK (PuntosMaxCriterio > 0),
        CONSTRAINT FK_CriteriosEvaluacion_Evaluaciones
            FOREIGN KEY (EvaluacionId) REFERENCES dbo.Evaluaciones(EvaluacionId) ON DELETE CASCADE
    );
END
GO

IF OBJECT_ID(N'dbo.ResultadosEvaluacion', N'U') IS NULL
BEGIN
    CREATE TABLE dbo.ResultadosEvaluacion
    (
        ResultadoId INT IDENTITY(1,1) NOT NULL
            CONSTRAINT PK_ResultadosEvaluacion PRIMARY KEY,
        EvaluacionId INT NOT NULL,
        InscripcionId INT NOT NULL,
        NotaFinal DECIMAL(8,2) NOT NULL,
        CalificadoEn DATETIME2(0) NOT NULL
            CONSTRAINT DF_ResultadosEvaluacion_CalificadoEn DEFAULT SYSDATETIME(),
        Observaciones NVARCHAR(600) NULL,
        CONSTRAINT UQ_Resultados_Evaluacion_Inscripcion UNIQUE (EvaluacionId, InscripcionId),
        CONSTRAINT CK_ResultadosEvaluacion_NotaFinal CHECK (NotaFinal >= 0),
        CONSTRAINT FK_ResultadosEvaluacion_Evaluaciones
            FOREIGN KEY (EvaluacionId) REFERENCES dbo.Evaluaciones(EvaluacionId),
        CONSTRAINT FK_ResultadosEvaluacion_Inscripciones
            FOREIGN KEY (InscripcionId) REFERENCES dbo.Inscripciones(InscripcionId)
    );
END
GO

IF OBJECT_ID(N'dbo.ResultadoCriterio', N'U') IS NULL
BEGIN
    CREATE TABLE dbo.ResultadoCriterio
    (
        ResultadoCriterioId INT IDENTITY(1,1) NOT NULL
            CONSTRAINT PK_ResultadoCriterio PRIMARY KEY,
        ResultadoId INT NOT NULL,
        CriterioId INT NOT NULL,
        PuntajeObtenido DECIMAL(8,2) NOT NULL,
        Observacion NVARCHAR(300) NULL,
        CONSTRAINT UQ_ResultadoCriterio_Resultado_Criterio UNIQUE (ResultadoId, CriterioId),
        CONSTRAINT CK_ResultadoCriterio_Puntaje CHECK (PuntajeObtenido >= 0),
        CONSTRAINT FK_ResultadoCriterio_ResultadosEvaluacion
            FOREIGN KEY (ResultadoId) REFERENCES dbo.ResultadosEvaluacion(ResultadoId) ON DELETE CASCADE,
        CONSTRAINT FK_ResultadoCriterio_CriteriosEvaluacion
            FOREIGN KEY (CriterioId) REFERENCES dbo.CriteriosEvaluacion(CriterioId)
    );
END
GO

/* ==============================
   INDICES
   ============================== */

IF NOT EXISTS (SELECT 1 FROM sys.indexes WHERE name = N'IX_Programas_CursoId' AND object_id = OBJECT_ID(N'dbo.Programas'))
    CREATE INDEX IX_Programas_CursoId ON dbo.Programas(CursoId);
GO
IF NOT EXISTS (SELECT 1 FROM sys.indexes WHERE name = N'IX_Programas_InstructorId' AND object_id = OBJECT_ID(N'dbo.Programas'))
    CREATE INDEX IX_Programas_InstructorId ON dbo.Programas(InstructorId);
GO
IF NOT EXISTS (SELECT 1 FROM sys.indexes WHERE name = N'IX_Evaluaciones_CursoId_Estado' AND object_id = OBJECT_ID(N'dbo.Evaluaciones'))
    CREATE INDEX IX_Evaluaciones_CursoId_Estado ON dbo.Evaluaciones(CursoId, Estado);
GO
IF NOT EXISTS (SELECT 1 FROM sys.indexes WHERE name = N'IX_Inscripciones_ParticipanteId' AND object_id = OBJECT_ID(N'dbo.Inscripciones'))
    CREATE INDEX IX_Inscripciones_ParticipanteId ON dbo.Inscripciones(ParticipanteId);
GO
IF NOT EXISTS (SELECT 1 FROM sys.indexes WHERE name = N'IX_Inscripciones_ProgramaId_Estado' AND object_id = OBJECT_ID(N'dbo.Inscripciones'))
    CREATE INDEX IX_Inscripciones_ProgramaId_Estado ON dbo.Inscripciones(ProgramaId, Estado);
GO
IF NOT EXISTS (SELECT 1 FROM sys.indexes WHERE name = N'IX_ResultadosEvaluacion_EvaluacionId' AND object_id = OBJECT_ID(N'dbo.ResultadosEvaluacion'))
    CREATE INDEX IX_ResultadosEvaluacion_EvaluacionId ON dbo.ResultadosEvaluacion(EvaluacionId);
GO
IF NOT EXISTS (SELECT 1 FROM sys.indexes WHERE name = N'IX_ResultadosEvaluacion_InscripcionId' AND object_id = OBJECT_ID(N'dbo.ResultadosEvaluacion'))
    CREATE INDEX IX_ResultadosEvaluacion_InscripcionId ON dbo.ResultadosEvaluacion(InscripcionId);
GO

/* ==============================
   TRIGGERS DE VALIDACION
   ============================== */

CREATE TRIGGER dbo.trg_CriteriosEvaluacion_PonderacionTotal
ON dbo.CriteriosEvaluacion
AFTER INSERT, UPDATE, DELETE
AS
BEGIN
    SET NOCOUNT ON;

    IF EXISTS
    (
        SELECT 1
        FROM
        (
            SELECT ce.EvaluacionId, SUM(ce.Ponderacion) AS PonderacionTotal
            FROM dbo.CriteriosEvaluacion ce
            WHERE ce.EvaluacionId IN
            (
                SELECT EvaluacionId FROM inserted
                UNION
                SELECT EvaluacionId FROM deleted
            )
            GROUP BY ce.EvaluacionId
        ) tot
        WHERE tot.PonderacionTotal > 100.00
    )
    BEGIN
        RAISERROR(N'La suma de ponderaciones por evaluacion no puede exceder 100.', 16, 1);
        ROLLBACK TRANSACTION;
        RETURN;
    END
END
GO

CREATE TRIGGER dbo.trg_ResultadosEvaluacion_Validaciones
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

CREATE TRIGGER dbo.trg_ResultadoCriterio_Validaciones
ON dbo.ResultadoCriterio
AFTER INSERT, UPDATE
AS
BEGIN
    SET NOCOUNT ON;

    IF EXISTS
    (
        SELECT 1
        FROM inserted i
        INNER JOIN dbo.ResultadosEvaluacion r
            ON r.ResultadoId = i.ResultadoId
        INNER JOIN dbo.CriteriosEvaluacion c
            ON c.CriterioId = i.CriterioId
        WHERE r.EvaluacionId <> c.EvaluacionId
    )
    BEGIN
        RAISERROR(N'El criterio seleccionado no pertenece a la evaluacion del resultado.', 16, 1);
        ROLLBACK TRANSACTION;
        RETURN;
    END

    IF EXISTS
    (
        SELECT 1
        FROM inserted i
        INNER JOIN dbo.CriteriosEvaluacion c
            ON c.CriterioId = i.CriterioId
        WHERE i.PuntajeObtenido > c.PuntosMaxCriterio
    )
    BEGIN
        RAISERROR(N'El puntaje obtenido no puede exceder los puntos maximos del criterio.', 16, 1);
        ROLLBACK TRANSACTION;
        RETURN;
    END
END
GO

/* ==============================
   VISTAS PARA REPORTES Y METRICAS
   ============================== */

CREATE OR ALTER VIEW dbo.vw_ResultadosDetallados
AS
SELECT
    re.ResultadoId,
    pa.ParticipanteId,
    pa.NombreCompleto AS Participante,
    pa.CorreoElectronico AS CorreoParticipante,
    pr.ProgramaId,
    pr.Nombre AS Programa,
    cu.CursoId,
    cu.Codigo AS CodigoCurso,
    cu.Nombre AS Curso,
    ev.EvaluacionId,
    ev.Titulo AS Evaluacion,
    ev.Tipo AS TipoEvaluacion,
    ev.Momento,
    ev.PuntosMax,
    re.NotaFinal,
    CAST((re.NotaFinal * 100.0) / NULLIF(ev.PuntosMax, 0) AS DECIMAL(6,2)) AS PorcentajeLogrado,
    re.CalificadoEn,
    re.Observaciones
FROM dbo.ResultadosEvaluacion re
INNER JOIN dbo.Evaluaciones ev
    ON ev.EvaluacionId = re.EvaluacionId
INNER JOIN dbo.Inscripciones ins
    ON ins.InscripcionId = re.InscripcionId
INNER JOIN dbo.Participantes pa
    ON pa.ParticipanteId = ins.ParticipanteId
INNER JOIN dbo.Programas pr
    ON pr.ProgramaId = ins.ProgramaId
INNER JOIN dbo.Cursos cu
    ON cu.CursoId = ev.CursoId;
GO

CREATE OR ALTER VIEW dbo.vw_MetricasCurso
AS
SELECT
    cu.CursoId,
    cu.Codigo,
    cu.Nombre AS Curso,
    COUNT(DISTINCT ev.EvaluacionId) AS TotalEvaluaciones,
    COUNT(re.ResultadoId) AS TotalResultados,
    CAST(AVG(CAST(re.NotaFinal AS DECIMAL(10,2))) AS DECIMAL(10,2)) AS PromedioNota,
    CAST
    (
        AVG
        (
            CASE
                WHEN re.ResultadoId IS NULL THEN NULL
                WHEN re.NotaFinal >= (ev.PuntosMax * 0.70) THEN 100.0
                ELSE 0.0
            END
        )
        AS DECIMAL(6,2)
    ) AS PorcentajeAprobacion
FROM dbo.Cursos cu
LEFT JOIN dbo.Evaluaciones ev
    ON ev.CursoId = cu.CursoId
LEFT JOIN dbo.ResultadosEvaluacion re
    ON re.EvaluacionId = ev.EvaluacionId
GROUP BY
    cu.CursoId,
    cu.Codigo,
    cu.Nombre;
GO

CREATE OR ALTER VIEW dbo.vw_MetricasPrograma
AS
SELECT
    pr.ProgramaId,
    pr.Nombre AS Programa,
    pr.Estado,
    COUNT(DISTINCT ins.ParticipanteId) AS TotalInscritos,
    COUNT(re.ResultadoId) AS ResultadosRegistrados,
    CAST(AVG(CAST(re.NotaFinal AS DECIMAL(10,2))) AS DECIMAL(10,2)) AS PromedioGeneral
FROM dbo.Programas pr
LEFT JOIN dbo.Inscripciones ins
    ON ins.ProgramaId = pr.ProgramaId
LEFT JOIN dbo.ResultadosEvaluacion re
    ON re.InscripcionId = ins.InscripcionId
GROUP BY
    pr.ProgramaId,
    pr.Nombre,
    pr.Estado;
GO

/* ==============================
   DATOS SEMILLA
   ============================== */

IF NOT EXISTS (SELECT 1 FROM dbo.Usuarios WHERE CorreoElectronico = N'admin@appingsoft.local')
BEGIN
    INSERT INTO dbo.Usuarios (NombreCompleto, CorreoElectronico, Contrasena, Rol, Estado)
    VALUES (N'Administrador General', N'admin@appingsoft.local', N'Admin123*', N'Administrador', N'Activo');
END
GO

IF NOT EXISTS (SELECT 1 FROM dbo.Usuarios WHERE CorreoElectronico = N'laura.mendez@appingsoft.local')
BEGIN
    INSERT INTO dbo.Usuarios (NombreCompleto, CorreoElectronico, Contrasena, Rol, Estado)
    VALUES (N'Laura Mendez', N'laura.mendez@appingsoft.local', N'Instructor123*', N'Instructor', N'Activo');
END
GO

IF NOT EXISTS (SELECT 1 FROM dbo.Instructores WHERE CorreoElectronico = N'laura.mendez@appingsoft.local')
BEGIN
    INSERT INTO dbo.Instructores (UsuarioId, NombreCompleto, CorreoElectronico, Especialidad, Estado)
    SELECT
        u.UsuarioId,
        N'Laura Mendez',
        N'laura.mendez@appingsoft.local',
        N'Bases de Datos',
        N'Activo'
    FROM dbo.Usuarios u
    WHERE u.CorreoElectronico = N'laura.mendez@appingsoft.local';
END
GO

IF NOT EXISTS (SELECT 1 FROM dbo.Instructores WHERE CorreoElectronico = N'carlos.garcia@appingsoft.local')
BEGIN
    INSERT INTO dbo.Instructores (UsuarioId, NombreCompleto, CorreoElectronico, Especialidad, Estado)
    VALUES (NULL, N'Carlos Garcia', N'carlos.garcia@appingsoft.local', N'Calidad de Software', N'Activo');
END
GO

IF NOT EXISTS (SELECT 1 FROM dbo.Cursos WHERE Codigo = N'BD101')
BEGIN
    INSERT INTO dbo.Cursos (Codigo, Nombre, Descripcion, DuracionHoras, Estado)
    VALUES (N'BD101', N'Fundamentos de Bases de Datos', N'Diseno relacional, SQL y normalizacion.', 60, N'Activo');
END
GO

IF NOT EXISTS (SELECT 1 FROM dbo.Cursos WHERE Codigo = N'IS220')
BEGIN
    INSERT INTO dbo.Cursos (Codigo, Nombre, Descripcion, DuracionHoras, Estado)
    VALUES (N'IS220', N'Ingenieria del Software II', N'Requisitos, arquitectura y calidad del software.', 64, N'Activo');
END
GO

IF NOT EXISTS
(
    SELECT 1
    FROM dbo.Programas
    WHERE Nombre = N'Programa Base de Datos 2026'
      AND FechaInicio = '2026-01-15'
)
BEGIN
    INSERT INTO dbo.Programas (Nombre, FechaInicio, FechaFin, Estado, InstructorId, CursoId)
    SELECT
        N'Programa Base de Datos 2026',
        '2026-01-15',
        '2026-06-15',
        N'Activo',
        i.InstructorId,
        c.CursoId
    FROM dbo.Instructores i
    CROSS JOIN dbo.Cursos c
    WHERE i.CorreoElectronico = N'laura.mendez@appingsoft.local'
      AND c.Codigo = N'BD101';
END
GO

IF NOT EXISTS
(
    SELECT 1
    FROM dbo.Programas
    WHERE Nombre = N'Programa Ing. Software 2026'
      AND FechaInicio = '2026-02-01'
)
BEGIN
    INSERT INTO dbo.Programas (Nombre, FechaInicio, FechaFin, Estado, InstructorId, CursoId)
    SELECT
        N'Programa Ing. Software 2026',
        '2026-02-01',
        '2026-07-01',
        N'Activo',
        i.InstructorId,
        c.CursoId
    FROM dbo.Instructores i
    CROSS JOIN dbo.Cursos c
    WHERE i.CorreoElectronico = N'carlos.garcia@appingsoft.local'
      AND c.Codigo = N'IS220';
END
GO

IF NOT EXISTS (SELECT 1 FROM dbo.Participantes WHERE CorreoElectronico = N'ana.rojas@estudiante.local')
BEGIN
    INSERT INTO dbo.Participantes (NombreCompleto, CorreoElectronico, Telefono, Estado)
    VALUES (N'Ana Rojas', N'ana.rojas@estudiante.local', N'8888-1111', N'Activo');
END
GO

IF NOT EXISTS (SELECT 1 FROM dbo.Participantes WHERE CorreoElectronico = N'luis.solano@estudiante.local')
BEGIN
    INSERT INTO dbo.Participantes (NombreCompleto, CorreoElectronico, Telefono, Estado)
    VALUES (N'Luis Solano', N'luis.solano@estudiante.local', N'8888-2222', N'Activo');
END
GO

IF NOT EXISTS (SELECT 1 FROM dbo.Participantes WHERE CorreoElectronico = N'maria.campos@estudiante.local')
BEGIN
    INSERT INTO dbo.Participantes (NombreCompleto, CorreoElectronico, Telefono, Estado)
    VALUES (N'Maria Campos', N'maria.campos@estudiante.local', N'8888-3333', N'Activo');
END
GO

IF NOT EXISTS
(
    SELECT 1
    FROM dbo.Inscripciones ins
    INNER JOIN dbo.Participantes pa ON pa.ParticipanteId = ins.ParticipanteId
    INNER JOIN dbo.Programas pr ON pr.ProgramaId = ins.ProgramaId
    WHERE pa.CorreoElectronico = N'ana.rojas@estudiante.local'
      AND pr.Nombre = N'Programa Base de Datos 2026'
)
BEGIN
    INSERT INTO dbo.Inscripciones (ProgramaId, ParticipanteId, FechaInscripcion, Estado)
    SELECT
        pr.ProgramaId,
        pa.ParticipanteId,
        '2026-01-20',
        N'Activa'
    FROM dbo.Programas pr
    CROSS JOIN dbo.Participantes pa
    WHERE pr.Nombre = N'Programa Base de Datos 2026'
      AND pa.CorreoElectronico = N'ana.rojas@estudiante.local';
END
GO

IF NOT EXISTS
(
    SELECT 1
    FROM dbo.Inscripciones ins
    INNER JOIN dbo.Participantes pa ON pa.ParticipanteId = ins.ParticipanteId
    INNER JOIN dbo.Programas pr ON pr.ProgramaId = ins.ProgramaId
    WHERE pa.CorreoElectronico = N'luis.solano@estudiante.local'
      AND pr.Nombre = N'Programa Base de Datos 2026'
)
BEGIN
    INSERT INTO dbo.Inscripciones (ProgramaId, ParticipanteId, FechaInscripcion, Estado)
    SELECT
        pr.ProgramaId,
        pa.ParticipanteId,
        '2026-01-21',
        N'Activa'
    FROM dbo.Programas pr
    CROSS JOIN dbo.Participantes pa
    WHERE pr.Nombre = N'Programa Base de Datos 2026'
      AND pa.CorreoElectronico = N'luis.solano@estudiante.local';
END
GO

IF NOT EXISTS
(
    SELECT 1
    FROM dbo.Inscripciones ins
    INNER JOIN dbo.Participantes pa ON pa.ParticipanteId = ins.ParticipanteId
    INNER JOIN dbo.Programas pr ON pr.ProgramaId = ins.ProgramaId
    WHERE pa.CorreoElectronico = N'maria.campos@estudiante.local'
      AND pr.Nombre = N'Programa Ing. Software 2026'
)
BEGIN
    INSERT INTO dbo.Inscripciones (ProgramaId, ParticipanteId, FechaInscripcion, Estado)
    SELECT
        pr.ProgramaId,
        pa.ParticipanteId,
        '2026-02-10',
        N'Activa'
    FROM dbo.Programas pr
    CROSS JOIN dbo.Participantes pa
    WHERE pr.Nombre = N'Programa Ing. Software 2026'
      AND pa.CorreoElectronico = N'maria.campos@estudiante.local';
END
GO

IF NOT EXISTS
(
    SELECT 1
    FROM dbo.Evaluaciones ev
    INNER JOIN dbo.Cursos cu ON cu.CursoId = ev.CursoId
    WHERE ev.Titulo = N'Examen Parcial SQL'
      AND cu.Codigo = N'BD101'
)
BEGIN
    INSERT INTO dbo.Evaluaciones
    (
        CursoId, Titulo, Tipo, Momento, FechaApertura, FechaCierre, PuntosMax, Estado
    )
    SELECT
        cu.CursoId,
        N'Examen Parcial SQL',
        N'Sumativa',
        N'Mitad del curso',
        '2026-03-10',
        '2026-03-10',
        100.00,
        N'Activo'
    FROM dbo.Cursos cu
    WHERE cu.Codigo = N'BD101';
END
GO

IF NOT EXISTS
(
    SELECT 1
    FROM dbo.Evaluaciones ev
    INNER JOIN dbo.Cursos cu ON cu.CursoId = ev.CursoId
    WHERE ev.Titulo = N'Proyecto Final IS2'
      AND cu.Codigo = N'IS220'
)
BEGIN
    INSERT INTO dbo.Evaluaciones
    (
        CursoId, Titulo, Tipo, Momento, FechaApertura, FechaCierre, PuntosMax, Estado
    )
    SELECT
        cu.CursoId,
        N'Proyecto Final IS2',
        N'Sumativa',
        N'Cierre del curso',
        '2026-06-20',
        '2026-06-28',
        100.00,
        N'Pendiente'
    FROM dbo.Cursos cu
    WHERE cu.Codigo = N'IS220';
END
GO

IF NOT EXISTS
(
    SELECT 1
    FROM dbo.CriteriosEvaluacion ce
    INNER JOIN dbo.Evaluaciones ev ON ev.EvaluacionId = ce.EvaluacionId
    WHERE ev.Titulo = N'Examen Parcial SQL'
      AND ce.NombreCriterio = N'Diseno de tablas'
)
BEGIN
    INSERT INTO dbo.CriteriosEvaluacion (EvaluacionId, NombreCriterio, Ponderacion, PuntosMaxCriterio)
    SELECT ev.EvaluacionId, N'Diseno de tablas', 40.00, 40.00
    FROM dbo.Evaluaciones ev
    WHERE ev.Titulo = N'Examen Parcial SQL';
END
GO

IF NOT EXISTS
(
    SELECT 1
    FROM dbo.CriteriosEvaluacion ce
    INNER JOIN dbo.Evaluaciones ev ON ev.EvaluacionId = ce.EvaluacionId
    WHERE ev.Titulo = N'Examen Parcial SQL'
      AND ce.NombreCriterio = N'Consultas SQL'
)
BEGIN
    INSERT INTO dbo.CriteriosEvaluacion (EvaluacionId, NombreCriterio, Ponderacion, PuntosMaxCriterio)
    SELECT ev.EvaluacionId, N'Consultas SQL', 30.00, 30.00
    FROM dbo.Evaluaciones ev
    WHERE ev.Titulo = N'Examen Parcial SQL';
END
GO

IF NOT EXISTS
(
    SELECT 1
    FROM dbo.CriteriosEvaluacion ce
    INNER JOIN dbo.Evaluaciones ev ON ev.EvaluacionId = ce.EvaluacionId
    WHERE ev.Titulo = N'Examen Parcial SQL'
      AND ce.NombreCriterio = N'Normalizacion'
)
BEGIN
    INSERT INTO dbo.CriteriosEvaluacion (EvaluacionId, NombreCriterio, Ponderacion, PuntosMaxCriterio)
    SELECT ev.EvaluacionId, N'Normalizacion', 30.00, 30.00
    FROM dbo.Evaluaciones ev
    WHERE ev.Titulo = N'Examen Parcial SQL';
END
GO

IF NOT EXISTS
(
    SELECT 1
    FROM dbo.ResultadosEvaluacion re
    INNER JOIN dbo.Evaluaciones ev ON ev.EvaluacionId = re.EvaluacionId
    INNER JOIN dbo.Inscripciones ins ON ins.InscripcionId = re.InscripcionId
    INNER JOIN dbo.Participantes pa ON pa.ParticipanteId = ins.ParticipanteId
    WHERE ev.Titulo = N'Examen Parcial SQL'
      AND pa.CorreoElectronico = N'ana.rojas@estudiante.local'
)
BEGIN
    INSERT INTO dbo.ResultadosEvaluacion (EvaluacionId, InscripcionId, NotaFinal, Observaciones)
    SELECT
        ev.EvaluacionId,
        ins.InscripcionId,
        88.50,
        N'Buen dominio de SQL y modelado.'
    FROM dbo.Evaluaciones ev
    INNER JOIN dbo.Cursos cu ON cu.CursoId = ev.CursoId
    INNER JOIN dbo.Programas pr ON pr.CursoId = cu.CursoId
    INNER JOIN dbo.Inscripciones ins ON ins.ProgramaId = pr.ProgramaId
    INNER JOIN dbo.Participantes pa ON pa.ParticipanteId = ins.ParticipanteId
    WHERE ev.Titulo = N'Examen Parcial SQL'
      AND pa.CorreoElectronico = N'ana.rojas@estudiante.local';
END
GO

IF NOT EXISTS
(
    SELECT 1
    FROM dbo.ResultadosEvaluacion re
    INNER JOIN dbo.Evaluaciones ev ON ev.EvaluacionId = re.EvaluacionId
    INNER JOIN dbo.Inscripciones ins ON ins.InscripcionId = re.InscripcionId
    INNER JOIN dbo.Participantes pa ON pa.ParticipanteId = ins.ParticipanteId
    WHERE ev.Titulo = N'Examen Parcial SQL'
      AND pa.CorreoElectronico = N'luis.solano@estudiante.local'
)
BEGIN
    INSERT INTO dbo.ResultadosEvaluacion (EvaluacionId, InscripcionId, NotaFinal, Observaciones)
    SELECT
        ev.EvaluacionId,
        ins.InscripcionId,
        74.00,
        N'Requiere reforzar normalizacion avanzada.'
    FROM dbo.Evaluaciones ev
    INNER JOIN dbo.Cursos cu ON cu.CursoId = ev.CursoId
    INNER JOIN dbo.Programas pr ON pr.CursoId = cu.CursoId
    INNER JOIN dbo.Inscripciones ins ON ins.ProgramaId = pr.ProgramaId
    INNER JOIN dbo.Participantes pa ON pa.ParticipanteId = ins.ParticipanteId
    WHERE ev.Titulo = N'Examen Parcial SQL'
      AND pa.CorreoElectronico = N'luis.solano@estudiante.local';
END
GO

PRINT N'Base de datos APPingSoftII creada/configurada correctamente.';
GO
