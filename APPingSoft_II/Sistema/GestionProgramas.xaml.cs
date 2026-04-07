using APPingSoft_II.Logic;
using APPingSoft_II.Logic.Windows;
using APPingSoft_II.Models;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Input;

namespace APPingSoft_II.Sistema;

public partial class GestionProgramas : Window
{
    private readonly GestionProgramasLogic _logic = new();
    private int _programaSeleccionadoId = 0;
    private List<Programa> _todosProgramas = new();

    public GestionProgramas()
    {
        InitializeComponent();
        Loaded += GestionProgramas_Loaded;
    }

    private void GestionProgramas_Loaded(object sender, RoutedEventArgs e)
    {
        AplicarPermisosPorRol();
        CargarCombos();
        CargarTabla();
        LimpiarFormulario();
    }

    private void AplicarPermisosPorRol()
    {
        navGestionProgramas.Visibility    = Permisos.VisibleSi(Permisos.PuedeAccederGestionProgramas());
        navParticipantes.Visibility       = Permisos.VisibleSi(Permisos.PuedeAccederGestionParticipantes());
        navGestionEvaluaciones.Visibility = Permisos.VisibleSi(Permisos.PuedeAccederGestionEvaluaciones());
        navGestionUsuarios.Visibility     = Permisos.VisibleSi(Permisos.PuedeAccederGestionUsuarios());

        bool puedeEditar = Permisos.PuedeGestionarProgramas();
        btnIngresar.IsEnabled = puedeEditar;
        btnModificar.IsEnabled = puedeEditar;
        btnBorrar.IsEnabled    = puedeEditar;
    }

    // ── Carga de datos ────────────────────────────────────────────────────────

    private void CargarCombos()
    {
        try
        {
            cmbInstructor.ItemsSource = _logic.ObtenerInstructores();
            cmbCurso.ItemsSource = _logic.ObtenerCursos();
        }
        catch (Exception ex)
        {
            MessageBox.Show($"Error al cargar datos auxiliares:\n{ex.Message}", "Error",
                MessageBoxButton.OK, MessageBoxImage.Warning);
        }
    }

    private void CargarTabla()
    {
        try
        {
            _todosProgramas = _logic.ObtenerProgramas();
            dgProgramas.ItemsSource = _todosProgramas;
            txtBuscar.Clear();
        }
        catch (Exception ex)
        {
            MessageBox.Show($"Error al cargar programas:\n{ex.Message}", "Error",
                MessageBoxButton.OK, MessageBoxImage.Warning);
        }
    }

    // ── Búsqueda ──────────────────────────────────────────────────────────────

    private void BtnBuscar_Click(object sender, RoutedEventArgs e) => EjecutarBusqueda();

    private void BtnVerTodos_Click(object sender, RoutedEventArgs e)
    {
        txtBuscar.Clear();
        dgProgramas.ItemsSource = _todosProgramas;
    }

    private void TxtBuscar_KeyDown(object sender, KeyEventArgs e)
    {
        if (e.Key == Key.Enter) EjecutarBusqueda();
        if (e.Key == Key.Escape) { txtBuscar.Clear(); dgProgramas.ItemsSource = _todosProgramas; }
    }

    private void EjecutarBusqueda()
    {
        var termino = txtBuscar.Text.Trim().ToLower();
        if (string.IsNullOrEmpty(termino))
        {
            dgProgramas.ItemsSource = _todosProgramas;
            return;
        }

        var filtrados = _todosProgramas
            .Where(p =>
                p.Nombre.ToLower().Contains(termino) ||
                p.Estado.ToLower().Contains(termino) ||
                (p.Instructor?.NombreCompleto.ToLower().Contains(termino) ?? false) ||
                (p.Curso?.Nombre.ToLower().Contains(termino) ?? false))
            .ToList();

        dgProgramas.ItemsSource = filtrados;

        if (filtrados.Count == 0)
            MessageBox.Show("No se encontraron programas que coincidan con la búsqueda.", "Sin resultados",
                MessageBoxButton.OK, MessageBoxImage.Information);
    }

    // ── Botones CRUD ──────────────────────────────────────────────────────────

    private void BtnIngresar_Click(object sender, RoutedEventArgs e)
    {
        var programa = LeerFormulario();
        if (programa == null) return;

        var (ok, mensaje) = _logic.Insertar(programa);
        MostrarMensaje(ok, mensaje);
        if (ok) { CargarTabla(); LimpiarFormulario(); }
    }

    private void BtnModificar_Click(object sender, RoutedEventArgs e)
    {
        if (_programaSeleccionadoId <= 0)
        {
            MessageBox.Show("Seleccione un programa de la tabla para modificar.", "Aviso",
                MessageBoxButton.OK, MessageBoxImage.Information);
            return;
        }

        var programa = LeerFormulario();
        if (programa == null) return;
        programa.ProgramaId = _programaSeleccionadoId;

        var (ok, mensaje) = _logic.Actualizar(programa);
        MostrarMensaje(ok, mensaje);
        if (ok) { CargarTabla(); LimpiarFormulario(); }
    }

