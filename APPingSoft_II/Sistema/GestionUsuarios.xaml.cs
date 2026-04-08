using APPingSoft_II.Logic;
using APPingSoft_II.Logic.Windows;
using APPingSoft_II.Models;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Input;

namespace APPingSoft_II.Sistema;

public partial class GestionUsuarios : Window
{
    private readonly GestionUsuariosLogic _logic = new();
    private int _usuarioSeleccionadoId = 0;
    private string _contrasenaActual = string.Empty;   // guarda hash/texto original al modificar
    private List<Usuario> _todosUsuarios = new();

    public GestionUsuarios()
    {
        InitializeComponent();
        Loaded += GestionUsuarios_Loaded;
    }

    private void GestionUsuarios_Loaded(object sender, RoutedEventArgs e)
    {
        // Si por alguna razón un rol no-Admin llega aquí, cerrar inmediatamente
        if (!Permisos.PuedeAccederGestionUsuarios())
        {
            MessageBox.Show("Acceso denegado. Solo los Administradores pueden gestionar usuarios.",
                "Sin permisos", MessageBoxButton.OK, MessageBoxImage.Stop);
            var home = new Home();
            home.Show();
            this.Close();
            return;
        }

        AplicarPermisosPorRol();
        CargarTabla();
        LimpiarFormulario();
    }

    // ── Permisos de navbar ────────────────────────────────────────────────────

    private void AplicarPermisosPorRol()
    {
        navGestionProgramas.Visibility    = Permisos.VisibleSi(Permisos.PuedeAccederGestionProgramas());
        navParticipantes.Visibility       = Permisos.VisibleSi(Permisos.PuedeAccederGestionParticipantes());
        navGestionEvaluaciones.Visibility = Permisos.VisibleSi(Permisos.PuedeAccederGestionEvaluaciones());
        navGestionUsuarios.Visibility     = Permisos.VisibleSi(Permisos.PuedeAccederGestionUsuarios());
    }

    // ── Carga de tabla ────────────────────────────────────────────────────────

    private void CargarTabla()
    {
        try
        {
            _todosUsuarios = _logic.ObtenerTodos();
            dgUsuarios.ItemsSource = _todosUsuarios;
            txtBuscar.Clear();
        }
        catch (Exception ex)
        {
            MessageBox.Show($"Error al cargar usuarios:\n{ex.Message}", "Error",
                MessageBoxButton.OK, MessageBoxImage.Warning);
        }
    }

    // ── Búsqueda ──────────────────────────────────────────────────────────────

    private void BtnBuscar_Click(object sender, RoutedEventArgs e) => EjecutarBusqueda();

    private void BtnVerTodos_Click(object sender, RoutedEventArgs e)
    {
        txtBuscar.Clear();
        dgUsuarios.ItemsSource = _todosUsuarios;
    }

    private void TxtBuscar_KeyDown(object sender, KeyEventArgs e)
    {
        if (e.Key == Key.Enter) EjecutarBusqueda();
        if (e.Key == Key.Escape) { txtBuscar.Clear(); dgUsuarios.ItemsSource = _todosUsuarios; }
    }

    private void EjecutarBusqueda()
    {
        var termino = txtBuscar.Text.Trim();
        if (string.IsNullOrEmpty(termino))
        {
            dgUsuarios.ItemsSource = _todosUsuarios;
            return;
        }

        var filtrados = _logic.Buscar(termino);
        dgUsuarios.ItemsSource = filtrados;

        if (filtrados.Count == 0)
            MessageBox.Show("No se encontraron usuarios que coincidan con la búsqueda.",
                "Sin resultados", MessageBoxButton.OK, MessageBoxImage.Information);
    }

    // ── Botones CRUD ──────────────────────────────────────────────────────────

