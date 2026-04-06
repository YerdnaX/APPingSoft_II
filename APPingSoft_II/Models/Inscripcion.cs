namespace APPingSoft_II.Models;

public class Inscripcion
{
    public int InscripcionId { get; set; }
    public int ProgramaId { get; set; }
    public int ParticipanteId { get; set; }
    public DateTime FechaInscripcion { get; set; } = DateTime.Today;
    public string Estado { get; set; } = "Activa";

    // Propiedades de navegación
    public Participante? Participante { get; set; }
    public Programa? Programa { get; set; }

    public override string ToString() =>
        Participante != null ? Participante.NombreCompleto : $"Inscripcion #{InscripcionId}";
}
