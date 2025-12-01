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
    /// Interaction logic for MainToolBar.xaml
    /// </summary>
    public partial class MainToolBar : UserControl
    {
        private AppDbRouteContextData _appData;
        public AppDbRouteContextData AppData
        {
            get => _appData;
            set
            {
                _appData = value;
            }
        }
        public event RoutedEventHandler OpenFileClicked;
        public event RoutedEventHandler OpenForImport_menuItemClicked;
        public event RoutedEventHandler CloseFileClicked;
        public event RoutedEventHandler ElectonicMapClicked;
        public event RoutedEventHandler EmapShowClicked;
        public event RoutedEventHandler TestClicked;
        public event RoutedEventHandler EgisConnectClicked;
        public event RoutedEventHandler EgisDisconnectClicked;
        public event RoutedEventHandler ExportSpeedsClicked;
        public event KeyEventHandler ScaleKeyDown;
        public event RoutedEventHandler ShowInclineControlClicked;
        public event RoutedEventHandler DeleteNopointSignClicked;
        public event RoutedEventHandler DelerteNoPointUkspsClicked;
        public event RoutedEventHandler DelerteNoPointKtsmClicked;
        public event RoutedEventHandler DelerteNoPointCrossingClicked;
        public event RoutedEventHandler DeleteNopointSign2Clicked;
        public event RoutedEventHandler DeleteAllInclinesClicked;
        public event RoutedEventHandler FrogModelMenuItemClicked;
        public event RoutedEventHandler AutoBlockFrequency1Clicked;
        public event RoutedEventHandler AutoBlockFrequency2Clicked;


        public MainToolBar()
        {
            InitializeComponent();
        }

        private void OpenFile_Click(object sender, RoutedEventArgs e)
        {
            OpenFileClicked?.Invoke(sender, e);
        }
        private void OpenForImport_menuItem_Click(object sender, RoutedEventArgs e)
        {
            OpenForImport_menuItemClicked?.Invoke(sender, e);
        }

        private void CloseFile_Click(object sender, RoutedEventArgs e)
        {
            CloseFileClicked?.Invoke(sender, e);
        }

        private void ElectonicMap_menuItem_Click(object sender, RoutedEventArgs e)
        {
            ElectonicMapClicked?.Invoke(sender, e);
        }

        private void EmapShowMenuItem_Click(object sender, RoutedEventArgs e)
        {
            EmapShowClicked?.Invoke(sender, e);
        }

        private void Test_Click(object sender, RoutedEventArgs e)
        {
            TestClicked?.Invoke(sender, e);
        }

        private void EgisConnectMenuItem_Click(object sender, RoutedEventArgs e)
        {
            EgisConnectClicked?.Invoke(sender, e);
        }

        private void EgisDisconnectMenuItem_Click(object sender, RoutedEventArgs e)
        {
            EgisDisconnectClicked?.Invoke(sender, e);
        }

        private void ExportSpeedsClick(object sender, RoutedEventArgs e)
        {
            // экспорт скоростей для базы с теми же
            var openFileDialog2 = new OpenFileDialog { };
            var result = openFileDialog2.ShowDialog();
            if (result != true) return;

            string fileName2 = openFileDialog2.FileName;
            //Title = fileName;
            _appData.ConnectString2 = _appData.ConnectString1 + fileName2 + ";";

            DbRouteDataExporter.SaveSpeedRestrictions(_appData.ConnectString2, _appData.Route1);
            
            MessageBox.Show("Экспортированы ограничения скорости" +
                            "\n " + fileName2 +
                            " \n всего: " + _appData.Route1.SpeedRestrictions.Count.ToString(), "постоянные ограничения скорости");


            ExportSpeedsClicked?.Invoke(sender, e);
        }

        private void ScaletextBox_KeyDown(object sender, KeyEventArgs e)
        {
            ScaleKeyDown?.Invoke(sender, e);
        }

        private void ShowInclineItemCLick(object sender, RoutedEventArgs e)
        {
            ShowInclineControlClicked?.Invoke(sender, e);
        }

        private void DeleteNopointSign_Click(object sender, RoutedEventArgs e)
        {
            DeleteNopointSignClicked?.Invoke(sender, e);
        }

        private void DeleteNoPointUksps_Click(object sender, RoutedEventArgs e)
        {
            DelerteNoPointUkspsClicked?.Invoke(sender, e);
        }

        private void DeleteNoPointKtsm_Click(object sender, RoutedEventArgs e)
        {
            DelerteNoPointKtsmClicked?.Invoke(sender, e);
        }

        private void DeleteNoPointCrossing_Click(object sender, RoutedEventArgs e)
        {
            DelerteNoPointCrossingClicked?.Invoke(sender, e);
        }

        private void DeleteNopointSign2_Click(object sender, RoutedEventArgs e)
        {
            DeleteNopointSign2Clicked?.Invoke(sender, e);
        }

        private void DeleteAllInclines_Click(object sender, RoutedEventArgs e)
        {
            DeleteAllInclinesClicked?.Invoke(sender, e);
        }

        private void FrogModelMenuItem_Click(object sender, RoutedEventArgs e)
        {
            FrogModelMenuItemClicked?.Invoke(sender, e);
        }

        private void AutoBlockFrequency1_Click(object sender, RoutedEventArgs e)
        {
            AutoBlockFrequency1Clicked?.Invoke(sender, e);
        }

        private void AutoBlockFrequency2_Click(object sender, RoutedEventArgs e)
        {
            AutoBlockFrequency2Clicked?.Invoke(sender, e);
        }
    }
}
