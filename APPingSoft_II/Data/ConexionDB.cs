using APPingSoft_II.Config;
using Microsoft.Data.SqlClient;

namespace APPingSoft_II.Data;

/// <summary>
/// Manejador central de conexiones a SQL Server.
/// Provee SqlConnection listas para usar en repositorios.
/// </summary>
public static class ConexionDB
{
    /// <summary>
    /// Retorna una nueva SqlConnection con la cadena configurada.
    /// El llamador es responsable de abrir y cerrar la conexión (using).
    /// </summary>
    public static SqlConnection ObtenerConexion()
    {
        return new SqlConnection(Configuracion.CadenaConexion);
    }

    /// <summary>
    /// Prueba si la base de datos es accesible.
    /// </summary>
    public static bool ProbarConexion(out string mensaje)
    {
        try
        {
            using var conn = ObtenerConexion();
            conn.Open();
            mensaje = "Conexión exitosa con APPingSoftII.";
            return true;
        }
        catch (SqlException ex)
        {
            mensaje = $"Error SQL ({ex.Number}): {ex.Message}";
            return false;
        }
        catch (Exception ex)
        {
            mensaje = $"Error de conexión: {ex.Message}";
            return false;
        }
    }
}
