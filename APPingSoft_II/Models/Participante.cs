namespace APPingSoft_II.Models;

public class Participante
{
    public int ParticipanteId { get; set; }
    public string NombreCompleto { get; set; } = string.Empty;
    public string CorreoElectronico { get; set; } = string.Empty;
    public string? Telefono { get; set; }
    public string Estado { get; set; } = "Activo";
    public DateTime FechaRegistro { get; set; } = DateTime.Now;

    public override string ToString() => NombreCompleto;
}
