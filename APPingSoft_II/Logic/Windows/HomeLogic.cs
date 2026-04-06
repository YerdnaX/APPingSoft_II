namespace APPingSoft_II.Logic.Windows;

/// <summary>
/// Lógica para la ventana Home/Dashboard.
/// </summary>
public class HomeLogic
{
    private readonly SistemaApp _sistema = SistemaApp.Instancia;

    /// <summary>
    /// Retorna un resumen del estado del sistema para mostrar en el dashboard.
    /// </summary>
    public (int programas, int evaluaciones, int resultados, string usuarioNombre) ObtenerResumen()
    {
        return (
            _sistema.Programas.Count,
            _sistema.Evaluaciones.Count,
            _sistema.Resultados.Count,
            _sistema.UsuarioActual?.NombreCompleto ?? "Sin sesión"
        );
    }

    public string ObtenerNombreUsuario() =>
        _sistema.UsuarioActual?.NombreCompleto ?? string.Empty;

    public string ObtenerRolUsuario() =>
        _sistema.UsuarioActual?.Rol ?? string.Empty;

    public void CerrarSesion() => _sistema.CerrarSesion();
}
