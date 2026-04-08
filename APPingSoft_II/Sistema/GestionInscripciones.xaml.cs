using APPingSoft_II.Logic;
using APPingSoft_II.Logic.Windows;
using APPingSoft_II.Models;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Input;

namespace APPingSoft_II.Sistema;

public partial class GestionInscripciones : Window
{
    private readonly GestionInscripcionesLogic _logic = new();
    private int _inscripcionSeleccionadaId = 0;
    private List<Inscripcion> _todasInscripciones = new();

    public GestionInscripciones()
    {
        InitializeComponent();
        Loaded += GestionInscripciones_Loaded;
    }

    private void GestionInscripciones_Loaded(object sender, RoutedEventArgs e)
    {
        if (!Permisos.PuedeAccederGestionInscripciones())
        {
            MessageBox.Show("Acceso denegado. No tiene permisos para gestionar inscripciones.",
                "Sin permisos", MessageBoxButton.OK, MessageBoxImage.Stop);
            new Home().Show();
            this.Close();
            return;
        }

        AplicarPermisosPorRol();
        CargarCombos();
        CargarTabla();
        LimpiarFormulario();
    }

    // ── Permisos ──────────────────────────────────────────────────────────────

    private void AplicarPermisosPorRol()
    {
        navGestionProgramas.Visibility       = Permisos.VisibleSi(Permisos.PuedeAccederGestionProgramas());
        navGestionCursos.Visibility          = Permisos.VisibleSi(Permisos.PuedeAccederGestionCursos());
        navParticipantes.Visibility          = Permisos.VisibleSi(Permisos.PuedeAccederGestionParticipantes());
        navGestionInscripciones.Visibility   = Permisos.VisibleSi(Permisos.PuedeAccederGestionInscripciones());
        navGestionEvaluaciones.Visibility    = Permisos.VisibleSi(Permisos.PuedeAccederGestionEvaluaciones());
        navGestionUsuarios.Visibility        = Permisos.VisibleSi(Permisos.PuedeAccederGestionUsuarios());

        bool puedeEditar = Permisos.PuedeGestionarInscripciones();
        btnRegistrar.IsEnabled = puedeEditar;
        btnModificar.IsEnabled = puedeEditar;
        btnRetirar.IsEnabled   = puedeEditar;
    }

    // ── Carga de datos ────────────────────────────────────────────────────────

    private void CargarCombos()
    {
        try
        {
            cmbParticipante.ItemsSource = _logic.ObtenerParticipantes();
            cmbPrograma.ItemsSource     = _logic.ObtenerProgramas();
            cmbEstado.SelectedIndex     = 0;
        }
        catch (Exception ex)
        {
            MessageBox.Show($"Error al cargar combos:\n{ex.Message}", "Error",
                MessageBoxButton.OK, MessageBoxImage.Warning);
        }
    }

    private void CargarTabla()
    {
        try
        {
            _todasInscripciones = _logic.ObtenerTodas();
            dgInscripciones.ItemsSource = _todasInscripciones;
            txtBuscar.Clear();
        }
        catch (Exception ex)
        {
            MessageBox.Show($"Error al cargar inscripciones:\n{ex.Message}", "Error",
                MessageBoxButton.OK, MessageBoxImage.Warning);
        }
    }

    // ── Dependencia Programa → Curso ──────────────────────────────────────────

    private void CmbPrograma_SelectionChanged(object sender, SelectionChangedEventArgs e)
    {
        if (cmbPrograma.SelectedValue is int programaId)
        {
            var cursos = _logic.ObtenerCursosPorPrograma(programaId);
            txtCurso.Text = cursos.Count > 0 ? cursos[0].Nombre : "(sin curso asociado)";
        }
        else
        {
            txtCurso.Text = string.Empty;
        }
    }

    // ── Búsqueda ──────────────────────────────────────────────────────────────

    private void BtnBuscar_Click(object sender, RoutedEventArgs e) => EjecutarBusqueda();

    private void BtnVerTodos_Click(object sender, RoutedEventArgs e)
    {
        txtBuscar.Clear();
        dgInscripciones.ItemsSource = _todasInscripciones;
    }

