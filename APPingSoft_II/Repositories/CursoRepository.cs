using APPingSoft_II.Data;
using APPingSoft_II.Models;
using Microsoft.Data.SqlClient;

namespace APPingSoft_II.Repositories;

public class CursoRepository
{
    public List<Curso> ObtenerTodos()
    {
        var lista = new List<Curso>();
        const string sql = "SELECT CursoId, Codigo, Nombre, Descripcion, DuracionHoras, Estado, FechaCreacion FROM dbo.Cursos WHERE Estado = N'Activo' ORDER BY Nombre";
        using var conn = ConexionDB.ObtenerConexion();
        conn.Open();
        using var cmd = new SqlCommand(sql, conn);
        using var reader = cmd.ExecuteReader();
        while (reader.Read())
            lista.Add(MapearCurso(reader));
        return lista;
    }

    public Curso? ObtenerPorId(int id)
    {
        const string sql = "SELECT CursoId, Codigo, Nombre, Descripcion, DuracionHoras, Estado, FechaCreacion FROM dbo.Cursos WHERE CursoId = @Id";
        using var conn = ConexionDB.ObtenerConexion();
        conn.Open();
        using var cmd = new SqlCommand(sql, conn);
        cmd.Parameters.AddWithValue("@Id", id);
        using var reader = cmd.ExecuteReader();
        return reader.Read() ? MapearCurso(reader) : null;
    }

    public int Insertar(Curso c)
    {
        const string sql = @"INSERT INTO dbo.Cursos (Codigo, Nombre, Descripcion, DuracionHoras, Estado)
                             OUTPUT INSERTED.CursoId
                             VALUES (@Codigo, @Nombre, @Descripcion, @Duracion, @Estado)";
        using var conn = ConexionDB.ObtenerConexion();
        conn.Open();
        using var cmd = new SqlCommand(sql, conn);
        cmd.Parameters.AddWithValue("@Codigo", c.Codigo);
        cmd.Parameters.AddWithValue("@Nombre", c.Nombre);
        cmd.Parameters.AddWithValue("@Descripcion", (object?)c.Descripcion ?? DBNull.Value);
        cmd.Parameters.AddWithValue("@Duracion", c.DuracionHoras);
        cmd.Parameters.AddWithValue("@Estado", c.Estado);
        return (int)cmd.ExecuteScalar()!;
    }

    public void Actualizar(Curso c)
    {
        const string sql = @"UPDATE dbo.Cursos SET Codigo=@Codigo, Nombre=@Nombre, Descripcion=@Descripcion,
                             DuracionHoras=@Duracion, Estado=@Estado WHERE CursoId=@Id";
        using var conn = ConexionDB.ObtenerConexion();
        conn.Open();
        using var cmd = new SqlCommand(sql, conn);
        cmd.Parameters.AddWithValue("@Codigo", c.Codigo);
        cmd.Parameters.AddWithValue("@Nombre", c.Nombre);
        cmd.Parameters.AddWithValue("@Descripcion", (object?)c.Descripcion ?? DBNull.Value);
        cmd.Parameters.AddWithValue("@Duracion", c.DuracionHoras);
        cmd.Parameters.AddWithValue("@Estado", c.Estado);
        cmd.Parameters.AddWithValue("@Id", c.CursoId);
        cmd.ExecuteNonQuery();
    }

    public void Eliminar(int id)
    {
        const string sql = "UPDATE dbo.Cursos SET Estado = N'Inactivo' WHERE CursoId = @Id";
        using var conn = ConexionDB.ObtenerConexion();
        conn.Open();
        using var cmd = new SqlCommand(sql, conn);
        cmd.Parameters.AddWithValue("@Id", id);
        cmd.ExecuteNonQuery();
    }

    private static Curso MapearCurso(SqlDataReader r) => new()
    {
        CursoId = r.GetInt32(0),
        Codigo = r.GetString(1),
        Nombre = r.GetString(2),
        Descripcion = r.IsDBNull(3) ? null : r.GetString(3),
        DuracionHoras = r.GetInt32(4),
        Estado = r.GetString(5),
        FechaCreacion = r.GetDateTime(6)
    };
}
