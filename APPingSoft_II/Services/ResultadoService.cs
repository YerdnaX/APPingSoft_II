using APPingSoft_II.Models;
using APPingSoft_II.Repositories;

namespace APPingSoft_II.Services;

public class ResultadoService
{
    private readonly ResultadoEvaluacionRepository _repo = new();
    private readonly EvaluacionRepository _evalRepo = new();
    private readonly InscripcionRepository _insRepo = new();

    public (bool ok, string mensaje) Insertar(ResultadoEvaluacion r)
    {
        if (r.EvaluacionId <= 0)
            return (false, "Debe seleccionar una evaluación.");
        if (r.InscripcionId <= 0)
            return (false, "Debe seleccionar un participante.");
        if (r.NotaFinal < 0)
            return (false, "La nota final no puede ser negativa.");

        // Validar que la nota no supere el máximo de la evaluación
        var evaluacion = _evalRepo.ObtenerPorId(r.EvaluacionId);
        if (evaluacion != null && r.NotaFinal > evaluacion.PuntosMax)
            return (false, $"La nota final ({r.NotaFinal}) supera los puntos máximos de la evaluación ({evaluacion.PuntosMax}).");

        // Validar duplicado
        if (_repo.ExisteResultado(r.EvaluacionId, r.InscripcionId))
            return (false, "Ya existe un resultado registrado para este participante en esta evaluación.");

        try
        {
            r.CalificadoEn = DateTime.Now;
            r.ResultadoId = _repo.Insertar(r);
            return (true, "Resultado registrado correctamente.");
        }
        catch (Exception ex)
        {
            return (false, $"Error al registrar resultado: {ex.Message}");
        }
    }

    public (bool ok, string mensaje) Actualizar(ResultadoEvaluacion r)
    {
        if (r.ResultadoId <= 0)
            return (false, "Seleccione un resultado de la tabla para modificar.");
        if (r.NotaFinal < 0)
            return (false, "La nota final no puede ser negativa.");

        var evaluacion = _evalRepo.ObtenerPorId(r.EvaluacionId);
        if (evaluacion != null && r.NotaFinal > evaluacion.PuntosMax)
            return (false, $"La nota final supera los puntos máximos ({evaluacion.PuntosMax}).");

        try
        {
            r.CalificadoEn = DateTime.Now;
            _repo.Actualizar(r);
            return (true, "Resultado actualizado correctamente.");
        }
        catch (Exception ex)
        {
            return (false, $"Error al actualizar: {ex.Message}");
        }
    }

    public (bool ok, string mensaje) Eliminar(int resultadoId)
    {
        if (resultadoId <= 0)
            return (false, "Seleccione un resultado para eliminar.");
        try
        {
            _repo.Eliminar(resultadoId);
            return (true, "Resultado eliminado.");
        }
        catch (Exception ex)
        {
            return (false, $"Error al eliminar: {ex.Message}");
        }
    }

    public List<ResultadoEvaluacion> ObtenerTodos() => _repo.ObtenerTodos();

    public List<Inscripcion> ObtenerInscripcionesPorEvaluacion(int evaluacionId) =>
        _insRepo.ObtenerPorCursoDeEvaluacion(evaluacionId);
}
