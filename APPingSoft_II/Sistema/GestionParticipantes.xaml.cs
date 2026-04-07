using APPingSoft_II.Logic;
using APPingSoft_II.Logic.Windows;
using APPingSoft_II.Models;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Input;

namespace APPingSoft_II.Sistema;

public partial class GestionParticipantes : Window
{
    private readonly GestionParticipantesLogic _logic = new();
    private int _participanteSeleccionadoId = 0;

    public GestionParticipantes()
    {
        InitializeComponent();
        Loaded += GestionParticipantes_Loaded;
    }

    private void GestionParticipantes_Loaded(object sender, RoutedEventArgs e)
    {
        AplicarPermisosPorRol();
        CargarTabla();
        LimpiarFormulario();
    }

    private void AplicarPermisosPorRol()
    {
        navGestionProgramas.Visibility    = Permisos.VisibleSi(Permisos.PuedeAccederGestionProgramas());
        navParticipantes.Visibility       = Permisos.VisibleSi(Permisos.PuedeAccederGestionParticipantes());
        navGestionEvaluaciones.Visibility = Permisos.VisibleSi(Permisos.PuedeAccederGestionEvaluaciones());

        bool puedeEditar = Permisos.PuedeGestionarParticipantes();
        btnIngresar.IsEnabled = puedeEditar;
        btnModificar.IsEnabled = puedeEditar;
        btnBorrar.IsEnabled    = puedeEditar;
    }

    // ── Carga de datos ────────────────────────────────────────────────────────

    private void CargarTabla()
    {
        try
        {
            dgParticipantes.ItemsSource = _logic.ObtenerTodos();
        }
        catch (Exception ex)
        {
            MessageBox.Show($"Error al cargar participantes:\n{ex.Message}", "Error",
                MessageBoxButton.OK, MessageBoxImage.Warning);
        }
    }

    // ── Búsqueda ──────────────────────────────────────────────────────────────

    private void BtnBuscar_Click(object sender, RoutedEventArgs e) => EjecutarBusqueda();

    private void TxtBuscar_KeyDown(object sender, KeyEventArgs e)
    {
        if (e.Key == Key.Enter) EjecutarBusqueda();
        if (e.Key == Key.Escape) { txtBuscar.Clear(); CargarTabla(); }
    }

    private void EjecutarBusqueda()
    {
        try
        {
            var termino = txtBuscar.Text.Trim();
            dgParticipantes.ItemsSource = string.IsNullOrEmpty(termino)
                ? _logic.ObtenerTodos()
                : _logic.Buscar(termino);
        }
        catch (Exception ex)
        {
            MessageBox.Show($"Error al buscar:\n{ex.Message}", "Error",
                MessageBoxButton.OK, MessageBoxImage.Warning);
        }
    }

    // ── Botones CRUD ──────────────────────────────────────────────────────────

    private void BtnIngresar_Click(object sender, RoutedEventArgs e)
    {
        var participante = LeerFormulario();
        if (participante == null) return;

        var (ok, mensaje) = _logic.Insertar(participante);
        MostrarMensaje(ok, mensaje);
        if (ok) { CargarTabla(); LimpiarFormulario(); }
    }

    private void BtnModificar_Click(object sender, RoutedEventArgs e)
    {
        if (_participanteSeleccionadoId <= 0)
        {
            MessageBox.Show("Seleccione un participante de la tabla para modificar.", "Aviso",
                MessageBoxButton.OK, MessageBoxImage.Information);
            return;
        }

        var participante = LeerFormulario();
        if (participante == null) return;
        participante.ParticipanteId = _participanteSeleccionadoId;

        var (ok, mensaje) = _logic.Actualizar(participante);
        MostrarMensaje(ok, mensaje);
        if (ok) { CargarTabla(); LimpiarFormulario(); }
    }

