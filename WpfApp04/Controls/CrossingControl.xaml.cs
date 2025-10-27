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

namespace WpfApp04.Controls
{
    /// <summary>
    /// Interaction logic for CrossingControl.xaml
    /// </summary>
    public partial class CrossingControl : UserControl
    {
        public Crossing t;
        public CrossingControl(Crossing _t)
        {
            InitializeComponent();
            this.t = _t;
        }

        private void UserControl_MouseRightButtonDown(object sender, MouseButtonEventArgs e)
        {


            string message = "переезд " + "\n" +
                "  Id: " + t.TrackObjectID.ToString() + "\n" +
                t.StartPointOnTrackKm + "-" + t.StartPointOnTrackPk + "-" + t.StartPointOnTrackM;

            MessageBox.Show(message);
        }
    }
}
