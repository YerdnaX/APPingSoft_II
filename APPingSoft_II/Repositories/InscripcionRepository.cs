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
}
