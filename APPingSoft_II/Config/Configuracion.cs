namespace APPingSoft_II.Config;

/// <summary>
/// Configuración central de conexión a base de datos.
/// Modifique CadenaConexion para apuntar a su servidor SQL Server.
/// </summary>
public static class Configuracion
{
    // *** MODIFIQUE ESTA CADENA PARA SU ENTORNO ***
    // Para SQL Server Express: "Server=.\SQLEXPRESS;Database=APPingSoftII;Trusted_Connection=True;TrustServerCertificate=True;"
    // Para instancia nombrada: "Server=NOMBRESERVIDOR\INSTANCIA;Database=APPingSoftII;Trusted_Connection=True;TrustServerCertificate=True;"
    // Con usuario/clave:       "Server=localhost;Database=APPingSoftII;User Id=sa;Password=TuClave;TrustServerCertificate=True;"
    public const string CadenaConexion =
        "Server=localhost;Database=APPingSoftII;Trusted_Connection=True;TrustServerCertificate=True;";
}
