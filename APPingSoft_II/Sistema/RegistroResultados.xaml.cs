using APPingSoft_II.Logic;
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
    private List<ResultadoEvaluacion> _todosResultados = new();
    private bool _sincronizandoSeleccion;

    public RegistroResultados()
    {
        InitializeComponent();
        Loaded += RegistroResultados_Loaded;
    }

    private void RegistroResultados_Loaded(object sender, RoutedEventArgs e)
    {
        AplicarPermisosPorRol();
        CargarEvaluaciones();
        CargarTabla();
        LimpiarFormulario();
    }

    private void AplicarPermisosPorRol()
    {
        navGestionProgramas.Visibility = Permisos.VisibleSi(Permisos.PuedeAccederGestionProgramas());
        navGestionCursos.Visibility = Permisos.VisibleSi(Permisos.PuedeAccederGestionCursos());
        navParticipantes.Visibility = Permisos.VisibleSi(Permisos.PuedeAccederGestionParticipantes());
        navGestionInscripciones.Visibility = Permisos.VisibleSi(Permisos.PuedeAccederGestionInscripciones());
        navGestionEvaluaciones.Visibility = Permisos.VisibleSi(Permisos.PuedeAccederGestionEvaluaciones());
        navGestionUsuarios.Visibility = Permisos.VisibleSi(Permisos.PuedeAccederGestionUsuarios());

        bool puedeEditar = Permisos.PuedeRegistrarResultados();
        btnIngresar.IsEnabled = puedeEditar;
        btnModificar.IsEnabled = puedeEditar;
        btnBorrar.IsEnabled = puedeEditar;
    }

    // Carga

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
            _todosResultados = _logic.ObtenerResultados();
            dgResultados.ItemsSource = _todosResultados;
            txtBuscar.Clear();
        }
        catch (Exception ex)
        {
            MessageBox.Show($"Error al cargar resultados:\n{ex.Message}", "Error",
                MessageBoxButton.OK, MessageBoxImage.Warning);
        }
    }

    // Busqueda

    private void BtnBuscar_Click(object sender, RoutedEventArgs e) => EjecutarBusqueda();

    private void BtnVerTodos_Click(object sender, RoutedEventArgs e)
    {
        txtBuscar.Clear();
        dgResultados.ItemsSource = _todosResultados;
    }

    private void TxtBuscar_KeyDown(object sender, KeyEventArgs e)
    {
        if (e.Key == Key.Enter) EjecutarBusqueda();
        if (e.Key == Key.Escape)
        {
            txtBuscar.Clear();
            dgResultados.ItemsSource = _todosResultados;
        }
    }

    private void EjecutarBusqueda()
    {
        var termino = txtBuscar.Text.Trim().ToLowerInvariant();
        if (string.IsNullOrEmpty(termino))
        {
            dgResultados.ItemsSource = _todosResultados;
            return;
        }

        var filtrados = _todosResultados
            .Where(r =>
                (r.Inscripcion?.Participante?.NombreCompleto.ToLowerInvariant().Contains(termino) ?? false) ||
                (r.Inscripcion?.Programa?.Nombre.ToLowerInvariant().Contains(termino) ?? false) ||
                (r.Evaluacion?.Titulo.ToLowerInvariant().Contains(termino) ?? false) ||
                r.NotaFinal.ToString().Contains(termino) ||
                (r.Observaciones?.ToLowerInvariant().Contains(termino) ?? false))
            .ToList();

        dgResultados.ItemsSource = filtrados;

        if (filtrados.Count == 0)
        {
            MessageBox.Show("No se encontraron resultados que coincidan con la busqueda.",
                "Sin resultados", MessageBoxButton.OK, MessageBoxImage.Information);
        }
    }

    // Dependencias de combos

    private void CmbEvaluacion_SelectionChanged(object sender, SelectionChangedEventArgs e)
    {
        if (_sincronizandoSeleccion) return;

        if (cmbEvaluacion.SelectedItem is not Evaluacion ev)
        {
            cmbPrograma.ItemsSource = null;
            cmbInscripcion.ItemsSource = null;
            return;
        }

        CargarProgramasPorEvaluacion(ev.EvaluacionId, null, mostrarMensajeSiVacio: true);
    }

    private void CmbPrograma_SelectionChanged(object sender, SelectionChangedEventArgs e)
    {
        if (_sincronizandoSeleccion) return;
        if (cmbEvaluacion.SelectedItem is not Evaluacion ev) return;

        int? programaId = cmbPrograma.SelectedValue is int id ? id : null;
        CargarInscripcionesPorEvaluacionYPrograma(ev.EvaluacionId, programaId, null, mostrarMensajeSiVacio: true);
    }

    private void CargarProgramasPorEvaluacion(int evaluacionId, int? programaSeleccionadoId, bool mostrarMensajeSiVacio)
    {
        var programas = _logic.ObtenerProgramasPorEvaluacion(evaluacionId);
        cmbPrograma.ItemsSource = programas;
        cmbPrograma.SelectedIndex = -1;
        cmbInscripcion.ItemsSource = null;
        cmbInscripcion.SelectedIndex = -1;

        if (programaSeleccionadoId.HasValue && programas.Any(p => p.ProgramaId == programaSeleccionadoId.Value))
        {
            cmbPrograma.SelectedValue = programaSeleccionadoId.Value;
        }
        else if (programas.Count == 1)
        {
            cmbPrograma.SelectedIndex = 0;
        }
        else if (programas.Count == 0 && mostrarMensajeSiVacio)
        {
            MessageBox.Show("La evaluacion seleccionada no tiene programas con participantes calificables.",
                "Sin programas", MessageBoxButton.OK, MessageBoxImage.Information);
        }
    }

    private void CargarInscripcionesPorEvaluacionYPrograma(
        int evaluacionId,
        int? programaId,
        int? inscripcionSeleccionadaId,
        bool mostrarMensajeSiVacio)
    {
        var inscripciones = _logic.ObtenerInscripcionesPorEvaluacion(evaluacionId, programaId);
        cmbInscripcion.ItemsSource = inscripciones;
        cmbInscripcion.SelectedIndex = -1;

        if (inscripcionSeleccionadaId.HasValue && inscripciones.Any(i => i.InscripcionId == inscripcionSeleccionadaId.Value))
        {
            cmbInscripcion.SelectedValue = inscripcionSeleccionadaId.Value;
        }
        else if (inscripciones.Count == 1)
        {
            cmbInscripcion.SelectedIndex = 0;
        }
        else if (inscripciones.Count == 0 && mostrarMensajeSiVacio)
        {
            MessageBox.Show("No hay participantes calificables para la combinacion evaluacion/programa seleccionada.",
                "Sin participantes", MessageBoxButton.OK, MessageBoxImage.Information);
        }
    }

    // Botones CRUD

    private void BtnIngresar_Click(object sender, RoutedEventArgs e)
    {
        if (_resultadoSeleccionadoId > 0)
        {
            MessageBox.Show("Hay un resultado seleccionado para edicion. Limpie el formulario antes de registrar uno nuevo.",
                "Aviso", MessageBoxButton.OK, MessageBoxImage.Information);
            return;
        }

        var resultado = LeerFormulario();
        if (resultado == null) return;

        var (ok, mensaje) = _logic.Insertar(resultado);
        MostrarMensaje(ok, mensaje);
        if (ok)
        {
            CargarTabla();
            LimpiarFormulario();
        }
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
        if (ok)
        {
            CargarTabla();
            LimpiarFormulario();
        }
    }

    private void BtnBorrar_Click(object sender, RoutedEventArgs e)
    {
        if (_resultadoSeleccionadoId <= 0)
        {
            MessageBox.Show("Seleccione un resultado de la tabla para eliminar.", "Aviso",
                MessageBoxButton.OK, MessageBoxImage.Information);
            return;
        }

        var result = MessageBox.Show("Desea eliminar el resultado seleccionado?", "Confirmar",
            MessageBoxButton.YesNo, MessageBoxImage.Question);
        if (result != MessageBoxResult.Yes) return;

        var (ok, mensaje) = _logic.Eliminar(_resultadoSeleccionadoId);
        MostrarMensaje(ok, mensaje);
        if (ok)
        {
            CargarTabla();
            LimpiarFormulario();
        }
    }

    // Seleccion en tabla

    private void DgResultados_SelectionChanged(object sender, SelectionChangedEventArgs e)
    {
        if (dgResultados.SelectedItem is not ResultadoEvaluacion r) return;
        if (r.Evaluacion == null || r.Inscripcion == null) return;

        _resultadoSeleccionadoId = r.ResultadoId;
        _sincronizandoSeleccion = true;
        try
        {
            // Seleccionar evaluacion
            foreach (var item in cmbEvaluacion.Items)
            {
                if (item is Evaluacion ev && ev.EvaluacionId == r.EvaluacionId)
                {
                    cmbEvaluacion.SelectedItem = ev;
                    break;
                }
            }

            int? programaId = r.Inscripcion.ProgramaId > 0
                ? r.Inscripcion.ProgramaId
                : r.Inscripcion.Programa?.ProgramaId;

            CargarProgramasPorEvaluacion(r.EvaluacionId, programaId, mostrarMensajeSiVacio: false);
            CargarInscripcionesPorEvaluacionYPrograma(
                r.EvaluacionId,
                programaId,
                r.InscripcionId,
                mostrarMensajeSiVacio: false);

            txtNotaFinal.Text = r.NotaFinal.ToString();
            txtCalificadoEn.Text = r.CalificadoEn.ToString("dd/MM/yyyy HH:mm");
            txtObservaciones.Text = r.Observaciones ?? string.Empty;

            BloquearClavesEdicion(true);
        }
        finally
        {
            _sincronizandoSeleccion = false;
        }
    }

    // Helpers

    private ResultadoEvaluacion? LeerFormulario()
    {
        if (cmbEvaluacion.SelectedItem is not Evaluacion ev)
        {
            MessageBox.Show("Seleccione una evaluacion.", "Validacion",
                MessageBoxButton.OK, MessageBoxImage.Warning);
            return null;
        }

        if (cmbPrograma.SelectedItem is not Programa)
        {
            MessageBox.Show("Seleccione un programa.", "Validacion",
                MessageBoxButton.OK, MessageBoxImage.Warning);
            return null;
        }

        if (cmbInscripcion.SelectedItem is not Inscripcion ins)
        {
            MessageBox.Show("Seleccione un participante.", "Validacion",
                MessageBoxButton.OK, MessageBoxImage.Warning);
            return null;
        }

        if (!decimal.TryParse(txtNotaFinal.Text.Replace(",", "."), System.Globalization.NumberStyles.Any,
            System.Globalization.CultureInfo.InvariantCulture, out decimal nota) || nota < 0)
        {
            MessageBox.Show("Ingrese una nota final numerica valida (>= 0).", "Validacion",
                MessageBoxButton.OK, MessageBoxImage.Warning);
            txtNotaFinal.Focus();
            return null;
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
        _sincronizandoSeleccion = true;
        try
        {
            cmbEvaluacion.SelectedIndex = -1;
            cmbPrograma.ItemsSource = null;
            cmbPrograma.SelectedIndex = -1;
            cmbInscripcion.ItemsSource = null;
            cmbInscripcion.SelectedIndex = -1;
            txtNotaFinal.Clear();
            txtCalificadoEn.Clear();
            txtObservaciones.Clear();
            dgResultados.SelectedItem = null;
            _resultadoSeleccionadoId = 0;
            BloquearClavesEdicion(false);
        }
        finally
        {
            _sincronizandoSeleccion = false;
        }
    }

    private void BloquearClavesEdicion(bool bloqueado)
    {
        cmbEvaluacion.IsEnabled = !bloqueado;
        cmbPrograma.IsEnabled = !bloqueado;
        cmbInscripcion.IsEnabled = !bloqueado;
    }

    private static void MostrarMensaje(bool ok, string mensaje)
    {
        if (ok)
            MessageBox.Show(mensaje, "Exito", MessageBoxButton.OK, MessageBoxImage.Information);
        else
            MessageBox.Show(mensaje, "Error", MessageBoxButton.OK, MessageBoxImage.Warning);
    }

    // Navegacion

    private void IrHome(object sender, MouseButtonEventArgs e) { new Home().Show(); this.Close(); }
    private void IrGestionProgramas(object sender, MouseButtonEventArgs e) { new GestionProgramas().Show(); this.Close(); }
    private void IrGestionCursos(object sender, MouseButtonEventArgs e) { new GestionCursos().Show(); this.Close(); }
    private void IrParticipantes(object sender, MouseButtonEventArgs e) { new GestionParticipantes().Show(); this.Close(); }
    private void IrGestionInscripciones(object sender, MouseButtonEventArgs e) { new GestionInscripciones().Show(); this.Close(); }
    private void IrGestionEvaluaciones(object sender, MouseButtonEventArgs e) { new GestionEvaluaciones().Show(); this.Close(); }
    private void IrRegistrarResultados(object sender, MouseButtonEventArgs e) { new RegistroResultados().Show(); this.Close(); }
    private void IrReportes(object sender, MouseButtonEventArgs e) { new ReporteMetricas().Show(); this.Close(); }
    private void IrGestionUsuarios(object sender, MouseButtonEventArgs e) { new GestionUsuarios().Show(); this.Close(); }
    private void BtnCerrarSesion_Click(object sender, MouseButtonEventArgs e) { new Login().Show(); this.Close(); }
}
