namespace APPingSoft_II.Models;

public class Curso
{
    public int CursoId { get; set; }
    public string Codigo { get; set; } = string.Empty;
    public string Nombre { get; set; } = string.Empty;
    public string? Descripcion { get; set; }
    public int DuracionHoras { get; set; } = 40;
    public string Estado { get; set; } = "Activo";
    public DateTime FechaCreacion { get; set; } = DateTime.Now;

    public override string ToString() => $"{Codigo} - {Nombre}";
}
