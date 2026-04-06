/*
    Proyecto: APPingSoft_II
    Archivo: Datostest.sql
    Objetivo: Cargar datos de prueba realistas para pruebas funcionales
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

    /* ==============================
       USUARIOS
       ============================== */
    INSERT INTO dbo.Usuarios (NombreCompleto, CorreoElectronico, Contrasena, Rol, Estado)
    SELECT v.NombreCompleto, v.CorreoElectronico, v.Contrasena, v.Rol, v.Estado
    FROM
    (
        VALUES
            (N'Andrea Vargas', N'andrea.vargas@appingsoft.local', N'Coord2026*', N'Coordinador', N'Activo'),
            (N'Jose Calvo', N'jose.calvo@appingsoft.local', N'Instructor2026*', N'Instructor', N'Activo'),
            (N'Paula Herrera', N'paula.herrera@appingsoft.local', N'Instructor2026*', N'Instructor', N'Activo')
    ) v (NombreCompleto, CorreoElectronico, Contrasena, Rol, Estado)
    WHERE NOT EXISTS
    (
        SELECT 1
        FROM dbo.Usuarios u
        WHERE u.CorreoElectronico = v.CorreoElectronico
    );

    /* ==============================
       INSTRUCTORES
       ============================== */
    INSERT INTO dbo.Instructores (UsuarioId, NombreCompleto, CorreoElectronico, Especialidad, Estado)
    SELECT
        u.UsuarioId,
        v.NombreCompleto,
        v.CorreoElectronico,
        v.Especialidad,
        v.Estado
    FROM
    (
        VALUES
            (N'Jose Calvo', N'jose.calvo@appingsoft.local', N'Arquitectura de Software', N'Activo'),
            (N'Paula Herrera', N'paula.herrera@appingsoft.local', N'Calidad y Pruebas de Software', N'Activo')
    ) v (NombreCompleto, CorreoElectronico, Especialidad, Estado)
    INNER JOIN dbo.Usuarios u
        ON u.CorreoElectronico = v.CorreoElectronico
    WHERE NOT EXISTS
    (
        SELECT 1
        FROM dbo.Instructores i
        WHERE i.CorreoElectronico = v.CorreoElectronico
    );

    /* ==============================
       CURSOS
       ============================== */
    INSERT INTO dbo.Cursos (Codigo, Nombre, Descripcion, DuracionHoras, Estado)
    SELECT v.Codigo, v.Nombre, v.Descripcion, v.DuracionHoras, v.Estado
    FROM
    (
        VALUES
            (N'QA310', N'Pruebas de Software', N'Diseno de pruebas, automatizacion y gestion de defectos.', 56, N'Activo'),
            (N'ARQ330', N'Arquitectura de Software', N'Patrones de arquitectura, capas y decisiones tecnicas.', 60, N'Activo'),
            (N'UX205', N'Diseno UX Aplicado', N'Investigacion de usuario, prototipado y pruebas de usabilidad.', 48, N'Activo')
    ) v (Codigo, Nombre, Descripcion, DuracionHoras, Estado)
    WHERE NOT EXISTS
    (
        SELECT 1
        FROM dbo.Cursos c
        WHERE c.Codigo = v.Codigo
    );

    /* ==============================
       PROGRAMAS
       ============================== */
    INSERT INTO dbo.Programas (Nombre, FechaInicio, FechaFin, Estado, InstructorId, CursoId)
    SELECT
        v.Nombre,
        v.FechaInicio,
        v.FechaFin,
        v.Estado,
        i.InstructorId,
        c.CursoId
    FROM
    (
        VALUES
            (N'Bootcamp QA 2026 - Grupo A', CAST('2026-03-01' AS DATE), CAST('2026-08-15' AS DATE), N'Activo', N'paula.herrera@appingsoft.local', N'QA310'),
            (N'Arquitectura .NET 2026 - Nocturno', CAST('2026-04-05' AS DATE), CAST('2026-09-30' AS DATE), N'Activo', N'jose.calvo@appingsoft.local', N'ARQ330'),
            (N'UX para Desarrollo 2026', CAST('2026-02-15' AS DATE), CAST('2026-07-15' AS DATE), N'Activo', N'laura.mendez@appingsoft.local', N'UX205')
    ) v (Nombre, FechaInicio, FechaFin, Estado, CorreoInstructor, CodigoCurso)
    INNER JOIN dbo.Instructores i
        ON i.CorreoElectronico = v.CorreoInstructor
    INNER JOIN dbo.Cursos c
        ON c.Codigo = v.CodigoCurso
    WHERE NOT EXISTS
    (
        SELECT 1
        FROM dbo.Programas p
        WHERE p.Nombre = v.Nombre
          AND p.FechaInicio = v.FechaInicio
    );

    /* ==============================
       PARTICIPANTES
       ============================== */
    INSERT INTO dbo.Participantes (NombreCompleto, CorreoElectronico, Telefono, Estado)
    SELECT v.NombreCompleto, v.CorreoElectronico, v.Telefono, v.Estado
    FROM
    (
        VALUES
            (N'Daniela Soto', N'daniela.soto@estudiante.cr', N'8812-1001', N'Activo'),
            (N'Mauricio Arias', N'mauricio.arias@estudiante.cr', N'8812-1002', N'Activo'),
            (N'Sofia Jimenez', N'sofia.jimenez@estudiante.cr', N'8812-1003', N'Activo'),
            (N'Emilio Navarro', N'emilio.navarro@estudiante.cr', N'8812-1004', N'Activo'),
            (N'Karla Chaves', N'karla.chaves@estudiante.cr', N'8812-1005', N'Activo'),
            (N'Ivan Murillo', N'ivan.murillo@estudiante.cr', N'8812-1006', N'Activo'),
            (N'Gabriela Acuna', N'gabriela.acuna@estudiante.cr', N'8812-1007', N'Activo'),
            (N'Pedro Cordero', N'pedro.cordero@estudiante.cr', N'8812-1008', N'Activo')
    ) v (NombreCompleto, CorreoElectronico, Telefono, Estado)
    WHERE NOT EXISTS
    (
        SELECT 1
        FROM dbo.Participantes p
        WHERE p.CorreoElectronico = v.CorreoElectronico
    );

    /* ==============================
       INSCRIPCIONES
       ============================== */
    INSERT INTO dbo.Inscripciones (ProgramaId, ParticipanteId, FechaInscripcion, Estado)
    SELECT
        pr.ProgramaId,
        pa.ParticipanteId,
        v.FechaInscripcion,
        v.Estado
    FROM
    (
        VALUES
            (N'Bootcamp QA 2026 - Grupo A', N'daniela.soto@estudiante.cr', CAST('2026-03-02' AS DATE), N'Activa'),
            (N'Bootcamp QA 2026 - Grupo A', N'mauricio.arias@estudiante.cr', CAST('2026-03-03' AS DATE), N'Activa'),
            (N'Bootcamp QA 2026 - Grupo A', N'sofia.jimenez@estudiante.cr', CAST('2026-03-04' AS DATE), N'Activa'),
            (N'Arquitectura .NET 2026 - Nocturno', N'emilio.navarro@estudiante.cr', CAST('2026-04-08' AS DATE), N'Activa'),
            (N'Arquitectura .NET 2026 - Nocturno', N'karla.chaves@estudiante.cr', CAST('2026-04-08' AS DATE), N'Activa'),
            (N'Arquitectura .NET 2026 - Nocturno', N'ivan.murillo@estudiante.cr', CAST('2026-04-09' AS DATE), N'Activa'),
            (N'UX para Desarrollo 2026', N'gabriela.acuna@estudiante.cr', CAST('2026-02-16' AS DATE), N'Activa'),
            (N'UX para Desarrollo 2026', N'pedro.cordero@estudiante.cr', CAST('2026-02-16' AS DATE), N'Activa')
    ) v (NombrePrograma, CorreoParticipante, FechaInscripcion, Estado)
    INNER JOIN dbo.Programas pr
        ON pr.Nombre = v.NombrePrograma
    INNER JOIN dbo.Participantes pa
        ON pa.CorreoElectronico = v.CorreoParticipante
    WHERE NOT EXISTS
    (
        SELECT 1
        FROM dbo.Inscripciones i
        WHERE i.ProgramaId = pr.ProgramaId
          AND i.ParticipanteId = pa.ParticipanteId
    );

    /* ==============================
       EVALUACIONES
       ============================== */
    INSERT INTO dbo.Evaluaciones
    (
        CursoId, Titulo, Tipo, Momento, FechaApertura, FechaCierre, PuntosMax, Estado
    )
    SELECT
        c.CursoId,
        v.Titulo,
        v.Tipo,
        v.Momento,
        v.FechaApertura,
        v.FechaCierre,
        v.PuntosMax,
        v.Estado
    FROM
    (
        VALUES
            (N'QA310', N'Quiz Fundamentos QA', N'Formativa', N'Semana 3', CAST('2026-03-24' AS DATE), CAST('2026-03-24' AS DATE), CAST(20.00 AS DECIMAL(8,2)), N'Cerrado'),
            (N'QA310', N'Proyecto Plan de Pruebas', N'Sumativa', N'Cierre del programa', CAST('2026-08-10' AS DATE), CAST('2026-08-15' AS DATE), CAST(100.00 AS DECIMAL(8,2)), N'Activo'),
            (N'ARQ330', N'Caso Arquitectura Limpia', N'Sumativa', N'Semana 10', CAST('2026-06-15' AS DATE), CAST('2026-06-18' AS DATE), CAST(100.00 AS DECIMAL(8,2)), N'Activo'),
            (N'UX205', N'Taller de Prototipos', N'Formativa', N'Semana 5', CAST('2026-03-20' AS DATE), CAST('2026-03-24' AS DATE), CAST(50.00 AS DECIMAL(8,2)), N'Cerrado')
    ) v (CodigoCurso, Titulo, Tipo, Momento, FechaApertura, FechaCierre, PuntosMax, Estado)
    INNER JOIN dbo.Cursos c
        ON c.Codigo = v.CodigoCurso
    WHERE NOT EXISTS
    (
        SELECT 1
        FROM dbo.Evaluaciones e
        WHERE e.CursoId = c.CursoId
          AND e.Titulo = v.Titulo
          AND e.FechaApertura = v.FechaApertura
    );

    /* ==============================
       CRITERIOS POR EVALUACION
       ============================== */
    INSERT INTO dbo.CriteriosEvaluacion (EvaluacionId, NombreCriterio, Ponderacion, PuntosMaxCriterio)
    SELECT
        e.EvaluacionId,
        v.NombreCriterio,
        v.Ponderacion,
        v.PuntosMaxCriterio
    FROM
    (
        VALUES
            (N'QA310', N'Quiz Fundamentos QA', N'Diseno de casos de prueba', CAST(50.00 AS DECIMAL(5,2)), CAST(10.00 AS DECIMAL(8,2))),
            (N'QA310', N'Quiz Fundamentos QA', N'Ejecucion y evidencia', CAST(50.00 AS DECIMAL(5,2)), CAST(10.00 AS DECIMAL(8,2))),

            (N'QA310', N'Proyecto Plan de Pruebas', N'Cobertura funcional', CAST(30.00 AS DECIMAL(5,2)), CAST(30.00 AS DECIMAL(8,2))),
            (N'QA310', N'Proyecto Plan de Pruebas', N'Automatizacion', CAST(40.00 AS DECIMAL(5,2)), CAST(40.00 AS DECIMAL(8,2))),
            (N'QA310', N'Proyecto Plan de Pruebas', N'Reporte de hallazgos', CAST(30.00 AS DECIMAL(5,2)), CAST(30.00 AS DECIMAL(8,2))),

            (N'ARQ330', N'Caso Arquitectura Limpia', N'Modelado de dominios', CAST(35.00 AS DECIMAL(5,2)), CAST(35.00 AS DECIMAL(8,2))),
            (N'ARQ330', N'Caso Arquitectura Limpia', N'Justificacion tecnica', CAST(35.00 AS DECIMAL(5,2)), CAST(35.00 AS DECIMAL(8,2))),
            (N'ARQ330', N'Caso Arquitectura Limpia', N'Calidad del prototipo', CAST(30.00 AS DECIMAL(5,2)), CAST(30.00 AS DECIMAL(8,2))),

            (N'UX205', N'Taller de Prototipos', N'Investigacion de usuario', CAST(40.00 AS DECIMAL(5,2)), CAST(20.00 AS DECIMAL(8,2))),
            (N'UX205', N'Taller de Prototipos', N'Wireframes y flujo', CAST(30.00 AS DECIMAL(5,2)), CAST(15.00 AS DECIMAL(8,2))),
            (N'UX205', N'Taller de Prototipos', N'Validacion con usuarios', CAST(30.00 AS DECIMAL(5,2)), CAST(15.00 AS DECIMAL(8,2)))
    ) v (CodigoCurso, TituloEvaluacion, NombreCriterio, Ponderacion, PuntosMaxCriterio)
    INNER JOIN dbo.Cursos c
        ON c.Codigo = v.CodigoCurso
    INNER JOIN dbo.Evaluaciones e
        ON e.CursoId = c.CursoId
       AND e.Titulo = v.TituloEvaluacion
    WHERE NOT EXISTS
    (
        SELECT 1
        FROM dbo.CriteriosEvaluacion ce
        WHERE ce.EvaluacionId = e.EvaluacionId
          AND ce.NombreCriterio = v.NombreCriterio
    );

    /* ==============================
       RESULTADOS FINALES
       ============================== */
    INSERT INTO dbo.ResultadosEvaluacion (EvaluacionId, InscripcionId, NotaFinal, Observaciones)
    SELECT
        e.EvaluacionId,
        i.InscripcionId,
        v.NotaFinal,
        v.Observaciones
    FROM
    (
        VALUES
            (N'Bootcamp QA 2026 - Grupo A', N'daniela.soto@estudiante.cr', N'QA310', N'Quiz Fundamentos QA', CAST(17.00 AS DECIMAL(8,2)), N'Manejo solido de casos de prueba funcionales.'),
            (N'Bootcamp QA 2026 - Grupo A', N'mauricio.arias@estudiante.cr', N'QA310', N'Quiz Fundamentos QA', CAST(15.00 AS DECIMAL(8,2)), N'Buen desempeno, mejorar precision en evidencia.'),
            (N'Bootcamp QA 2026 - Grupo A', N'sofia.jimenez@estudiante.cr', N'QA310', N'Quiz Fundamentos QA', CAST(19.00 AS DECIMAL(8,2)), N'Resultado destacado en diseno y ejecucion.'),

            (N'Bootcamp QA 2026 - Grupo A', N'daniela.soto@estudiante.cr', N'QA310', N'Proyecto Plan de Pruebas', CAST(86.00 AS DECIMAL(8,2)), N'Plan completo y bien estructurado.'),
            (N'Bootcamp QA 2026 - Grupo A', N'mauricio.arias@estudiante.cr', N'QA310', N'Proyecto Plan de Pruebas', CAST(76.00 AS DECIMAL(8,2)), N'Cumple objetivos, falta profundidad en automatizacion.'),
            (N'Bootcamp QA 2026 - Grupo A', N'sofia.jimenez@estudiante.cr', N'QA310', N'Proyecto Plan de Pruebas', CAST(94.00 AS DECIMAL(8,2)), N'Excelente cobertura y analisis de defectos.'),

            (N'Arquitectura .NET 2026 - Nocturno', N'emilio.navarro@estudiante.cr', N'ARQ330', N'Caso Arquitectura Limpia', CAST(85.00 AS DECIMAL(8,2)), N'Decisiones tecnicas consistentes con el caso.'),
            (N'Arquitectura .NET 2026 - Nocturno', N'karla.chaves@estudiante.cr', N'ARQ330', N'Caso Arquitectura Limpia', CAST(80.00 AS DECIMAL(8,2)), N'Buena propuesta, reforzar argumentacion tecnica.'),
            (N'Arquitectura .NET 2026 - Nocturno', N'ivan.murillo@estudiante.cr', N'ARQ330', N'Caso Arquitectura Limpia', CAST(92.00 AS DECIMAL(8,2)), N'Arquitectura robusta y buen prototipo.')
    ) v (NombrePrograma, CorreoParticipante, CodigoCurso, TituloEvaluacion, NotaFinal, Observaciones)
    INNER JOIN dbo.Programas pr
        ON pr.Nombre = v.NombrePrograma
    INNER JOIN dbo.Participantes pa
        ON pa.CorreoElectronico = v.CorreoParticipante
    INNER JOIN dbo.Inscripciones i
        ON i.ProgramaId = pr.ProgramaId
       AND i.ParticipanteId = pa.ParticipanteId
    INNER JOIN dbo.Cursos c
        ON c.Codigo = v.CodigoCurso
    INNER JOIN dbo.Evaluaciones e
        ON e.CursoId = c.CursoId
       AND e.Titulo = v.TituloEvaluacion
    WHERE NOT EXISTS
    (
        SELECT 1
        FROM dbo.ResultadosEvaluacion r
        WHERE r.EvaluacionId = e.EvaluacionId
          AND r.InscripcionId = i.InscripcionId
    );

    /* ==============================
       DETALLE POR CRITERIO
       ============================== */
    INSERT INTO dbo.ResultadoCriterio (ResultadoId, CriterioId, PuntajeObtenido, Observacion)
    SELECT
        r.ResultadoId,
        ce.CriterioId,
        v.PuntajeObtenido,
        v.Observacion
    FROM
    (
        VALUES
            (N'Bootcamp QA 2026 - Grupo A', N'daniela.soto@estudiante.cr', N'QA310', N'Quiz Fundamentos QA', N'Diseno de casos de prueba', CAST(9.00 AS DECIMAL(8,2)), N'Cobertura de escenarios criticos.'),
            (N'Bootcamp QA 2026 - Grupo A', N'daniela.soto@estudiante.cr', N'QA310', N'Quiz Fundamentos QA', N'Ejecucion y evidencia', CAST(8.00 AS DECIMAL(8,2)), N'Evidencia clara con trazabilidad.'),
            (N'Bootcamp QA 2026 - Grupo A', N'mauricio.arias@estudiante.cr', N'QA310', N'Quiz Fundamentos QA', N'Diseno de casos de prueba', CAST(8.00 AS DECIMAL(8,2)), N'Buenos casos base.'),
            (N'Bootcamp QA 2026 - Grupo A', N'mauricio.arias@estudiante.cr', N'QA310', N'Quiz Fundamentos QA', N'Ejecucion y evidencia', CAST(7.00 AS DECIMAL(8,2)), N'Falto detalle en resultados esperados.'),
            (N'Bootcamp QA 2026 - Grupo A', N'sofia.jimenez@estudiante.cr', N'QA310', N'Quiz Fundamentos QA', N'Diseno de casos de prueba', CAST(10.00 AS DECIMAL(8,2)), N'Casos completos y bien priorizados.'),
            (N'Bootcamp QA 2026 - Grupo A', N'sofia.jimenez@estudiante.cr', N'QA310', N'Quiz Fundamentos QA', N'Ejecucion y evidencia', CAST(9.00 AS DECIMAL(8,2)), N'Excelente evidencia de ejecucion.'),

            (N'Bootcamp QA 2026 - Grupo A', N'daniela.soto@estudiante.cr', N'QA310', N'Proyecto Plan de Pruebas', N'Cobertura funcional', CAST(27.00 AS DECIMAL(8,2)), N'Cubre requisitos funcionales clave.'),
            (N'Bootcamp QA 2026 - Grupo A', N'daniela.soto@estudiante.cr', N'QA310', N'Proyecto Plan de Pruebas', N'Automatizacion', CAST(34.00 AS DECIMAL(8,2)), N'Suite automatizada con buena estabilidad.'),
            (N'Bootcamp QA 2026 - Grupo A', N'daniela.soto@estudiante.cr', N'QA310', N'Proyecto Plan de Pruebas', N'Reporte de hallazgos', CAST(25.00 AS DECIMAL(8,2)), N'Documentacion clara de defectos.'),
            (N'Bootcamp QA 2026 - Grupo A', N'mauricio.arias@estudiante.cr', N'QA310', N'Proyecto Plan de Pruebas', N'Cobertura funcional', CAST(24.00 AS DECIMAL(8,2)), N'Cobertura aceptable con oportunidades de mejora.'),
            (N'Bootcamp QA 2026 - Grupo A', N'mauricio.arias@estudiante.cr', N'QA310', N'Proyecto Plan de Pruebas', N'Automatizacion', CAST(30.00 AS DECIMAL(8,2)), N'Automatizacion parcial en flujo critico.'),
            (N'Bootcamp QA 2026 - Grupo A', N'mauricio.arias@estudiante.cr', N'QA310', N'Proyecto Plan de Pruebas', N'Reporte de hallazgos', CAST(22.00 AS DECIMAL(8,2)), N'Hallazgos relevantes pero breves.'),
            (N'Bootcamp QA 2026 - Grupo A', N'sofia.jimenez@estudiante.cr', N'QA310', N'Proyecto Plan de Pruebas', N'Cobertura funcional', CAST(29.00 AS DECIMAL(8,2)), N'Excelente alcance funcional.'),
            (N'Bootcamp QA 2026 - Grupo A', N'sofia.jimenez@estudiante.cr', N'QA310', N'Proyecto Plan de Pruebas', N'Automatizacion', CAST(37.00 AS DECIMAL(8,2)), N'Muy buen uso de pruebas automatas.'),
            (N'Bootcamp QA 2026 - Grupo A', N'sofia.jimenez@estudiante.cr', N'QA310', N'Proyecto Plan de Pruebas', N'Reporte de hallazgos', CAST(28.00 AS DECIMAL(8,2)), N'Analisis de causa raiz bien presentado.'),

            (N'Arquitectura .NET 2026 - Nocturno', N'emilio.navarro@estudiante.cr', N'ARQ330', N'Caso Arquitectura Limpia', N'Modelado de dominios', CAST(30.00 AS DECIMAL(8,2)), N'Modelo de dominio consistente.'),
            (N'Arquitectura .NET 2026 - Nocturno', N'emilio.navarro@estudiante.cr', N'ARQ330', N'Caso Arquitectura Limpia', N'Justificacion tecnica', CAST(31.00 AS DECIMAL(8,2)), N'Buena defensa de decisiones de arquitectura.'),
            (N'Arquitectura .NET 2026 - Nocturno', N'emilio.navarro@estudiante.cr', N'ARQ330', N'Caso Arquitectura Limpia', N'Calidad del prototipo', CAST(24.00 AS DECIMAL(8,2)), N'Prototipo funcional con buena separacion de capas.'),
            (N'Arquitectura .NET 2026 - Nocturno', N'karla.chaves@estudiante.cr', N'ARQ330', N'Caso Arquitectura Limpia', N'Modelado de dominios', CAST(28.00 AS DECIMAL(8,2)), N'Modelado correcto, faltan escenarios borde.'),
            (N'Arquitectura .NET 2026 - Nocturno', N'karla.chaves@estudiante.cr', N'ARQ330', N'Caso Arquitectura Limpia', N'Justificacion tecnica', CAST(29.00 AS DECIMAL(8,2)), N'Argumentacion tecnica aceptable.'),
            (N'Arquitectura .NET 2026 - Nocturno', N'karla.chaves@estudiante.cr', N'ARQ330', N'Caso Arquitectura Limpia', N'Calidad del prototipo', CAST(23.00 AS DECIMAL(8,2)), N'Buen prototipo, faltaron pruebas.'),
            (N'Arquitectura .NET 2026 - Nocturno', N'ivan.murillo@estudiante.cr', N'ARQ330', N'Caso Arquitectura Limpia', N'Modelado de dominios', CAST(32.00 AS DECIMAL(8,2)), N'Modelo robusto y mantenible.'),
            (N'Arquitectura .NET 2026 - Nocturno', N'ivan.murillo@estudiante.cr', N'ARQ330', N'Caso Arquitectura Limpia', N'Justificacion tecnica', CAST(33.00 AS DECIMAL(8,2)), N'Decisiones bien sustentadas.'),
            (N'Arquitectura .NET 2026 - Nocturno', N'ivan.murillo@estudiante.cr', N'ARQ330', N'Caso Arquitectura Limpia', N'Calidad del prototipo', CAST(27.00 AS DECIMAL(8,2)), N'Prototipo estable y claro.')
    ) v (NombrePrograma, CorreoParticipante, CodigoCurso, TituloEvaluacion, NombreCriterio, PuntajeObtenido, Observacion)
    INNER JOIN dbo.Programas pr
        ON pr.Nombre = v.NombrePrograma
    INNER JOIN dbo.Participantes pa
        ON pa.CorreoElectronico = v.CorreoParticipante
    INNER JOIN dbo.Inscripciones i
        ON i.ProgramaId = pr.ProgramaId
       AND i.ParticipanteId = pa.ParticipanteId
    INNER JOIN dbo.Cursos c
        ON c.Codigo = v.CodigoCurso
    INNER JOIN dbo.Evaluaciones e
        ON e.CursoId = c.CursoId
       AND e.Titulo = v.TituloEvaluacion
    INNER JOIN dbo.ResultadosEvaluacion r
        ON r.EvaluacionId = e.EvaluacionId
       AND r.InscripcionId = i.InscripcionId
    INNER JOIN dbo.CriteriosEvaluacion ce
        ON ce.EvaluacionId = e.EvaluacionId
       AND ce.NombreCriterio = v.NombreCriterio
    WHERE NOT EXISTS
    (
        SELECT 1
        FROM dbo.ResultadoCriterio rc
        WHERE rc.ResultadoId = r.ResultadoId
          AND rc.CriterioId = ce.CriterioId
    );

    COMMIT TRANSACTION;
END TRY
BEGIN CATCH
    IF @@TRANCOUNT > 0
        ROLLBACK TRANSACTION;

    THROW;
END CATCH;

PRINT N'Datos de prueba insertados correctamente en APPingSoftII.';
