using APPingSoft_II.Models;
using APPingSoft_II.Repositories;

namespace APPingSoft_II.Services;

public class InscripcionService
{
    private readonly InscripcionRepository _repo = new();

    // ── Consultas ─────────────────────────────────────────────────────────────

    public List<Inscripcion> ObtenerTodasParaGestion() => _repo.ObtenerTodasParaGestion();

    // ── CREATE ────────────────────────────────────────────────────────────────

    public (bool ok, string mensaje) Registrar(Inscripcion ins)
    {
        if (ins.ParticipanteId <= 0)
            return (false, "Seleccione un participante.");
        if (ins.ProgramaId <= 0)
            return (false, "Seleccione un programa.");
        if (ins.FechaInscripcion == default)
            return (false, "Indique la fecha de inscripción.");

        if (_repo.ExisteDuplicado(ins.ProgramaId, ins.ParticipanteId))
            return (false, "El participante ya está inscrito en ese programa.");

        try
        {
            ins.InscripcionId = _repo.Insertar(ins);
            return (true, "Inscripción registrada correctamente.");
        }
        catch (Microsoft.Data.SqlClient.SqlException ex) when (ex.Number == 2601 || ex.Number == 2627)
        {
            return (false, "El participante ya está inscrito en ese programa (restricción de base de datos).");
        }
        catch (Exception ex)
        {
            return (false, $"Error al registrar inscripción: {ex.Message}");
        }
    }

    // ── UPDATE ────────────────────────────────────────────────────────────────

    public (bool ok, string mensaje) Actualizar(int inscripcionId, DateTime fecha, string estado)
    {
        if (inscripcionId <= 0)
            return (false, "Seleccione una inscripción de la tabla para modificar.");
        if (fecha == default)
            return (false, "Indique la fecha de inscripción.");
        if (string.IsNullOrWhiteSpace(estado))
            return (false, "Seleccione un estado válido.");

        try
        {
            _repo.Actualizar(inscripcionId, fecha, estado);
            return (true, "Inscripción actualizada correctamente.");
        }
        catch (Exception ex)
        {
            return (false, $"Error al actualizar inscripción: {ex.Message}");
        }
    }

    // ── DELETE (lógico) ───────────────────────────────────────────────────────

    public (bool ok, string mensaje) Desactivar(int inscripcionId)
    {
        if (inscripcionId <= 0)
            return (false, "Seleccione una inscripción de la tabla para retirar.");

        bool tieneResultados = _repo.TieneResultados(inscripcionId);
        try
        {
            _repo.CambiarEstado(inscripcionId, "Retirada");
            string extra = tieneResultados
                ? " Los resultados registrados para esta inscripción se conservan."
                : string.Empty;
            return (true, $"Inscripción marcada como Retirada correctamente.{extra}");
        }
        catch (Exception ex)
        {
            return (false, $"Error al retirar inscripción: {ex.Message}");
        }
    }
}
