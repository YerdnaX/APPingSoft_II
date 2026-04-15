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
            return (false, "Debe seleccionar una evaluacion.");
        if (r.InscripcionId <= 0)
            return (false, "Debe seleccionar un participante.");
        if (r.NotaFinal < 0)
            return (false, "La nota final no puede ser negativa.");

        var evaluacion = _evalRepo.ObtenerPorId(r.EvaluacionId);
        if (evaluacion == null)
            return (false, "La evaluacion seleccionada no existe.");

        var inscripcion = _insRepo.ObtenerPorIdConDetalle(r.InscripcionId);
        if (inscripcion == null)
            return (false, "La inscripcion seleccionada no existe.");

        var validacionRelacion = ValidarRelacionEvaluacionInscripcion(evaluacion, inscripcion, validarEstadoCalificable: true);
        if (!validacionRelacion.ok)
            return validacionRelacion;

        if (r.NotaFinal > evaluacion.PuntosMax)
            return (false, $"La nota final ({r.NotaFinal}) supera los puntos maximos de la evaluacion ({evaluacion.PuntosMax}).");

        if (_repo.ExisteResultado(r.EvaluacionId, r.InscripcionId))
            return (false, "Ya existe un resultado registrado para este participante en esta evaluacion.");

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

        var existente = _repo.ObtenerPorId(r.ResultadoId);
        if (existente == null)
            return (false, "El resultado seleccionado ya no existe.");

        var evaluacionReal = _evalRepo.ObtenerPorId(existente.EvaluacionId);
        if (evaluacionReal == null)
            return (false, "No se pudo validar la evaluacion asociada al resultado.");

        var inscripcionReal = _insRepo.ObtenerPorIdConDetalle(existente.InscripcionId);
        if (inscripcionReal == null)
            return (false, "No se pudo validar la inscripcion asociada al resultado.");

        var validacionRelacion = ValidarRelacionEvaluacionInscripcion(evaluacionReal, inscripcionReal, validarEstadoCalificable: false);
        if (!validacionRelacion.ok)
            return validacionRelacion;

        if (r.NotaFinal > evaluacionReal.PuntosMax)
            return (false, $"La nota final supera los puntos maximos ({evaluacionReal.PuntosMax}).");

        // La actualizacion solo modifica nota y observaciones; no cambia evaluacion ni participante.
        r.EvaluacionId = existente.EvaluacionId;
        r.InscripcionId = existente.InscripcionId;

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

    public List<Programa> ObtenerProgramasPorEvaluacion(int evaluacionId) =>
        _insRepo.ObtenerProgramasConInscripcionesPorEvaluacion(evaluacionId);

    public List<Inscripcion> ObtenerInscripcionesPorEvaluacion(int evaluacionId, int? programaId = null) =>
        _insRepo.ObtenerPorCursoDeEvaluacion(evaluacionId, programaId);

    private static (bool ok, string mensaje) ValidarRelacionEvaluacionInscripcion(
        Evaluacion evaluacion,
        Inscripcion inscripcion,
        bool validarEstadoCalificable)
    {
        if (inscripcion.Programa == null)
            return (false, "No se pudo validar el programa de la inscripcion.");

        if (inscripcion.Programa.CursoId != evaluacion.CursoId)
            return (false, "La inscripcion no pertenece al curso de la evaluacion seleccionada.");

        if (string.Equals(inscripcion.Programa.Estado, "Inactivo", StringComparison.OrdinalIgnoreCase))
            return (false, "No se puede registrar resultado para un programa inactivo.");

        if (validarEstadoCalificable && !EsEstadoInscripcionCalificable(inscripcion.Estado))
            return (false, "Solo se pueden calificar inscripciones en estado Activa o Finalizada.");

        return (true, string.Empty);
    }

    private static bool EsEstadoInscripcionCalificable(string estado) =>
        string.Equals(estado, "Activa", StringComparison.OrdinalIgnoreCase) ||
        string.Equals(estado, "Finalizada", StringComparison.OrdinalIgnoreCase);
}
