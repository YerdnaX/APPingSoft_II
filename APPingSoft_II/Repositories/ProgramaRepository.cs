using APPingSoft_II.Data;
using APPingSoft_II.Models;
using Microsoft.Data.SqlClient;

namespace APPingSoft_II.Repositories;

public class ProgramaRepository
{
    /// <summary>
    /// Obtiene todos los programas con sus instructores y cursos cargados.
    /// </summary>
    public List<Programa> ObtenerTodos()
    {
        var lista = new List<Programa>();
        const string sql = @"
            SELECT p.ProgramaId, p.Nombre, p.FechaInicio, p.FechaFin, p.Estado, p.InstructorId, p.CursoId, p.FechaCreacion,
                   i.NombreCompleto AS InstructorNombre, i.CorreoElectronico AS InstructorCorreo,
                   c.Codigo AS CursoCodigo, c.Nombre AS CursoNombre
            FROM dbo.Programas p
            INNER JOIN dbo.Instructores i ON i.InstructorId = p.InstructorId
            INNER JOIN dbo.Cursos c ON c.CursoId = p.CursoId
            ORDER BY p.FechaInicio DESC";

        using var conn = ConexionDB.ObtenerConexion();
        conn.Open();
        using var cmd = new SqlCommand(sql, conn);
        using var reader = cmd.ExecuteReader();
        while (reader.Read())
            lista.Add(MapearPrograma(reader));
        return lista;
    }

    public Programa? ObtenerPorId(int id)
    {
        const string sql = @"
            SELECT p.ProgramaId, p.Nombre, p.FechaInicio, p.FechaFin, p.Estado, p.InstructorId, p.CursoId, p.FechaCreacion,
                   i.NombreCompleto, i.CorreoElectronico,
                   c.Codigo, c.Nombre
            FROM dbo.Programas p
            INNER JOIN dbo.Instructores i ON i.InstructorId = p.InstructorId
            INNER JOIN dbo.Cursos c ON c.CursoId = p.CursoId
            WHERE p.ProgramaId = @Id";
        using var conn = ConexionDB.ObtenerConexion();
        conn.Open();
        using var cmd = new SqlCommand(sql, conn);
        cmd.Parameters.AddWithValue("@Id", id);
        using var reader = cmd.ExecuteReader();
        return reader.Read() ? MapearPrograma(reader) : null;
    }

    public int Insertar(Programa p)
    {
        const string sql = @"INSERT INTO dbo.Programas (Nombre, FechaInicio, FechaFin, Estado, InstructorId, CursoId)
                             OUTPUT INSERTED.ProgramaId
                             VALUES (@Nombre, @FechaInicio, @FechaFin, @Estado, @InstructorId, @CursoId)";
        using var conn = ConexionDB.ObtenerConexion();
        conn.Open();
        using var cmd = new SqlCommand(sql, conn);
        cmd.Parameters.AddWithValue("@Nombre", p.Nombre);
        cmd.Parameters.AddWithValue("@FechaInicio", p.FechaInicio.Date);
        cmd.Parameters.AddWithValue("@FechaFin", p.FechaFin.Date);
        cmd.Parameters.AddWithValue("@Estado", p.Estado);
        cmd.Parameters.AddWithValue("@InstructorId", p.InstructorId);
        cmd.Parameters.AddWithValue("@CursoId", p.CursoId);
        return (int)cmd.ExecuteScalar()!;
    }

    public void Actualizar(Programa p)
    {
        const string sql = @"UPDATE dbo.Programas SET Nombre=@Nombre, FechaInicio=@FechaInicio, FechaFin=@FechaFin,
                             Estado=@Estado, InstructorId=@InstructorId, CursoId=@CursoId WHERE ProgramaId=@Id";
        using var conn = ConexionDB.ObtenerConexion();
        conn.Open();
        using var cmd = new SqlCommand(sql, conn);
        cmd.Parameters.AddWithValue("@Nombre", p.Nombre);
        cmd.Parameters.AddWithValue("@FechaInicio", p.FechaInicio.Date);
        cmd.Parameters.AddWithValue("@FechaFin", p.FechaFin.Date);
        cmd.Parameters.AddWithValue("@Estado", p.Estado);
        cmd.Parameters.AddWithValue("@InstructorId", p.InstructorId);
        cmd.Parameters.AddWithValue("@CursoId", p.CursoId);
        cmd.Parameters.AddWithValue("@Id", p.ProgramaId);
        cmd.ExecuteNonQuery();
    }

    public void Eliminar(int id)
    {
        const string sql = "UPDATE dbo.Programas SET Estado = N'Inactivo' WHERE ProgramaId = @Id";
        using var conn = ConexionDB.ObtenerConexion();
        conn.Open();
        using var cmd = new SqlCommand(sql, conn);
        cmd.Parameters.AddWithValue("@Id", id);
        cmd.ExecuteNonQuery();
    }

    private static Programa MapearPrograma(SqlDataReader r) => new()
    {
        ProgramaId = r.GetInt32(0),
        Nombre = r.GetString(1),
        FechaInicio = r.GetDateTime(2),
        FechaFin = r.GetDateTime(3),
        Estado = r.GetString(4),
        InstructorId = r.GetInt32(5),
        CursoId = r.GetInt32(6),
        FechaCreacion = r.GetDateTime(7),
        Instructor = new Instructor
        {
            InstructorId = r.GetInt32(5),
            NombreCompleto = r.GetString(8),
            CorreoElectronico = r.GetString(9)
        },
        Curso = new Curso
        {
            CursoId = r.GetInt32(6),
            Codigo = r.GetString(10),
            Nombre = r.GetString(11)
        }
    };
}
