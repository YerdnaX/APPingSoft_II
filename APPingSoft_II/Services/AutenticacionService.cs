using APPingSoft_II.Models;
using APPingSoft_II.Repositories;

namespace APPingSoft_II.Services;

public class AutenticacionService
{
    private readonly UsuarioRepository _repo;

    public AutenticacionService()
    {
        _repo = new UsuarioRepository();
    }

    /// <summary>
    /// Valida credenciales contra la base de datos.
    /// Retorna el usuario si es válido, null si no.
    /// </summary>
    public Usuario? IniciarSesion(string correo, string contrasena, out string mensaje)
    {
        if (string.IsNullOrWhiteSpace(correo))
        {
            mensaje = "El correo electrónico es obligatorio.";
            return null;
        }
        if (string.IsNullOrWhiteSpace(contrasena))
        {
            mensaje = "La contraseña es obligatoria.";
            return null;
        }

        var usuario = _repo.AutenticarUsuario(correo.Trim(), contrasena);
        if (usuario == null)
        {
            mensaje = "Credenciales incorrectas o usuario inactivo.";
            return null;
        }

        mensaje = $"Bienvenido, {usuario.NombreCompleto}.";
        return usuario;
    }
}
