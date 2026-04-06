using APPingSoft_II.Data;
using APPingSoft_II.Models;
using Microsoft.Data.SqlClient;

namespace APPingSoft_II.Repositories;

public class ParticipanteRepository
{
    public List<Participante> ObtenerTodos()
    {
        var lista = new List<Participante>();
        const string sql = "SELECT ParticipanteId, NombreCompleto, CorreoElectronico, Telefono, Estado, FechaRegistro FROM dbo.Participantes WHERE Estado = N'Activo' ORDER BY NombreCompleto";
        using var conn = ConexionDB.ObtenerConexion();
        conn.Open();
        using var cmd = new SqlCommand(sql, conn);
        using var reader = cmd.ExecuteReader();
        while (reader.Read())
            lista.Add(MapearParticipante(reader));
        return lista;
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
        const string sql = @"UPDATE dbo.Participantes SET NombreCompleto=@Nombre, CorreoElectronico=@Correo,
                             Telefono=@Telefono, Estado=@Estado WHERE ParticipanteId=@Id";
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
