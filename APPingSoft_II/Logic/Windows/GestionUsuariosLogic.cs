using APPingSoft_II.Logic;
using APPingSoft_II.Models;
using APPingSoft_II.Services;

namespace APPingSoft_II.Logic.Windows;

/// <summary>
/// Lógica de negocio para la ventana GestionUsuarios.
/// Solo accesible por el rol Administrador.
/// </summary>
public class GestionUsuariosLogic
{
    private readonly SistemaApp _sistema = SistemaApp.Instancia;
    private readonly UsuarioService _service = new();

    // ── Lectura ───────────────────────────────────────────────────────────────

    public List<Usuario> ObtenerTodos()
    {
        _sistema.RecargarUsuarios();
        return _sistema.Usuarios;
    }

    public List<Usuario> Buscar(string termino) =>
        _service.Buscar(_sistema.Usuarios, termino);

    // ── Escritura (todas validadas contra Permisos en capa Logic) ─────────────

    public (bool ok, string mensaje) Insertar(Usuario u)
    {
        Permisos.Requerir(Permisos.PuedeGestionarUsuarios(), "No tiene permisos para crear usuarios.");
        var resultado = _service.Insertar(u);
        if (resultado.ok) _sistema.RecargarUsuarios();
        return resultado;
    }

    public (bool ok, string mensaje) Actualizar(Usuario u)
    {
        Permisos.Requerir(Permisos.PuedeGestionarUsuarios(), "No tiene permisos para modificar usuarios.");
        var resultado = _service.Actualizar(u);
        if (resultado.ok) _sistema.RecargarUsuarios();
        return resultado;
    }

    public (bool ok, string mensaje) Desactivar(int usuarioId)
    {
        Permisos.Requerir(Permisos.PuedeGestionarUsuarios(), "No tiene permisos para desactivar usuarios.");
        if (usuarioId == _sistema.UsuarioActual?.UsuarioId)
            return (false, "No puede desactivar su propia cuenta mientras tiene la sesión activa.");
        var resultado = _service.Desactivar(usuarioId);
        if (resultado.ok) _sistema.RecargarUsuarios();
        return resultado;
    }

    public string ObtenerNombreUsuarioActual() =>
        _sistema.UsuarioActual?.NombreCompleto ?? string.Empty;
}
