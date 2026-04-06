using APPingSoft_II.Logic.Windows;
using APPingSoft_II.Models;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Input;

namespace APPingSoft_II.Sistema;

public partial class GestionEvaluaciones : Window
{
    private readonly GestionEvaluacionesLogic _logic = new();
    private int _evaluacionSeleccionadaId = 0;
    private int _criterioSeleccionadoId = 0;

    public GestionEvaluaciones()
    {
        InitializeComponent();
        Loaded += GestionEvaluaciones_Loaded;
    }

    private void GestionEvaluaciones_Loaded(object sender, RoutedEventArgs e)
    {
        CargarCursos();
        CargarTabla();
        LimpiarFormulario();
    }

    // ── Carga ─────────────────────────────────────────────────────────────────

    private void CargarCursos()
    {
        try
        {
            cmbCurso.ItemsSource = _logic.ObtenerCursos();
        }
        catch (Exception ex)
        {
            MessageBox.Show($"Error al cargar cursos:\n{ex.Message}", "Error",
                MessageBoxButton.OK, MessageBoxImage.Warning);
        }
    }

    private void CargarTabla()
    {
        try
        {
            dgEvaluaciones.ItemsSource = _logic.ObtenerEvaluaciones();
        }
        catch (Exception ex)
        {
            MessageBox.Show($"Error al cargar evaluaciones:\n{ex.Message}", "Error",
                MessageBoxButton.OK, MessageBoxImage.Warning);
        }
    }

    // ── Botones CRUD ──────────────────────────────────────────────────────────

    private void BtnIngresar_Click(object sender, RoutedEventArgs e)
    {
        // Si hay criterio completo, insertar criterio en evaluación seleccionada
        if (_evaluacionSeleccionadaId > 0 && !string.IsNullOrWhiteSpace(txtCriterio.Text))
        {
            InsertarCriterio();
            return;
        }

        // Si no, insertar evaluación
        var evaluacion = LeerFormularioEvaluacion();
        if (evaluacion == null) return;

        var (ok, mensaje) = _logic.InsertarEvaluacion(evaluacion);
        MostrarMensaje(ok, mensaje);
        if (ok)
        {
            _evaluacionSeleccionadaId = evaluacion.EvaluacionId;
            CargarTabla();
            LimpiarCamposEvaluacion();
        }
    }

    private void BtnModificar_Click(object sender, RoutedEventArgs e)
    {
        // Si hay criterio seleccionado, modificar criterio
        if (_criterioSeleccionadoId > 0)
        {
            ModificarCriterio();
            return;
        }

        // Si no, modificar evaluación
        if (_evaluacionSeleccionadaId <= 0)
        {
            MessageBox.Show("Seleccione una evaluación de la tabla para modificar.", "Aviso",
                MessageBoxButton.OK, MessageBoxImage.Information);
            return;
        }

        var evaluacion = LeerFormularioEvaluacion();
        if (evaluacion == null) return;
        evaluacion.EvaluacionId = _evaluacionSeleccionadaId;

        var (ok, mensaje) = _logic.ActualizarEvaluacion(evaluacion);
        MostrarMensaje(ok, mensaje);
        if (ok) { CargarTabla(); LimpiarFormulario(); }
    }

    private void BtnBorrar_Click(object sender, RoutedEventArgs e)
    {
        // Si hay criterio seleccionado, eliminar criterio
        if (_criterioSeleccionadoId > 0)
        {
            var r = MessageBox.Show("¿Eliminar el criterio seleccionado?", "Confirmar",
                MessageBoxButton.YesNo, MessageBoxImage.Question);
            if (r != MessageBoxResult.Yes) return;

            var (ok, msg) = _logic.EliminarCriterio(_criterioSeleccionadoId);
            MostrarMensaje(ok, msg);
            if (ok) LimpiarCamposCriterio();
            return;
        }

        if (_evaluacionSeleccionadaId <= 0)
        {
            MessageBox.Show("Seleccione una evaluación de la tabla para eliminar.", "Aviso",
                MessageBoxButton.OK, MessageBoxImage.Information);
            return;
        }

        var result = MessageBox.Show("¿Cerrar/eliminar la evaluación seleccionada?", "Confirmar",
            MessageBoxButton.YesNo, MessageBoxImage.Question);
        if (result != MessageBoxResult.Yes) return;

        var (ok2, mensaje2) = _logic.EliminarEvaluacion(_evaluacionSeleccionadaId);
        MostrarMensaje(ok2, mensaje2);
        if (ok2) { CargarTabla(); LimpiarFormulario(); }
    }

    // ── Operaciones de criterio ───────────────────────────────────────────────

    private void InsertarCriterio()
    {
        var criterio = LeerFormularioCriterio();
        if (criterio == null) return;
        criterio.EvaluacionId = _evaluacionSeleccionadaId;

        var (ok, mensaje) = _logic.InsertarCriterio(criterio);
        MostrarMensaje(ok, mensaje);
        if (ok) LimpiarCamposCriterio();
    }

    private void ModificarCriterio()
    {
        var criterio = LeerFormularioCriterio();
        if (criterio == null) return;
        criterio.CriterioId = _criterioSeleccionadoId;
        criterio.EvaluacionId = _evaluacionSeleccionadaId;

        var (ok, mensaje) = _logic.ActualizarCriterio(criterio);
        MostrarMensaje(ok, mensaje);
        if (ok) LimpiarCamposCriterio();
    }

    // ── Selección en tabla ────────────────────────────────────────────────────

