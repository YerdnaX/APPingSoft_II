namespace APPingSoft_II.Models.ViewModels;

/// <summary>
/// Mapea la vista vw_MetricasPrograma de la base de datos.
/// </summary>
public class MetricaPrograma
{
    public int ProgramaId { get; set; }
    public string Programa { get; set; } = string.Empty;
    public string Estado { get; set; } = string.Empty;
    public int TotalInscritos { get; set; }
    public int ResultadosRegistrados { get; set; }
    public decimal? PromedioGeneral { get; set; }
}
