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
    /// Interaction logic for StationsTabControl.xaml
    /// </summary>
    public partial class StationsTabControl : UserControl
    {
        public event RoutedEventHandler ImportInitialStationNamesToBaseClicked;
        public StationsTabControl()
        {
            InitializeComponent();
        }
        private void ImportInitialStationNamesToBaseButton_Click(object sender, RoutedEventArgs e)
        {
            ImportInitialStationNamesToBaseClicked?.Invoke(sender, e);
        }
    }
}
