using APPingSoft_II.Data;
using APPingSoft_II.Models;

namespace APPingSoft_II.Logic.Windows;

/// <summary>
/// Lógica de negocio para la ventana de Login.
/// </summary>
public class LoginLogic
{
    private readonly SistemaApp _sistema = SistemaApp.Instancia;

    /// <summary>
    /// Intenta iniciar sesión con las credenciales dadas.
    /// Inicializa el sistema si la autenticación es exitosa.
    /// </summary>
    public (bool ok, string mensaje, Usuario? usuario) IniciarSesion(string correo, string contrasena)
    {
        if (string.IsNullOrWhiteSpace(correo))
            return (false, "Ingrese su correo electrónico.", null);
        if (string.IsNullOrWhiteSpace(contrasena))
            return (false, "Ingrese su contraseña.", null);

        // Primero verificar conexión
        if (!ConexionDB.ProbarConexion(out string mensajeConexion))
            return (false, $"Sin conexión a la base de datos.\n{mensajeConexion}", null);

        var usuario = _sistema.AutenticarUsuario(correo.Trim(), contrasena, out string mensajeAuth);
        if (usuario == null)
            return (false, mensajeAuth, null);

        // Cargar datos del sistema al autenticarse
        try
        {
            _sistema.CargarDatosIniciales();
        }
        catch (Exception ex)
        {
            return (false, $"Error al cargar datos del sistema: {ex.Message}", null);
        }

        return (true, $"Bienvenido, {usuario.NombreCompleto}.", usuario);
    }
}
