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
    /// Interaction logic for NeutralControl2.xaml
    /// </summary>
    public partial class NeutralControl2 : UserControl
    {
        public NeutralSection t;
        public NeutralControl2(NeutralSection _NeutralSection)
        {
            this.t = _NeutralSection;
            InitializeComponent();
        }

        private void Rectangle0_MouseRightButtonDown(object sender, MouseButtonEventArgs e)
        {
            string message = "нейтральная вставка \n" +
                             "  Id: " + t.TrackObjectID.ToString() + "\n" +
                             t.StartPointOnTrackKm + "-" + t.StartPointOnTrackPk + "-" + t.StartPointOnTrackM + "\n" +
                             t.EndPointOnTrackKm + "-" + t.EndPointOnTrackPk + "-" + t.EndPointOnTrackM;// + "\n" +
            //t.Start.station;

            MessageBox.Show(message);
        }
    }
}