    private void BtnIngresar_Click(object sender, RoutedEventArgs e)
    {
        var usuario = LeerFormulario(esNuevo: true);
        if (usuario == null) return;

        try
        {
            var (ok, mensaje) = _logic.Insertar(usuario);
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
        if (_usuarioSeleccionadoId <= 0)
        {
            MessageBox.Show("Seleccione un usuario de la tabla para modificar.", "Aviso",
                MessageBoxButton.OK, MessageBoxImage.Information);
            return;
        }

        var usuario = LeerFormulario(esNuevo: false);
        if (usuario == null) return;
        usuario.UsuarioId = _usuarioSeleccionadoId;

        // Si no se ingresó contraseña nueva, conservar la contraseña actual
        if (string.IsNullOrWhiteSpace(pwdContrasena.Password))
            usuario.Contrasena = _contrasenaActual;
        else
            usuario.Contrasena = pwdContrasena.Password;

        try
        {
            var (ok, mensaje) = _logic.Actualizar(usuario);
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
        if (_usuarioSeleccionadoId <= 0)
        {
            MessageBox.Show("Seleccione un usuario de la tabla para desactivar.", "Aviso",
                MessageBoxButton.OK, MessageBoxImage.Information);
            return;
        }

        var result = MessageBox.Show("¿Desea desactivar el usuario seleccionado?", "Confirmar",
            MessageBoxButton.YesNo, MessageBoxImage.Question);
        if (result != MessageBoxResult.Yes) return;

        try
        {
            var (ok, mensaje) = _logic.Desactivar(_usuarioSeleccionadoId);
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

    private void DgUsuarios_SelectionChanged(object sender, SelectionChangedEventArgs e)
    {
        if (dgUsuarios.SelectedItem is not Usuario u) return;

        _usuarioSeleccionadoId = u.UsuarioId;
        _contrasenaActual = u.Contrasena;

        txtNombre.Text  = u.NombreCompleto;
        txtCorreo.Text  = u.CorreoElectronico;
        pwdContrasena.Password = string.Empty; // nunca mostrar la contraseña

        foreach (ComboBoxItem item in cmbRol.Items)
        {
            if (item.Content?.ToString() == u.Rol) { cmbRol.SelectedItem = item; break; }
        }
        foreach (ComboBoxItem item in cmbEstado.Items)
        {
            if (item.Content?.ToString() == u.Estado) { cmbEstado.SelectedItem = item; break; }
        }
    }

    // ── Helpers de formulario ─────────────────────────────────────────────────

    private Usuario? LeerFormulario(bool esNuevo)
    {
        if (string.IsNullOrWhiteSpace(txtNombre.Text))
        {
            MessageBox.Show("El nombre completo es obligatorio.", "Validación",
                MessageBoxButton.OK, MessageBoxImage.Warning);
            txtNombre.Focus(); return null;
        }
        if (string.IsNullOrWhiteSpace(txtCorreo.Text))
        {
            MessageBox.Show("El correo electrónico es obligatorio.", "Validación",
                MessageBoxButton.OK, MessageBoxImage.Warning);
            txtCorreo.Focus(); return null;
        }
        if (esNuevo && string.IsNullOrWhiteSpace(pwdContrasena.Password))
        {
            MessageBox.Show("La contraseña es obligatoria para un nuevo usuario.", "Validación",
                MessageBoxButton.OK, MessageBoxImage.Warning);
            pwdContrasena.Focus(); return null;
        }

        var rol    = (cmbRol.SelectedItem    as ComboBoxItem)?.Content?.ToString() ?? "Instructor";
        var estado = (cmbEstado.SelectedItem as ComboBoxItem)?.Content?.ToString() ?? "Activo";

        return new Usuario
        {
            NombreCompleto    = txtNombre.Text.Trim(),
            CorreoElectronico = txtCorreo.Text.Trim(),
            Contrasena        = pwdContrasena.Password,   // service maneja el caso vacío en modify
            Rol               = rol,
            Estado            = estado
        };
    }

    private void LimpiarFormulario()
    {
        txtNombre.Clear();
        txtCorreo.Clear();
        pwdContrasena.Password = string.Empty;
        cmbRol.SelectedIndex    = 0;
        cmbEstado.SelectedIndex = 0;
        dgUsuarios.SelectedItem = null;
        _usuarioSeleccionadoId  = 0;
        _contrasenaActual       = string.Empty;
        txtNombre.Focus();
    }

    private static void MostrarMensaje(bool ok, string mensaje)
    {
        if (ok)
            MessageBox.Show(mensaje, "Éxito", MessageBoxButton.OK, MessageBoxImage.Information);
        else
            MessageBox.Show(mensaje, "Error", MessageBoxButton.OK, MessageBoxImage.Warning);
    }

    // ── Navegación ────────────────────────────────────────────────────────────

    private void IrHome(object sender, MouseButtonEventArgs e)                 { new Home().Show(); this.Close(); }
    private void IrGestionProgramas(object sender, MouseButtonEventArgs e)     { new GestionProgramas().Show(); this.Close(); }
    private void IrGestionEvaluaciones(object sender, MouseButtonEventArgs e)  { new GestionEvaluaciones().Show(); this.Close(); }
    private void IrRegistrarResultados(object sender, MouseButtonEventArgs e)  { new RegistroResultados().Show(); this.Close(); }
    private void IrReportes(object sender, MouseButtonEventArgs e)             { new ReporteMetricas().Show(); this.Close(); }
    private void IrParticipantes(object sender, MouseButtonEventArgs e)        { new GestionParticipantes().Show(); this.Close(); }
    private void IrGestionUsuarios(object sender, MouseButtonEventArgs e)      { new GestionUsuarios().Show(); this.Close(); }
    private void BtnCerrarSesion_Click(object sender, MouseButtonEventArgs e)  { new Login().Show(); this.Close(); }
}
