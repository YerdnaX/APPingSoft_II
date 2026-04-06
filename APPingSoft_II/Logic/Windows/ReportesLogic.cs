using APPingSoft_II.Models;
using APPingSoft_II.Models.ViewModels;
using APPingSoft_II.Services;

namespace APPingSoft_II.Logic.Windows;

/// <summary>
/// Lógica de negocio para la ventana ReporteMetricas.
/// </summary>
public class ReportesLogic
{
    private readonly SistemaApp _sistema = SistemaApp.Instancia;
    private readonly ReporteService _service = new();

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

    public (List<MetricaCurso> datos, string error) ObtenerMetricasCurso()
    {
        try
        {
            return (_service.ObtenerMetricasCurso(), string.Empty);
        }
        catch (Exception ex)
        {
            return (new(), ex.Message);
        }
    }

    public (List<MetricaPrograma> datos, string error) ObtenerMetricasPrograma()
    {
        try
        {
            return (_service.ObtenerMetricasPrograma(), string.Empty);
        }
        catch (Exception ex)
        {
            return (new(), ex.Message);
        }
    }
}
