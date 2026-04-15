using APPingSoft_II.Models;
using APPingSoft_II.Models.ViewModels;
using APPingSoft_II.Services;

namespace APPingSoft_II.Logic.Windows;

/// <summary>
/// Lógica de negocio para la ventana ReporteMetricas.
/// Soporta métricas globales y filtradas por programa/curso.
/// </summary>
public class ReportesLogic
{
    private readonly SistemaApp _sistema = SistemaApp.Instancia;
    private readonly ReporteService _service = new();

    // ── Datos para filtros ────────────────────────────────────────────────────

    public List<Programa> ObtenerProgramas()
    {
        _sistema.RecargarProgramas();
        return _sistema.Programas;
    }

    public List<Curso> ObtenerCursos()
    {
        _sistema.RecargarCursos();
        return _sistema.Cursos;
    }

    /// <summary>
    /// Retorna los cursos asociados al programa seleccionado.
    /// Como cada Programa tiene exactamente un CursoId, devuelve
    /// una lista con ese único curso (o vacía si el programa no existe).
    /// </summary>
    public List<Curso> ObtenerCursosPorPrograma(int programaId)
    {
        _sistema.RecargarProgramas();
        _sistema.RecargarCursos();

        var programa = _sistema.Programas.FirstOrDefault(p => p.ProgramaId == programaId);
        if (programa == null) return new List<Curso>();

        if (programa.Curso != null)
            return new List<Curso> { programa.Curso };

        return _sistema.Cursos
            .Where(c => c.CursoId == programa.CursoId)
            .ToList();
    }

    // ── Resultados detallados (ya soporta filtros) ────────────────────────────

    public (List<ResultadoDetallado> datos, string error) ObtenerResultadosDetallados(
        int? programaId = null, int? cursoId = null)
    {
        try
        {
            return (_service.ObtenerResultadosDetallados(programaId, cursoId), string.Empty);
        }
        catch (Exception ex)
        {
            return (new(), ex.Message);
        }
    }

    // ── Métricas por curso (con filtro opcional) ──────────────────────────────

    public (List<MetricaCurso> datos, string error) ObtenerMetricasCurso(int? cursoId = null)
    {
        try
        {
            return (_service.ObtenerMetricasCurso(cursoId), string.Empty);
        }
        catch (Exception ex)
        {
            return (new(), ex.Message);
        }
    }

    // ── Métricas por programa (con filtro opcional) ───────────────────────────

    public (List<MetricaPrograma> datos, string error) ObtenerMetricasPrograma(int? programaId = null)
    {
        try
        {
            return (_service.ObtenerMetricasPrograma(programaId), string.Empty);
        }
        catch (Exception ex)
        {
            return (new(), ex.Message);
        }
    }

    // ── KPIs ──────────────────────────────────────────────────────────────────

    public int ObtenerTotalParticipantes()
    {
        _sistema.RecargarParticipantes();
        return _sistema.Participantes.Count;
    }

    public int ObtenerTotalEvaluaciones()
    {
        _sistema.RecargarEvaluaciones();
        return _sistema.Evaluaciones.Count;
    }

    /// <summary>
    /// Retorna el total de participantes inscritos en un programa específico.
    /// Se obtiene de la MetricaPrograma (campo TotalInscritos).
    /// </summary>
    public int ObtenerInscritosPorPrograma(int programaId)
    {
        var (datos, _) = ObtenerMetricasPrograma(programaId);
        return datos.Sum(m => m.TotalInscritos);
    }

    /// <summary>
    /// Retorna el total de evaluaciones de un curso específico.
    /// Se obtiene de la MetricaCurso (campo TotalEvaluaciones).
    /// </summary>
    public int ObtenerEvaluacionesPorCurso(int cursoId)
    {
        var (datos, _) = ObtenerMetricasCurso(cursoId);
        return datos.Sum(m => m.TotalEvaluaciones);
    }
}
