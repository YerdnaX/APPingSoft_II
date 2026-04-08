using APPingSoft_II.Data;
using APPingSoft_II.Models;
using Microsoft.Data.SqlClient;

namespace APPingSoft_II.Repositories;

public class InscripcionRepository
{
    /// <summary>
    /// Obtiene inscripciones activas de un programa con datos del participante.
    /// </summary>
    public List<Inscripcion> ObtenerPorPrograma(int programaId)
    {
        var lista = new List<Inscripcion>();
        const string sql = @"
            SELECT ins.InscripcionId, ins.ProgramaId, ins.ParticipanteId, ins.FechaInscripcion, ins.Estado,
                   pa.NombreCompleto, pa.CorreoElectronico
            FROM dbo.Inscripciones ins
            INNER JOIN dbo.Participantes pa ON pa.ParticipanteId = ins.ParticipanteId
            WHERE ins.ProgramaId = @ProgramaId AND ins.Estado = N'Activa'
            ORDER BY pa.NombreCompleto";
        using var conn = ConexionDB.ObtenerConexion();
        conn.Open();
        using var cmd = new SqlCommand(sql, conn);
        cmd.Parameters.AddWithValue("@ProgramaId", programaId);
        using var reader = cmd.ExecuteReader();
        while (reader.Read())
            lista.Add(MapearInscripcion(reader));
        return lista;
    }

    /// <summary>
    /// Obtiene inscripciones activas de participantes en el curso de una evaluación.
    /// Se usa para llenar el combo de participantes al registrar resultados.
    /// </summary>
    public List<Inscripcion> ObtenerPorCursoDeEvaluacion(int evaluacionId)
    {
        var lista = new List<Inscripcion>();
        const string sql = @"
            SELECT ins.InscripcionId, ins.ProgramaId, ins.ParticipanteId, ins.FechaInscripcion, ins.Estado,
                   pa.NombreCompleto, pa.CorreoElectronico
            FROM dbo.Inscripciones ins
            INNER JOIN dbo.Participantes pa ON pa.ParticipanteId = ins.ParticipanteId
            INNER JOIN dbo.Programas pr ON pr.ProgramaId = ins.ProgramaId
            INNER JOIN dbo.Evaluaciones ev ON ev.CursoId = pr.CursoId
            WHERE ev.EvaluacionId = @EvaluacionId AND ins.Estado = N'Activa'
            ORDER BY pa.NombreCompleto";
        using var conn = ConexionDB.ObtenerConexion();
        conn.Open();
        using var cmd = new SqlCommand(sql, conn);
        cmd.Parameters.AddWithValue("@EvaluacionId", evaluacionId);
        using var reader = cmd.ExecuteReader();
        while (reader.Read())
            lista.Add(MapearInscripcion(reader));
        return lista;
    }

    public List<Inscripcion> ObtenerTodas()
    {
        var lista = new List<Inscripcion>();
        const string sql = @"
            SELECT ins.InscripcionId, ins.ProgramaId, ins.ParticipanteId, ins.FechaInscripcion, ins.Estado,
                   pa.NombreCompleto, pa.CorreoElectronico
            FROM dbo.Inscripciones ins
            INNER JOIN dbo.Participantes pa ON pa.ParticipanteId = ins.ParticipanteId
            WHERE ins.Estado = N'Activa'
            ORDER BY pa.NombreCompleto";
        using var conn = ConexionDB.ObtenerConexion();
        conn.Open();
        using var cmd = new SqlCommand(sql, conn);
        using var reader = cmd.ExecuteReader();
        while (reader.Read())
            lista.Add(MapearInscripcion(reader));
        return lista;
    }

    /// <summary>
    /// Obtiene todas las inscripciones (cualquier estado) con datos de programa y curso.
    /// Usado por la ventana de Gestión de Inscripciones.
    /// </summary>
    public List<Inscripcion> ObtenerTodasParaGestion()
    {
        var lista = new List<Inscripcion>();
        const string sql = @"
            SELECT ins.InscripcionId, ins.ProgramaId, ins.ParticipanteId, ins.FechaInscripcion, ins.Estado,
                   pa.NombreCompleto, pa.CorreoElectronico,
                   pr.Nombre AS ProgramaNombre,
                   cu.CursoId, cu.Nombre AS CursoNombre
            FROM dbo.Inscripciones ins
            INNER JOIN dbo.Participantes pa ON pa.ParticipanteId = ins.ParticipanteId
            INNER JOIN dbo.Programas    pr ON pr.ProgramaId      = ins.ProgramaId
            INNER JOIN dbo.Cursos       cu ON cu.CursoId          = pr.CursoId
            ORDER BY pa.NombreCompleto, pr.Nombre";
        using var conn = ConexionDB.ObtenerConexion();
        conn.Open();
        using var cmd = new SqlCommand(sql, conn);
        using var reader = cmd.ExecuteReader();
        while (reader.Read())
            lista.Add(MapearInscripcionCompleta(reader));
        return lista;
    }

