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
    /// Interaction logic for BridgeControl.xaml
    /// </summary>
    public partial class BridgeControl : UserControl
    {
        public RailBridge k;
        public BridgeControl(RailBridge _railBridge)
        {
            InitializeComponent();
            k = _railBridge;
        }

        private void ControlGrid_MouseRightButtonDown(object sender, MouseButtonEventArgs e)
        {
            string message = "Мост " + "\n" +
                            "  Id: " + k.TrackObjectID.ToString() + "\n" +
                            k.StartPointOnTrackKm + "-" + k.StartPointOnTrackPk + "-" + k.StartPointOnTrackM + "\n" +
                            k.EndPointOnTrackKm + "-" + k.EndPointOnTrackPk + "-" + k.EndPointOnTrackM + "\n" +
                            "длина " + (k.End.RouteCoordinate - k.Start.RouteCoordinate);

            MessageBox.Show(message);
        }
    }
}
