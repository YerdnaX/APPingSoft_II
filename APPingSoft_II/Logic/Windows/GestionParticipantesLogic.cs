using APPingSoft_II.Logic;
using APPingSoft_II.Models;
using APPingSoft_II.Services;

namespace APPingSoft_II.Logic.Windows;

/// <summary>
/// Lógica de negocio para la ventana GestionParticipantes.
/// </summary>
public class GestionParticipantesLogic
{
    private readonly SistemaApp _sistema = SistemaApp.Instancia;
    private readonly ParticipanteService _service = new();

    public List<Participante> ObtenerTodos()
    {
        _sistema.RecargarParticipantes();
        return _sistema.Participantes;
    }

    public List<Participante> Buscar(string termino) =>
        _service.Buscar(termino);

    public (bool ok, string mensaje) Insertar(Participante p)
    {
        Permisos.Requerir(Permisos.PuedeGestionarParticipantes(), "No tiene permisos para registrar participantes.");
        var resultado = _service.Insertar(p);
        if (resultado.ok) _sistema.RecargarParticipantes();
        return resultado;
    }

    public (bool ok, string mensaje) Actualizar(Participante p)
    {
        Permisos.Requerir(Permisos.PuedeGestionarParticipantes(), "No tiene permisos para modificar participantes.");
        var resultado = _service.Actualizar(p);
        if (resultado.ok) _sistema.RecargarParticipantes();
        return resultado;
    }

    public (bool ok, string mensaje) Desactivar(int participanteId)
    {
        Permisos.Requerir(Permisos.PuedeGestionarParticipantes(), "No tiene permisos para desactivar participantes.");
        var resultado = _service.Desactivar(participanteId);
        if (resultado.ok) _sistema.RecargarParticipantes();
        return resultado;
    }

    public bool TieneInscripciones(int participanteId) =>
        _service.TieneInscripciones(participanteId);
}
