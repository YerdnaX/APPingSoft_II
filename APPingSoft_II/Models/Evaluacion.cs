namespace APPingSoft_II.Models;

public class Evaluacion
{
    public int EvaluacionId { get; set; }
    public int CursoId { get; set; }
    public string Titulo { get; set; } = string.Empty;
    public string Tipo { get; set; } = "Formativa";
    public string Momento { get; set; } = string.Empty;
    public DateTime FechaApertura { get; set; }
    public DateTime FechaCierre { get; set; }
    public decimal PuntosMax { get; set; }
    public string Estado { get; set; } = "Pendiente";
    public DateTime FechaCreacion { get; set; } = DateTime.Now;

    // Propiedad de navegación
    public Curso? Curso { get; set; }

    public override string ToString() => Titulo;
}
