using APPingSoft_II.Data;
using APPingSoft_II.Models.ViewModels;
using Microsoft.Data.SqlClient;

namespace APPingSoft_II.Repositories;

public class ReporteRepository
{
    public List<ResultadoDetallado> ObtenerResultadosDetallados(int? programaId = null, int? cursoId = null)
    {
        var lista = new List<ResultadoDetallado>();
        var sql = @"SELECT ResultadoId, ParticipanteId, Participante, CorreoParticipante,
                          ProgramaId, Programa, CursoId, CodigoCurso, Curso,
                          EvaluacionId, Evaluacion, TipoEvaluacion, Momento,
                          PuntosMax, NotaFinal, PorcentajeLogrado, CalificadoEn, Observaciones
                   FROM dbo.vw_ResultadosDetallados WHERE 1=1";
        if (programaId.HasValue) sql += " AND ProgramaId = @ProgramaId";
        if (cursoId.HasValue) sql += " AND CursoId = @CursoId";
        sql += " ORDER BY Participante, Evaluacion";

        using var conn = ConexionDB.ObtenerConexion();
        conn.Open();
        using var cmd = new SqlCommand(sql, conn);
        if (programaId.HasValue) cmd.Parameters.AddWithValue("@ProgramaId", programaId.Value);
        if (cursoId.HasValue) cmd.Parameters.AddWithValue("@CursoId", cursoId.Value);

        using var reader = cmd.ExecuteReader();
        while (reader.Read())
        {
            lista.Add(new ResultadoDetallado
            {
                ResultadoId = reader.GetInt32(0),
                ParticipanteId = reader.GetInt32(1),
                Participante = reader.GetString(2),
                CorreoParticipante = reader.GetString(3),
                ProgramaId = reader.GetInt32(4),
                Programa = reader.GetString(5),
                CursoId = reader.GetInt32(6),
                CodigoCurso = reader.GetString(7),
                Curso = reader.GetString(8),
                EvaluacionId = reader.GetInt32(9),
                Evaluacion = reader.GetString(10),
                TipoEvaluacion = reader.GetString(11),
                Momento = reader.GetString(12),
                PuntosMax = reader.GetDecimal(13),
                NotaFinal = reader.GetDecimal(14),
                PorcentajeLogrado = reader.IsDBNull(15) ? null : reader.GetDecimal(15),
                CalificadoEn = reader.GetDateTime(16),
                Observaciones = reader.IsDBNull(17) ? null : reader.GetString(17)
            });
        }
        return lista;
    }

    public List<MetricaCurso> ObtenerMetricasCurso()
    {
        var lista = new List<MetricaCurso>();
        const string sql = @"SELECT CursoId, Codigo, Curso, TotalEvaluaciones, TotalResultados, PromedioNota, PorcentajeAprobacion
                             FROM dbo.vw_MetricasCurso ORDER BY Curso";
        using var conn = ConexionDB.ObtenerConexion();
        conn.Open();
        using var cmd = new SqlCommand(sql, conn);
        using var reader = cmd.ExecuteReader();
        while (reader.Read())
        {
            lista.Add(new MetricaCurso
            {
                CursoId = reader.GetInt32(0),
                Codigo = reader.GetString(1),
                Curso = reader.GetString(2),
                TotalEvaluaciones = reader.GetInt32(3),
                TotalResultados = reader.GetInt32(4),
                PromedioNota = reader.IsDBNull(5) ? null : reader.GetDecimal(5),
                PorcentajeAprobacion = reader.IsDBNull(6) ? null : reader.GetDecimal(6)
            });
        }
        return lista;
    }

    public List<MetricaPrograma> ObtenerMetricasPrograma()
    {
        var lista = new List<MetricaPrograma>();
        const string sql = @"SELECT ProgramaId, Programa, Estado, TotalInscritos, ResultadosRegistrados, PromedioGeneral
                             FROM dbo.vw_MetricasPrograma ORDER BY Programa";
        using var conn = ConexionDB.ObtenerConexion();
        conn.Open();
        using var cmd = new SqlCommand(sql, conn);
        using var reader = cmd.ExecuteReader();
        while (reader.Read())
        {
            lista.Add(new MetricaPrograma
            {
                ProgramaId = reader.GetInt32(0),
                Programa = reader.GetString(1),
                Estado = reader.GetString(2),
                TotalInscritos = reader.GetInt32(3),
                ResultadosRegistrados = reader.GetInt32(4),
                PromedioGeneral = reader.IsDBNull(5) ? null : reader.GetDecimal(5)
            });
        }
        return lista;
    }
}
