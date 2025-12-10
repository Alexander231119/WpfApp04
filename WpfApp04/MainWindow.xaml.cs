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
using System.Data.OleDb;
using System.Data.Common;
using System.Globalization;
using System.Data.SqlClient;
using System.Reflection.PortableExecutable;
using System.Collections;
using System.Reflection;
using System.Drawing;
using Color = System.Drawing.Color;
using System.Dynamic;
using System.Windows.Ink;
using System.Data;
using System.ComponentModel;
using WpfApp04.Controls;
using WpfApp04.ViewModels;
using System.Windows.Media.Animation;
using Pen = System.Windows.Media.Pen;
using Brushes = System.Windows.Media.Brushes;
using System.IO;
using WpfAapp04;
using Microsoft.Extensions.Configuration;
using System.Collections.ObjectModel;
//using VideoLib;



namespace WpfApp04
{
    /// <summary>
    /// Interaction logic for MainWindow.xaml
    /// </summary>
    public partial class MainWindow : Window
    {

        public AppDbRouteContextData _appData = new AppDbRouteContextData();
        
        private PlatformsTabControlViewModel _platformsVm;


        public MainWindow()
        {

            InitializeComponent();
            this.DataContext = _appData;

            var config = ConfigLoader.Load();
            string connectionString = config.GetConnectionString("RailDB");


            _appData.EgisConnectionString = connectionString;
            _appData.EgisConnection = new SqlConnection(_appData.EgisConnectionString);

            _appData.RouteToShowInDataGrids = _appData.EgisRoute1;

            TabImportControl1.AppData=_appData;
            ObjectSearchTabControl1.AppData=_appData;
            BrakeChecksTabControl1.AppData = _appData;
            KmEditTabControl1.AppData = _appData;
            InclinesTabControl1.AppData = _appData;
            StationsTabControl1.AppData= _appData;
            EgisSearchControl1.AppData= _appData;
            SpeedEditTabControl1.AppData = _appData;
            MainToolBarControl.AppData = _appData;
            TrafficLightsTabControl1.AppData= _appData;
            PointOnTrackTabControl1.AppData= _appData;
            DbRouteEmapControl_1.AppData= _appData;
            
            KmEditTabControl1.KmGrid.ItemsSource = _appData.Route1.Kilometers;
            KmEditTabControl1.EgisKmGrid.ItemsSource = _appData.EgisRoute1.Kilometers;

            EgisSearchControl1.EgisStationsGrid.ItemsSource = _appData.EgisSelectedStations;
            EgisSearchControl1.EgisTrackGrid.ItemsSource = _appData.EgisSelectedTracks;

            //PlatformsTabControl1.EgisPlatformsGrid.ItemsSource = _appData.EgisRoute1.Platforms;
            // Создаем ViewModel
            _platformsVm = new PlatformsTabControlViewModel();
            // Привязываем к контролу
            PlatformsTabControl1.DataContext = _platformsVm;
            // Заполняем данными
            _platformsVm.Platforms = new ObservableCollection<Platform>(_appData.RouteToShowInDataGrids.Platforms);
            
            DbRouteEmapControl_1.dbElectronicMap = _appData.RoutesElectronicMap;

            DbRouteEmapControl_1.RouteSelected += OnRouteSelected;
            _appData.DbDataChanged += OnDbDataChanged;// данные в mdb были изменены, например сохранение или выполнение отдельного запроса

            // В конструкторе MainWindow подписываемся на событие изменения фильтров 
            PointOnTrackTabControl1.ImportOptionsControl2.FilterChanged += () =>
            {
                var source = EgisSearchControl1.StationDataGridSourceRadioButtonEgis.IsChecked == true ?
                    _appData.EgisRoute1.PointOnTracks :
                    _appData.Route1.PointOnTracks;

                _appData.PointOnTracksToShow = PointOnTrackTabControl1.ImportOptionsControl2.FilterPoints(source).ToList();
                PointOnTrackTabControl1.PointOnTrackEditGrid.ItemsSource = _appData.PointOnTracksToShow;
                PointOnTrackTabControl1.PointOnTrackEditGrid.Items.Refresh();
            };

            EgisSearchControl1.StationDataGridSourceRadioButtonDb.IsChecked = true;

        }

