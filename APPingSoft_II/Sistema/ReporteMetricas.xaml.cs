using APPingSoft_II.Logic.Windows;
using APPingSoft_II.Models;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Input;

namespace APPingSoft_II.Sistema;

public partial class ReporteMetricas : Window
{
    private readonly ReportesLogic _logic = new();

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
            dgMetricasCurso.ItemsSource = datos;
        }
        catch { /* Las métricas son opcionales, no mostrar error si falla */ }
    }

    private void CargarMetricasPrograma()
    {
        try
        {
            var (datos, error) = _logic.ObtenerMetricasPrograma();
            if (!string.IsNullOrEmpty(error)) return;
            dgMetricasPrograma.ItemsSource = datos;
        }
        catch { /* Las métricas son opcionales */ }
    }

    // ── Filtros ───────────────────────────────────────────────────────────────

    private void BtnFiltrar_Click(object sender, RoutedEventArgs e)
    {
        int? programaId = (cmbFiltroPrograma.SelectedItem as Programa)?.ProgramaId;
        int? cursoId = (cmbFiltroCurso.SelectedItem as Curso)?.CursoId;
        CargarResultadosDetallados(programaId, cursoId);
    }

    private void BtnLimpiarFiltros_Click(object sender, RoutedEventArgs e)
    {
        cmbFiltroPrograma.SelectedIndex = 0;
        cmbFiltroCurso.SelectedIndex = 0;
        CargarTodosLosReportes();
    }

    // ── Navegación ────────────────────────────────────────────────────────────

    private void IrHome(object sender, MouseButtonEventArgs e) { new Home().Show(); this.Close(); }
    private void IrGestionProgramas(object sender, MouseButtonEventArgs e) { new GestionProgramas().Show(); this.Close(); }
    private void IrGestionEvaluaciones(object sender, MouseButtonEventArgs e) { new GestionEvaluaciones().Show(); this.Close(); }
    private void IrRegistrarResultados(object sender, MouseButtonEventArgs e) { new RegistroResultados().Show(); this.Close(); }
    private void IrReportes(object sender, MouseButtonEventArgs e) { new ReporteMetricas().Show(); this.Close(); }
    private void IrParticipantes(object sender, MouseButtonEventArgs e) { new GestionParticipantes().Show(); this.Close(); }
}
