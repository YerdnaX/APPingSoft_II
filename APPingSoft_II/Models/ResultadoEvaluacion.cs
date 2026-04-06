namespace APPingSoft_II.Models;

public class ResultadoEvaluacion
{
    public int ResultadoId { get; set; }
    public int EvaluacionId { get; set; }
    public int InscripcionId { get; set; }
    public decimal NotaFinal { get; set; }
    public DateTime CalificadoEn { get; set; } = DateTime.Now;
    public string? Observaciones { get; set; }

    // Propiedades de navegación
    public Evaluacion? Evaluacion { get; set; }
    public Inscripcion? Inscripcion { get; set; }
}
