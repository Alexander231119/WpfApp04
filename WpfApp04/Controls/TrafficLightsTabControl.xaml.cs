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
    /// Interaction logic for TrafficLightsTabControl.xaml
    /// </summary>
    public partial class TrafficLightsTabControl : UserControl
    {
        public event SelectionChangedEventHandler EgisToExportTrafficLightsGridSelectionChanged;
        public event RoutedEventHandler AddTrafficLightToAddListClicked;
        public event RoutedEventHandler InsertTrafficLightsToDbClicked;
        public event RoutedEventHandler SetAll4AbValueClicked;

        public TrafficLightEditControl TrafficLightEditControl => trafficLightEditControlMenu;

        public TrafficLightsTabControl()
        {
            InitializeComponent();
        }

        private void EgisToExportTrafficLightsGrid_SelectionChanged(object sender, SelectionChangedEventArgs e)
        {
            EgisToExportTrafficLightsGridSelectionChanged?.Invoke(sender, e);
        }

        private void AddTrafficLightToAddList_Click(object sender, RoutedEventArgs e)
        {
            AddTrafficLightToAddListClicked?.Invoke(sender, e);
        }

        private void InsertTrafficLightsToDb_button_Click(object sender, RoutedEventArgs e)
        {
            InsertTrafficLightsToDbClicked?.Invoke(sender, e);
        }

        private void SetAll4AbValue_button_Click(object sender, RoutedEventArgs e)
        {
            SetAll4AbValueClicked?.Invoke(sender, e);
        }
    }
}
