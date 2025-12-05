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
using WpfApp04.ViewModels;

namespace WpfApp04.Controls
{
    /// <summary>
    /// Interaction logic for StationsTabControl.xaml
    /// </summary>
    public partial class StationsTabControl : UserControl
    {
        private AppDbRouteContextData _appData;
        public AppDbRouteContextData AppData
        {
            get => _appData;
            set
            {
                _appData = value;
                // Подписываемся на изменения если нужно
            }
        }
        public event RoutedEventHandler ImportInitialStationNamesToBaseClicked;

        public StationsTabControl()
        {
            InitializeComponent();
        }
        private void ImportInitialStationNamesToBaseButton_Click(object sender, RoutedEventArgs e)
        {
            List<Station> StationsToInsert = new List<Station>();

            foreach (var item in EgisToExportStationsGrid.SelectedItems)
            {
                StationsToInsert.Add((Station)item);
            }

            DbRouteQuery.ImportInitialStationsToDb(_appData.ConnectString, StationsToInsert);

            ImportInitialStationNamesToBaseClicked?.Invoke(sender, e);
            MessageBox.Show("ok");
        }
    }
}
