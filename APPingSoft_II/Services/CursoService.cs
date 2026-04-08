using APPingSoft_II.Models;
using APPingSoft_II.Repositories;

namespace APPingSoft_II.Services;

public class CursoService
{
    private readonly CursoRepository _repo = new();

    // ── Consultas ─────────────────────────────────────────────────────────────

    public List<Curso> ObtenerTodosParaGestion() => _repo.ObtenerTodosParaGestion();

    // ── CREATE ────────────────────────────────────────────────────────────────

    public (bool ok, string mensaje) Insertar(Curso c)
    {
        var v = Validar(c);
        if (!v.ok) return v;

        if (_repo.ExisteCodigo(c.Codigo))
            return (false, $"Ya existe un curso con el código '{c.Codigo}'.");

        try
        {
            c.CursoId = _repo.Insertar(c);
            return (true, $"Curso '{c.Nombre}' registrado correctamente.");
        }
        catch (Microsoft.Data.SqlClient.SqlException ex) when (ex.Number == 2601 || ex.Number == 2627)
        {
            return (false, $"Ya existe un curso con el código '{c.Codigo}'.");
        }
        catch (Exception ex)
        {
            return (false, $"Error al registrar curso: {ex.Message}");
        }
    }

    // ── UPDATE ────────────────────────────────────────────────────────────────

    public (bool ok, string mensaje) Actualizar(Curso c)
    {
        if (c.CursoId <= 0)
            return (false, "Seleccione un curso de la tabla para modificar.");

        var v = Validar(c);
        if (!v.ok) return v;

        if (_repo.ExisteCodigo(c.Codigo, c.CursoId))
            return (false, $"Otro curso ya usa el código '{c.Codigo}'.");

        try
        {
            _repo.Actualizar(c);
            return (true, $"Curso '{c.Nombre}' actualizado correctamente.");
        }
        catch (Exception ex)
        {
            return (false, $"Error al actualizar curso: {ex.Message}");
        }
    }

    // ── DELETE (lógico) ───────────────────────────────────────────────────────

    public (bool ok, string mensaje) Desactivar(int cursoId)
    {
        if (cursoId <= 0)
            return (false, "Seleccione un curso de la tabla para desactivar.");

        if (_repo.TieneProgramasAsociados(cursoId))
            return (false, "No se puede desactivar el curso porque tiene programas activos asociados. Finalice o desactive esos programas primero.");

        try
        {
            _repo.Eliminar(cursoId);
            return (true, "Curso desactivado correctamente.");
        }
        catch (Exception ex)
        {
            return (false, $"Error al desactivar curso: {ex.Message}");
        }
    }

    // ── Validaciones ──────────────────────────────────────────────────────────

    private static (bool ok, string mensaje) Validar(Curso c)
    {
        if (string.IsNullOrWhiteSpace(c.Codigo))
            return (false, "El código del curso es obligatorio.");
        if (c.Codigo.Length > 20)
            return (false, "El código no puede superar 20 caracteres.");
        if (string.IsNullOrWhiteSpace(c.Nombre))
            return (false, "El nombre del curso es obligatorio.");
        if (c.Nombre.Length > 120)
            return (false, "El nombre no puede superar 120 caracteres.");
        if (c.DuracionHoras <= 0)
            return (false, "La duración en horas debe ser mayor a cero.");
        if (c.Descripcion != null && c.Descripcion.Length > 500)
            return (false, "La descripción no puede superar 500 caracteres.");
        return (true, string.Empty);
    }
}