    private void TxtBuscar_KeyDown(object sender, KeyEventArgs e)
    {
        if (e.Key == Key.Enter) EjecutarBusqueda();
        if (e.Key == Key.Escape) { txtBuscar.Clear(); dgInscripciones.ItemsSource = _todasInscripciones; }
    }

    private void EjecutarBusqueda()
    {
        var termino = txtBuscar.Text.Trim().ToLower();
        if (string.IsNullOrEmpty(termino))
        {
            dgInscripciones.ItemsSource = _todasInscripciones;
            return;
        }

        var filtrados = _todasInscripciones
            .Where(i =>
                (i.Participante?.NombreCompleto.ToLower().Contains(termino) ?? false) ||
                (i.Programa?.Nombre.ToLower().Contains(termino) ?? false) ||
                (i.Programa?.Curso?.Nombre.ToLower().Contains(termino) ?? false) ||
                i.Estado.ToLower().Contains(termino) ||
                i.FechaInscripcion.ToString("dd/MM/yyyy").Contains(termino))
            .ToList();

        dgInscripciones.ItemsSource = filtrados;

        if (filtrados.Count == 0)
            MessageBox.Show("No se encontraron inscripciones que coincidan con la búsqueda.",
                "Sin resultados", MessageBoxButton.OK, MessageBoxImage.Information);
    }

    // ── Botones CRUD ──────────────────────────────────────────────────────────

    private void BtnRegistrar_Click(object sender, RoutedEventArgs e)
    {
        var ins = LeerFormulario();
        if (ins == null) return;

        try
        {
            var (ok, mensaje) = _logic.Registrar(ins);
            MostrarMensaje(ok, mensaje);
            if (ok) { CargarTabla(); LimpiarFormulario(); }
        }
        catch (UnauthorizedAccessException ex)
        {
            MessageBox.Show(ex.Message, "Sin permisos", MessageBoxButton.OK, MessageBoxImage.Stop);
        }
    }

    private void BtnModificar_Click(object sender, RoutedEventArgs e)
    {
        if (_inscripcionSeleccionadaId <= 0)
        {
            MessageBox.Show("Seleccione una inscripción de la tabla para modificar.", "Aviso",
                MessageBoxButton.OK, MessageBoxImage.Information);
            return;
        }

        if (dpFechaInscripcion.SelectedDate == null)
        {
            MessageBox.Show("Seleccione la fecha de inscripción.", "Validación",
                MessageBoxButton.OK, MessageBoxImage.Warning);
            return;
        }

        var estado = (cmbEstado.SelectedItem as ComboBoxItem)?.Content?.ToString() ?? "Activa";

        try
        {
            var (ok, mensaje) = _logic.Actualizar(
                _inscripcionSeleccionadaId,
                dpFechaInscripcion.SelectedDate.Value,
                estado);
            MostrarMensaje(ok, mensaje);
            if (ok) { CargarTabla(); LimpiarFormulario(); }
        }
        catch (UnauthorizedAccessException ex)
        {
            MessageBox.Show(ex.Message, "Sin permisos", MessageBoxButton.OK, MessageBoxImage.Stop);
        }
    }

    private void BtnRetirar_Click(object sender, RoutedEventArgs e)
    {
        if (_inscripcionSeleccionadaId <= 0)
        {
            MessageBox.Show("Seleccione una inscripción de la tabla para retirar.", "Aviso",
                MessageBoxButton.OK, MessageBoxImage.Information);
            return;
        }

        var result = MessageBox.Show(
            "¿Desea marcar esta inscripción como Retirada?\nEsta acción cambia el estado pero conserva el historial.",
            "Confirmar retiro", MessageBoxButton.YesNo, MessageBoxImage.Question);
        if (result != MessageBoxResult.Yes) return;

        try
        {
            var (ok, mensaje) = _logic.Desactivar(_inscripcionSeleccionadaId);
            MostrarMensaje(ok, mensaje);
            if (ok) { CargarTabla(); LimpiarFormulario(); }
        }
        catch (UnauthorizedAccessException ex)
        {
            MessageBox.Show(ex.Message, "Sin permisos", MessageBoxButton.OK, MessageBoxImage.Stop);
        }
    }

    private void BtnLimpiar_Click(object sender, RoutedEventArgs e) => LimpiarFormulario();

