namespace APPingSoft_II.Models.ViewModels;

/// <summary>
/// Mapea la vista vw_MetricasCurso de la base de datos.
/// </summary>
public class MetricaCurso
{
    public int CursoId { get; set; }
    public string Codigo { get; set; } = string.Empty;
    public string Curso { get; set; } = string.Empty;
    public int TotalEvaluaciones { get; set; }
    public int TotalResultados { get; set; }
    public decimal? PromedioNota { get; set; }
    public decimal? PorcentajeAprobacion { get; set; }
}
