using APPingSoft_II.Logic;
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

    // Listas en memoria para KPIs y gráficos
    private List<MetricaCurso>    _metricasCurso    = new();
    private List<MetricaPrograma> _metricasPrograma = new();

    // Suprimir SelectionChanged de combos durante carga
    private bool _cargandoFiltros = false;

    public ReporteMetricas()
    {
        InitializeComponent();
        Loaded += ReporteMetricas_Loaded;
    }

    private void ReporteMetricas_Loaded(object sender, RoutedEventArgs e)
    {
        AplicarPermisosPorRol();
        CargarFiltros();
        CargarTodosLosReportes();
    }

    // ── Permisos por rol ──────────────────────────────────────────────────────

    private void AplicarPermisosPorRol()
    {
        navGestionProgramas.Visibility    = Permisos.VisibleSi(Permisos.PuedeAccederGestionProgramas());
        navParticipantes.Visibility       = Permisos.VisibleSi(Permisos.PuedeAccederGestionParticipantes());
        navGestionEvaluaciones.Visibility = Permisos.VisibleSi(Permisos.PuedeAccederGestionEvaluaciones());

        // La pestaña de métricas por programa es solo para Admin y Coordinador
        tabMetricasPrograma.Visibility = Permisos.VisibleSi(Permisos.PuedeVerMetricasPrograma());
        pnlGraficoPrograma.Visibility  = Permisos.VisibleSi(Permisos.PuedeVerMetricasPrograma());
    }

    // ── Carga inicial ─────────────────────────────────────────────────────────

    private void CargarFiltros()
    {
        _cargandoFiltros = true;
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
        finally
        {
            _cargandoFiltros = false;
        }
    }

    private void CargarTodosLosReportes()
    {
        ActualizarEtiquetaFiltro(null, null);
        CargarResultadosDetallados(null, null);
        CargarMetricasCurso(null);
        if (Permisos.PuedeVerMetricasPrograma())
            CargarMetricasPrograma(null);
        ActualizarKPIs(null, null);
    }

    // ── SelectionChanged de los combos (dashboard dinámico) ───────────────────

    private void CmbFiltroPrograma_SelectionChanged(object sender, SelectionChangedEventArgs e)
    {
        if (_cargandoFiltros) return;

        var programa = cmbFiltroPrograma.SelectedItem as Programa;
        int? programaId = (programa?.ProgramaId > 0) ? programa.ProgramaId : null;

        // Parte 2: Filtrar combo de cursos según el programa seleccionado
        ActualizarComboCursos(programaId);

        // Parte 1: Recargar todo el dashboard con el filtro de programa
        int? cursoId = ObtenerCursoIdSeleccionado();
        ActualizarDashboard(programaId, cursoId);
    }

    private void CmbFiltroCurso_SelectionChanged(object sender, SelectionChangedEventArgs e)
    {
        if (_cargandoFiltros) return;

        int? programaId = ObtenerProgramaIdSeleccionado();
        int? cursoId    = ObtenerCursoIdSeleccionado();
        ActualizarDashboard(programaId, cursoId);
    }

    // ── Parte 2: Actualizar combo de cursos según programa ────────────────────

    private void ActualizarComboCursos(int? programaId)
    {
        _cargandoFiltros = true;
        try
        {
            List<Curso> cursos;
            if (programaId.HasValue)
            {
                cursos = _logic.ObtenerCursosPorPrograma(programaId.Value);
            }
            else
            {
                cursos = _logic.ObtenerCursos();
            }
            cursos.Insert(0, new Curso { CursoId = 0, Nombre = "— Todos —" });
            cmbFiltroCurso.ItemsSource = cursos;
            cmbFiltroCurso.SelectedIndex = 0;
        }
        finally
        {
            _cargandoFiltros = false;
        }
    }

    // ── Actualización central del dashboard ───────────────────────────────────

    private void ActualizarDashboard(int? programaId, int? cursoId)
    {
        ActualizarEtiquetaFiltro(programaId, cursoId);
        CargarResultadosDetallados(programaId, cursoId);
        CargarMetricasCurso(cursoId);
        if (Permisos.PuedeVerMetricasPrograma())
            CargarMetricasPrograma(programaId);
        ActualizarKPIs(programaId, cursoId);
    }

    // ── Carga de cada sección ─────────────────────────────────────────────────

    private void CargarResultadosDetallados(int? programaId, int? cursoId)
    {
        try
        {
            var (datos, error) = _logic.ObtenerResultadosDetallados(programaId, cursoId);
            if (!string.IsNullOrEmpty(error))
            {
                MessageBox.Show($"Error: {error}", "Error", MessageBoxButton.OK, MessageBoxImage.Warning);
                return;
            }
            dgResultadosDetallados.ItemsSource = datos;
        }
        catch (Exception ex)
        {
            MessageBox.Show($"Error al cargar resultados:\n{ex.Message}", "Error",
                MessageBoxButton.OK, MessageBoxImage.Warning);
        }
    }

    private void CargarMetricasCurso(int? cursoId)
    {
        try
        {
            var (datos, error) = _logic.ObtenerMetricasCurso(cursoId);
            if (!string.IsNullOrEmpty(error)) return;
            _metricasCurso = datos;
            dgMetricasCurso.ItemsSource = datos;
            ActualizarGraficoCursos();
        }
        catch { }
    }

    private void CargarMetricasPrograma(int? programaId)
    {
        try
        {
            var (datos, error) = _logic.ObtenerMetricasPrograma(programaId);
            if (!string.IsNullOrEmpty(error)) return;
            _metricasPrograma = datos;
            dgMetricasPrograma.ItemsSource = datos;
            ActualizarGraficoPrograma();
        }
        catch { }
    }

    // ── KPIs dinámicos ────────────────────────────────────────────────────────

    private void ActualizarKPIs(int? programaId, int? cursoId)
    {
        try
        {
            // Participantes: inscritos en el programa o total
            if (programaId.HasValue)
            {
                int inscritos = _logic.ObtenerInscritosPorPrograma(programaId.Value);
                kpiParticipantes.Text = inscritos.ToString();
                lblKpiParticipantes.Text = "Inscritos en Programa";
            }
            else
            {
                kpiParticipantes.Text = _logic.ObtenerTotalParticipantes().ToString();
                lblKpiParticipantes.Text = "Participantes";
            }

            // Evaluaciones: del curso seleccionado o total
            if (cursoId.HasValue)
            {
                int evals = _logic.ObtenerEvaluacionesPorCurso(cursoId.Value);
                kpiEvaluaciones.Text = evals.ToString();
                lblKpiEvaluaciones.Text = "Evaluaciones del Curso";
            }
            else
            {
                kpiEvaluaciones.Text = _logic.ObtenerTotalEvaluaciones().ToString();
                lblKpiEvaluaciones.Text = "Evaluaciones";
            }

            // Promedio: de los datos actuales en memoria
            var promGeneral = _metricasPrograma
                .Where(m => m.PromedioGeneral.HasValue)
                .Select(m => (double)m.PromedioGeneral!.Value)
                .DefaultIfEmpty(0)
                .Average();

            // Si no hay datos de programa (Instructor), usar métricas de curso
            if (promGeneral == 0)
            {
                promGeneral = _metricasCurso
                    .Where(m => m.PromedioNota.HasValue)
                    .Select(m => (double)m.PromedioNota!.Value)
                    .DefaultIfEmpty(0)
                    .Average();
            }
            kpiPromedio.Text = $"{promGeneral:N1}";

            // % Aprobación: de métricas de curso
            var pctAprobacion = _metricasCurso
                .Where(m => m.PorcentajeAprobacion.HasValue)
                .Select(m => (double)m.PorcentajeAprobacion!.Value)
                .DefaultIfEmpty(0)
                .Average();
            kpiAprobacion.Text = $"{pctAprobacion:N1}%";
        }
        catch { /* KPIs no bloquean la pantalla */ }
    }

    // ── Etiqueta de filtro activo ─────────────────────────────────────────────

    private void ActualizarEtiquetaFiltro(int? programaId, int? cursoId)
    {
        if (!programaId.HasValue && !cursoId.HasValue)
        {
            pnlFiltroActivo.Visibility = Visibility.Collapsed;
            return;
        }

        var partes = new List<string>();
        if (programaId.HasValue)
        {
            var prog = (cmbFiltroPrograma.SelectedItem as Programa)?.Nombre ?? "";
            partes.Add($"Programa: {prog}");
        }
        if (cursoId.HasValue)
        {
            var curso = (cmbFiltroCurso.SelectedItem as Curso)?.Nombre ?? "";
            partes.Add($"Curso: {curso}");
        }

        lblFiltroActivo.Text = string.Join(" | ", partes);
        pnlFiltroActivo.Visibility = Visibility.Visible;
    }

    // ── Gráficos ──────────────────────────────────────────────────────────────

    private static readonly Brush BrushPrimary = new SolidColorBrush((Color)ColorConverter.ConvertFromString("#3D4A6B"));
    private static readonly Brush BrushSecond  = new SolidColorBrush((Color)ColorConverter.ConvertFromString("#8B9DC3"));
    private static readonly Brush BrushGreen   = new SolidColorBrush((Color)ColorConverter.ConvertFromString("#1E8449"));
    private static readonly Brush BrushGray    = new SolidColorBrush((Color)ColorConverter.ConvertFromString("#AAAAAA"));

    private void ActualizarGraficoCursos()
    {
        if (!_metricasCurso.Any())
        {
            icChartCurso.Visibility    = Visibility.Collapsed;
            lblSinDatoCurso.Visibility = Visibility.Visible;
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
                BarHeight = Math.Max(4, val / maxVal * maxBarHeight),
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
                "Activo"     => BrushGreen,
                "Finalizado" => BrushSecond,
                _            => BrushGray
            };
            return new BarChartItem
            {
                Label     = m.Programa.Length > 16 ? m.Programa[..16] + "…" : m.Programa,
                ValueText = $"{val:N1}",
                BarHeight = Math.Max(4, val / maxVal * maxBarHeight),
                Fill      = fill
            };
        }).ToList();
    }

    // ── Helpers para leer filtros activos ─────────────────────────────────────

    private int? ObtenerProgramaIdSeleccionado()
    {
        var p = cmbFiltroPrograma.SelectedItem as Programa;
        return (p?.ProgramaId > 0) ? p.ProgramaId : null;
    }

    private int? ObtenerCursoIdSeleccionado()
    {
        var c = cmbFiltroCurso.SelectedItem as Curso;
        return (c?.CursoId > 0) ? c.CursoId : null;
    }

    // ── Botones de filtro (Filtrar y Limpiar) ─────────────────────────────────

    private void BtnFiltrar_Click(object sender, RoutedEventArgs e)
    {
        int? programaId = ObtenerProgramaIdSeleccionado();
        int? cursoId    = ObtenerCursoIdSeleccionado();
        ActualizarDashboard(programaId, cursoId);
    }

    private void BtnLimpiarFiltros_Click(object sender, RoutedEventArgs e)
    {
        _cargandoFiltros = true;
        cmbFiltroPrograma.SelectedIndex = 0;

        // Restaurar combo de cursos completo
        var cursos = _logic.ObtenerCursos();
        cursos.Insert(0, new Curso { CursoId = 0, Nombre = "— Todos —" });
        cmbFiltroCurso.ItemsSource = cursos;
        cmbFiltroCurso.SelectedIndex = 0;
        _cargandoFiltros = false;

        CargarTodosLosReportes();
    }

    // ── Navegación ────────────────────────────────────────────────────────────

    private void IrHome(object sender, MouseButtonEventArgs e)                { new Home().Show(); this.Close(); }
    private void IrGestionProgramas(object sender, MouseButtonEventArgs e)    { new GestionProgramas().Show(); this.Close(); }
    private void IrGestionEvaluaciones(object sender, MouseButtonEventArgs e) { new GestionEvaluaciones().Show(); this.Close(); }
    private void IrRegistrarResultados(object sender, MouseButtonEventArgs e) { new RegistroResultados().Show(); this.Close(); }
    private void IrReportes(object sender, MouseButtonEventArgs e)            { new ReporteMetricas().Show(); this.Close(); }
    private void IrParticipantes(object sender, MouseButtonEventArgs e)       { new GestionParticipantes().Show(); this.Close(); }
}