    // ── Selección en tabla ────────────────────────────────────────────────────

    private void DgInscripciones_SelectionChanged(object sender, SelectionChangedEventArgs e)
    {
        if (dgInscripciones.SelectedItem is not Inscripcion ins) return;

        _inscripcionSeleccionadaId = ins.InscripcionId;

        // Cargar participante
        cmbParticipante.SelectedValue = ins.ParticipanteId;

        // Cargar programa (dispara CmbPrograma_SelectionChanged → actualiza txtCurso)
        cmbPrograma.SelectedValue = ins.ProgramaId;

        dpFechaInscripcion.SelectedDate = ins.FechaInscripcion;

        foreach (ComboBoxItem item in cmbEstado.Items)
        {
            if (item.Content?.ToString() == ins.Estado) { cmbEstado.SelectedItem = item; break; }
        }
    }

    // ── Helpers ───────────────────────────────────────────────────────────────

    private Inscripcion? LeerFormulario()
    {
        if (cmbParticipante.SelectedValue == null)
        {
            MessageBox.Show("Seleccione un participante.", "Validación",
                MessageBoxButton.OK, MessageBoxImage.Warning);
            return null;
        }
        if (cmbPrograma.SelectedValue == null)
        {
            MessageBox.Show("Seleccione un programa.", "Validación",
                MessageBoxButton.OK, MessageBoxImage.Warning);
            return null;
        }
        if (dpFechaInscripcion.SelectedDate == null)
        {
            MessageBox.Show("Seleccione la fecha de inscripción.", "Validación",
                MessageBoxButton.OK, MessageBoxImage.Warning);
            return null;
        }

        var estado = (cmbEstado.SelectedItem as ComboBoxItem)?.Content?.ToString() ?? "Activa";

        return new Inscripcion
        {
            ParticipanteId   = (int)cmbParticipante.SelectedValue,
            ProgramaId       = (int)cmbPrograma.SelectedValue,
            FechaInscripcion = dpFechaInscripcion.SelectedDate.Value,
            Estado           = estado
        };
    }

    private void LimpiarFormulario()
    {
        cmbParticipante.SelectedIndex   = -1;
        cmbPrograma.SelectedIndex       = -1;
        txtCurso.Text                   = string.Empty;
        dpFechaInscripcion.SelectedDate = DateTime.Today;
        cmbEstado.SelectedIndex         = 0;
        dgInscripciones.SelectedItem    = null;
        _inscripcionSeleccionadaId      = 0;
    }

    private static void MostrarMensaje(bool ok, string mensaje)
    {
        if (ok)
            MessageBox.Show(mensaje, "Éxito", MessageBoxButton.OK, MessageBoxImage.Information);
        else
            MessageBox.Show(mensaje, "Error", MessageBoxButton.OK, MessageBoxImage.Warning);
    }

    // ── Navegación ────────────────────────────────────────────────────────────

    private void IrHome(object sender, MouseButtonEventArgs e)                   { new Home().Show(); this.Close(); }
    private void IrGestionProgramas(object sender, MouseButtonEventArgs e)       { new GestionProgramas().Show(); this.Close(); }
    private void IrGestionCursos(object sender, MouseButtonEventArgs e)          { new GestionCursos().Show(); this.Close(); }
    private void IrParticipantes(object sender, MouseButtonEventArgs e)          { new GestionParticipantes().Show(); this.Close(); }
    private void IrGestionInscripciones(object sender, MouseButtonEventArgs e)   { new GestionInscripciones().Show(); this.Close(); }
    private void IrGestionEvaluaciones(object sender, MouseButtonEventArgs e)    { new GestionEvaluaciones().Show(); this.Close(); }
    private void IrRegistrarResultados(object sender, MouseButtonEventArgs e)    { new RegistroResultados().Show(); this.Close(); }
    private void IrReportes(object sender, MouseButtonEventArgs e)               { new ReporteMetricas().Show(); this.Close(); }
    private void IrGestionUsuarios(object sender, MouseButtonEventArgs e)        { new GestionUsuarios().Show(); this.Close(); }
    private void BtnCerrarSesion_Click(object sender, MouseButtonEventArgs e)    { new Login().Show(); this.Close(); }
}