        private void OnDbDataChanged(object? sender, EventArgs e)
        {
            // данные в mdb были изменены
            // загрузить данные заново и нарисовать маршрут

            ClearDataAndCanvas();
            LoadData(_appData.ConnectString, _appData.Route1);
            DrawRoute(wrapPanel, _appData.Route1, _appData.ToAddRoute);
            RefreshDataGridsItemsSources();
            //throw new NotImplementedException();
            Title = _appData.FileName;
        }

        private void OnRouteSelected(int mapId, int routeId)
        {
            // Вызываем ваш метод DrawRouteWay с нужными параметрами
            //DbRouteEmapControl_1.waywrapPanel.Children.Clear();

            //нарисовать маршрут из электронной карты по его ID (не _appData.EkDbRoute)
            //DrawRouteWay(DbRouteEmapControl_1.waywrapPanel, DbRouteEmapControl_1.dbElectronicMap.RoutesEkklubsList[mapId].RoutesList[routeId]);
        }

        public static class ConfigLoader
        {
            public static IConfigurationRoot Load()
            {
                string appPath = System.IO.Path.GetDirectoryName(Assembly.GetEntryAssembly().Location);
                string projectRoot = System.IO.Path.Combine(appPath, @"..\..\..");


                return new ConfigurationBuilder()
                    .SetBasePath(projectRoot)
                    .AddJsonFile("appsettings.json", optional: false)
                    .Build();
            }
        }

        private void Window_Closing(object sender, System.ComponentModel.CancelEventArgs e)
        {
            if (_appData.MyConnection != null)
                _appData.MyConnection.Close();
        }
        
        private void CloseFile_Click(object sender, RoutedEventArgs e)
        {
            
            Close();
        }

        private void ElectonicMap_menuItem_Click(object sender, RoutedEventArgs e)
        {
            DbRouteEmapControl_1.UpdatedbElectronicMapListBox();
        }

        private void OpenForImport_menuItem_Click(object sender, RoutedEventArgs e)
        {
            
            LoadData(_appData.ConnectString2, _appData.EgisRoute1);
            RefreshDataGridsItemsSources();

            EgisSearchControl1.EgisTrackTextBlock.Text = _appData.EgisRoute1.Kilometers.Count.ToString() + "  " + _appData.EgisRoute1.PointOnTracks.Count.ToString();
            
        }

        void ClearDataAndCanvas() 
        {
            wrapPanel.Children.Clear();
            wrapPanel.ClearVisuals();

            _appData.SegmentsToFillFromEgis.Clear();
            _appData.SegmentsSourseFromEgis.Clear();// добавлено при перенесении функций в контрол
            _appData.Route1.DbRouteClear();
            _appData.SelectedKilometersToEdit.Clear();

            _appData.ToAddRoute.DbRouteClear();
            _appData.PointOnTracksToAdd.Clear();
        }
        
        void LoadData(string cstring, DbRoute route)
        {
            _appData.EgisPtNormsGridLock = true;
            // загрузить с использованием метода из отдельного файла
            DbDataLoader loader = new DbDataLoader(cstring, route);
            loader.LoadData();
            _appData.EgisPtNormsGridLock = false;
        }
        
        void DrawRoute(DrawingCanvas _canvas, DbRoute _route1, DbRoute _toAddRoute)
        {
            // для отображения двух маршрутов
            DbRouteDrawer routeDrawer = new DbRouteDrawer();
            routeDrawer.widtscale = _appData.Widtscale;
            routeDrawer.heighscale = _appData.Heighscale;
            routeDrawer.kscale = _appData.Kscale;
            routeDrawer.lscale = _appData.Lscale;

            routeDrawer.DrawRoute(_canvas, _route1, _toAddRoute);
        }
        
