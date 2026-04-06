using APPingSoft_II.Logic.Windows;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Input;

namespace APPingSoft_II.Sistema;

public partial class Login : Window
{
    private readonly LoginLogic _logic = new();
    private bool _mostrandoContrasena = false;

    public Login()
    {
        InitializeComponent();
        txtCorreo.Focus();
    }

    private void BtnIniciarSesion_Click(object sender, RoutedEventArgs e)
    {
        string correo = txtCorreo.Text.Trim();
        string contrasena = _mostrandoContrasena
            ? txtContrasenaVisible.Text
            : pwdContrasena.Password;

        lblError.Visibility = Visibility.Collapsed;

        var (ok, mensaje, _) = _logic.IniciarSesion(correo, contrasena);

        if (!ok)
        {
            lblError.Text = mensaje;
            lblError.Visibility = Visibility.Visible;
            return;
        }

        new Home().Show();
        this.Close();
    }

    private void BtnTogglePassword_MouseLeftButtonDown(object sender, MouseButtonEventArgs e)
    {
        _mostrandoContrasena = !_mostrandoContrasena;
        if (_mostrandoContrasena)
        {
            txtContrasenaVisible.Text = pwdContrasena.Password;
            pwdContrasena.Visibility = Visibility.Collapsed;
            txtContrasenaVisible.Visibility = Visibility.Visible;
            txtContrasenaVisible.Focus();
            lblToggleIcon.Text = "🙈";
        }
        else
        {
            pwdContrasena.Password = txtContrasenaVisible.Text;
            txtContrasenaVisible.Visibility = Visibility.Collapsed;
            pwdContrasena.Visibility = Visibility.Visible;
            pwdContrasena.Focus();
            lblToggleIcon.Text = "👁";
        }
    }

    private void TxtCorreo_KeyDown(object sender, KeyEventArgs e)
    {
        if (e.Key == Key.Enter) pwdContrasena.Focus();
    }

    private void PwdContrasena_KeyDown(object sender, KeyEventArgs e)
    {
        if (e.Key == Key.Enter) BtnIniciarSesion_Click(sender, e);
    }
}