    private void BtnBorrar_Click(object sender, RoutedEventArgs e)
    {
        if (_programaSeleccionadoId <= 0)
        {
            MessageBox.Show("Seleccione un programa de la tabla para eliminar.", "Aviso",
                MessageBoxButton.OK, MessageBoxImage.Information);
            return;
        }

        var result = MessageBox.Show("¿Desea desactivar el programa seleccionado?", "Confirmar",
            MessageBoxButton.YesNo, MessageBoxImage.Question);
        if (result != MessageBoxResult.Yes) return;

        var (ok, mensaje) = _logic.Eliminar(_programaSeleccionadoId);
        MostrarMensaje(ok, mensaje);
        if (ok) { CargarTabla(); LimpiarFormulario(); }
    }

    // ── Selección en tabla ────────────────────────────────────────────────────

    private void DgProgramas_SelectionChanged(object sender, SelectionChangedEventArgs e)
    {
        if (dgProgramas.SelectedItem is not Programa p) return;

        _programaSeleccionadoId = p.ProgramaId;
        txtNombre.Text = p.Nombre;
        dpFechaInicio.SelectedDate = p.FechaInicio;
        dpFechaFin.SelectedDate = p.FechaFin;

        foreach (ComboBoxItem item in cmbEstado.Items)
        {
            if (item.Content?.ToString() == p.Estado) { cmbEstado.SelectedItem = item; break; }
        }

        cmbInstructor.SelectedValue = p.InstructorId;
        cmbCurso.SelectedValue = p.CursoId;
    }

    // ── Helpers ───────────────────────────────────────────────────────────────

    private Programa? LeerFormulario()
    {
        if (string.IsNullOrWhiteSpace(txtNombre.Text))
        {
            MessageBox.Show("El nombre del programa es obligatorio.", "Validación",
                MessageBoxButton.OK, MessageBoxImage.Warning);
            txtNombre.Focus(); return null;
        }
        if (dpFechaInicio.SelectedDate == null)
        {
            MessageBox.Show("Seleccione la fecha de inicio.", "Validación",
                MessageBoxButton.OK, MessageBoxImage.Warning);
            return null;
        }
        if (dpFechaFin.SelectedDate == null)
        {
            MessageBox.Show("Seleccione la fecha de fin.", "Validación",
                MessageBoxButton.OK, MessageBoxImage.Warning);
            return null;
        }
        if (cmbInstructor.SelectedValue == null)
        {
            MessageBox.Show("Seleccione un instructor.", "Validación",
                MessageBoxButton.OK, MessageBoxImage.Warning);
            return null;
        }
        if (cmbCurso.SelectedValue == null)
        {
            MessageBox.Show("Seleccione un curso.", "Validación",
                MessageBoxButton.OK, MessageBoxImage.Warning);
            return null;
        }

        var estado = (cmbEstado.SelectedItem as ComboBoxItem)?.Content?.ToString() ?? "Activo";
        return new Programa
        {
            Nombre = txtNombre.Text.Trim(),
            FechaInicio = dpFechaInicio.SelectedDate!.Value,
            FechaFin = dpFechaFin.SelectedDate!.Value,
            Estado = estado,
            InstructorId = (int)cmbInstructor.SelectedValue,
            CursoId = (int)cmbCurso.SelectedValue
        };
    }

    private void LimpiarFormulario()
    {
        txtNombre.Clear();
        dpFechaInicio.SelectedDate = null;
        dpFechaFin.SelectedDate = null;
        cmbEstado.SelectedIndex = 0;
        cmbInstructor.SelectedIndex = -1;
        cmbCurso.SelectedIndex = -1;
        dgProgramas.SelectedItem = null;
        _programaSeleccionadoId = 0;
    }

    private static void MostrarMensaje(bool ok, string mensaje)
    {
        if (ok)
            MessageBox.Show(mensaje, "Éxito", MessageBoxButton.OK, MessageBoxImage.Information);
        else
            MessageBox.Show(mensaje, "Error", MessageBoxButton.OK, MessageBoxImage.Warning);
    }

    // ── Navegación ────────────────────────────────────────────────────────────

    private void IrHome(object sender, MouseButtonEventArgs e)                { new Home().Show(); this.Close(); }
    private void IrGestionProgramas(object sender, MouseButtonEventArgs e)    { new GestionProgramas().Show(); this.Close(); }
    private void IrGestionEvaluaciones(object sender, MouseButtonEventArgs e) { new GestionEvaluaciones().Show(); this.Close(); }
    private void IrRegistrarResultados(object sender, MouseButtonEventArgs e) { new RegistroResultados().Show(); this.Close(); }
    private void IrReportes(object sender, MouseButtonEventArgs e)            { new ReporteMetricas().Show(); this.Close(); }
    private void IrParticipantes(object sender, MouseButtonEventArgs e)       { new GestionParticipantes().Show(); this.Close(); }
    private void IrGestionUsuarios(object sender, MouseButtonEventArgs e)     { new GestionUsuarios().Show(); this.Close(); }
}
