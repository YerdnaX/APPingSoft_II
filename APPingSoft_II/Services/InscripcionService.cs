using APPingSoft_II.Models;
using APPingSoft_II.Repositories;

namespace APPingSoft_II.Services;

public class InscripcionService
{
    private readonly InscripcionRepository _repo = new();
    private static readonly HashSet<string> EstadosValidos = new(StringComparer.OrdinalIgnoreCase)
    {
        "Activa",
        "Finalizada",
        "Retirada"
    };

    // Consultas

    public List<Inscripcion> ObtenerTodasParaGestion() => _repo.ObtenerTodasParaGestion();
    public List<Programa> ObtenerProgramasConInscripcionesPorEvaluacion(int evaluacionId) =>
        _repo.ObtenerProgramasConInscripcionesPorEvaluacion(evaluacionId);
    public List<Inscripcion> ObtenerInscripcionesPorEvaluacionYPrograma(int evaluacionId, int? programaId = null) =>
        _repo.ObtenerPorCursoDeEvaluacion(evaluacionId, programaId);
    public Inscripcion? ObtenerPorIdConDetalle(int inscripcionId) =>
        _repo.ObtenerPorIdConDetalle(inscripcionId);

    // CREATE

    public (bool ok, string mensaje) Registrar(Inscripcion ins)
    {
        if (ins.ParticipanteId <= 0)
            return (false, "Seleccione un participante.");
        if (ins.ProgramaId <= 0)
            return (false, "Seleccione un programa.");
        if (ins.FechaInscripcion == default)
            return (false, "Indique la fecha de inscripcion.");
        if (!EstadosValidos.Contains(ins.Estado))
            return (false, "Estado de inscripcion no valido.");

        if (_repo.ExisteDuplicado(ins.ProgramaId, ins.ParticipanteId))
            return (false, "El participante ya esta inscrito en ese programa.");

        try
        {
            ins.InscripcionId = _repo.Insertar(ins);
            return (true, "Inscripcion registrada correctamente.");
        }
        catch (Microsoft.Data.SqlClient.SqlException ex) when (ex.Number == 2601 || ex.Number == 2627)
        {
            return (false, "El participante ya esta inscrito en ese programa (restriccion de base de datos).");
        }
        catch (Exception ex)
        {
            return (false, $"Error al registrar inscripcion: {ex.Message}");
        }
    }

    // UPDATE

    public (bool ok, string mensaje) Actualizar(int inscripcionId, DateTime fecha, string estado)
    {
        if (inscripcionId <= 0)
            return (false, "Seleccione una inscripcion de la tabla para modificar.");
        if (fecha == default)
            return (false, "Indique la fecha de inscripcion.");
        if (string.IsNullOrWhiteSpace(estado))
            return (false, "Seleccione un estado valido.");
        if (!EstadosValidos.Contains(estado))
            return (false, "Estado de inscripcion no valido.");

        try
        {
            _repo.Actualizar(inscripcionId, fecha, estado);
            return (true, "Inscripcion actualizada correctamente.");
        }
        catch (Exception ex)
        {
            return (false, $"Error al actualizar inscripcion: {ex.Message}");
        }
    }

    // DELETE (logico)

    public (bool ok, string mensaje) Desactivar(int inscripcionId)
    {
        if (inscripcionId <= 0)
            return (false, "Seleccione una inscripcion de la tabla para retirar.");

        bool tieneResultados = _repo.TieneResultados(inscripcionId);
        try
        {
            _repo.CambiarEstado(inscripcionId, "Retirada");
            string extra = tieneResultados
                ? " Los resultados registrados para esta inscripcion se conservan."
                : string.Empty;
            return (true, $"Inscripcion marcada como Retirada correctamente.{extra}");
        }
        catch (Exception ex)
        {
            return (false, $"Error al retirar inscripcion: {ex.Message}");
        }
    }
}
