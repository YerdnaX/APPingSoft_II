using APPingSoft_II.Data;
using APPingSoft_II.Models;
using Microsoft.Data.SqlClient;

namespace APPingSoft_II.Repositories;

public class EvaluacionRepository
{
    public List<Evaluacion> ObtenerTodas()
    {
        var lista = new List<Evaluacion>();
        const string sql = @"
            SELECT e.EvaluacionId, e.CursoId, e.Titulo, e.Tipo, e.Momento,
                   e.FechaApertura, e.FechaCierre, e.PuntosMax, e.Estado, e.FechaCreacion,
                   c.Codigo, c.Nombre
            FROM dbo.Evaluaciones e
            INNER JOIN dbo.Cursos c ON c.CursoId = e.CursoId
            ORDER BY e.FechaApertura DESC";
        using var conn = ConexionDB.ObtenerConexion();
        conn.Open();
        using var cmd = new SqlCommand(sql, conn);
        using var reader = cmd.ExecuteReader();
        while (reader.Read())
            lista.Add(MapearEvaluacion(reader));
        return lista;
    }

    public Evaluacion? ObtenerPorId(int id)
    {
        const string sql = @"
            SELECT e.EvaluacionId, e.CursoId, e.Titulo, e.Tipo, e.Momento,
                   e.FechaApertura, e.FechaCierre, e.PuntosMax, e.Estado, e.FechaCreacion,
                   c.Codigo, c.Nombre
            FROM dbo.Evaluaciones e
            INNER JOIN dbo.Cursos c ON c.CursoId = e.CursoId
            WHERE e.EvaluacionId = @Id";
        using var conn = ConexionDB.ObtenerConexion();
        conn.Open();
        using var cmd = new SqlCommand(sql, conn);
        cmd.Parameters.AddWithValue("@Id", id);
        using var reader = cmd.ExecuteReader();
        return reader.Read() ? MapearEvaluacion(reader) : null;
    }

    public int Insertar(Evaluacion e)
    {
        const string sql = @"INSERT INTO dbo.Evaluaciones (CursoId, Titulo, Tipo, Momento, FechaApertura, FechaCierre, PuntosMax, Estado)
                             OUTPUT INSERTED.EvaluacionId
                             VALUES (@CursoId, @Titulo, @Tipo, @Momento, @FechaApertura, @FechaCierre, @PuntosMax, @Estado)";
        using var conn = ConexionDB.ObtenerConexion();
        conn.Open();
        using var cmd = new SqlCommand(sql, conn);
        cmd.Parameters.AddWithValue("@CursoId", e.CursoId);
        cmd.Parameters.AddWithValue("@Titulo", e.Titulo);
        cmd.Parameters.AddWithValue("@Tipo", e.Tipo);
        cmd.Parameters.AddWithValue("@Momento", e.Momento);
        cmd.Parameters.AddWithValue("@FechaApertura", e.FechaApertura.Date);
        cmd.Parameters.AddWithValue("@FechaCierre", e.FechaCierre.Date);
        cmd.Parameters.AddWithValue("@PuntosMax", e.PuntosMax);
        cmd.Parameters.AddWithValue("@Estado", e.Estado);
        return (int)cmd.ExecuteScalar()!;
    }

    public void Actualizar(Evaluacion e)
    {
        const string sql = @"UPDATE dbo.Evaluaciones SET CursoId=@CursoId, Titulo=@Titulo, Tipo=@Tipo,
                             Momento=@Momento, FechaApertura=@FechaApertura, FechaCierre=@FechaCierre,
                             PuntosMax=@PuntosMax, Estado=@Estado WHERE EvaluacionId=@Id";
        using var conn = ConexionDB.ObtenerConexion();
        conn.Open();
        using var cmd = new SqlCommand(sql, conn);
        cmd.Parameters.AddWithValue("@CursoId", e.CursoId);
        cmd.Parameters.AddWithValue("@Titulo", e.Titulo);
        cmd.Parameters.AddWithValue("@Tipo", e.Tipo);
        cmd.Parameters.AddWithValue("@Momento", e.Momento);
        cmd.Parameters.AddWithValue("@FechaApertura", e.FechaApertura.Date);
        cmd.Parameters.AddWithValue("@FechaCierre", e.FechaCierre.Date);
        cmd.Parameters.AddWithValue("@PuntosMax", e.PuntosMax);
        cmd.Parameters.AddWithValue("@Estado", e.Estado);
        cmd.Parameters.AddWithValue("@Id", e.EvaluacionId);
        cmd.ExecuteNonQuery();
    }

    public void Eliminar(int id)
    {
        const string sql = "UPDATE dbo.Evaluaciones SET Estado = N'Cerrado' WHERE EvaluacionId = @Id";
        using var conn = ConexionDB.ObtenerConexion();
        conn.Open();
        using var cmd = new SqlCommand(sql, conn);
        cmd.Parameters.AddWithValue("@Id", id);
        cmd.ExecuteNonQuery();
    }

    private static Evaluacion MapearEvaluacion(SqlDataReader r) => new()
    {
        EvaluacionId = r.GetInt32(0),
        CursoId = r.GetInt32(1),
        Titulo = r.GetString(2),
        Tipo = r.GetString(3),
        Momento = r.GetString(4),
        FechaApertura = r.GetDateTime(5),
        FechaCierre = r.GetDateTime(6),
        PuntosMax = r.GetDecimal(7),
        Estado = r.GetString(8),
        FechaCreacion = r.GetDateTime(9),
        Curso = new Curso { CursoId = r.GetInt32(1), Codigo = r.GetString(10), Nombre = r.GetString(11) }
    };
}
