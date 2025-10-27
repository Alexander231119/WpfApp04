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
    /// Interaction logic for UkspsControl.xaml
    /// </summary>
    public partial class UkspsControl : UserControl
    {
        public Uksps t;
        public UkspsControl(Uksps uksps_)
        {
            InitializeComponent();
            this.t = uksps_;
        }

        private void Polygon_MouseRightButtonDown(object sender, MouseButtonEventArgs e)
        {
            string message = "укспс " + "\n" +
                "  Id: " + t.TrackObjectID.ToString() + "\n" +
                t.StartPointOnTrackKm + "-" + t.StartPointOnTrackPk + "-" + t.StartPointOnTrackM;

            MessageBox.Show(message);
        }
    }
}
