using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Data;
using System.Windows.Documents;
using System.Windows.Input;
using System.Windows.Media;
using System.Windows.Media.Imaging;
using System.Windows.Shapes;

namespace APPingSoft_II.Sistema
{
    /// <summary>
    /// Lógica de interacción para GestionEvaluaciones.xaml
    /// </summary>
    public partial class GestionEvaluaciones : Window
    {
        public GestionEvaluaciones()
        {
            InitializeComponent();
        }
        private void IrHome(object sender, MouseButtonEventArgs e)
        {
            new Home().Show();
            this.Close();
        }

        private void IrGestionProgramas(object sender, MouseButtonEventArgs e)
        {
            new GestionProgramas().Show();
            this.Close();
        }

        private void IrGestionEvaluaciones(object sender, MouseButtonEventArgs e)
        {
            new GestionEvaluaciones().Show();
            this.Close();
        }

        private void IrRegistrarResultados(object sender, MouseButtonEventArgs e)
        {
            new RegistroResultados().Show();
            this.Close();
        }

        private void IrReportes(object sender, MouseButtonEventArgs e)
        {
            new ReporteMetricas().Show();
            this.Close();
        }
    }
}
