using APPingSoft_II.Logic;
using APPingSoft_II.Models;
using APPingSoft_II.Services;

namespace APPingSoft_II.Logic.Windows;

/// <summary>
/// Lógica de negocio para la ventana GestionCursos.
/// </summary>
public class GestionCursosLogic
{
    private readonly SistemaApp _sistema = SistemaApp.Instancia;
    private readonly CursoService _service = new();

    public List<Curso> ObtenerTodos()
    {
        var lista = _service.ObtenerTodosParaGestion();
        _sistema.RecargarCursos();   // mantiene la lista activa sincronizada
        return lista;
    }

    public (bool ok, string mensaje) Insertar(Curso c)
    {
        Permisos.Requerir(Permisos.PuedeGestionarCursos(), "No tiene permisos para registrar cursos.");
        var resultado = _service.Insertar(c);
        if (resultado.ok) _sistema.RecargarCursos();
        return resultado;
    }

    public (bool ok, string mensaje) Actualizar(Curso c)
    {
        Permisos.Requerir(Permisos.PuedeGestionarCursos(), "No tiene permisos para modificar cursos.");
        var resultado = _service.Actualizar(c);
        if (resultado.ok) _sistema.RecargarCursos();
        return resultado;
    }

    public (bool ok, string mensaje) Desactivar(int cursoId)
    {
        Permisos.Requerir(Permisos.PuedeGestionarCursos(), "No tiene permisos para desactivar cursos.");
        var resultado = _service.Desactivar(cursoId);
        if (resultado.ok) _sistema.RecargarCursos();
        return resultado;
    }
}
