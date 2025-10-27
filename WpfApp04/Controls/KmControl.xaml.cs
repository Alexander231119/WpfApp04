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
using System.Windows.Navigation;
using System.Windows.Shapes;

namespace WpfApp04
{
    /// <summary>
    /// Interaction logic for KmControl.xaml
    /// </summary>
    public partial class KmControl : UserControl
    {
        public Kilometer klm;
        internal DataGrid kmgrid;
        public KmControl(Kilometer kilometer)
        {
            this.klm = kilometer;
            InitializeComponent();
        }

        private void rectangle_MouseRightButtonDown(object sender, MouseButtonEventArgs e)
        {
            if (kmgrid != null)
            {
                kmgrid.SelectedItem = klm;
                kmgrid.ScrollIntoView(klm);
                kmgrid.Focus();
            }
            

            

            KmEditMenu editMenu = new KmEditMenu(klm);
            //editMenu.kmControl= this;
            editMenu.KmNameTextBox.Text = klm.Km;
            editMenu.KmLengthTextBox.Text = klm.Length.ToString();
            //editMenu.KmLengthTextBox.Text = Math.Round((klm.End.RouteCoordinate - klm.Start.RouteCoordinate), 2).ToString();
            editMenu.ShowDialog();
        }

        private void rectangle_MouseLeftButtonDown(object sender, MouseButtonEventArgs e)
        {
            if (kmgrid != null)
            {
                kmgrid.SelectedItem = klm;
                kmgrid.ScrollIntoView(klm);
                kmgrid.Focus();
            }

            //string message = "КМ " + klm.Km + " длина " + Math.Round((klm.End.RouteCoordinate - klm.Start.RouteCoordinate),2).ToString();
            //MessageBox.Show(message);
        }
    }
}