    private void BtnBorrar_Click(object sender, RoutedEventArgs e)
    {
        if (_participanteSeleccionadoId <= 0)
        {
            MessageBox.Show("Seleccione un participante de la tabla para desactivar.", "Aviso",
                MessageBoxButton.OK, MessageBoxImage.Information);
            return;
        }

        bool tieneInscripciones = _logic.TieneInscripciones(_participanteSeleccionadoId);
        string aviso = tieneInscripciones
            ? "Este participante tiene inscripciones registradas.\n" +
              "Se desactivará pero su historial se conservará.\n\n¿Desea continuar?"
            : "¿Desea desactivar el participante seleccionado?";

        var result = MessageBox.Show(aviso, "Confirmar desactivación",
            MessageBoxButton.YesNo, MessageBoxImage.Question);
        if (result != MessageBoxResult.Yes) return;

        var (ok, mensaje) = _logic.Desactivar(_participanteSeleccionadoId);
        MostrarMensaje(ok, mensaje);
        if (ok) { CargarTabla(); LimpiarFormulario(); }
    }

    private void BtnLimpiar_Click(object sender, RoutedEventArgs e) => LimpiarFormulario();

    // ── Selección en tabla ────────────────────────────────────────────────────

    private void DgParticipantes_SelectionChanged(object sender, SelectionChangedEventArgs e)
    {
        if (dgParticipantes.SelectedItem is not Participante p) return;

        _participanteSeleccionadoId = p.ParticipanteId;
        txtNombre.Text = p.NombreCompleto;
        txtCorreo.Text = p.CorreoElectronico;
        txtTelefono.Text = p.Telefono ?? string.Empty;

        foreach (ComboBoxItem item in cmbEstado.Items)
        {
            if (item.Content?.ToString() == p.Estado)
            {
                cmbEstado.SelectedItem = item;
                break;
            }
        }
    }

    // ── Helpers ───────────────────────────────────────────────────────────────

    private Participante? LeerFormulario()
    {
        if (string.IsNullOrWhiteSpace(txtNombre.Text))
        {
            MessageBox.Show("El nombre completo es obligatorio.", "Validación",
                MessageBoxButton.OK, MessageBoxImage.Warning);
            txtNombre.Focus();
            return null;
        }
        if (string.IsNullOrWhiteSpace(txtCorreo.Text))
        {
            MessageBox.Show("El correo electrónico es obligatorio.", "Validación",
                MessageBoxButton.OK, MessageBoxImage.Warning);
            txtCorreo.Focus();
            return null;
        }

        var estado = (cmbEstado.SelectedItem as ComboBoxItem)?.Content?.ToString() ?? "Activo";
        var telefono = string.IsNullOrWhiteSpace(txtTelefono.Text) ? null : txtTelefono.Text.Trim();

        return new Participante
        {
            NombreCompleto = txtNombre.Text.Trim(),
            CorreoElectronico = txtCorreo.Text.Trim(),
            Telefono = telefono,
            Estado = estado
        };
    }

    private void LimpiarFormulario()
    {
        txtNombre.Clear();
        txtCorreo.Clear();
        txtTelefono.Clear();
        cmbEstado.SelectedIndex = 0;
        dgParticipantes.SelectedItem = null;
        _participanteSeleccionadoId = 0;
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

    private void IrHome(object sender, MouseButtonEventArgs e) { new Home().Show(); this.Close(); }
    private void IrGestionProgramas(object sender, MouseButtonEventArgs e) { new GestionProgramas().Show(); this.Close(); }
    private void IrGestionEvaluaciones(object sender, MouseButtonEventArgs e) { new GestionEvaluaciones().Show(); this.Close(); }
    private void IrRegistrarResultados(object sender, MouseButtonEventArgs e) { new RegistroResultados().Show(); this.Close(); }
    private void IrReportes(object sender, MouseButtonEventArgs e) { new ReporteMetricas().Show(); this.Close(); }
    private void IrParticipantes(object sender, MouseButtonEventArgs e) { new GestionParticipantes().Show(); this.Close(); }
}
