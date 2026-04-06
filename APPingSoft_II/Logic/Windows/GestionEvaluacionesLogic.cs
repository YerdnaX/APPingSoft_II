using APPingSoft_II.Models;
using APPingSoft_II.Services;

namespace APPingSoft_II.Logic.Windows;

/// <summary>
/// Lógica de negocio para la ventana GestionEvaluaciones.
/// Gestiona tanto evaluaciones como sus criterios.
/// </summary>
public class GestionEvaluacionesLogic
{
    private readonly SistemaApp _sistema = SistemaApp.Instancia;
    private readonly EvaluacionService _service = new();

    // ── Evaluaciones ──────────────────────────────────────────────────────────

    public List<Evaluacion> ObtenerEvaluaciones()
    {
        _sistema.RecargarEvaluaciones();
        return _sistema.Evaluaciones;
    }

    public List<Curso> ObtenerCursos()
    {
        _sistema.RecargarCursos();
        return _sistema.Cursos;
    }

    public (bool ok, string mensaje) InsertarEvaluacion(Evaluacion e) =>
        _service.InsertarEvaluacion(e);

    public (bool ok, string mensaje) ActualizarEvaluacion(Evaluacion e) =>
        _service.ActualizarEvaluacion(e);

    public (bool ok, string mensaje) EliminarEvaluacion(int id) =>
        _service.EliminarEvaluacion(id);

    // ── Criterios ─────────────────────────────────────────────────────────────

    public List<CriterioEvaluacion> ObtenerCriterios(int evaluacionId)
    {
        _sistema.RecargarCriterios(evaluacionId);
        return _sistema.Criterios;
    }

    public (bool ok, string mensaje) InsertarCriterio(CriterioEvaluacion c) =>
        _service.InsertarCriterio(c);

    public (bool ok, string mensaje) ActualizarCriterio(CriterioEvaluacion c) =>
        _service.ActualizarCriterio(c);

    public (bool ok, string mensaje) EliminarCriterio(int criterioId) =>
        _service.EliminarCriterio(criterioId);
}
