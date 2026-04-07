using APPingSoft_II.Logic;
using APPingSoft_II.Logic.Windows;
using System.Windows;
using System.Windows.Input;

namespace APPingSoft_II.Sistema;

public partial class Home : Window
{
    private readonly HomeLogic _logic = new();

    public Home()
    {
        InitializeComponent();
        Loaded += Home_Loaded;
    }

    private void Home_Loaded(object sender, RoutedEventArgs e)
    {
        var nombre = _logic.ObtenerNombreUsuario();
        var rol    = _logic.ObtenerRolUsuario();
        if (!string.IsNullOrEmpty(nombre))
        {
            lblBienvenida.Text = $"Bienvenido, {nombre}";
            lblRol.Text        = rol;
        }
        AplicarPermisosPorRol();
    }

    private void AplicarPermisosPorRol()
    {
        navGestionProgramas.Visibility    = Permisos.VisibleSi(Permisos.PuedeAccederGestionProgramas());
        navParticipantes.Visibility       = Permisos.VisibleSi(Permisos.PuedeAccederGestionParticipantes());
        navGestionEvaluaciones.Visibility = Permisos.VisibleSi(Permisos.PuedeAccederGestionEvaluaciones());
        navGestionUsuarios.Visibility     = Permisos.VisibleSi(Permisos.PuedeAccederGestionUsuarios());
    }

    // ── Cerrar sesión ─────────────────────────────────────────────────────────

    private void BtnCerrarSesion_Click(object sender, RoutedEventArgs e)
    {
        _logic.CerrarSesion();
        var login = new Login();
        login.Show();
        this.Close();
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
