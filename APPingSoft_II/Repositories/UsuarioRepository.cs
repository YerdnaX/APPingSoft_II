using APPingSoft_II.Data;
using APPingSoft_II.Models;
using Microsoft.Data.SqlClient;

namespace APPingSoft_II.Repositories;

public class UsuarioRepository
{
    public List<Usuario> ObtenerTodos()
    {
        var lista = new List<Usuario>();
        const string sql = "SELECT UsuarioId, NombreCompleto, CorreoElectronico, Contrasena, Rol, Estado, FechaCreacion FROM dbo.Usuarios ORDER BY NombreCompleto";

        using var conn = ConexionDB.ObtenerConexion();
        conn.Open();
        using var cmd = new SqlCommand(sql, conn);
        using var reader = cmd.ExecuteReader();
        while (reader.Read())
            lista.Add(MapearUsuario(reader));

        return lista;
    }

    public Usuario? ObtenerPorId(int id)
    {
        const string sql = "SELECT UsuarioId, NombreCompleto, CorreoElectronico, Contrasena, Rol, Estado, FechaCreacion FROM dbo.Usuarios WHERE UsuarioId = @Id";
        using var conn = ConexionDB.ObtenerConexion();
        conn.Open();
        using var cmd = new SqlCommand(sql, conn);
        cmd.Parameters.AddWithValue("@Id", id);
        using var reader = cmd.ExecuteReader();
        return reader.Read() ? MapearUsuario(reader) : null;
    }

    public Usuario? AutenticarUsuario(string correo, string contrasena)
    {
        const string sql = @"SELECT UsuarioId, NombreCompleto, CorreoElectronico, Contrasena, Rol, Estado, FechaCreacion
                             FROM dbo.Usuarios
                             WHERE CorreoElectronico = @Correo AND Contrasena = @Contrasena AND Estado = N'Activo'";
        using var conn = ConexionDB.ObtenerConexion();
        conn.Open();
        using var cmd = new SqlCommand(sql, conn);
        cmd.Parameters.AddWithValue("@Correo", correo);
        cmd.Parameters.AddWithValue("@Contrasena", contrasena);
        using var reader = cmd.ExecuteReader();
        return reader.Read() ? MapearUsuario(reader) : null;
        // NOTA DE MEJORA: Implementar hash de contraseña (bcrypt/argon2) en producción.
    }

    public int Insertar(Usuario u)
    {
        const string sql = @"INSERT INTO dbo.Usuarios (NombreCompleto, CorreoElectronico, Contrasena, Rol, Estado)
                             OUTPUT INSERTED.UsuarioId
                             VALUES (@Nombre, @Correo, @Contrasena, @Rol, @Estado)";
        using var conn = ConexionDB.ObtenerConexion();
        conn.Open();
        using var cmd = new SqlCommand(sql, conn);
        cmd.Parameters.AddWithValue("@Nombre", u.NombreCompleto);
        cmd.Parameters.AddWithValue("@Correo", u.CorreoElectronico);
        cmd.Parameters.AddWithValue("@Contrasena", u.Contrasena);
        cmd.Parameters.AddWithValue("@Rol", u.Rol);
        cmd.Parameters.AddWithValue("@Estado", u.Estado);
        return (int)cmd.ExecuteScalar()!;
    }

    public void Actualizar(Usuario u)
    {
        const string sql = @"UPDATE dbo.Usuarios SET NombreCompleto=@Nombre, CorreoElectronico=@Correo,
                             Contrasena=@Contrasena, Rol=@Rol, Estado=@Estado WHERE UsuarioId=@Id";
        using var conn = ConexionDB.ObtenerConexion();
        conn.Open();
        using var cmd = new SqlCommand(sql, conn);
        cmd.Parameters.AddWithValue("@Nombre", u.NombreCompleto);
        cmd.Parameters.AddWithValue("@Correo", u.CorreoElectronico);
        cmd.Parameters.AddWithValue("@Contrasena", u.Contrasena);
        cmd.Parameters.AddWithValue("@Rol", u.Rol);
        cmd.Parameters.AddWithValue("@Estado", u.Estado);
        cmd.Parameters.AddWithValue("@Id", u.UsuarioId);
        cmd.ExecuteNonQuery();
    }

    /// <summary>Desactiva un usuario (borrado lógico). No elimina el registro.</summary>
    public void Desactivar(int id)
    {
        const string sql = "UPDATE dbo.Usuarios SET Estado = N'Inactivo' WHERE UsuarioId = @Id";
        using var conn = ConexionDB.ObtenerConexion();
        conn.Open();
        using var cmd = new SqlCommand(sql, conn);
        cmd.Parameters.AddWithValue("@Id", id);
        cmd.ExecuteNonQuery();
    }

    /// <summary>
    /// Verifica si el correo ya existe para otro usuario (para validación de unicidad).
    /// excluirId = 0 en insert, = UsuarioId del usuario a modificar.
    /// </summary>
    public bool ExisteCorreo(string correo, int excluirId = 0)
    {
        const string sql = @"SELECT COUNT(1) FROM dbo.Usuarios
                             WHERE CorreoElectronico = @Correo AND UsuarioId <> @ExcluirId";
        using var conn = ConexionDB.ObtenerConexion();
        conn.Open();
        using var cmd = new SqlCommand(sql, conn);
        cmd.Parameters.AddWithValue("@Correo", correo.Trim().ToLower());
        cmd.Parameters.AddWithValue("@ExcluirId", excluirId);
        return (int)cmd.ExecuteScalar()! > 0;
    }

    private static Usuario MapearUsuario(SqlDataReader r) => new()
    {
        UsuarioId = r.GetInt32(0),
        NombreCompleto = r.GetString(1),
        CorreoElectronico = r.GetString(2),
        Contrasena = r.GetString(3),
        Rol = r.GetString(4),
        Estado = r.GetString(5),
        FechaCreacion = r.GetDateTime(6)
    };
}
