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
    /// Interaction logic for StationControl.xaml
    /// </summary>
    public partial class StationControl : UserControl
    {
        public StationControl()
        {
            InitializeComponent();
        }

        private void rectangle_MouseEnter(object sender, MouseEventArgs e)
        {
            // StartCoord.Visibility = Visibility.Visible;
           // EndCoord.Visibility = Visibility.Visible;
            // StartKM.Visibility = Visibility.Visible;
            // EndKM.Visibility = Visibility.Visible;
        
        }

        private void rectangle_MouseLeave(object sender, MouseEventArgs e)
        {
          //  StartCoord.Visibility = Visibility.Hidden;
          //  EndCoord.Visibility = Visibility.Hidden;
          //  StartKM.Visibility = Visibility.Hidden;
          //  EndKM.Visibility = Visibility.Hidden;

        }
    }
}
