using APPingSoft_II.Data;
using APPingSoft_II.Models;
using Microsoft.Data.SqlClient;

namespace APPingSoft_II.Repositories;

public class ParticipanteRepository
{
    /// <summary>
    /// Retorna todos los participantes (activos e inactivos) para la pantalla de gestión.
    /// </summary>
    public List<Participante> ObtenerTodos()
    {
        var lista = new List<Participante>();
        const string sql = @"SELECT ParticipanteId, NombreCompleto, CorreoElectronico, Telefono, Estado, FechaRegistro
                             FROM dbo.Participantes
                             ORDER BY NombreCompleto";
        using var conn = ConexionDB.ObtenerConexion();
        conn.Open();
        using var cmd = new SqlCommand(sql, conn);
        using var reader = cmd.ExecuteReader();
        while (reader.Read())
            lista.Add(MapearParticipante(reader));
        return lista;
    }

    /// <summary>
    /// Retorna solo los activos (para combos en otras pantallas).
    /// </summary>
    public List<Participante> ObtenerActivos()
    {
        var lista = new List<Participante>();
        const string sql = @"SELECT ParticipanteId, NombreCompleto, CorreoElectronico, Telefono, Estado, FechaRegistro
                             FROM dbo.Participantes WHERE Estado = N'Activo'
                             ORDER BY NombreCompleto";
        using var conn = ConexionDB.ObtenerConexion();
        conn.Open();
        using var cmd = new SqlCommand(sql, conn);
        using var reader = cmd.ExecuteReader();
        while (reader.Read())
            lista.Add(MapearParticipante(reader));
        return lista;
    }

    /// <summary>
    /// Busca participantes por nombre, correo o teléfono (búsqueda parcial).
    /// </summary>
    public List<Participante> Buscar(string termino)
    {
        var lista = new List<Participante>();
        const string sql = @"SELECT ParticipanteId, NombreCompleto, CorreoElectronico, Telefono, Estado, FechaRegistro
                             FROM dbo.Participantes
                             WHERE NombreCompleto LIKE @Termino
                                OR CorreoElectronico LIKE @Termino
                                OR ISNULL(Telefono,'') LIKE @Termino
                             ORDER BY NombreCompleto";
        using var conn = ConexionDB.ObtenerConexion();
        conn.Open();
        using var cmd = new SqlCommand(sql, conn);
        cmd.Parameters.AddWithValue("@Termino", $"%{termino}%");
        using var reader = cmd.ExecuteReader();
        while (reader.Read())
            lista.Add(MapearParticipante(reader));
        return lista;
    }

    public Participante? ObtenerPorId(int id)
    {
        const string sql = @"SELECT ParticipanteId, NombreCompleto, CorreoElectronico, Telefono, Estado, FechaRegistro
                             FROM dbo.Participantes WHERE ParticipanteId = @Id";
        using var conn = ConexionDB.ObtenerConexion();
        conn.Open();
        using var cmd = new SqlCommand(sql, conn);
        cmd.Parameters.AddWithValue("@Id", id);
        using var reader = cmd.ExecuteReader();
        return reader.Read() ? MapearParticipante(reader) : null;
    }

    public bool ExisteCorreo(string correo, int excluirId = 0)
    {
        const string sql = @"SELECT COUNT(1) FROM dbo.Participantes
                             WHERE CorreoElectronico = @Correo AND ParticipanteId <> @ExcluirId";
        using var conn = ConexionDB.ObtenerConexion();
        conn.Open();
        using var cmd = new SqlCommand(sql, conn);
        cmd.Parameters.AddWithValue("@Correo", correo);
        cmd.Parameters.AddWithValue("@ExcluirId", excluirId);
        return (int)cmd.ExecuteScalar()! > 0;
    }

    public bool TieneInscripciones(int participanteId)
    {
        const string sql = "SELECT COUNT(1) FROM dbo.Inscripciones WHERE ParticipanteId = @Id";
        using var conn = ConexionDB.ObtenerConexion();
        conn.Open();
        using var cmd = new SqlCommand(sql, conn);
        cmd.Parameters.AddWithValue("@Id", participanteId);
        return (int)cmd.ExecuteScalar()! > 0;
    }

    public int Insertar(Participante p)
    {
        const string sql = @"INSERT INTO dbo.Participantes (NombreCompleto, CorreoElectronico, Telefono, Estado)
                             OUTPUT INSERTED.ParticipanteId
                             VALUES (@Nombre, @Correo, @Telefono, @Estado)";
        using var conn = ConexionDB.ObtenerConexion();
        conn.Open();
        using var cmd = new SqlCommand(sql, conn);
        cmd.Parameters.AddWithValue("@Nombre", p.NombreCompleto);
        cmd.Parameters.AddWithValue("@Correo", p.CorreoElectronico);
        cmd.Parameters.AddWithValue("@Telefono", (object?)p.Telefono ?? DBNull.Value);
        cmd.Parameters.AddWithValue("@Estado", p.Estado);
        return (int)cmd.ExecuteScalar()!;
    }

    public void Actualizar(Participante p)
    {
        const string sql = @"UPDATE dbo.Participantes
                             SET NombreCompleto=@Nombre, CorreoElectronico=@Correo,
                                 Telefono=@Telefono, Estado=@Estado
                             WHERE ParticipanteId=@Id";
        using var conn = ConexionDB.ObtenerConexion();
        conn.Open();
        using var cmd = new SqlCommand(sql, conn);
        cmd.Parameters.AddWithValue("@Nombre", p.NombreCompleto);
        cmd.Parameters.AddWithValue("@Correo", p.CorreoElectronico);
        cmd.Parameters.AddWithValue("@Telefono", (object?)p.Telefono ?? DBNull.Value);
        cmd.Parameters.AddWithValue("@Estado", p.Estado);
        cmd.Parameters.AddWithValue("@Id", p.ParticipanteId);
        cmd.ExecuteNonQuery();
    }

    /// <summary>
    /// Desactiva un participante (borrado lógico). Preserva relaciones históricas.
    /// </summary>
    public void Desactivar(int id)
    {
        const string sql = "UPDATE dbo.Participantes SET Estado = N'Inactivo' WHERE ParticipanteId = @Id";
        using var conn = ConexionDB.ObtenerConexion();
        conn.Open();
        using var cmd = new SqlCommand(sql, conn);
        cmd.Parameters.AddWithValue("@Id", id);
        cmd.ExecuteNonQuery();
    }

    private static Participante MapearParticipante(SqlDataReader r) => new()
    {
        ParticipanteId = r.GetInt32(0),
        NombreCompleto = r.GetString(1),
        CorreoElectronico = r.GetString(2),
        Telefono = r.IsDBNull(3) ? null : r.GetString(3),
        Estado = r.GetString(4),
        FechaRegistro = r.GetDateTime(5)
    };
}
