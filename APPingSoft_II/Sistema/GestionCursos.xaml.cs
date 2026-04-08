using APPingSoft_II.Logic;
using APPingSoft_II.Logic.Windows;
using APPingSoft_II.Models;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Input;

namespace APPingSoft_II.Sistema;

public partial class GestionCursos : Window
{
    private readonly GestionCursosLogic _logic = new();
    private int _cursoSeleccionadoId = 0;
    private List<Curso> _todosCursos = new();

    public GestionCursos()
    {
        InitializeComponent();
        Loaded += GestionCursos_Loaded;
    }

    private void GestionCursos_Loaded(object sender, RoutedEventArgs e)
    {
        if (!Permisos.PuedeAccederGestionCursos())
        {
            MessageBox.Show("Acceso denegado. No tiene permisos para gestionar cursos.",
                "Sin permisos", MessageBoxButton.OK, MessageBoxImage.Stop);
            new Home().Show();
            this.Close();
            return;
        }

        AplicarPermisosPorRol();
        CargarTabla();
        LimpiarFormulario();
    }

    // ── Permisos ──────────────────────────────────────────────────────────────

    private void AplicarPermisosPorRol()
    {
        navGestionProgramas.Visibility     = Permisos.VisibleSi(Permisos.PuedeAccederGestionProgramas());
        navGestionCursos.Visibility        = Permisos.VisibleSi(Permisos.PuedeAccederGestionCursos());
        navParticipantes.Visibility        = Permisos.VisibleSi(Permisos.PuedeAccederGestionParticipantes());
        navGestionInscripciones.Visibility = Permisos.VisibleSi(Permisos.PuedeAccederGestionInscripciones());
        navGestionEvaluaciones.Visibility  = Permisos.VisibleSi(Permisos.PuedeAccederGestionEvaluaciones());
        navGestionUsuarios.Visibility      = Permisos.VisibleSi(Permisos.PuedeAccederGestionUsuarios());

        bool puedeEditar = Permisos.PuedeGestionarCursos();
        btnIngresar.IsEnabled = puedeEditar;
        btnModificar.IsEnabled = puedeEditar;
        btnBorrar.IsEnabled    = puedeEditar;
    }

    // ── Carga de tabla ────────────────────────────────────────────────────────

    private void CargarTabla()
    {
        try
        {
            _todosCursos = _logic.ObtenerTodos();
            dgCursos.ItemsSource = _todosCursos;
            txtBuscar.Clear();
        }
        catch (Exception ex)
        {
            MessageBox.Show($"Error al cargar cursos:\n{ex.Message}", "Error",
                MessageBoxButton.OK, MessageBoxImage.Warning);
        }
    }

    // ── Búsqueda ──────────────────────────────────────────────────────────────

    private void BtnBuscar_Click(object sender, RoutedEventArgs e) => EjecutarBusqueda();

    private void BtnVerTodos_Click(object sender, RoutedEventArgs e)
    {
        txtBuscar.Clear();
        dgCursos.ItemsSource = _todosCursos;
    }

    private void TxtBuscar_KeyDown(object sender, KeyEventArgs e)
    {
        if (e.Key == Key.Enter) EjecutarBusqueda();
        if (e.Key == Key.Escape) { txtBuscar.Clear(); dgCursos.ItemsSource = _todosCursos; }
    }

    private void EjecutarBusqueda()
    {
        var termino = txtBuscar.Text.Trim().ToLower();
        if (string.IsNullOrEmpty(termino))
        {
            dgCursos.ItemsSource = _todosCursos;
            return;
        }

        var filtrados = _todosCursos
            .Where(c =>
                c.Codigo.ToLower().Contains(termino) ||
                c.Nombre.ToLower().Contains(termino) ||
                (c.Descripcion?.ToLower().Contains(termino) ?? false) ||
                c.Estado.ToLower().Contains(termino) ||
                c.DuracionHoras.ToString().Contains(termino))
            .ToList();

        dgCursos.ItemsSource = filtrados;

        if (filtrados.Count == 0)
            MessageBox.Show("No se encontraron cursos que coincidan con la búsqueda.",
                "Sin resultados", MessageBoxButton.OK, MessageBoxImage.Information);
    }

    // ── Botones CRUD ──────────────────────────────────────────────────────────

