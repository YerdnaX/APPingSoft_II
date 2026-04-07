using APPingSoft_II.Logic;
using APPingSoft_II.Models;
using APPingSoft_II.Services;

namespace APPingSoft_II.Logic.Windows;

/// <summary>
/// Lógica de negocio para la ventana GestionProgramas.
/// </summary>
public class GestionProgramasLogic
{
    private readonly SistemaApp _sistema = SistemaApp.Instancia;
    private readonly ProgramaService _service = new();

    public List<Programa> ObtenerProgramas()
    {
        _sistema.RecargarProgramas();
        return _sistema.Programas;
    }

    public List<Instructor> ObtenerInstructores()
    {
        _sistema.RecargarInstructores();
        return _sistema.Instructores;
    }

    public List<Curso> ObtenerCursos()
    {
        _sistema.RecargarCursos();
        return _sistema.Cursos;
    }

    public (bool ok, string mensaje) Insertar(Programa p)
    {
        Permisos.Requerir(Permisos.PuedeGestionarProgramas(), "No tiene permisos para crear programas.");
        return _service.Insertar(p);
    }

    public (bool ok, string mensaje) Actualizar(Programa p)
    {
        Permisos.Requerir(Permisos.PuedeGestionarProgramas(), "No tiene permisos para modificar programas.");
        return _service.Actualizar(p);
    }

    public (bool ok, string mensaje) Eliminar(int programaId)
    {
        Permisos.Requerir(Permisos.PuedeGestionarProgramas(), "No tiene permisos para eliminar programas.");
        return _service.Eliminar(programaId);
    }
}