        private void wrapPanel_MouseRightButtonDown(object sender, MouseButtonEventArgs e)
        {
            double mX = Mouse.GetPosition(wrapPanel).X;
            double mY = wrapPanel.ActualHeight - Mouse.GetPosition(wrapPanel).Y;

            _appData.LastX = mX;
            _appData.LastY = mY;
        }
        private void wrapPanel_MouseLeftButtonDown(object sender, MouseButtonEventArgs e)
        {
            
        }
        private void ScrollViewer_MouseWheel(object sender, MouseWheelEventArgs e)
        {
            
        }
        private void ScaletextBox_KeyDown(object sender, KeyEventArgs e)
        {
            wrapPanel.Children.Clear();
            DrawRoute(wrapPanel, _appData.Route1, _appData.ToAddRoute);

        }
        
        //удалить все SpeedrestrictionControl из canvas wrapPanel
        void RemoveAllSpeedControls()
        {
            bool scl = true;
            while (scl == true)
            {
                scl = removfirstspeedcontrol();
            }
            

            bool removfirstspeedcontrol()
            {
                foreach (UIElement child in wrapPanel.Children)
                {
                    if (child is SpeedRestrictionControl)
                    {
                        wrapPanel.Children.Remove(child);
                        return true;
                    }
                }
                return false;
            }
        }
        
        private void SpeedDataGrid_SpeedChanged(object sender, EventArgs e)
        {
            // вызывается когда пользователь внёс изменения скоростей например в SpeedTabControl
            RemoveAllSpeedControls();
            DbRouteDrawer routeDrawer = new DbRouteDrawer();
            routeDrawer.widtscale = _appData.Widtscale;
            routeDrawer.heighscale = _appData.Heighscale;
            routeDrawer.kscale = _appData.Kscale;
            routeDrawer.lscale = _appData.Lscale;

            routeDrawer.DrawSpeedrestrictions(wrapPanel, _appData.Route1, false);
            routeDrawer.DrawSpeedrestrictions(wrapPanel, _appData.ToAddRoute, true);
        }
        
        void LoadEgisData()
        {
            // используется двумя разными контролами

            EgisImporter egisImporter = new EgisImporter(_appData.EgisConnectionString, _appData.EgisRoute1) ;
            egisImporter.EgisSelectedTrack = _appData.EgisSelectedTrack;
            egisImporter._speedKindToFind = _appData.SpeedKindToFind;
            egisImporter._usageDirectionToFind = _appData.UsageDirectionToFind;


            if (_appData.EgisSelectedTrack != null)
            {
                //_appData.EgisPtNormsGridLock = true;
                egisImporter.LoadEgisData();
                RefreshDataGridsItemsSources();

                // информация о пути (длина в км и кол-во точек на пути) в текстовом блоке
                string message1 = "";
                message1 = _appData.EgisRoute1.Kilometers.Count.ToString() + "  " + _appData.EgisRoute1.PointOnTracks.Count.ToString();
                EgisSearchControl1.EgisTrackTextBlock.Text = message1;
                message1 = "";
                //_appData.EgisPtNormsGridLock = false;
            }

        }
        
        private void EgisLoadDataButton_Click(object sender, RoutedEventArgs e)
        {
            LoadEgisData();
        }
        
        private void AddSegmentsToFillFromEgisButton_Click(object sender, RoutedEventArgs e)
        {
            
        }

        private void FillFromEgisButton_Click(object sender, RoutedEventArgs e)
        {
            // после импорта обьектов из егис в toAddRoute
            // нарисовать всё заново но не загружать данные

            wrapPanel.Children.Clear();
            DrawRoute(wrapPanel, _appData.Route1, _appData.ToAddRoute);
            RefreshDataGridsItemsSources();
        }
        
        private void EgisFindPointObjectsButton_Click(object sender, RoutedEventArgs e)
        {
            //найти обьект в егис по названию обьекта и станции - перенесено в контрол

            EgisSearchControl1.EgisTrackGrid.Items.Refresh();
        }