    private void DgEvaluaciones_SelectionChanged(object sender, SelectionChangedEventArgs e)
    {
        if (dgEvaluaciones.SelectedItem is not Evaluacion ev) return;

        _evaluacionSeleccionadaId = ev.EvaluacionId;
        _criterioSeleccionadoId = 0;

        cmbCurso.SelectedValue = ev.CursoId;
        txtTitulo.Text = ev.Titulo;
        txtMomento.Text = ev.Momento;
        dpFechaApertura.SelectedDate = ev.FechaApertura;
        dpFechaCierre.SelectedDate = ev.FechaCierre;
        txtPuntosMax.Text = ev.PuntosMax.ToString();

        // Seleccionar tipo
        foreach (ComboBoxItem item in cmbTipo.Items)
        {
            if (item.Content?.ToString() == ev.Tipo) { cmbTipo.SelectedItem = item; break; }
        }
        // Seleccionar estado
        foreach (ComboBoxItem item in cmbEstado.Items)
        {
            if (item.Content?.ToString() == ev.Estado) { cmbEstado.SelectedItem = item; break; }
        }
    }

    // ── Helpers de formulario ─────────────────────────────────────────────────

    private Evaluacion? LeerFormularioEvaluacion()
    {
        if (cmbCurso.SelectedValue == null)
        {
            MessageBox.Show("Seleccione un curso.", "Validación", MessageBoxButton.OK, MessageBoxImage.Warning);
            return null;
        }
        if (string.IsNullOrWhiteSpace(txtTitulo.Text))
        {
            MessageBox.Show("El título es obligatorio.", "Validación", MessageBoxButton.OK, MessageBoxImage.Warning);
            txtTitulo.Focus(); return null;
        }
        if (!decimal.TryParse(txtPuntosMax.Text.Replace(",", "."), System.Globalization.NumberStyles.Any,
            System.Globalization.CultureInfo.InvariantCulture, out decimal puntosMax) || puntosMax <= 0)
        {
            MessageBox.Show("Ingrese un valor numérico válido para los puntos máximos.", "Validación",
                MessageBoxButton.OK, MessageBoxImage.Warning);
            txtPuntosMax.Focus(); return null;
        }
        if (dpFechaApertura.SelectedDate == null || dpFechaCierre.SelectedDate == null)
        {
            MessageBox.Show("Seleccione las fechas de apertura y cierre.", "Validación",
                MessageBoxButton.OK, MessageBoxImage.Warning);
            return null;
        }

        return new Evaluacion
        {
            CursoId = (int)cmbCurso.SelectedValue,
            Titulo = txtTitulo.Text.Trim(),
            Tipo = (cmbTipo.SelectedItem as ComboBoxItem)?.Content?.ToString() ?? "Formativa",
            Momento = txtMomento.Text.Trim(),
            FechaApertura = dpFechaApertura.SelectedDate!.Value,
            FechaCierre = dpFechaCierre.SelectedDate!.Value,
            PuntosMax = puntosMax,
            Estado = (cmbEstado.SelectedItem as ComboBoxItem)?.Content?.ToString() ?? "Pendiente"
        };
    }

    private CriterioEvaluacion? LeerFormularioCriterio()
    {
        if (string.IsNullOrWhiteSpace(txtCriterio.Text))
        {
            MessageBox.Show("El nombre del criterio es obligatorio.", "Validación",
                MessageBoxButton.OK, MessageBoxImage.Warning);
            txtCriterio.Focus(); return null;
        }
        if (!decimal.TryParse(txtPonderacion.Text.Replace(",", "."), System.Globalization.NumberStyles.Any,
            System.Globalization.CultureInfo.InvariantCulture, out decimal ponderacion) || ponderacion <= 0)
        {
            MessageBox.Show("Ingrese una ponderación válida (mayor a 0).", "Validación",
                MessageBoxButton.OK, MessageBoxImage.Warning);
            txtPonderacion.Focus(); return null;
        }
        if (!decimal.TryParse(txtPuntosMaxCriterio.Text.Replace(",", "."), System.Globalization.NumberStyles.Any,
            System.Globalization.CultureInfo.InvariantCulture, out decimal puntosMaxCriterio) || puntosMaxCriterio <= 0)
        {
            MessageBox.Show("Ingrese puntos máximos del criterio válidos.", "Validación",
                MessageBoxButton.OK, MessageBoxImage.Warning);
            txtPuntosMaxCriterio.Focus(); return null;
        }

        return new CriterioEvaluacion
        {
            NombreCriterio = txtCriterio.Text.Trim(),
            Ponderacion = ponderacion,
            PuntosMaxCriterio = puntosMaxCriterio
        };
    }

    private void LimpiarFormulario()
    {
        LimpiarCamposEvaluacion();
        LimpiarCamposCriterio();
        _evaluacionSeleccionadaId = 0;
        _criterioSeleccionadoId = 0;
        dgEvaluaciones.SelectedItem = null;
    }

    private void LimpiarCamposEvaluacion()
    {
        cmbCurso.SelectedIndex = -1;
        txtTitulo.Clear();
        cmbTipo.SelectedIndex = 0;
        txtMomento.Clear();
        dpFechaApertura.SelectedDate = null;
        dpFechaCierre.SelectedDate = null;
        txtPuntosMax.Clear();
        cmbEstado.SelectedIndex = 0;
    }

    private void LimpiarCamposCriterio()
    {
        txtCriterio.Clear();
        txtPonderacion.Clear();
        txtPuntosMaxCriterio.Clear();
        _criterioSeleccionadoId = 0;
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
    private void IrParticipantes(object sender, MouseButtonEventArgs e) { new GestionParticipantes().Show(); this.Close(); }
}
