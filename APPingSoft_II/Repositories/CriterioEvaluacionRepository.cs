using APPingSoft_II.Data;
using APPingSoft_II.Models;
using Microsoft.Data.SqlClient;

namespace APPingSoft_II.Repositories;

public class CriterioEvaluacionRepository
{
    public List<CriterioEvaluacion> ObtenerPorEvaluacion(int evaluacionId)
    {
        var lista = new List<CriterioEvaluacion>();
        const string sql = @"SELECT CriterioId, EvaluacionId, NombreCriterio, Ponderacion, PuntosMaxCriterio
                             FROM dbo.CriteriosEvaluacion WHERE EvaluacionId = @Id ORDER BY CriterioId";
        using var conn = ConexionDB.ObtenerConexion();
        conn.Open();
        using var cmd = new SqlCommand(sql, conn);
        cmd.Parameters.AddWithValue("@Id", evaluacionId);
        using var reader = cmd.ExecuteReader();
        while (reader.Read())
            lista.Add(MapearCriterio(reader));
        return lista;
    }

    public int Insertar(CriterioEvaluacion c)
    {
        const string sql = @"INSERT INTO dbo.CriteriosEvaluacion (EvaluacionId, NombreCriterio, Ponderacion, PuntosMaxCriterio)
                             OUTPUT INSERTED.CriterioId
                             VALUES (@EvaluacionId, @Nombre, @Ponderacion, @PuntosMax)";
        using var conn = ConexionDB.ObtenerConexion();
        conn.Open();
        using var cmd = new SqlCommand(sql, conn);
        cmd.Parameters.AddWithValue("@EvaluacionId", c.EvaluacionId);
        cmd.Parameters.AddWithValue("@Nombre", c.NombreCriterio);
        cmd.Parameters.AddWithValue("@Ponderacion", c.Ponderacion);
        cmd.Parameters.AddWithValue("@PuntosMax", c.PuntosMaxCriterio);
        return (int)cmd.ExecuteScalar()!;
    }

    public void Actualizar(CriterioEvaluacion c)
    {
        const string sql = @"UPDATE dbo.CriteriosEvaluacion SET NombreCriterio=@Nombre, Ponderacion=@Ponderacion,
                             PuntosMaxCriterio=@PuntosMax WHERE CriterioId=@Id";
        using var conn = ConexionDB.ObtenerConexion();
        conn.Open();
        using var cmd = new SqlCommand(sql, conn);
        cmd.Parameters.AddWithValue("@Nombre", c.NombreCriterio);
        cmd.Parameters.AddWithValue("@Ponderacion", c.Ponderacion);
        cmd.Parameters.AddWithValue("@PuntosMax", c.PuntosMaxCriterio);
        cmd.Parameters.AddWithValue("@Id", c.CriterioId);
        cmd.ExecuteNonQuery();
    }

    public void Eliminar(int criterioId)
    {
        const string sql = "DELETE FROM dbo.CriteriosEvaluacion WHERE CriterioId = @Id";
        using var conn = ConexionDB.ObtenerConexion();
        conn.Open();
        using var cmd = new SqlCommand(sql, conn);
        cmd.Parameters.AddWithValue("@Id", criterioId);
        cmd.ExecuteNonQuery();
    }

    public decimal ObtenerPonderacionTotal(int evaluacionId)
    {
        const string sql = "SELECT ISNULL(SUM(Ponderacion),0) FROM dbo.CriteriosEvaluacion WHERE EvaluacionId = @Id";
        using var conn = ConexionDB.ObtenerConexion();
        conn.Open();
        using var cmd = new SqlCommand(sql, conn);
        cmd.Parameters.AddWithValue("@Id", evaluacionId);
        return (decimal)cmd.ExecuteScalar()!;
    }

    private static CriterioEvaluacion MapearCriterio(SqlDataReader r) => new()
    {
        CriterioId = r.GetInt32(0),
        EvaluacionId = r.GetInt32(1),
        NombreCriterio = r.GetString(2),
        Ponderacion = r.GetDecimal(3),
        PuntosMaxCriterio = r.GetDecimal(4)
    };
}
