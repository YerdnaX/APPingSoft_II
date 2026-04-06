namespace APPingSoft_II.Models;

public class CriterioEvaluacion
{
    public int CriterioId { get; set; }
    public int EvaluacionId { get; set; }
    public string NombreCriterio { get; set; } = string.Empty;
    public decimal Ponderacion { get; set; }
    public decimal PuntosMaxCriterio { get; set; }

    public override string ToString() => NombreCriterio;
}
