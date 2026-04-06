using APPingSoft_II.Models.ViewModels;
using APPingSoft_II.Repositories;

namespace APPingSoft_II.Services;

public class ReporteService
{
    private readonly ReporteRepository _repo = new();

    public List<ResultadoDetallado> ObtenerResultadosDetallados(int? programaId = null, int? cursoId = null)
    {
        try
        {
            return _repo.ObtenerResultadosDetallados(programaId, cursoId);
        }
        catch (Exception ex)
        {
            throw new InvalidOperationException($"Error al obtener resultados detallados: {ex.Message}", ex);
        }
    }

    public List<MetricaCurso> ObtenerMetricasCurso()
    {
        try
        {
            return _repo.ObtenerMetricasCurso();
        }
        catch (Exception ex)
        {
            throw new InvalidOperationException($"Error al obtener métricas de cursos: {ex.Message}", ex);
        }
    }

    public List<MetricaPrograma> ObtenerMetricasPrograma()
    {
        try
        {
            return _repo.ObtenerMetricasPrograma();
        }
        catch (Exception ex)
        {
            throw new InvalidOperationException($"Error al obtener métricas de programas: {ex.Message}", ex);
        }
    }
}
