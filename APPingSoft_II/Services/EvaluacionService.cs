using APPingSoft_II.Models;
using APPingSoft_II.Repositories;

namespace APPingSoft_II.Services;

public class EvaluacionService
{
    private readonly EvaluacionRepository _evalRepo = new();
    private readonly CriterioEvaluacionRepository _criterioRepo = new();

    // ── Evaluaciones ─────────────────────────────────────────────────────────

    public (bool ok, string mensaje) InsertarEvaluacion(Evaluacion e)
    {
        if (string.IsNullOrWhiteSpace(e.Titulo))
            return (false, "El título de la evaluación es obligatorio.");
        if (e.CursoId <= 0)
            return (false, "Debe seleccionar un curso.");
        if (string.IsNullOrWhiteSpace(e.Momento))
            return (false, "El momento de la evaluación es obligatorio.");
        if (e.PuntosMax <= 0)
            return (false, "Los puntos máximos deben ser mayores a cero.");
        if (e.FechaCierre < e.FechaApertura)
            return (false, "La fecha de cierre no puede ser anterior a la de apertura.");

        try
        {
            e.EvaluacionId = _evalRepo.Insertar(e);
            return (true, $"Evaluación '{e.Titulo}' registrada correctamente.");
        }
        catch (Microsoft.Data.SqlClient.SqlException ex) when (ex.Number == 2601 || ex.Number == 2627)
        {
            return (false, "Ya existe una evaluación con ese título, curso y fecha de apertura.");
        }
        catch (Exception ex)
        {
            return (false, $"Error al guardar evaluación: {ex.Message}");
        }
    }

    public (bool ok, string mensaje) ActualizarEvaluacion(Evaluacion e)
    {
        if (e.EvaluacionId <= 0)
            return (false, "Seleccione una evaluación de la tabla para modificar.");
        if (string.IsNullOrWhiteSpace(e.Titulo))
            return (false, "El título es obligatorio.");
        if (e.PuntosMax <= 0)
            return (false, "Los puntos máximos deben ser mayores a cero.");

        try
        {
            _evalRepo.Actualizar(e);
            return (true, $"Evaluación '{e.Titulo}' actualizada.");
        }
        catch (Exception ex)
        {
            return (false, $"Error al actualizar: {ex.Message}");
        }
    }

    public (bool ok, string mensaje) EliminarEvaluacion(int evaluacionId)
    {
        if (evaluacionId <= 0)
            return (false, "Seleccione una evaluación para eliminar.");
        try
        {
            _evalRepo.Eliminar(evaluacionId);
            return (true, "Evaluación cerrada/eliminada.");
        }
        catch (Exception ex)
        {
            return (false, $"Error al eliminar: {ex.Message}");
        }
    }

    // ── Criterios ─────────────────────────────────────────────────────────────

    public (bool ok, string mensaje) InsertarCriterio(CriterioEvaluacion c)
    {
        if (c.EvaluacionId <= 0)
            return (false, "Debe tener una evaluación seleccionada para agregar criterios.");
        if (string.IsNullOrWhiteSpace(c.NombreCriterio))
            return (false, "El nombre del criterio es obligatorio.");
        if (c.Ponderacion <= 0 || c.Ponderacion > 100)
            return (false, "La ponderación debe estar entre 1 y 100.");
        if (c.PuntosMaxCriterio <= 0)
            return (false, "Los puntos máximos del criterio deben ser mayores a cero.");

        // Verificar que la suma de ponderaciones no supere 100
        var totalActual = _criterioRepo.ObtenerPonderacionTotal(c.EvaluacionId);
        if (totalActual + c.Ponderacion > 100)
            return (false, $"La ponderación acumulada superaría el 100%. Disponible: {100 - totalActual}%.");

        try
        {
            c.CriterioId = _criterioRepo.Insertar(c);
            return (true, $"Criterio '{c.NombreCriterio}' agregado correctamente.");
        }
        catch (Exception ex)
        {
            return (false, $"Error al guardar criterio: {ex.Message}");
        }
    }

    public (bool ok, string mensaje) ActualizarCriterio(CriterioEvaluacion c)
    {
        if (c.CriterioId <= 0)
            return (false, "Seleccione un criterio para modificar.");
        if (string.IsNullOrWhiteSpace(c.NombreCriterio))
            return (false, "El nombre del criterio es obligatorio.");

        try
        {
            _criterioRepo.Actualizar(c);
            return (true, "Criterio actualizado.");
        }
        catch (Exception ex)
        {
            return (false, $"Error al actualizar criterio: {ex.Message}");
        }
    }

    public (bool ok, string mensaje) EliminarCriterio(int criterioId)
    {
        if (criterioId <= 0)
            return (false, "Seleccione un criterio para eliminar.");
        try
        {
            _criterioRepo.Eliminar(criterioId);
            return (true, "Criterio eliminado.");
        }
        catch (Exception ex)
        {
            return (false, $"Error al eliminar criterio: {ex.Message}");
        }
    }

    public List<Evaluacion> ObtenerTodasEvaluaciones() => _evalRepo.ObtenerTodas();
    public List<CriterioEvaluacion> ObtenerCriterios(int evaluacionId) => _criterioRepo.ObtenerPorEvaluacion(evaluacionId);
}
