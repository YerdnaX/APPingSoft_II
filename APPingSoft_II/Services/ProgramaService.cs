using APPingSoft_II.Models;
using APPingSoft_II.Repositories;

namespace APPingSoft_II.Services;

public class ProgramaService
{
    private readonly ProgramaRepository _repo = new();

    public (bool ok, string mensaje) Insertar(Programa p)
    {
        if (string.IsNullOrWhiteSpace(p.Nombre))
            return (false, "El nombre del programa es obligatorio.");
        if (p.FechaFin < p.FechaInicio)
            return (false, "La fecha de fin no puede ser anterior a la fecha de inicio.");
        if (p.InstructorId <= 0)
            return (false, "Debe seleccionar un instructor.");
        if (p.CursoId <= 0)
            return (false, "Debe seleccionar un curso.");

        try
        {
            p.ProgramaId = _repo.Insertar(p);
            return (true, $"Programa '{p.Nombre}' registrado correctamente.");
        }
        catch (Microsoft.Data.SqlClient.SqlException ex) when (ex.Number == 2601 || ex.Number == 2627)
        {
            return (false, "Ya existe un programa con ese nombre y fecha de inicio.");
        }
        catch (Exception ex)
        {
            return (false, $"Error al guardar: {ex.Message}");
        }
    }

    public (bool ok, string mensaje) Actualizar(Programa p)
    {
        if (string.IsNullOrWhiteSpace(p.Nombre))
            return (false, "El nombre del programa es obligatorio.");
        if (p.FechaFin < p.FechaInicio)
            return (false, "La fecha de fin no puede ser anterior a la fecha de inicio.");
        if (p.ProgramaId <= 0)
            return (false, "Seleccione un programa de la tabla para modificar.");

        try
        {
            _repo.Actualizar(p);
            return (true, $"Programa '{p.Nombre}' actualizado correctamente.");
        }
        catch (Exception ex)
        {
            return (false, $"Error al actualizar: {ex.Message}");
        }
    }

    public (bool ok, string mensaje) Eliminar(int programaId)
    {
        if (programaId <= 0)
            return (false, "Seleccione un programa de la tabla para eliminar.");
        try
        {
            _repo.Eliminar(programaId);
            return (true, "Programa desactivado correctamente.");
        }
        catch (Exception ex)
        {
            return (false, $"Error al eliminar: {ex.Message}");
        }
    }

    public List<Programa> ObtenerTodos() => _repo.ObtenerTodos();
}
