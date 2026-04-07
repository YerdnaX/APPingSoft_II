using APPingSoft_II.Models;
using APPingSoft_II.Repositories;
using System.Text.RegularExpressions;

namespace APPingSoft_II.Services;

public class UsuarioService
{
    private readonly UsuarioRepository _repo = new();

    private static readonly string[] RolesValidos = ["Administrador", "Coordinador", "Instructor"];

    // ── Insertar ──────────────────────────────────────────────────────────────

    public (bool ok, string mensaje) Insertar(Usuario u)
    {
        var validacion = Validar(u, esNuevo: true);
        if (!validacion.ok) return validacion;

        try
        {
            u.UsuarioId = _repo.Insertar(u);
            return (true, $"Usuario '{u.NombreCompleto}' registrado correctamente.");
        }
        catch (Exception ex)
        {
            return (false, $"Error al registrar usuario: {ex.Message}");
        }
    }

    // ── Actualizar ────────────────────────────────────────────────────────────

    public (bool ok, string mensaje) Actualizar(Usuario u)
    {
        var validacion = Validar(u, esNuevo: false);
        if (!validacion.ok) return validacion;

        try
        {
            _repo.Actualizar(u);
            return (true, $"Usuario '{u.NombreCompleto}' actualizado correctamente.");
        }
        catch (Exception ex)
        {
            return (false, $"Error al actualizar usuario: {ex.Message}");
        }
    }

    // ── Desactivar ────────────────────────────────────────────────────────────

    public (bool ok, string mensaje) Desactivar(int usuarioId)
    {
        try
        {
            _repo.Desactivar(usuarioId);
            return (true, "Usuario desactivado correctamente.");
        }
        catch (Exception ex)
        {
            return (false, $"Error al desactivar usuario: {ex.Message}");
        }
    }

    // ── Búsqueda en memoria ───────────────────────────────────────────────────

    public List<Usuario> Buscar(List<Usuario> todos, string termino)
    {
        termino = termino.Trim().ToLower();
        return todos
            .Where(u =>
                u.NombreCompleto.ToLower().Contains(termino) ||
                u.CorreoElectronico.ToLower().Contains(termino) ||
                u.Rol.ToLower().Contains(termino) ||
                u.Estado.ToLower().Contains(termino))
            .ToList();
    }

    // ── Validación ────────────────────────────────────────────────────────────

    private (bool ok, string mensaje) Validar(Usuario u, bool esNuevo)
    {
        if (string.IsNullOrWhiteSpace(u.NombreCompleto))
            return (false, "El nombre completo es obligatorio.");

        if (string.IsNullOrWhiteSpace(u.CorreoElectronico))
            return (false, "El correo electrónico es obligatorio.");

        if (!EsCorreoValido(u.CorreoElectronico))
            return (false, "El formato del correo electrónico no es válido.");

        if (esNuevo && string.IsNullOrWhiteSpace(u.Contrasena))
            return (false, "La contraseña es obligatoria para un nuevo usuario.");

        if (!string.IsNullOrWhiteSpace(u.Contrasena) && u.Contrasena.Length < 6)
            return (false, "La contraseña debe tener al menos 6 caracteres.");

        if (!RolesValidos.Contains(u.Rol))
            return (false, $"Rol inválido. Los roles válidos son: {string.Join(", ", RolesValidos)}.");

        int excluirId = esNuevo ? 0 : u.UsuarioId;
        if (_repo.ExisteCorreo(u.CorreoElectronico, excluirId))
            return (false, $"Ya existe un usuario con el correo '{u.CorreoElectronico}'.");

        return (true, string.Empty);
    }

    private static bool EsCorreoValido(string correo) =>
        Regex.IsMatch(correo.Trim(), @"^[^@\s]+@[^@\s]+\.[^@\s]+$", RegexOptions.IgnoreCase);
}
