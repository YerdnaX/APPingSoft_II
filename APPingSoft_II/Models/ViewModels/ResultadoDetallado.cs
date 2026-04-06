namespace APPingSoft_II.Models.ViewModels;

/// <summary>
/// Mapea la vista vw_ResultadosDetallados de la base de datos.
/// </summary>
public class ResultadoDetallado
{
    public int ResultadoId { get; set; }
    public int ParticipanteId { get; set; }
    public string Participante { get; set; } = string.Empty;
    public string CorreoParticipante { get; set; } = string.Empty;
    public int ProgramaId { get; set; }
    public string Programa { get; set; } = string.Empty;
    public int CursoId { get; set; }
    public string CodigoCurso { get; set; } = string.Empty;
    public string Curso { get; set; } = string.Empty;
    public int EvaluacionId { get; set; }
    public string Evaluacion { get; set; } = string.Empty;
    public string TipoEvaluacion { get; set; } = string.Empty;
    public string Momento { get; set; } = string.Empty;
    public decimal PuntosMax { get; set; }
    public decimal NotaFinal { get; set; }
    public decimal? PorcentajeLogrado { get; set; }
    public DateTime CalificadoEn { get; set; }
    public string? Observaciones { get; set; }
}