    public bool ExisteDuplicado(int programaId, int participanteId, int excluirId = 0)
    {
        const string sql = @"
            SELECT COUNT(1) FROM dbo.Inscripciones
            WHERE ProgramaId = @ProgramaId AND ParticipanteId = @ParticipanteId
              AND InscripcionId <> @ExcluirId";
        using var conn = ConexionDB.ObtenerConexion();
        conn.Open();
        using var cmd = new SqlCommand(sql, conn);
        cmd.Parameters.AddWithValue("@ProgramaId",    programaId);
        cmd.Parameters.AddWithValue("@ParticipanteId", participanteId);
        cmd.Parameters.AddWithValue("@ExcluirId",     excluirId);
        return (int)cmd.ExecuteScalar()! > 0;
    }

    public void Actualizar(int inscripcionId, DateTime fecha, string estado)
    {
        const string sql = @"
            UPDATE dbo.Inscripciones
            SET FechaInscripcion = @Fecha, Estado = @Estado
            WHERE InscripcionId = @Id";
        using var conn = ConexionDB.ObtenerConexion();
        conn.Open();
        using var cmd = new SqlCommand(sql, conn);
        cmd.Parameters.AddWithValue("@Fecha",  fecha.Date);
        cmd.Parameters.AddWithValue("@Estado", estado);
        cmd.Parameters.AddWithValue("@Id",     inscripcionId);
        cmd.ExecuteNonQuery();
    }

    public bool TieneResultados(int inscripcionId)
    {
        const string sql = "SELECT COUNT(1) FROM dbo.ResultadosEvaluacion WHERE InscripcionId = @Id";
        using var conn = ConexionDB.ObtenerConexion();
        conn.Open();
        using var cmd = new SqlCommand(sql, conn);
        cmd.Parameters.AddWithValue("@Id", inscripcionId);
        return (int)cmd.ExecuteScalar()! > 0;
    }

    public int Insertar(Inscripcion i)
    {
        const string sql = @"INSERT INTO dbo.Inscripciones (ProgramaId, ParticipanteId, FechaInscripcion, Estado)
                             OUTPUT INSERTED.InscripcionId
                             VALUES (@ProgramaId, @ParticipanteId, @Fecha, @Estado)";
        using var conn = ConexionDB.ObtenerConexion();
        conn.Open();
        using var cmd = new SqlCommand(sql, conn);
        cmd.Parameters.AddWithValue("@ProgramaId", i.ProgramaId);
        cmd.Parameters.AddWithValue("@ParticipanteId", i.ParticipanteId);
        cmd.Parameters.AddWithValue("@Fecha", i.FechaInscripcion.Date);
        cmd.Parameters.AddWithValue("@Estado", i.Estado);
        return (int)cmd.ExecuteScalar()!;
    }

    public void CambiarEstado(int inscripcionId, string nuevoEstado)
    {
        const string sql = "UPDATE dbo.Inscripciones SET Estado = @Estado WHERE InscripcionId = @Id";
        using var conn = ConexionDB.ObtenerConexion();
        conn.Open();
        using var cmd = new SqlCommand(sql, conn);
        cmd.Parameters.AddWithValue("@Estado", nuevoEstado);
        cmd.Parameters.AddWithValue("@Id", inscripcionId);
        cmd.ExecuteNonQuery();
    }

    private static Inscripcion MapearInscripcion(SqlDataReader r) => new()
    {
        InscripcionId = r.GetInt32(0),
        ProgramaId = r.GetInt32(1),
        ParticipanteId = r.GetInt32(2),
        FechaInscripcion = r.GetDateTime(3),
        Estado = r.GetString(4),
        Participante = new Participante
        {
            ParticipanteId = r.GetInt32(2),
            NombreCompleto = r.GetString(5),
            CorreoElectronico = r.GetString(6)
        }
    };

    // cols: 0=InscripcionId, 1=ProgramaId, 2=ParticipanteId, 3=FechaInscripcion, 4=Estado,
    //       5=NombreCompleto(pa), 6=CorreoElectronico(pa), 7=ProgramaNombre,
    //       8=CursoId, 9=CursoNombre
    private static Inscripcion MapearInscripcionCompleta(SqlDataReader r) => new()
    {
        InscripcionId    = r.GetInt32(0),
        ProgramaId       = r.GetInt32(1),
        ParticipanteId   = r.GetInt32(2),
        FechaInscripcion = r.GetDateTime(3),
        Estado           = r.GetString(4),
        Participante = new Participante
        {
            ParticipanteId    = r.GetInt32(2),
            NombreCompleto    = r.GetString(5),
            CorreoElectronico = r.GetString(6)
        },
        Programa = new Programa
        {
            ProgramaId = r.GetInt32(1),
            Nombre     = r.GetString(7),
            CursoId    = r.GetInt32(8),
            Curso      = new Curso
            {
                CursoId = r.GetInt32(8),
                Nombre  = r.GetString(9)
            }
        }
    };
}