    private void BtnIngresar_Click(object sender, RoutedEventArgs e)
    {
        var curso = LeerFormulario();
        if (curso == null) return;

        try
        {
            var (ok, mensaje) = _logic.Insertar(curso);
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
        if (_cursoSeleccionadoId <= 0)
        {
            MessageBox.Show("Seleccione un curso de la tabla para modificar.", "Aviso",
                MessageBoxButton.OK, MessageBoxImage.Information);
            return;
        }

        var curso = LeerFormulario();
        if (curso == null) return;
        curso.CursoId = _cursoSeleccionadoId;

        try
        {
            var (ok, mensaje) = _logic.Actualizar(curso);
            MostrarMensaje(ok, mensaje);
            if (ok) { CargarTabla(); LimpiarFormulario(); }
        }
        catch (UnauthorizedAccessException ex)
        {
            MessageBox.Show(ex.Message, "Sin permisos", MessageBoxButton.OK, MessageBoxImage.Stop);
        }
    }

    private void BtnBorrar_Click(object sender, RoutedEventArgs e)
    {
        if (_cursoSeleccionadoId <= 0)
        {
            MessageBox.Show("Seleccione un curso de la tabla para desactivar.", "Aviso",
                MessageBoxButton.OK, MessageBoxImage.Information);
            return;
        }

        var result = MessageBox.Show(
            "¿Desea desactivar el curso seleccionado?\nNo se puede desactivar si tiene programas activos asociados.",
            "Confirmar", MessageBoxButton.YesNo, MessageBoxImage.Question);
        if (result != MessageBoxResult.Yes) return;

        try
        {
            var (ok, mensaje) = _logic.Desactivar(_cursoSeleccionadoId);
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

    private void DgCursos_SelectionChanged(object sender, SelectionChangedEventArgs e)
    {
        if (dgCursos.SelectedItem is not Curso c) return;

        _cursoSeleccionadoId = c.CursoId;
        txtCodigo.Text       = c.Codigo;
        txtNombre.Text       = c.Nombre;
        txtDescripcion.Text  = c.Descripcion ?? string.Empty;
        txtDuracion.Text     = c.DuracionHoras.ToString();

        foreach (ComboBoxItem item in cmbEstado.Items)
        {
            if (item.Content?.ToString() == c.Estado) { cmbEstado.SelectedItem = item; break; }
        }
    }

    // ── Helpers ───────────────────────────────────────────────────────────────

    private Curso? LeerFormulario()
    {
        if (string.IsNullOrWhiteSpace(txtCodigo.Text))
        {
            MessageBox.Show("El código del curso es obligatorio.", "Validación",
                MessageBoxButton.OK, MessageBoxImage.Warning);
            txtCodigo.Focus(); return null;
        }
        if (string.IsNullOrWhiteSpace(txtNombre.Text))
        {
            MessageBox.Show("El nombre del curso es obligatorio.", "Validación",
                MessageBoxButton.OK, MessageBoxImage.Warning);
            txtNombre.Focus(); return null;
        }
        if (!int.TryParse(txtDuracion.Text.Trim(), out int horas) || horas <= 0)
        {
            MessageBox.Show("La duración en horas debe ser un número entero mayor a cero.", "Validación",
                MessageBoxButton.OK, MessageBoxImage.Warning);
            txtDuracion.Focus(); return null;
        }

        var estado = (cmbEstado.SelectedItem as ComboBoxItem)?.Content?.ToString() ?? "Activo";

        return new Curso
        {
            Codigo        = txtCodigo.Text.Trim().ToUpper(),
            Nombre        = txtNombre.Text.Trim(),
            Descripcion   = string.IsNullOrWhiteSpace(txtDescripcion.Text) ? null : txtDescripcion.Text.Trim(),
            DuracionHoras = horas,
            Estado        = estado
        };
    }

    private void LimpiarFormulario()
    {
        txtCodigo.Clear();
        txtNombre.Clear();
        txtDescripcion.Clear();
        txtDuracion.Clear();
        cmbEstado.SelectedIndex    = 0;
        dgCursos.SelectedItem      = null;
        _cursoSeleccionadoId       = 0;
        txtCodigo.Focus();
    }

    private static void MostrarMensaje(bool ok, string mensaje)
    {
        if (ok)
            MessageBox.Show(mensaje, "Éxito", MessageBoxButton.OK, MessageBoxImage.Information);
        else
            MessageBox.Show(mensaje, "Error", MessageBoxButton.OK, MessageBoxImage.Warning);
    }

    // ── Navegación ────────────────────────────────────────────────────────────

    private void IrHome(object sender, MouseButtonEventArgs e)                    { new Home().Show(); this.Close(); }
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
