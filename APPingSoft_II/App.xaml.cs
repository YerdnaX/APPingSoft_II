using System.Windows;

namespace APPingSoft_II;

public partial class App : Application
{
    protected override void OnStartup(StartupEventArgs e)
    {
        base.OnStartup(e);

        // Verificar conexión a base de datos al arrancar la aplicación.
        // Si falla, informar al usuario pero permitir que la pantalla de login
        // muestre el error detallado al intentar iniciar sesión.
        if (!APPingSoft_II.Data.ConexionDB.ProbarConexion(out string msg))
        {
            MessageBox.Show(
                $"Advertencia: No se pudo conectar a la base de datos.\n\n{msg}\n\n" +
                "Verifique que SQL Server esté activo y la cadena de conexión en:\n" +
                "APPingSoft_II/Config/Configuracion.cs",
                "Advertencia de conexión",
                MessageBoxButton.OK,
                MessageBoxImage.Warning);
        }
    }
}
