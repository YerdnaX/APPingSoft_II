using APPingSoft_II.Data;
using APPingSoft_II.Models;
using Microsoft.Data.SqlClient;

namespace APPingSoft_II.Repositories;

public class ResultadoEvaluacionRepository
{
    public List<ResultadoEvaluacion> ObtenerTodos()
    {
        var lista = new List<ResultadoEvaluacion>();
        const string sql = @"
            SELECT re.ResultadoId, re.EvaluacionId, re.InscripcionId, re.NotaFinal, re.CalificadoEn, re.Observaciones,
                   ev.Titulo AS EvaluacionTitulo, ev.PuntosMax,
                   pa.NombreCompleto AS ParticipanteNombre, pa.ParticipanteId,
                   ins.ProgramaId, ins.Estado AS EstadoInscripcion, pr.Nombre AS ProgramaNombre
            FROM dbo.ResultadosEvaluacion re
            INNER JOIN dbo.Evaluaciones ev ON ev.EvaluacionId = re.EvaluacionId
            INNER JOIN dbo.Inscripciones ins ON ins.InscripcionId = re.InscripcionId
            INNER JOIN dbo.Programas pr ON pr.ProgramaId = ins.ProgramaId
            INNER JOIN dbo.Participantes pa ON pa.ParticipanteId = ins.ParticipanteId
            ORDER BY re.CalificadoEn DESC";

        using var conn = ConexionDB.ObtenerConexion();
        conn.Open();
        using var cmd = new SqlCommand(sql, conn);
        using var reader = cmd.ExecuteReader();
        while (reader.Read())
            lista.Add(MapearResultado(reader));

        return lista;
    }

    public ResultadoEvaluacion? ObtenerPorId(int id)
    {
        const string sql = @"
            SELECT re.ResultadoId, re.EvaluacionId, re.InscripcionId, re.NotaFinal, re.CalificadoEn, re.Observaciones,
                   ev.Titulo, ev.PuntosMax,
                   pa.NombreCompleto, pa.ParticipanteId,
                   ins.ProgramaId, ins.Estado AS EstadoInscripcion, pr.Nombre AS ProgramaNombre
            FROM dbo.ResultadosEvaluacion re
            INNER JOIN dbo.Evaluaciones ev ON ev.EvaluacionId = re.EvaluacionId
            INNER JOIN dbo.Inscripciones ins ON ins.InscripcionId = re.InscripcionId
            INNER JOIN dbo.Programas pr ON pr.ProgramaId = ins.ProgramaId
            INNER JOIN dbo.Participantes pa ON pa.ParticipanteId = ins.ParticipanteId
            WHERE re.ResultadoId = @Id";

        using var conn = ConexionDB.ObtenerConexion();
        conn.Open();
        using var cmd = new SqlCommand(sql, conn);
        cmd.Parameters.AddWithValue("@Id", id);
        using var reader = cmd.ExecuteReader();
        return reader.Read() ? MapearResultado(reader) : null;
    }

    public bool ExisteResultado(int evaluacionId, int inscripcionId)
    {
        const string sql = "SELECT COUNT(1) FROM dbo.ResultadosEvaluacion WHERE EvaluacionId=@EvalId AND InscripcionId=@InsId";

        using var conn = ConexionDB.ObtenerConexion();
        conn.Open();
        using var cmd = new SqlCommand(sql, conn);
        cmd.Parameters.AddWithValue("@EvalId", evaluacionId);
        cmd.Parameters.AddWithValue("@InsId", inscripcionId);
        return (int)cmd.ExecuteScalar()! > 0;
    }

    public int Insertar(ResultadoEvaluacion r)
    {
        const string sql = @"INSERT INTO dbo.ResultadosEvaluacion (EvaluacionId, InscripcionId, NotaFinal, CalificadoEn, Observaciones)
                             VALUES (@EvaluacionId, @InscripcionId, @NotaFinal, @CalificadoEn, @Observaciones);
                             SELECT CAST(SCOPE_IDENTITY() AS int);";

        using var conn = ConexionDB.ObtenerConexion();
        conn.Open();
        using var cmd = new SqlCommand(sql, conn);
        cmd.Parameters.AddWithValue("@EvaluacionId", r.EvaluacionId);
        cmd.Parameters.AddWithValue("@InscripcionId", r.InscripcionId);
        cmd.Parameters.AddWithValue("@NotaFinal", r.NotaFinal);
        cmd.Parameters.AddWithValue("@CalificadoEn", r.CalificadoEn);
        cmd.Parameters.AddWithValue("@Observaciones", (object?)r.Observaciones ?? DBNull.Value);
        return (int)cmd.ExecuteScalar()!;
    }

    public void Actualizar(ResultadoEvaluacion r)
    {
        const string sql = @"UPDATE dbo.ResultadosEvaluacion SET NotaFinal=@NotaFinal, CalificadoEn=@CalificadoEn,
                             Observaciones=@Observaciones WHERE ResultadoId=@Id";

        using var conn = ConexionDB.ObtenerConexion();
        conn.Open();
        using var cmd = new SqlCommand(sql, conn);
        cmd.Parameters.AddWithValue("@NotaFinal", r.NotaFinal);
        cmd.Parameters.AddWithValue("@CalificadoEn", r.CalificadoEn);
        cmd.Parameters.AddWithValue("@Observaciones", (object?)r.Observaciones ?? DBNull.Value);
        cmd.Parameters.AddWithValue("@Id", r.ResultadoId);
        cmd.ExecuteNonQuery();
    }

    public void Eliminar(int id)
    {
        const string sql = "DELETE FROM dbo.ResultadosEvaluacion WHERE ResultadoId = @Id";

        using var conn = ConexionDB.ObtenerConexion();
        conn.Open();
        using var cmd = new SqlCommand(sql, conn);
        cmd.Parameters.AddWithValue("@Id", id);
        cmd.ExecuteNonQuery();
    }

    private static ResultadoEvaluacion MapearResultado(SqlDataReader r) => new()
    {
        ResultadoId = r.GetInt32(0),
        EvaluacionId = r.GetInt32(1),
        InscripcionId = r.GetInt32(2),
        NotaFinal = r.GetDecimal(3),
        CalificadoEn = r.GetDateTime(4),
        Observaciones = r.IsDBNull(5) ? null : r.GetString(5),
        Evaluacion = new Evaluacion
        {
            EvaluacionId = r.GetInt32(1),
            Titulo = r.GetString(6),
            PuntosMax = r.GetDecimal(7)
        },
        Inscripcion = new Inscripcion
        {
            InscripcionId = r.GetInt32(2),
            ProgramaId = r.GetInt32(10),
            Estado = r.GetString(11),
            Programa = new Programa
            {
                ProgramaId = r.GetInt32(10),
                Nombre = r.GetString(12)
            },
            Participante = new Participante
            {
                ParticipanteId = r.GetInt32(9),
                NombreCompleto = r.GetString(8)
            }
        }
    };
}
