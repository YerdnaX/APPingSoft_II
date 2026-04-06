namespace APPingSoft_II.Models;

public class Instructor
{
    public int InstructorId { get; set; }
    public int? UsuarioId { get; set; }
    public string NombreCompleto { get; set; } = string.Empty;
    public string CorreoElectronico { get; set; } = string.Empty;
    public string? Especialidad { get; set; }
    public string Estado { get; set; } = "Activo";
    public DateTime FechaRegistro { get; set; } = DateTime.Now;

    public override string ToString() => NombreCompleto;
}
