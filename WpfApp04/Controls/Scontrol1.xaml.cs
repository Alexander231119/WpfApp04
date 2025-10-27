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
    /// Interaction logic for Scontrol1.xaml
    /// </summary>
    public partial class Scontrol1 : UserControl
    {
        public TrafficSignal t;
        public Scontrol1()
        {
            InitializeComponent();
        }

        private void Grid_MouseRightButtonDown(object sender, MouseButtonEventArgs e)
        {
            string message = "сигнал \n" +
                "  Id: " + t.TrackObjectID.ToString() + "\n" +
                t.StartPointOnTrackKm + "-" + t.StartPointOnTrackPk + "-" + t.StartPointOnTrackM;// + "\n" +
                //t.EndPointOnTrackKm + "-" + t.EndPointOnTrackPk + "-" + t.EndPointOnTrackM;// + "\n" +
                                                                                           //t.Start.station;

            MessageBox.Show(message);
        }
    }
}
