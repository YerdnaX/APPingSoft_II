using APPingSoft_II.Logic.Windows;
using APPingSoft_II.Models;
using APPingSoft_II.Models.ViewModels;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Input;
using System.Windows.Media;

namespace APPingSoft_II.Sistema;

public partial class ReporteMetricas : Window
{
    private readonly ReportesLogic _logic = new();

    private List<MetricaCurso>    _metricasCurso    = new();
    private List<MetricaPrograma> _metricasPrograma = new();

    public ReporteMetricas()
    {
        InitializeComponent();
        Loaded += ReporteMetricas_Loaded;
    }

    private void ReporteMetricas_Loaded(object sender, RoutedEventArgs e)
    {
        CargarFiltros();
        CargarTodosLosReportes();
    }

    // ── Carga ─────────────────────────────────────────────────────────────────

    private void CargarFiltros()
    {
        try
        {
            var programas = _logic.ObtenerProgramas();
            programas.Insert(0, new Programa { ProgramaId = 0, Nombre = "— Todos —" });
            cmbFiltroPrograma.ItemsSource = programas;
            cmbFiltroPrograma.SelectedIndex = 0;

            var cursos = _logic.ObtenerCursos();
            cursos.Insert(0, new Curso { CursoId = 0, Nombre = "— Todos —" });
            cmbFiltroCurso.ItemsSource = cursos;
            cmbFiltroCurso.SelectedIndex = 0;
        }
        catch (Exception ex)
        {
            MessageBox.Show($"Error al cargar filtros:\n{ex.Message}", "Error",
                MessageBoxButton.OK, MessageBoxImage.Warning);
        }
    }

    private void CargarTodosLosReportes()
    {
        CargarResultadosDetallados(null, null);
        CargarMetricasCurso();
        CargarMetricasPrograma();
        ActualizarKPIs();
    }

    private void CargarResultadosDetallados(int? programaId, int? cursoId)
    {
        try
        {
            var (datos, error) = _logic.ObtenerResultadosDetallados(
                programaId == 0 ? null : programaId,
                cursoId == 0 ? null : cursoId);

            if (!string.IsNullOrEmpty(error))
            {
                MessageBox.Show($"Error: {error}", "Error", MessageBoxButton.OK, MessageBoxImage.Warning);
                return;
            }
            dgResultadosDetallados.ItemsSource = datos;
        }
        catch (Exception ex)
        {
            MessageBox.Show($"Error al cargar resultados detallados:\n{ex.Message}", "Error",
                MessageBoxButton.OK, MessageBoxImage.Warning);
        }
    }

    private void CargarMetricasCurso()
    {
        try
        {
            var (datos, error) = _logic.ObtenerMetricasCurso();
            if (!string.IsNullOrEmpty(error)) return;
            _metricasCurso = datos;
            dgMetricasCurso.ItemsSource = datos;
            ActualizarGraficoCursos();
        }
        catch { }
    }

    private void CargarMetricasPrograma()
    {
        try
        {
            var (datos, error) = _logic.ObtenerMetricasPrograma();
            if (!string.IsNullOrEmpty(error)) return;
            _metricasPrograma = datos;
            dgMetricasPrograma.ItemsSource = datos;
            ActualizarGraficoPrograma();
        }
        catch { }
    }

    // ── KPI ───────────────────────────────────────────────────────────────────

    private void ActualizarKPIs()
    {
        try
        {
            kpiParticipantes.Text = _logic.ObtenerTotalParticipantes().ToString();
            kpiEvaluaciones.Text  = _logic.ObtenerTotalEvaluaciones().ToString();

            var promGeneral = _metricasPrograma
                .Where(m => m.PromedioGeneral.HasValue)
                .Select(m => m.PromedioGeneral!.Value)
                .DefaultIfEmpty(0)
                .Average();
            kpiPromedio.Text = $"{promGeneral:N1}";

            var pctAprobacion = _metricasCurso
                .Where(m => m.PorcentajeAprobacion.HasValue)
                .Select(m => m.PorcentajeAprobacion!.Value)
                .DefaultIfEmpty(0)
                .Average();
            kpiAprobacion.Text = $"{pctAprobacion:N1}%";
        }
        catch { /* KPIs son opcionales */ }
    }

    // ── Gráficos ──────────────────────────────────────────────────────────────

