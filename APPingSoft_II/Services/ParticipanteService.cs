using APPingSoft_II.Models;
using APPingSoft_II.Repositories;
using System.Text.RegularExpressions;

namespace APPingSoft_II.Services;

public class ParticipanteService
{
    private readonly ParticipanteRepository _repo = new();

    // ── Consultas ─────────────────────────────────────────────────────────────

    public List<Participante> ObtenerTodos() => _repo.ObtenerTodos();

    public List<Participante> Buscar(string termino)
    {
        if (string.IsNullOrWhiteSpace(termino))
            return _repo.ObtenerTodos();
        return _repo.Buscar(termino.Trim());
    }

    // ── CREATE ────────────────────────────────────────────────────────────────

    public (bool ok, string mensaje) Insertar(Participante p)
    {
        var validacion = ValidarCampos(p);
        if (!validacion.ok) return validacion;

        if (_repo.ExisteCorreo(p.CorreoElectronico))
            return (false, $"Ya existe un participante registrado con el correo '{p.CorreoElectronico}'.");

        try
        {
            p.ParticipanteId = _repo.Insertar(p);
            return (true, $"Participante '{p.NombreCompleto}' registrado correctamente.");
        }
        catch (Microsoft.Data.SqlClient.SqlException ex) when (ex.Number == 2601 || ex.Number == 2627)
        {
            return (false, "Ya existe un participante con ese correo electrónico.");
        }
        catch (Exception ex)
        {
            return (false, $"Error al registrar participante: {ex.Message}");
        }
    }

    // ── UPDATE ────────────────────────────────────────────────────────────────

    public (bool ok, string mensaje) Actualizar(Participante p)
    {
        if (p.ParticipanteId <= 0)
            return (false, "Seleccione un participante de la tabla para modificar.");

        var validacion = ValidarCampos(p);
        if (!validacion.ok) return validacion;

        if (_repo.ExisteCorreo(p.CorreoElectronico, p.ParticipanteId))
            return (false, $"Otro participante ya usa el correo '{p.CorreoElectronico}'.");

        try
        {
            _repo.Actualizar(p);
            return (true, $"Participante '{p.NombreCompleto}' actualizado correctamente.");
        }
        catch (Exception ex)
        {
            return (false, $"Error al actualizar participante: {ex.Message}");
        }
    }

    // ── DELETE (lógico) ───────────────────────────────────────────────────────

    public (bool ok, string mensaje) Desactivar(int participanteId)
    {
        if (participanteId <= 0)
            return (false, "Seleccione un participante de la tabla para desactivar.");

        // Advertir si tiene inscripciones, pero permitir desactivar igual
        // (las inscripciones quedan como historial, no se eliminan)
        bool tieneInscripciones = _repo.TieneInscripciones(participanteId);
        try
        {
            _repo.Desactivar(participanteId);
            string extra = tieneInscripciones
                ? " Sus inscripciones e historial de resultados se conservan."
                : string.Empty;
            return (true, $"Participante desactivado correctamente.{extra}");
        }
        catch (Exception ex)
        {
            return (false, $"Error al desactivar participante: {ex.Message}");
        }
    }

    public bool TieneInscripciones(int participanteId) => _repo.TieneInscripciones(participanteId);

    // ── Validaciones ──────────────────────────────────────────────────────────

    private static (bool ok, string mensaje) ValidarCampos(Participante p)
    {
        if (string.IsNullOrWhiteSpace(p.NombreCompleto))
            return (false, "El nombre completo es obligatorio.");
        if (p.NombreCompleto.Length > 120)
            return (false, "El nombre no puede superar 120 caracteres.");
        if (string.IsNullOrWhiteSpace(p.CorreoElectronico))
            return (false, "El correo electrónico es obligatorio.");
        if (!EsCorreoValido(p.CorreoElectronico))
            return (false, "El correo electrónico no tiene un formato válido.");
        if (p.CorreoElectronico.Length > 150)
            return (false, "El correo no puede superar 150 caracteres.");
        if (p.Telefono != null && p.Telefono.Length > 25)
            return (false, "El teléfono no puede superar 25 caracteres.");

        return (true, string.Empty);
    }

    private static bool EsCorreoValido(string correo) =>
        Regex.IsMatch(correo, @"^[^@\s]+@[^@\s]+\.[^@\s]+$", RegexOptions.IgnoreCase);
}
