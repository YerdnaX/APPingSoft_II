using APPingSoft_II.Data;
using APPingSoft_II.Models;
using Microsoft.Data.SqlClient;

namespace APPingSoft_II.Repositories;

public class InstructorRepository
{
    public List<Instructor> ObtenerTodos()
    {
        var lista = new List<Instructor>();
        const string sql = @"SELECT InstructorId, UsuarioId, NombreCompleto, CorreoElectronico, Especialidad, Estado, FechaRegistro
                             FROM dbo.Instructores WHERE Estado = N'Activo' ORDER BY NombreCompleto";
        using var conn = ConexionDB.ObtenerConexion();
        conn.Open();
        using var cmd = new SqlCommand(sql, conn);
        using var reader = cmd.ExecuteReader();
        while (reader.Read())
            lista.Add(MapearInstructor(reader));
        return lista;
    }

    public Instructor? ObtenerPorId(int id)
    {
        const string sql = @"SELECT InstructorId, UsuarioId, NombreCompleto, CorreoElectronico, Especialidad, Estado, FechaRegistro
                             FROM dbo.Instructores WHERE InstructorId = @Id";
        using var conn = ConexionDB.ObtenerConexion();
        conn.Open();
        using var cmd = new SqlCommand(sql, conn);
        cmd.Parameters.AddWithValue("@Id", id);
        using var reader = cmd.ExecuteReader();
        return reader.Read() ? MapearInstructor(reader) : null;
    }

    public int Insertar(Instructor i)
    {
        const string sql = @"INSERT INTO dbo.Instructores (UsuarioId, NombreCompleto, CorreoElectronico, Especialidad, Estado)
                             OUTPUT INSERTED.InstructorId
                             VALUES (@UsuarioId, @Nombre, @Correo, @Especialidad, @Estado)";
        using var conn = ConexionDB.ObtenerConexion();
        conn.Open();
        using var cmd = new SqlCommand(sql, conn);
        cmd.Parameters.AddWithValue("@UsuarioId", (object?)i.UsuarioId ?? DBNull.Value);
        cmd.Parameters.AddWithValue("@Nombre", i.NombreCompleto);
        cmd.Parameters.AddWithValue("@Correo", i.CorreoElectronico);
        cmd.Parameters.AddWithValue("@Especialidad", (object?)i.Especialidad ?? DBNull.Value);
        cmd.Parameters.AddWithValue("@Estado", i.Estado);
        return (int)cmd.ExecuteScalar()!;
    }

    public void Actualizar(Instructor i)
    {
        const string sql = @"UPDATE dbo.Instructores SET NombreCompleto=@Nombre, CorreoElectronico=@Correo,
                             Especialidad=@Especialidad, Estado=@Estado WHERE InstructorId=@Id";
        using var conn = ConexionDB.ObtenerConexion();
        conn.Open();
        using var cmd = new SqlCommand(sql, conn);
        cmd.Parameters.AddWithValue("@Nombre", i.NombreCompleto);
        cmd.Parameters.AddWithValue("@Correo", i.CorreoElectronico);
        cmd.Parameters.AddWithValue("@Especialidad", (object?)i.Especialidad ?? DBNull.Value);
        cmd.Parameters.AddWithValue("@Estado", i.Estado);
        cmd.Parameters.AddWithValue("@Id", i.InstructorId);
        cmd.ExecuteNonQuery();
    }

    public void Eliminar(int id)
    {
        const string sql = "UPDATE dbo.Instructores SET Estado = N'Inactivo' WHERE InstructorId = @Id";
        using var conn = ConexionDB.ObtenerConexion();
        conn.Open();
        using var cmd = new SqlCommand(sql, conn);
        cmd.Parameters.AddWithValue("@Id", id);
        cmd.ExecuteNonQuery();
    }

    private static Instructor MapearInstructor(SqlDataReader r) => new()
    {
        InstructorId = r.GetInt32(0),
        UsuarioId = r.IsDBNull(1) ? null : r.GetInt32(1),
        NombreCompleto = r.GetString(2),
        CorreoElectronico = r.GetString(3),
        Especialidad = r.IsDBNull(4) ? null : r.GetString(4),
        Estado = r.GetString(5),
        FechaRegistro = r.GetDateTime(6)
    };
}