    private static readonly Brush BrushPrimary  = new SolidColorBrush((Color)ColorConverter.ConvertFromString("#3D4A6B"));
    private static readonly Brush BrushSecond   = new SolidColorBrush((Color)ColorConverter.ConvertFromString("#8B9DC3"));
    private static readonly Brush BrushGreen    = new SolidColorBrush((Color)ColorConverter.ConvertFromString("#1E8449"));
    private static readonly Brush BrushGray     = new SolidColorBrush((Color)ColorConverter.ConvertFromString("#AAAAAA"));

    private void ActualizarGraficoCursos()
    {
        if (!_metricasCurso.Any())
        {
            icChartCurso.Visibility     = Visibility.Collapsed;
            lblSinDatoCurso.Visibility  = Visibility.Visible;
            return;
        }

        lblSinDatoCurso.Visibility = Visibility.Collapsed;
        icChartCurso.Visibility    = Visibility.Visible;

        const double maxBarHeight = 120;
        var maxVal = (double)(_metricasCurso.Max(m => m.PromedioNota) ?? 1);
        if (maxVal == 0) maxVal = 1;

        icChartCurso.ItemsSource = _metricasCurso.Select(m =>
        {
            var val = (double)(m.PromedioNota ?? 0);
            return new BarChartItem
            {
                Label     = m.Curso.Length > 14 ? m.Curso[..14] + "…" : m.Curso,
                ValueText = $"{val:N1}",
                BarHeight = val / maxVal * maxBarHeight,
                Fill      = val >= maxVal * 0.7 ? BrushGreen
                          : val >= maxVal * 0.4 ? BrushPrimary
                          : BrushSecond
            };
        }).ToList();
    }

    private void ActualizarGraficoPrograma()
    {
        if (!_metricasPrograma.Any())
        {
            icChartPrograma.Visibility    = Visibility.Collapsed;
            lblSinDatoPrograma.Visibility = Visibility.Visible;
            return;
        }

        lblSinDatoPrograma.Visibility = Visibility.Collapsed;
        icChartPrograma.Visibility    = Visibility.Visible;

        const double maxBarHeight = 120;
        var maxVal = (double)(_metricasPrograma.Max(m => m.PromedioGeneral) ?? 1);
        if (maxVal == 0) maxVal = 1;

        icChartPrograma.ItemsSource = _metricasPrograma.Select(m =>
        {
            var val = (double)(m.PromedioGeneral ?? 0);
            Brush fill = m.Estado switch
            {
                "Activo"    => BrushGreen,
                "Finalizado"=> BrushSecond,
                _           => BrushGray
            };
            return new BarChartItem
            {
                Label     = m.Programa.Length > 16 ? m.Programa[..16] + "…" : m.Programa,
                ValueText = $"{val:N1}",
                BarHeight = val / maxVal * maxBarHeight,
                Fill      = fill
            };
        }).ToList();
    }

    // ── Filtros ───────────────────────────────────────────────────────────────

    private void BtnFiltrar_Click(object sender, RoutedEventArgs e)
    {
        int? programaId = (cmbFiltroPrograma.SelectedItem as Programa)?.ProgramaId;
        int? cursoId    = (cmbFiltroCurso.SelectedItem    as Curso)?.CursoId;
        CargarResultadosDetallados(programaId, cursoId);
    }

    private void BtnLimpiarFiltros_Click(object sender, RoutedEventArgs e)
    {
        cmbFiltroPrograma.SelectedIndex = 0;
        cmbFiltroCurso.SelectedIndex    = 0;
        CargarTodosLosReportes();
    }

    // ── Navegación ────────────────────────────────────────────────────────────

    private void IrHome(object sender, MouseButtonEventArgs e)               { new Home().Show(); this.Close(); }
    private void IrGestionProgramas(object sender, MouseButtonEventArgs e)   { new GestionProgramas().Show(); this.Close(); }
    private void IrGestionEvaluaciones(object sender, MouseButtonEventArgs e){ new GestionEvaluaciones().Show(); this.Close(); }
    private void IrRegistrarResultados(object sender, MouseButtonEventArgs e){ new RegistroResultados().Show(); this.Close(); }
    private void IrReportes(object sender, MouseButtonEventArgs e)           { new ReporteMetricas().Show(); this.Close(); }
    private void IrParticipantes(object sender, MouseButtonEventArgs e)      { new GestionParticipantes().Show(); this.Close(); }
}
