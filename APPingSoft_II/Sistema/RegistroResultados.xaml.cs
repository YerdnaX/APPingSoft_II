using APPingSoft_II.Logic.Windows;
using APPingSoft_II.Models;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Input;

namespace APPingSoft_II.Sistema;

public partial class RegistroResultados : Window
{
    private readonly RegistroResultadosLogic _logic = new();
    private int _resultadoSeleccionadoId = 0;

    public RegistroResultados()
    {
        InitializeComponent();
        Loaded += RegistroResultados_Loaded;
    }

    private void RegistroResultados_Loaded(object sender, RoutedEventArgs e)
    {
        CargarEvaluaciones();
        CargarTabla();
        LimpiarFormulario();
    }

    // ── Carga ─────────────────────────────────────────────────────────────────

    private void CargarEvaluaciones()
    {
        try
        {
            cmbEvaluacion.ItemsSource = _logic.ObtenerEvaluaciones();
        }
        catch (Exception ex)
        {
            MessageBox.Show($"Error al cargar evaluaciones:\n{ex.Message}", "Error",
                MessageBoxButton.OK, MessageBoxImage.Warning);
        }
    }

    private void CargarTabla()
    {
        try
        {
            dgResultados.ItemsSource = _logic.ObtenerResultados();
        }
        catch (Exception ex)
        {
            MessageBox.Show($"Error al cargar resultados:\n{ex.Message}", "Error",
                MessageBoxButton.OK, MessageBoxImage.Warning);
        }
    }

    // ── Cambio de evaluación: recarga participantes inscritos ─────────────────

    private void CmbEvaluacion_SelectionChanged(object sender, SelectionChangedEventArgs e)
    {
        if (cmbEvaluacion.SelectedItem is not Evaluacion ev) return;

        try
        {
            var inscripciones = _logic.ObtenerInscripcionesPorEvaluacion(ev.EvaluacionId);
            cmbInscripcion.ItemsSource = inscripciones;
            cmbInscripcion.SelectedIndex = -1;
        }
        catch (Exception ex)
        {
            MessageBox.Show($"Error al cargar participantes:\n{ex.Message}", "Error",
                MessageBoxButton.OK, MessageBoxImage.Warning);
        }
    }

    // ── Botones CRUD ──────────────────────────────────────────────────────────

    private void BtnIngresar_Click(object sender, RoutedEventArgs e)
    {
        var resultado = LeerFormulario();
        if (resultado == null) return;

        var (ok, mensaje) = _logic.Insertar(resultado);
        MostrarMensaje(ok, mensaje);
        if (ok) { CargarTabla(); LimpiarFormulario(); }
    }

    private void BtnModificar_Click(object sender, RoutedEventArgs e)
    {
        if (_resultadoSeleccionadoId <= 0)
        {
            MessageBox.Show("Seleccione un resultado de la tabla para modificar.", "Aviso",
                MessageBoxButton.OK, MessageBoxImage.Information);
            return;
        }

        var resultado = LeerFormulario();
        if (resultado == null) return;
        resultado.ResultadoId = _resultadoSeleccionadoId;

        var (ok, mensaje) = _logic.Actualizar(resultado);
        MostrarMensaje(ok, mensaje);
        if (ok) { CargarTabla(); LimpiarFormulario(); }
    }

    private void BtnBorrar_Click(object sender, RoutedEventArgs e)
    {
        if (_resultadoSeleccionadoId <= 0)
        {
            MessageBox.Show("Seleccione un resultado de la tabla para eliminar.", "Aviso",
                MessageBoxButton.OK, MessageBoxImage.Information);
            return;
        }

        var result = MessageBox.Show("¿Eliminar el resultado seleccionado?", "Confirmar",
            MessageBoxButton.YesNo, MessageBoxImage.Question);
        if (result != MessageBoxResult.Yes) return;

        var (ok, mensaje) = _logic.Eliminar(_resultadoSeleccionadoId);
        MostrarMensaje(ok, mensaje);
        if (ok) { CargarTabla(); LimpiarFormulario(); }
    }

    // ── Selección en tabla ────────────────────────────────────────────────────

