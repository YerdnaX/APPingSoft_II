using APPingSoft_II.Logic;
using APPingSoft_II.Models;
using APPingSoft_II.Services;

namespace APPingSoft_II.Logic.Windows;

/// <summary>
/// Logica de negocio para la ventana RegistroResultados.
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

    public List<Programa> ObtenerProgramasPorEvaluacion(int evaluacionId) =>
        _service.ObtenerProgramasPorEvaluacion(evaluacionId);

    public List<Inscripcion> ObtenerInscripcionesPorEvaluacion(int evaluacionId, int? programaId = null) =>
        _service.ObtenerInscripcionesPorEvaluacion(evaluacionId, programaId);

    public List<ResultadoEvaluacion> ObtenerResultados()
    {
        _sistema.RecargarResultados();
        return _sistema.Resultados;
    }

    public (bool ok, string mensaje) Insertar(ResultadoEvaluacion r)
    {
        Permisos.Requerir(Permisos.PuedeRegistrarResultados(), "No tiene permisos para registrar resultados.");
        return _service.Insertar(r);
    }

    public (bool ok, string mensaje) Actualizar(ResultadoEvaluacion r)
    {
        Permisos.Requerir(Permisos.PuedeRegistrarResultados(), "No tiene permisos para modificar resultados.");
        return _service.Actualizar(r);
    }

    public (bool ok, string mensaje) Eliminar(int resultadoId)
    {
        Permisos.Requerir(Permisos.PuedeRegistrarResultados(), "No tiene permisos para eliminar resultados.");
        return _service.Eliminar(resultadoId);
    }
}
