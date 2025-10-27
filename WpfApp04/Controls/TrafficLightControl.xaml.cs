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
    /// Interaction logic for TrafficLightControl.xaml
    /// </summary>
    public partial class TrafficLightControl : UserControl
    {
        public TrafficLight t;
        public TrafficLightControl( TrafficLight t)
        {
            this.t = t;
            InitializeComponent();
        }

        private void Grid1_MouseRightButtonDown(object sender, MouseButtonEventArgs e)
        {
            string message = t.TrafficLightName + "\n" + 
                "  Id: " + t.TrackObjectID.ToString() + "\n" +
                t.StartPointOnTrackKm + "-"+ t.StartPointOnTrackPk + "-" + t.StartPointOnTrackM + "\n"+
                t.DicTrafficLightKindName + "\n"+
                t.Station;

            MessageBox.Show(message);
        }
    }
}