    private void DgResultados_SelectionChanged(object sender, SelectionChangedEventArgs e)
    {
        if (dgResultados.SelectedItem is not ResultadoEvaluacion r) return;

        _resultadoSeleccionadoId = r.ResultadoId;

        // Seleccionar evaluación en combo
        foreach (var item in cmbEvaluacion.Items)
        {
            if (item is Evaluacion ev && ev.EvaluacionId == r.EvaluacionId)
            {
                cmbEvaluacion.SelectedItem = ev;
                break;
            }
        }

        // Después de cargar participantes, seleccionar inscripción
        Dispatcher.BeginInvoke(new Action(() =>
        {
            foreach (var item in cmbInscripcion.Items)
            {
                if (item is Inscripcion ins && ins.InscripcionId == r.InscripcionId)
                {
                    cmbInscripcion.SelectedItem = ins;
                    break;
                }
            }
        }), System.Windows.Threading.DispatcherPriority.Loaded);

        txtNotaFinal.Text = r.NotaFinal.ToString();
        txtCalificadoEn.Text = r.CalificadoEn.ToString("dd/MM/yyyy HH:mm");
        txtObservaciones.Text = r.Observaciones ?? string.Empty;
    }

    // ── Helpers ───────────────────────────────────────────────────────────────

    private ResultadoEvaluacion? LeerFormulario()
    {
        if (cmbEvaluacion.SelectedItem is not Evaluacion ev)
        {
            MessageBox.Show("Seleccione una evaluación.", "Validación",
                MessageBoxButton.OK, MessageBoxImage.Warning);
            return null;
        }
        if (cmbInscripcion.SelectedValue == null || cmbInscripcion.SelectedItem is not Inscripcion ins)
        {
            MessageBox.Show("Seleccione un participante.", "Validación",
                MessageBoxButton.OK, MessageBoxImage.Warning);
            return null;
        }
        if (!decimal.TryParse(txtNotaFinal.Text.Replace(",", "."), System.Globalization.NumberStyles.Any,
            System.Globalization.CultureInfo.InvariantCulture, out decimal nota) || nota < 0)
        {
            MessageBox.Show("Ingrese una nota final numérica válida (>= 0).", "Validación",
                MessageBoxButton.OK, MessageBoxImage.Warning);
            txtNotaFinal.Focus(); return null;
        }

        return new ResultadoEvaluacion
        {
            EvaluacionId = ev.EvaluacionId,
            InscripcionId = ins.InscripcionId,
            NotaFinal = nota,
            CalificadoEn = DateTime.Now,
            Observaciones = string.IsNullOrWhiteSpace(txtObservaciones.Text) ? null : txtObservaciones.Text.Trim()
        };
    }

    private void LimpiarFormulario()
    {
        cmbEvaluacion.SelectedIndex = -1;
        cmbInscripcion.ItemsSource = null;
        cmbInscripcion.SelectedIndex = -1;
        txtNotaFinal.Clear();
        txtCalificadoEn.Clear();
        txtObservaciones.Clear();
        dgResultados.SelectedItem = null;
        _resultadoSeleccionadoId = 0;
    }

    private static void MostrarMensaje(bool ok, string mensaje)
    {
        if (ok)
            MessageBox.Show(mensaje, "Éxito", MessageBoxButton.OK, MessageBoxImage.Information);
        else
            MessageBox.Show(mensaje, "Error", MessageBoxButton.OK, MessageBoxImage.Warning);
    }

    // ── Navegación ────────────────────────────────────────────────────────────

    private void IrHome(object sender, MouseButtonEventArgs e) { new Home().Show(); this.Close(); }
    private void IrGestionProgramas(object sender, MouseButtonEventArgs e) { new GestionProgramas().Show(); this.Close(); }
    private void IrGestionEvaluaciones(object sender, MouseButtonEventArgs e) { new GestionEvaluaciones().Show(); this.Close(); }
    private void IrRegistrarResultados(object sender, MouseButtonEventArgs e) { new RegistroResultados().Show(); this.Close(); }
    private void IrReportes(object sender, MouseButtonEventArgs e) { new ReporteMetricas().Show(); this.Close(); }
}
