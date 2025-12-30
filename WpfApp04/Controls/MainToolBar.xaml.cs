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

        //public event RoutedEventHandler EgisConnectClicked;
        //public event RoutedEventHandler EgisDisconnectClicked;

        public event RoutedEventHandler ExportSpeedsClicked;

        public event KeyEventHandler ScaleEnterKeyDown;
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
            var openFileDialog = new OpenFileDialog { };
            var result = openFileDialog.ShowDialog();
            if (result != true) return;

            _appData.FileName = openFileDialog.FileName;
            
            _appData.ConnectString = _appData.ConnectString1 + _appData.FileName + ";";
            //OpenFileClicked?.Invoke(sender, e);
            _appData.DbData_Changed();
        }
        private void OpenForImport_menuItem_Click(object sender, RoutedEventArgs e)
        {
            var openFileDialog = new OpenFileDialog { };
            var result = openFileDialog.ShowDialog();
            if (result != true) return;

            string fileName2 = openFileDialog.FileName;
            _appData.ConnectString2 = _appData.ConnectString1 + fileName2 + ";";
            _appData.EgisRoute1.DbRouteClear();

            OpenForImport_menuItemClicked?.Invoke(sender, e);
        }

        private void CloseFile_Click(object sender, RoutedEventArgs e)
        {
            if (_appData.MyConnection != null) _appData.MyConnection.Close();
            CloseFileClicked?.Invoke(sender, e);
        }

        private void ElectonicMap_menuItem_Click(object sender, RoutedEventArgs e)
        {
            // Открыть ElectonicMap

            string mapfilename;
            var openFileDialog = new OpenFileDialog { };
            var result = openFileDialog.ShowDialog();
            if (result != true) return;

            mapfilename = openFileDialog.FileName;

            _appData.Map1 = _appData.Map1.Load(mapfilename);
            //_appData.EkDbRoute.DbRouteClear();
            //_appData.EkDbRoute.DbRouteFromEkRoute(_appData.Map1);


            _appData.RoutesElectronicMap.RoutesEkklubsList?.Clear();
            _appData.RoutesElectronicMap.DbRouteFromEkRoute(_appData.Map1);

            //_appData.EkDbRoute = _appData.RoutesElectronicMap.RoutesEkklubsList[9].RoutesList[2];
            //_appData.EkDbRoute = _appData.RoutesElectronicMap.RoutesEkklubsList[9].RoutesList[DbRouteEmapControl_1.routeId];

            ElectonicMapClicked?.Invoke(sender, e);
        }

        private void EmapShowMenuItem_Click(object sender, RoutedEventArgs e)
        {
            if (_appData.RoutesElectronicMap.RoutesEkklubsList.Count == 0) return;
            //_appData.EkDbRoute = _appData.RoutesElectronicMap.RoutesEkklubsList[DbRouteEmapControl_1.mapId].RoutesList[DbRouteEmapControl_1.routeId];
            EgisPreview egisPreview = new EgisPreview();

            DbRouteDrawer routeDrawer = new DbRouteDrawer()
            {
                widtscale = _appData.Widtscale,
                heighscale = _appData.Heighscale,
                kscale = _appData.Kscale,
                lscale = _appData.Lscale

            };
            routeDrawer.DrawRouteWay(egisPreview.EgisCanvas, _appData.EkDbRoute);

            egisPreview.Show();
            EmapShowClicked?.Invoke(sender, e);
        }

        private void Test_Click(object sender, RoutedEventArgs e)
        {
            string errorMessage = "";
            foreach (var s in _appData.Route1.SpeedRestrictions)
            {
                errorMessage += s.Start.CheckCoordinate(_appData.Route1.PointOnTracks, _appData.Route1.Segments) + s.End.CheckCoordinate(_appData.Route1.PointOnTracks, _appData.Route1.Segments);
            }

            //if ((_appData.Route1.Segments.Count > 0) && (_appData.Route1.Stations.Count > 0) && (_appData.Route1.SpeedRestrictions.Count > 0) && (_appData.Route1.Kilometers.Count > 0))
            //{
            //    wrapPanel.Children.Clear();
            //    _appData.Route1.SpeedRestrictions.Sort(_appData.Scts);
            //    DrawRoute(wrapPanel, _appData.Route1, _appData.ToAddRoute);
            //}

            if (errorMessage != "") MessageBox.Show(errorMessage);

            TestClicked?.Invoke(sender, e);
        }

        //private void EgisConnectMenuItem_Click(object sender, RoutedEventArgs e)
        //{
        //    EgisConnectClicked?.Invoke(sender, e);
        //}

        //private void EgisDisconnectMenuItem_Click(object sender, RoutedEventArgs e)
        //{
        //    EgisDisconnectClicked?.Invoke(sender, e);
        //}

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
            if (e.Key == Key.Enter)
            {
                _appData.Widtscale = Convert.ToDouble(ScaletextBox.Text) / 1000;
                ScaleEnterKeyDown?.Invoke(sender, e);
            }
        }

        private void ShowInclineItemCLick(object sender, RoutedEventArgs e)
        {
            // аоказать Inclinecontrol
            var window = new Window();
            InclineEditor inclineEditor1 = new Controls.InclineEditor(_appData.Route1.Segments, _appData.Route1.Kilometers, _appData.Route1.PointOnTracks, _appData.Route1.Inclines);
            //inclineEditor1.pointOnTracks = PointOnTracks;
            window.Content = inclineEditor1;
            window.Show();
            ShowInclineControlClicked?.Invoke(sender, e);
        }

        private void DeleteNopointSign_Click(object sender, RoutedEventArgs e)
        {
            //удалить знаки С без точки на пути
            DbRouteQuery.DeleteNoPointSigns(_appData.ConnectString);
            _appData.DbData_Changed();
            MessageBox.Show("удалены сигнальные знаки без точки на пути");

            //DeleteNopointSignClicked?.Invoke(sender, e);
        }
        private void DeleteNoPointTrafficLight_Click(object sender, RoutedEventArgs e)
        {
            //удалить светофоры без точки на пути
            DbRouteQuery.DeleteNoPointTrafficLight(_appData.ConnectString);
            _appData.DbData_Changed();
            MessageBox.Show("удалены светофоры без точки на пути");
        }
        private void DeleteNoPointUksps_Click(object sender, RoutedEventArgs e)
        {
            DbRouteQuery.DeleteNoFrameObjects(_appData.ConnectString, "", 16, 25);
            _appData.DbData_Changed();
            MessageBox.Show("удалены непроставленные укспс");
            //DelerteNoPointUkspsClicked?.Invoke(sender, e);
        }

        private void DeleteNoPointKtsm_Click(object sender, RoutedEventArgs e)
        {
            // удалить непроставленные КТСМ
            DbRouteQuery.DeleteNoFrameObjects(_appData.ConnectString, "", 15, 24);
            _appData.DbData_Changed();
            MessageBox.Show("удалены непроставленные КТСМ");
            //DelerteNoPointKtsmClicked?.Invoke(sender, e);
        }

        private void DeleteNoPointCrossing_Click(object sender, RoutedEventArgs e)
        {
            // удалить непроставленные переезды

            DbRouteQuery.DeleteNoFrameObjects(_appData.ConnectString, "Crossing", 9, 23);
            _appData.DbData_Changed();
            MessageBox.Show("удалены непроставленные переезды");
            //DelerteNoPointCrossingClicked?.Invoke(sender, e);
        }

        private void DeleteNopointSign2_Click(object sender, RoutedEventArgs e)
        {
            // удалить непроставленные знаки С

            DbRouteQuery.DeleteNoFrameObjects(_appData.ConnectString, "TrafficSignal", 21, 37);

            _appData.DbData_Changed();
            MessageBox.Show("удалены непроставленные знаки С");


            //DeleteNopointSign2Clicked?.Invoke(sender, e);
        }

        private void DeleteAllInclines_Click(object sender, RoutedEventArgs e)
        {
            // удалить все уклоны
            string tok = "Incline";
            DbRouteQuery.DeleteAllObjectsByKind(_appData.ConnectString, tok, 10, 32);

            _appData.DbData_Changed();
            MessageBox.Show("удалены все уклоны");

            //DeleteAllInclinesClicked?.Invoke(sender, e);
            
        }

        private void FrogModelMenuItem_Click(object sender, RoutedEventArgs e)
        {
            //установить марку крестовины 22 для всех стрелок
            DbRouteQuery.UpdateFrogModels(_appData.ConnectString);
            //FrogModelMenuItemClicked?.Invoke(sender, e);
            _appData.DbData_Changed();
        }

        private void AutoBlockFrequency1_Click(object sender, RoutedEventArgs e)
        {
            // частота алс 25
            DbRouteQuery.UpdateAutoBlockFrequency(_appData.ConnectString, 1);
            _appData.DbData_Changed();
            //AutoBlockFrequency1Clicked?.Invoke(sender, e);

        }

        private void AutoBlockFrequency2_Click(object sender, RoutedEventArgs e)
        {
            //частота алс 50
            DbRouteQuery.UpdateAutoBlockFrequency(_appData.ConnectString, 2);
            _appData.DbData_Changed();
            //AutoBlockFrequency2Clicked?.Invoke(sender, e);
        }

        
    }
}