        private void EgisFoundPointObjectsGrid_MouseDoubleClick(object sender, MouseButtonEventArgs e)
        {
            //загрузить обьекты по выбранному пути после поиска обьекта - перенесено в контрол

            LoadEgisData();
        }

        private void ClearToAddListsButtony_Click(object sender, RoutedEventArgs e)
        {

            // были очищены списки обьектов и точек на добавление
            //ClearToAddLists();

            wrapPanel.Children.Clear();
            DrawRoute(wrapPanel, _appData.Route1, _appData.ToAddRoute);
        }
        
        private void AddTrafficLightToAddList_Click(object sender, RoutedEventArgs e)
        {
            //добавлен светофор в trafficLightTabControl

        }

        private void AddPointOnTrackButton1_Click(object sender, RoutedEventArgs e)
        {
            // пользователь добавил точку на пути в PointOntrackTabControl

        }
        
        private void RefreshDataGridsItemsSources()
        {
            _appData.EgisPtNormsGridLock = true;

            // Применяем фильтр
            _appData.PointOnTracksToShow = PointOnTrackTabControl1.ImportOptionsControl2.FilterPoints(_appData.RouteToShowInDataGrids.PointOnTracks).ToList();
            PointOnTrackTabControl1.PointOnTrackEditGrid.ItemsSource = _appData.PointOnTracksToShow;
            PointOnTrackTabControl1.PointOnTrackEditGrid.Items.Refresh();

            StationsTabControl1.EgisToExportStationsGrid.ItemsSource = _appData.RouteToShowInDataGrids.Stations;
            StationsTabControl1.EgisToExportStationsGrid.Items.Refresh();
            TrafficLightsTabControl1.EgisToExportTrafficLightsGrid.ItemsSource = _appData.RouteToShowInDataGrids.TrafficLights;
            TrafficLightsTabControl1.EgisToExportTrafficLightsGrid.Items.Refresh();
            InclinesTabControl1.EgisToExportInclinesGrid.ItemsSource = _appData.RouteToShowInDataGrids.Inclines;
            InclinesTabControl1.EgisToExportInclinesGrid.Items.Refresh();

            //PlatformsTabControl1.EgisPlatformsGrid.ItemsSource = _appData.RouteToShowInDataGrids.Platforms;
            //PlatformsTabControl1.EgisPlatformsGrid.Items.Refresh();
            //if(_platformsVm!=null)  _platformsVm.Platforms = new ObservableCollection<Platform>(_appData.RouteToShowInDataGrids.Platforms);

            _platformsVm.Platforms = new ObservableCollection<Platform>(_appData.RouteToShowInDataGrids.Platforms);

            SpeedEditTabControl1.SpeedDataGrid.ItemsSource = _appData.RouteToShowInDataGrids.SpeedRestrictions;
            SpeedEditTabControl1.SpeedDataGrid.Items.Refresh();
            SpeedEditTabControl1.SpeedDataGrid.Items.SortDescriptions.Clear();
            SpeedEditTabControl1.SpeedDataGrid.Items.SortDescriptions.Add(new SortDescription("StartRouteCoordinate", ListSortDirection.Ascending));
            BrakeChecksTabControl1.EgisPtGrid.ItemsSource = _appData.RouteToShowInDataGrids.BrakeCheckPlaces;
            BrakeChecksTabControl1.EgisPtGrid.Items.Refresh();
            TabImportControl1.SegmentsToFillFromEgisGrid.ItemsSource = _appData.RouteToShowInDataGrids.Segments;
            TabImportControl1.SegmentsToFillFromEgisGrid.Items.Refresh();
            
            KmEditTabControl1.KmGrid.Items.Refresh();

            _appData.EgisPtNormsGridLock = false;
        }
        
        private void EgisSearchControl1_DataGridSorceRadioButtonChanged(object sender, EventArgs e)
        {
            RefreshDataGridsItemsSources();
        }
    }
}
