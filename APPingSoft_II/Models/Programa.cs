namespace APPingSoft_II.Models;

public class Programa
{
    public int ProgramaId { get; set; }
    public string Nombre { get; set; } = string.Empty;
    public DateTime FechaInicio { get; set; }
    public DateTime FechaFin { get; set; }
    public string Estado { get; set; } = "Activo";
    public int InstructorId { get; set; }
    public int CursoId { get; set; }
    public DateTime FechaCreacion { get; set; } = DateTime.Now;

    // Propiedades de navegación (cargadas por el repositorio)
    public Instructor? Instructor { get; set; }
    public Curso? Curso { get; set; }

    public override string ToString() => Nombre;
}
