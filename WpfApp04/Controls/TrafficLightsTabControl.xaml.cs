using Microsoft.Win32;
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
    /// Interaction logic for TrafficLightsTabControl.xaml
    /// </summary>
    public partial class TrafficLightsTabControl : UserControl
    {
        private AppDbRouteContextData _appData;
        public AppDbRouteContextData AppData
        {
            get => _appData;
            set { _appData = value; }
        }
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
            trafficLightEditControlMenu.TrafficLight = (TrafficLight)EgisToExportTrafficLightsGrid.SelectedItem;
            trafficLightEditControlMenu.RefreshFromTrafficLight();

            EgisToExportTrafficLightsGridSelectionChanged?.Invoke(sender, e);
        }

        private void AddTrafficLightToAddList_Click(object sender, RoutedEventArgs e)
        {
            TrafficLight t = new TrafficLight();

            if (EgisToExportTrafficLightsGrid.ItemsSource == _appData.EgisRoute1.TrafficLights)
            { _appData.EgisRoute1.TrafficLights.Add(t); }
            else if (EgisToExportTrafficLightsGrid.ItemsSource == _appData.Route1.TrafficLights)
            { _appData.Route1.TrafficLights.Add(t); }
            //t.DicTrafficLightKindID = 20;
            EgisToExportTrafficLightsGrid.Items.Refresh();
            AddTrafficLightToAddListClicked?.Invoke(sender, e);
        }

        private void InsertTrafficLightsToDb_button_Click(object sender, RoutedEventArgs e)
        {
            //для ввода ограничений скоростей в уже имеющиеся светофоры
            // сохранение только tlispeedrestrictions
            // для чего сохраняет tlirestrictions но не светофоры?
            DbRouteQuery.InsertTrafficLightsToDb(_appData.ConnectString, _appData.Route1.TrafficLights);
            //DbRouteQuery.InsertTrafficLightsToDb(_appData.ConnectString, EgisToExportTrafficLightsGrid.ItemsSource);
            EgisToExportTrafficLightsGrid.Items.Refresh();

            _appData.DbData_Changed();
            //InsertTrafficLightsToDbClicked?.Invoke(sender, e);
        }

        private void SetAll4AbValue_button_Click(object sender, RoutedEventArgs e)
        {
            // применить четырёхзначную сигнализацию ко всем выбранным светофорам
            // использовать для светофоров в источнике данных например егис до импорта

            foreach (var item in EgisToExportTrafficLightsGrid.SelectedItems)
            {
                TrafficLight t = (TrafficLight)item;
                if (t.EgisABValue == 244) t.EgisABValue = 245;

            }

            EgisToExportTrafficLightsGrid.Items.Refresh();

            SetAll4AbValueClicked?.Invoke(sender, e);
        }

        private void ImportTliFromExcel_button_Click(object sender, RoutedEventArgs e)
        {
            var openFileDialog = new OpenFileDialog { };
            var result = openFileDialog.ShowDialog();
            if (result != true) return;

            _appData.TliExcelFileName = openFileDialog.FileName;


        }
    }
}
