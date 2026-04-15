using APPingSoft_II.Logic;
using APPingSoft_II.Models;
using APPingSoft_II.Services;

namespace APPingSoft_II.Logic.Windows;

/// <summary>
/// Lógica de negocio para la ventana GestionInscripciones.
/// </summary>
public class GestionInscripcionesLogic
{
    private readonly SistemaApp _sistema = SistemaApp.Instancia;
    private readonly InscripcionService _service = new();

    // ── Consultas de datos maestros ───────────────────────────────────────────

    public List<Inscripcion> ObtenerTodas()
    {
        var lista = _service.ObtenerTodasParaGestion();
        _sistema.RecargarInscripciones();   // mantiene la lista activa sincronizada
        return lista;
    }

    public List<Participante> ObtenerParticipantes()
    {
        _sistema.RecargarParticipantes();
        return _sistema.Participantes.Where(p => p.Estado == "Activo").ToList();
    }

    public List<Programa> ObtenerProgramas()
    {
        _sistema.RecargarProgramas();
        return _sistema.Programas.Where(p => p.Estado == "Activo").ToList();
    }

    /// <summary>
    /// Devuelve el curso asociado al programa indicado.
    /// Cada programa tiene exactamente un curso; retorna lista de un elemento (o vacía).
    /// </summary>
    public List<Curso> ObtenerCursosPorPrograma(int programaId)
    {
        var programa = _sistema.Programas.FirstOrDefault(p => p.ProgramaId == programaId);
        if (programa == null) return new List<Curso>();

        if (programa.Curso != null)
            return new List<Curso> { programa.Curso };

        var curso = _sistema.Cursos.FirstOrDefault(c => c.CursoId == programa.CursoId);
        return curso != null ? new List<Curso> { curso } : new List<Curso>();
    }

    // ── CRUD ──────────────────────────────────────────────────────────────────

    public (bool ok, string mensaje) Registrar(Inscripcion ins)
    {
        Permisos.Requerir(Permisos.PuedeGestionarInscripciones(),
            "No tiene permisos para registrar inscripciones.");
        var resultado = _service.Registrar(ins);
        if (resultado.ok) _sistema.RecargarInscripciones();
        return resultado;
    }

    public (bool ok, string mensaje) Actualizar(int inscripcionId, DateTime fecha, string estado)
    {
        Permisos.Requerir(Permisos.PuedeGestionarInscripciones(),
            "No tiene permisos para modificar inscripciones.");
        var resultado = _service.Actualizar(inscripcionId, fecha, estado);
        if (resultado.ok) _sistema.RecargarInscripciones();
        return resultado;
    }

    public (bool ok, string mensaje) Desactivar(int inscripcionId)
    {
        Permisos.Requerir(Permisos.PuedeGestionarInscripciones(),
            "No tiene permisos para retirar inscripciones.");
        var resultado = _service.Desactivar(inscripcionId);
        if (resultado.ok) _sistema.RecargarInscripciones();
        return resultado;
    }
}
