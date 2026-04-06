namespace APPingSoft_II.Models;

public class ResultadoCriterio
{
    public int ResultadoCriterioId { get; set; }
    public int ResultadoId { get; set; }
    public int CriterioId { get; set; }
    public decimal PuntajeObtenido { get; set; }
    public string? Observacion { get; set; }
}
