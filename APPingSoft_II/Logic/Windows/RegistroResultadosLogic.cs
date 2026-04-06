using APPingSoft_II.Models;
using APPingSoft_II.Services;

namespace APPingSoft_II.Logic.Windows;

/// <summary>
/// Lógica de negocio para la ventana RegistroResultados.
/// </summary>
public class RegistroResultadosLogic
{
    private readonly SistemaApp _sistema = SistemaApp.Instancia;
    private readonly ResultadoService _service = new();

    public List<Evaluacion> ObtenerEvaluaciones()
    {
        _sistema.RecargarEvaluaciones();
        return _sistema.Evaluaciones;
    }

    /// <summary>
    /// Retorna inscripciones activas de participantes en el curso de la evaluación seleccionada.
    /// Se usa para poblar el combo de participantes.
    /// </summary>
    public List<Inscripcion> ObtenerInscripcionesPorEvaluacion(int evaluacionId) =>
        _service.ObtenerInscripcionesPorEvaluacion(evaluacionId);

    public List<ResultadoEvaluacion> ObtenerResultados()
    {
        _sistema.RecargarResultados();
        return _sistema.Resultados;
    }

    public (bool ok, string mensaje) Insertar(ResultadoEvaluacion r) => _service.Insertar(r);

    public (bool ok, string mensaje) Actualizar(ResultadoEvaluacion r) => _service.Actualizar(r);

    public (bool ok, string mensaje) Eliminar(int resultadoId) => _service.Eliminar(resultadoId);
}
