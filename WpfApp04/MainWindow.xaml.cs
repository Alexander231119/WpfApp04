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

            SpeedEditTabControl1.SpeedDataGrid.ItemsSource = _appData.Route1.SpeedRestrictions;

            KmEditTabControl1.AppData = _appData;
            KmEditTabControl1.KmGrid.ItemsSource = _appData.Route1.Kilometers;
            KmEditTabControl1.EgisKmGrid.ItemsSource = _appData.EgisRoute1.Kilometers;

            //KmEditTabControl1.SelectedKilometersToEdit = _appData.SelectedKilometersToEdit;
            //KmEditTabControl1.EgisPtNormsGridLock = _appData.EgisPtNormsGridLock;
            //KmEditTabControl1.EgisRoute1 = _appData.EgisRoute1;
            //KmEditTabControl1.Route1 = _appData.Route1;

            //KmEditTabControl1.ConnectString = _appData.ConnectString;

            

            EgisSearchControl1.EgisStationsGrid.ItemsSource = _appData.EgisSelectedStations;
            EgisSearchControl1.EgisTrackGrid.ItemsSource = _appData.EgisSelectedTracks;

            

            //PlatformsTabControl1.EgisPlatformsGrid.ItemsSource = _appData.EgisRoute1.Platforms;
            // Создаем ViewModel
            _platformsVm = new PlatformsTabControlViewModel();
            // Привязываем к контролу
            PlatformsTabControl1.DataContext = _platformsVm;
            // Заполняем данными
            _platformsVm.Platforms = new ObservableCollection<Platform>(_appData.RouteToShowInDataGrids.Platforms);
            //EgisSearchControl1.StationDataGridSourceRadioButtonEgis.IsChecked = true;

            TabImportControl1.SegmentsToFillFromEgisGrid.ItemsSource = _appData.Route1.Segments;
            ObjectSearchTabControl1.EgisFoundPointObjectsGrid.ItemsSource = _appData.EgisFoundPointObjects;
            TrafficLightsTabControl1.EgisToExportTrafficLightsGrid.ItemsSource = _appData.EgisRoute1.TrafficLights;
            StationsTabControl1.EgisToExportStationsGrid.ItemsSource = _appData.EgisRoute1.Stations;
            InclinesTabControl1.EgisToExportInclinesGrid.ItemsSource = _appData.EgisRoute1.Inclines;
            BrakeChecksTabControl1.EgisPtGrid.ItemsSource = _appData.EgisRoute1.BrakeCheckPlaces;

            DbRouteEmapControl_1.dbElectronicMap = _appData.RoutesElectronicMap;
            //DbRouteEmapControl_1._window = this;
            DbRouteEmapControl_1.RouteSelected += OnRouteSelected;


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
        }
        private void OnRouteSelected(int mapId, int routeId)
        {
            // Вызываем ваш метод DrawRouteWay с нужными параметрами
            DbRouteEmapControl_1.waywrapPanel.Children.Clear();
            DrawRouteWay(DbRouteEmapControl_1.waywrapPanel, DbRouteEmapControl_1.dbElectronicMap.RoutesEkklubsList[mapId].RoutesList[routeId]);
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

        private void Test_Click(object sender, RoutedEventArgs e)
        {
            string errorMessage = "";
            foreach (var s in _appData.Route1.SpeedRestrictions) 
            {
                errorMessage += s.Start.CheckCoordinate(_appData.Route1.PointOnTracks, _appData.Route1.Segments) + s.End.CheckCoordinate(_appData.Route1.PointOnTracks, _appData.Route1.Segments);
            }
            
            if ((_appData.Route1.Segments.Count > 0)&&(_appData.Route1.Stations.Count > 0)&&(_appData.Route1.SpeedRestrictions.Count > 0)&&(_appData.Route1.Kilometers.Count>0)) 
            {
                wrapPanel.Children.Clear();
                _appData.Route1.SpeedRestrictions.Sort(_appData.Scts);
                DrawRoute(wrapPanel, _appData.Route1, _appData.ToAddRoute);
            }
            if (errorMessage != "") MessageBox.Show(errorMessage);
        }


        private void CloseFile_Click(object sender, RoutedEventArgs e)
        {
            ClearDataAndCanvas();

            if (_appData.MyConnection != null)
            _appData.MyConnection.Close();
            Close();
        }

        private void ElectonicMap_menuItem_Click(object sender, RoutedEventArgs e)
        {

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
            
            DbRouteEmapControl_1.UpdatedbElectronicMapListBox();
        }

        private void OpenForImport_menuItem_Click(object sender, RoutedEventArgs e)
        {
            
            var openFileDialog = new OpenFileDialog { };
            var result = openFileDialog.ShowDialog();
            if (result != true) return;

            string fileName2 = openFileDialog.FileName;
            //Title = fileName;
            string ConnectString2 = _appData.ConnectString1 + fileName2 + ";";

            _appData.EgisPtNormsGridLock = true;
            //ClearDataAndCanvas();
            _appData.EgisRoute1.DbRouteClear();
            LoadData(ConnectString2, _appData.EgisRoute1);


            //PlatformsTabControl1.EgisPlatformsGrid.Items.Refresh();
            // Вместо прямого доступа к контролу:
            // Обновляем через ViewModel:
            //_platformsVm.Platforms = new ObservableCollection<Platform>(_appData.EgisRoute1.Platforms);

            InclinesTabControl1.EgisToExportInclinesGrid.ItemsSource = _appData.EgisRoute1.Inclines;
            InclinesTabControl1.EgisToExportInclinesGrid.Items.Refresh();
            StationsTabControl1.EgisToExportStationsGrid.Items.Refresh();
            BrakeChecksTabControl1.EgisPtGrid.Items.Refresh();


            string message1 = "";
            message1 = _appData.EgisRoute1.Kilometers.Count.ToString() + "  " + _appData.EgisRoute1.PointOnTracks.Count.ToString();
            EgisSearchControl1.EgisTrackTextBlock.Text = message1;
            message1 = "";


            _appData.EgisPtNormsGridLock = false;
        }

        private void OpenFile_Click(object sender, RoutedEventArgs e)
        {
            var openFileDialog = new OpenFileDialog { };
                var result = openFileDialog.ShowDialog();
            if (result != true) return;

            _appData.FileName = openFileDialog.FileName;
            Title = _appData.FileName;
            _appData.ConnectString = _appData.ConnectString1 + _appData.FileName + ";";
            //_appData.ConnectString = _appData.ConnectSrting1 + _appData.FileName + ";";

            //KmEditTabControl1.ConnectString = _appData.ConnectString;



            ClearDataAndCanvas();
            LoadData(_appData.ConnectString, _appData.Route1);
            
            DrawRoute(wrapPanel, _appData.Route1, _appData.ToAddRoute);

            // сортировка таблицы по координате по маршруту

            //SpeedEditTabControl1.SpeedDataGrid.Items.SortDescriptions.Clear();
            //SpeedEditTabControl1.SpeedDataGrid.Items.SortDescriptions.Add(new SortDescription("StartRouteCoordinate", ListSortDirection.Ascending));
            
            //SpeedEditTabControl1.SpeedDataGrid.Items.Refresh();
            //SpeedEditTabControl1.SpeedDataGrid.SelectedIndex = 0;
            //KmEditTabControl1.KmGrid.Items.Refresh();

            RefreshDataGridsItemsSources();
            
        }

        void ClearDataAndCanvas() 
        {
            wrapPanel.Children.Clear();
            wrapPanel.ClearVisuals();
            _appData.SegmentsToFillFromEgis.Clear();
            _appData.Route1.DbRouteClear();
            _appData.SelectedKilometersToEdit.Clear();
            ClearToAddLists();
        }


        void ClearToAddLists()
        {
            _appData.ToAddRoute.DbRouteClear();
            _appData.PointOnTracksToAdd.Clear();
        }

        #region Load & Fill Data

        void LoadData(string cstring, DbRoute route)
        {
            _appData.EgisPtNormsGridLock = true;
            // загрузить с использованием метода из отдельного файла
            DbDataLoader loader = new DbDataLoader(cstring, route);
            loader.LoadData();
            _appData.EgisPtNormsGridLock = false;
        }

        #endregion

        #region Draw

        void DrawRoute(DrawingCanvas _canvas, DbRoute _route1, DbRoute _toAddRoute)
        {
            // для отображения двух маршрутов
            DbRouteDrawer routeDrawer = new DbRouteDrawer();
            routeDrawer.widtscale = _appData.Widtscale;
            routeDrawer.heighscale = _appData.Heighscale;
            //routeDrawer.maxSpeed = _appData.MaxSpeed;
            //routeDrawer.segmentsBottom = _appData.SegmentsBottom;
            //routeDrawer.segmentsHeight = _appData.SegmentsHeight;
            //routeDrawer.kilometersBottom = _appData.KilometersBottom;
            //routeDrawer.kilometersHeight = _appData.KilometersHeight;
            //routeDrawer.pkLineBottom = _appData.PkLineBottom;
            //routeDrawer.pkLineHeight = _appData.PkLineHeight;
            //routeDrawer.inclineControlBottom = _appData.InclineControlBottom;
            //routeDrawer.floorBottom=_appData.FloorBottom;
            routeDrawer.kscale = _appData.Kscale;
            routeDrawer.lscale = _appData.Lscale;

            routeDrawer.DrawRoute(_canvas, _route1, _toAddRoute);

            
        }

        public void DrawRouteWay(DrawingCanvas _canvas, DbRoute _route)
        {
            //для отображения одного маршррута
            DbRouteDrawer routeDrawer = new DbRouteDrawer();
            routeDrawer.widtscale = _appData.Widtscale;
            routeDrawer.heighscale = _appData.Heighscale;
            routeDrawer.kscale = _appData.Kscale;
            routeDrawer.lscale = _appData.Lscale;

            routeDrawer.DrawRouteWay(_canvas, _route);

        }
        #endregion

        #region Speedrestriction & DataPerform

        public void SaveSpeedrestrictions(string _connectstring)
        {
            
                DbRouteDataExporter.SaveSpeedRestrictions(_appData.ConnectString, _appData.Route1);

                MessageBox.Show("Введены ограничения скорости \n всего: " +
                                _appData.Route1.SpeedRestrictions.Count.ToString(), "постоянные ограничения скорости");
                // считываем данные из базы заново после сохранения
                ClearDataAndCanvas();
                LoadData(_appData.ConnectString, _appData.Route1);
                DrawRoute(wrapPanel, _appData.Route1, _appData.ToAddRoute);
            
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
            if (e.Key == Key.Enter) 
            {
                _appData.Widtscale = Convert.ToDouble(MainToolBarControl.ScaletextBox.Text)/1000;
                wrapPanel.Children.Clear();
                DrawRoute(wrapPanel, _appData.Route1, _appData.ToAddRoute);
            }
        }

        private void SaveSpeedsClick(object sender, RoutedEventArgs e)
        {
            SaveSpeedrestrictions(_appData.ConnectString);
        }

        private void SaveSpeedButton_Click(object sender, RoutedEventArgs e)
        {
            SaveSpeedrestrictions(_appData.ConnectString);
        }

        private void ExportSpeedsClick(object sender, RoutedEventArgs e)
        {
            var openFileDialog2 = new OpenFileDialog { };
            var result = openFileDialog2.ShowDialog();
            if (result != true) return;

            string fileName2 = openFileDialog2.FileName;
            //Title = fileName;
            string ConnectString2 = _appData.ConnectString1 + fileName2 + ";";
            
            SaveSpeedrestrictions(ConnectString2);

        }

        //
        void SpeedChangedPerform(double index)
        {
            //MessageBox.Show("SpeedChangedPerform index"+ index.ToString());
            //wrapPanel.Children.Clear();
            _appData.Route1.SpeedRestrictions.Sort(_appData.Scts);
            //DrawRoute(wrapPanel, _appData.Route1, _appData.ToAddRoute);
            SpeedEditTabControl1.SpeedDataGrid.Items.Refresh();
            RemoveAllSpeedControls();

            DbRouteDrawer routeDrawer = new DbRouteDrawer();
            routeDrawer.widtscale = _appData.Widtscale;
            routeDrawer.heighscale = _appData.Heighscale;
            routeDrawer.kscale = _appData.Kscale;
            routeDrawer.lscale = _appData.Lscale;

            routeDrawer.DrawSpeedrestrictions(wrapPanel, _appData.Route1, false);
            routeDrawer.DrawSpeedrestrictions(wrapPanel, _appData.ToAddRoute, true);
            
        }


        private void DeleteSpeedButton_Click(object sender, RoutedEventArgs e)
        {
            SpeedRestriction item = (SpeedRestriction)SpeedEditTabControl1.SpeedDataGrid.SelectedItem;
            _appData.Route1.SpeedRestrictions.Remove(item);
            RefreshSpeedDataGrid();
            //wrapPanel.Children.Clear();
            //DrawRoute(wrapPanel, _appData.Route1, _appData.ToAddRoute);
            RemoveAllSpeedControls();

            DbRouteDrawer routeDrawer = new DbRouteDrawer();
            routeDrawer.widtscale = _appData.Widtscale;
            routeDrawer.heighscale = _appData.Heighscale;
            routeDrawer.kscale = _appData.Kscale;
            routeDrawer.lscale = _appData.Lscale;

            routeDrawer.DrawSpeedrestrictions(wrapPanel, _appData.Route1, false);

        }

        void RefreshSpeedDataGrid() 
        {
            int selectedrow = SpeedEditTabControl1.SpeedDataGrid.SelectedIndex;

            SpeedEditTabControl1.SpeedDataGrid.ItemsSource = null;
            SpeedEditTabControl1.SpeedDataGrid.ItemsSource = _appData.Route1.SpeedRestrictions;
            SpeedEditTabControl1.SpeedDataGrid.Items.SortDescriptions.Clear();
            SpeedEditTabControl1.SpeedDataGrid.Items.SortDescriptions.Add(new SortDescription("StartRouteCoordinate", ListSortDirection.Ascending));
            SpeedEditTabControl1.SpeedDataGrid.Items.Refresh();

            SpeedEditTabControl1.SpeedDataGrid.SelectedIndex = selectedrow;

        }

        private void AddSpeedButton_Click(object sender, RoutedEventArgs e)
        {
            if (_appData.Route1.Segments.Count > 0)
            {
                SpeedRestriction item = (SpeedRestriction)SpeedEditTabControl1.SpeedDataGrid.SelectedItem;

                SpeedRestriction spdin = null;

                if (item != null)
                {
                    spdin = new SpeedRestriction(item);
                }
                else
                {
                    spdin = new SpeedRestriction(80, 0, 0);
                }

                SpeedEditMenu menu = new SpeedEditMenu(spdin);

                menu.Station.Text = spdin.Station?.ToString();
                menu.SegmentId.Text = spdin.Start.SegmentID.ToString();
                menu.Value.Text = "";
                menu.StartKm.Text = "";
                menu.StartPk.Text = "";
                menu.StartM.Text = "";
                menu.EndKm.Text = "";
                menu.EndPk.Text = "";
                menu.EndM.Text = "";
                menu.StartRouteCoordinate.Text = "";
                menu.EndRouteCoordinate.Text = "";
                menu.Length.Text = "";


                int sindex = _appData.Route1.Segments.FindIndex(y => (y.SegmentID == spdin.Start.SegmentID));
                int tindex = -1;
                if (sindex >= 0)
                {
                    tindex = _appData.Route1.Tracks.FindIndex(x => (x.TrackID == _appData.Route1.Segments[sindex].TrackID));
                }
                else
                {
                    sindex = 0;
                    tindex = _appData.Route1.Tracks.FindIndex(x => (x.TrackID == _appData.Route1.Segments[sindex].TrackID));
                }

                if (tindex >= 0)
                {
                    menu.TrackName.Text = _appData.Route1.Tracks[tindex].TrackNumber.ToString() + " " + _appData.Route1.Tracks[tindex].TrackName;
                }


                if (menu.ShowDialog() == true)
                {

                    //rectangle1.Height = Convert.ToDouble(menu.Value.Text) * heightscale;
                    //rectangle2.Height = (200 - Convert.ToDouble(menu.Value.Text))*heightscale;
                    spdin.Start.SegmentID = spdin.End.SegmentID = Convert.ToDouble(menu.SegmentId.Text);

                    spdin.Start.PointOnTrackKm = menu.StartKm.Text;
                    spdin.Start.RefreshCoordinate(_appData.Route1.PointOnTracks, _appData.Route1.Segments);
                    spdin.End.RefreshCoordinate(_appData.Route1.PointOnTracks, _appData.Route1.Segments);

                    spdin.Start.RefreshRouteCoordinate(_appData.Route1.Segments);
                    spdin.End.RefreshRouteCoordinate(_appData.Route1.Segments);
                    spdin.Value = Convert.ToDouble(menu.Value.Text);

                    _appData.Route1.PointOnTracks.Add(spdin.Start);
                    _appData.Route1.PointOnTracks.Add(spdin.End);
                    _appData.Route1.SpeedRestrictions.Add(spdin);

                    RefreshSpeedDataGrid();

                    //wrapPanel.Children.Clear();
                    RemoveAllSpeedControls();

                    _appData.Route1.SpeedRestrictions.Sort(_appData.Scts);
                    _appData.Route1.PointOnTracks.Sort(_appData.Pcr);

                    DbRouteDrawer routeDrawer = new DbRouteDrawer();
                    routeDrawer.widtscale = _appData.Widtscale;
                    routeDrawer.heighscale = _appData.Heighscale;
                    routeDrawer.kscale = _appData.Kscale;
                    routeDrawer.lscale = _appData.Lscale;

                    routeDrawer.DrawSpeedrestrictions(wrapPanel, _appData.Route1, false);
                }

            }
        }
        
        private void DeleteAllSpeedButton_Click(object sender, RoutedEventArgs e)
        {
            _appData.Route1.SpeedRestrictions.Clear();
            RefreshSpeedDataGrid();
            RemoveAllSpeedControls();
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
        
        private void RouteCoordinateCheckBox_Checked(object sender, RoutedEventArgs e)
        {
            SpeedEditTabControl1.SpeedDataGrid.Columns[11].Visibility= Visibility.Visible;
        }

        private void RouteCoordinateCheckBox_Unchecked(object sender, RoutedEventArgs e)
        {
            SpeedEditTabControl1.SpeedDataGrid.Columns[11].Visibility = Visibility.Hidden;
        }

        

        private void SegmentIdTextBox_KeyDown(object sender, KeyEventArgs e)
        {
            if (e.Key == Key.Enter)
            {
                

            }
        }

        private void SpeedDataGrid_SelectionChanged(object sender, SelectionChangedEventArgs e)
        {
            SpeedRestriction item = (SpeedRestriction)SpeedEditTabControl1.SpeedDataGrid.SelectedItem;
            SpeedEditTabControl1.SegmentIdTextBox.Text = item?.Start.SegmentID.ToString();

            if (_appData.RowChanged == true)
            {

                foreach (SpeedRestriction s in _appData.Route1.SpeedRestrictions) 
                {
                    s.Start.RefreshCoordinate(_appData.Route1.PointOnTracks, _appData.Route1.Segments);
                    s.Start.RefreshRouteCoordinate(_appData.Route1.Segments);
                    s.End.RefreshCoordinate(_appData.Route1.PointOnTracks, _appData.Route1.Segments);
                    s.End.RefreshRouteCoordinate(_appData.Route1.Segments);
                }
                                                
                //wrapPanel.Children.Clear();
                RemoveAllSpeedControls();

                _appData.Route1.SpeedRestrictions.Sort(_appData.Scts);

                //DrawRoute(wrapPanel, _appData.Route1, _appData.ToAddRoute);


                DbRouteDrawer routeDrawer = new DbRouteDrawer();
                routeDrawer.widtscale = _appData.Widtscale;
                routeDrawer.heighscale = _appData.Heighscale;
                routeDrawer.kscale = _appData.Kscale;
                routeDrawer.lscale = _appData.Lscale;

                routeDrawer.DrawSpeedrestrictions(wrapPanel, _appData.Route1, false);

                //SpeedDataGrid.Items.Refresh();
                //SpeedDataGrid.Focus();
                _appData.RowChanged = false;
            }
        }

        private void SpeedDataGrid_RowEditEnding(object sender, DataGridRowEditEndingEventArgs e)
        {
            _appData.RowChanged = true;
        }
        
        private void SetSpeedSegmentIdButton_Click(object sender, RoutedEventArgs e)
        {
            SpeedRestriction item = (SpeedRestriction)SpeedEditTabControl1.SpeedDataGrid.SelectedItem;
            if (item != null)
            {

                item.Start.SegmentID = item.End.SegmentID = Convert.ToDouble(SpeedEditTabControl1.SegmentIdTextBox.Text);
                item.Start.RefreshCoordinate(_appData.Route1.PointOnTracks, _appData.Route1.Segments);
                item.Start.RefreshRouteCoordinate(_appData.Route1.Segments);
                item.End.RefreshCoordinate(_appData.Route1.PointOnTracks, _appData.Route1.Segments);
                item.End.RefreshRouteCoordinate(_appData.Route1.Segments);
            }
        }
        

        private void DbKmSetLengthButton_Click(object sender, RoutedEventArgs e)
        {
            // задать длину для выбранного километра

            //Kilometer klm = (Kilometer)KmEditTabControl1.KmGrid.SelectedItem;
            //if (klm is null) return;
            //klm.Length = Convert.ToDouble(KmEditTabControl1.DbKmTextBox.Text);
            

            //if (klm.Start.SegmentID == klm.End.SegmentID)
            //{
            //    DbRouteQuery.KmLengthSetPerform(_appData.ConnectString, klm, _appData.Route1);
            //    MessageBox.Show("Изменения внесены в " + _appData.FileName);
            //}
            
            ClearDataAndCanvas();
            LoadData(_appData.ConnectString, _appData.Route1);
            DrawRoute(wrapPanel, _appData.Route1, _appData.ToAddRoute);
            RefreshDataGridsItemsSources();

        }

        //private void KmGrid_SelectionChanged(object sender, SelectionChangedEventArgs e)
        //{
            
        //}
        
        private void SetKmGroupLengthWithEgisButton_Click(object sender, RoutedEventArgs e)
        {
            // задать общую длину для выбранных километров с учётом длины километров из егис

            //List<Kilometer> egisSourseKmlist = new List<Kilometer>();
            //double egisKmtotalLength = 0; // суммарная длина соотвесттвующих километров из егис
            //double selectedKmsTotalLength = 0; // суммарная длина выбранных км из базы

            //for (int i = 0; i < _appData.SelectedKilometersToEdit.Count; i++)
            //{
            //    Kilometer EgisKm = _appData.EgisRoute1.Kilometers.Find(x => x.Km == _appData.SelectedKilometersToEdit[i].Km);
            //    egisSourseKmlist.Add(EgisKm);

            //    selectedKmsTotalLength += _appData.SelectedKilometersToEdit[i].Length;
            //    egisKmtotalLength += EgisKm.Length;
            //}

            //for (int i = 0; i < _appData.SelectedKilometersToEdit.Count; i++)
            //{
            //    _appData.SelectedKilometersToEdit[i].Length = egisSourseKmlist[i].Length * (selectedKmsTotalLength / egisKmtotalLength);
            //    DbRouteQuery.KmLengthSetPerform(_appData.ConnectString, _appData.SelectedKilometersToEdit[i], _appData.Route1);
            //}
            ClearDataAndCanvas();
            LoadData(_appData.ConnectString, _appData.Route1);
            DrawRoute(wrapPanel, _appData.Route1, _appData.ToAddRoute);
            RefreshDataGridsItemsSources();
        }

        private void DbKmSegmentGroupSetLengthButton_Click(object sender, RoutedEventArgs e)
        {
            //задать общую длину для выбранных километров
            
            //if ((Kilometer)KmEditTabControl1.KmGrid.SelectedItem is null) return;

            //double klmLength = Convert.ToDouble(KmEditTabControl1.DbKmTextBox.Text);

            //foreach (Kilometer k in _appData.SelectedKilometersToEdit)
            //{
            //    if (k.Start.SegmentID == k.End.SegmentID)
            //        k.Length = klmLength;
            //        DbRouteQuery.KmLengthSetPerform(_appData.ConnectString,k, _appData.Route1);
            //}

            ClearDataAndCanvas();
            LoadData(_appData.ConnectString, _appData.Route1);
            DrawRoute(wrapPanel, _appData.Route1, _appData.ToAddRoute);
            RefreshDataGridsItemsSources();
        }

        #endregion

        #region Егис

        private void EgisConnectMenuItem_Click(object sender, RoutedEventArgs e)
        {
            string sql = "SELECT StationID, StationName FROM Station WHERE StationName like '%никель-мурманский%'";
            try
            {
                _appData.EgisConnection.Open();
                
            }
            catch (Exception ex)
            {
                MessageBox.Show(ex.Message);
            }
        }

        private void EgisDisconnectMenuItem_Click(object sender, RoutedEventArgs e)
        {
            if (_appData.EgisConnection != null)
                _appData.EgisConnection.Close();
        }
        private void EgisFindStationButton_Click(object sender, RoutedEventArgs e)
        {
            EgisSearchControl1.StaitonToFindTextBox.Text = DbRouteHelper.ConvertEngToRus(EgisSearchControl1.StaitonToFindTextBox.Text);
            EgisImporter.SelectStations(EgisSearchControl1.StaitonToFindTextBox.Text, _appData.EgisConnection, _appData.EgisSelectedStations);
            EgisSearchControl1.EgisStationsGrid.Items.Refresh();
            EgisSearchControl1.EgisStationsGrid.SelectedIndex = 0;
        }
        
        void EgisSelectTrack()
        {
            Station item = (Station)EgisSearchControl1.EgisStationsGrid.SelectedItem;
            EgisImporter.SelectTrack(item,(bool)EgisSearchControl1.MaintrackRadioButton.IsChecked, _appData.EgisConnection, _appData.EgisSelectedTracks);
            EgisSearchControl1.EgisTrackGrid.Items.Refresh();
        }

        //найти обьект в егис по названию обьекта и станции
        void EgisFindPointObject()
        {
            Station item = (Station)EgisSearchControl1.EgisStationsGrid.SelectedItem;
            string EgisStationID;
            if (item == null)
            {
                EgisStationID = "";
            }
            else
            {
                EgisStationID = item.EgisStationID.ToString();
            }
            string ObjectNameToFind = ObjectSearchTabControl1.PointObjectToFindTextBox.Text;
            string stationnametofind = EgisSearchControl1.StaitonToFindTextBox.Text;

            _appData.EgisSelectedTracks.Clear();
            _appData.EgisFoundPointObjects.Clear();

            EgisImporter.EgisFindPointObject(EgisStationID, ObjectNameToFind, stationnametofind, _appData.EgisConnectionString, _appData.EgisSelectedTracks, _appData.EgisFoundPointObjects);
            EgisSearchControl1.EgisTrackGrid.Items.Refresh();
            ObjectSearchTabControl1.EgisFoundPointObjectsGrid.Items.Refresh();
        }

        
        
        void LoadEgisData() 
        {
            //_appData.EgisSelectedTrack = (Track)EgisTrackGrid.SelectedItem;


            EgisImporter egisImporter = new EgisImporter(_appData.EgisConnectionString, _appData.EgisRoute1) ;
            egisImporter.EgisSelectedTrack = _appData.EgisSelectedTrack;
            egisImporter._speedKindToFind = _appData.SpeedKindToFind;
            egisImporter._usageDirectionToFind = _appData.UsageDirectionToFind;


            if (_appData.EgisSelectedTrack != null)
            {
                _appData.EgisPtNormsGridLock = true;
                egisImporter.LoadEgisData();

                //PlatformsTabControl1.EgisPlatformsGrid.Items.Refresh();
                //_platformsVm.Platforms = new ObservableCollection<Platform>(_appData.RouteToShowInDataGrids.Platforms);

                InclinesTabControl1.EgisToExportInclinesGrid.ItemsSource = _appData.EgisRoute1.Inclines;
                InclinesTabControl1.EgisToExportInclinesGrid.Items.Refresh();
                StationsTabControl1.EgisToExportStationsGrid.Items.Refresh();
                BrakeChecksTabControl1.EgisPtGrid.Items.Refresh();

                string message1 = "";
                message1 = _appData.EgisRoute1.Kilometers.Count.ToString() + "  " + _appData.EgisRoute1.PointOnTracks.Count.ToString();
                EgisSearchControl1.EgisTrackTextBlock.Text = message1;
                message1 = "";
                _appData.EgisPtNormsGridLock = false;
            }

        }

        


        private void EmapShowMenuItem_Click(object sender, RoutedEventArgs e)
        {
            if (_appData.RoutesElectronicMap.RoutesEkklubsList.Count == 0) return;
            _appData.EkDbRoute = _appData.RoutesElectronicMap.RoutesEkklubsList[DbRouteEmapControl_1.mapId].RoutesList[DbRouteEmapControl_1.routeId];
            EgisPreview egisPreview = new EgisPreview();
            DrawRouteWay(egisPreview.EgisCanvas, _appData.EkDbRoute);
            egisPreview.Show();
        }
        
        private void ShowEgisPreviewButton_Click(object sender, RoutedEventArgs e)
        {
            EgisPreview egisPreview = new EgisPreview();
            egisPreview.Title = _appData.EgisSelectedTrack?.TrackNumber;

            DrawRouteWay(egisPreview.EgisCanvas, _appData.EgisRoute1);
            egisPreview.Show();
        }

        private void EgisTrackGrid_SelectionChanged(object sender, SelectionChangedEventArgs e)
        {
                   

        }

        private void EgisTrackGrid_MouseDoubleClick(object sender, MouseButtonEventArgs e)
        {
            _appData.EgisSelectedTrack = (Track)EgisSearchControl1.EgisTrackGrid.SelectedItem;
        }

        private void DownUsageDirectionToFindRadioButton_Checked(object sender, RoutedEventArgs e)
        {
            _appData.UsageDirectionToFind = -1;
        }

        private void UpUsageDirectionToFindRadioButton_Checked(object sender, RoutedEventArgs e)
        {
            _appData.UsageDirectionToFind = 1;
        }

        private void EgisStationsGrid_MouseDoubleClick(object sender, MouseButtonEventArgs e)
        {
            EgisSelectTrack();
        }

        private void MaintrackRadioButton_Checked(object sender, RoutedEventArgs e)
        {
            EgisSelectTrack();
        }

        private void SideTrackRadioButton_Checked(object sender, RoutedEventArgs e)
        {
            EgisSelectTrack();
        }

        private void EgisLoadDataButton_Click(object sender, RoutedEventArgs e)
        {
            
            if ((Track)EgisSearchControl1.EgisTrackGrid.SelectedItem != null)
            {
                _appData.EgisSelectedTrack = (Track)EgisSearchControl1.EgisTrackGrid.SelectedItem;
                LoadEgisData();
            }
        }

        private void StaitonToFindTextBox_KeyDown(object sender, KeyEventArgs e)
        {
            if (e.Key == Key.Enter)
            {
                //EgisSelectStations();
                EgisImporter.SelectStations(EgisSearchControl1.StaitonToFindTextBox.Text, _appData.EgisConnection, _appData.EgisSelectedStations);
                EgisSearchControl1.EgisStationsGrid.Items.Refresh();
                EgisSearchControl1.EgisStationsGrid.SelectedIndex = 0;

            }
        }

        private void AddSegmentsToFillFromEgisButton_Click(object sender, RoutedEventArgs e)
        {
            IList<Segment> targetCollection;
            TextBlock targetTextBlock;
            string message = "";

            if (TabImportControl1.SegmentsToFillFromEgisGrid.ItemsSource == _appData.Route1.Segments)
            {
                targetCollection = _appData.SegmentsToFillFromEgis;
                targetTextBlock = TabImportControl1.SegmentsToFillFromEgisTextBlock;
            }
            else if (TabImportControl1.SegmentsToFillFromEgisGrid.ItemsSource == _appData.EgisRoute1.Segments)
            {
                targetCollection = _appData.SegmentsSourseFromEgis;
                targetTextBlock = TabImportControl1.SegmentsSourceFromEgisTextBlock;
            }
            else
            {
                return; // Неизвестный источник данных
            }

            targetCollection.Clear();

            if (TabImportControl1.SegmentsToFillFromEgisGrid.SelectedItems.Count > 0)
            {
                foreach (var item in TabImportControl1.SegmentsToFillFromEgisGrid.SelectedItems)
                {
                    Segment s = (Segment)item;
                    targetCollection.Add(s);
                    message += s.SegmentID.ToString() + " ";
                }
            }

            targetTextBlock.Text = message;
        }

        private void FillFromEgisButton_Click(object sender, RoutedEventArgs e)
        {

            //заполнение с использованием обьект loader
            IList<Segment> SourceSegmentsCollection;
            if (_appData.SegmentsSourseFromEgis.Count > 0)
            {
                SourceSegmentsCollection = _appData.SegmentsSourseFromEgis;
            }
            else
            {
                SourceSegmentsCollection = _appData.EgisRoute1.Segments;
            }

            RouteToRouteLoader routeLoader = new RouteToRouteLoader(_appData.EgisRoute1, _appData.ToAddRoute, _appData.Route1, _appData.SegmentsToFillFromEgis, _appData.PointOnTracksToAdd);

            if (_appData.SegmentsSourseFromEgis.Count > 0)
            {
                routeLoader._segmentsSourseFromEgis = _appData.SegmentsSourseFromEgis;
            }
            else
            {
                routeLoader._segmentsSourseFromEgis = _appData.EgisRoute1.Segments;
            }

            ApplyImportCheckBoxes(routeLoader._routeExportCheckBoxList); 

            routeLoader.FillFromRouteToRoute();

            wrapPanel.Children.Clear();
            DrawRoute(wrapPanel, _appData.Route1, _appData.ToAddRoute);

            EgisSearchControl1.EgisStationsGrid.Items.Refresh();
            TrafficLightsTabControl1.EgisToExportTrafficLightsGrid.Items.Refresh();

        }
        
        private void InsertFromEgisToBaseButton_Click(object sender, RoutedEventArgs e)
        {
            DbRouteDataExporter drde = new DbRouteDataExporter(_appData.ConnectString, _appData.ToAddRoute, _appData.Route1, _appData.PointOnTracksToAdd);

            ApplyImportCheckBoxes(drde._routeExportCheckBoxList);
            drde.AddTrackObjectsFromDbRouteToBase();

            ClearDataAndCanvas();
            LoadData(_appData.ConnectString, _appData.Route1);
            DrawRoute(wrapPanel, _appData.Route1, _appData.ToAddRoute);

        }

        public void ApplyImportCheckBoxes(RouteExportCheckBoxList _list)
        {
            _list._DeleteTrackCircuitsChickBox = TabImportControl1.DeleteTrackCircuitsChickBox.IsChecked ?? false;
            TabImportControl1.ImportOptionsControl1.ApplyToCheckBoxList(_list);
        }
        
        private void EgisFindPointObjectsButton_Click(object sender, RoutedEventArgs e)
        {
            EgisFindPointObject();
        }

        private void EgisFoundPointObjectsGrid_MouseDoubleClick(object sender, MouseButtonEventArgs e)
        {
            PointOnTrack p = (PointOnTrack)ObjectSearchTabControl1.EgisFoundPointObjectsGrid.SelectedItem;
            _appData.EgisSelectedTrack.TrackID = p.TrackID;
            LoadEgisData();
        }

        private void ClearToAddListsButtony_Click(object sender, RoutedEventArgs e)
        {
            ClearToAddLists();
            wrapPanel.Children.Clear();
            DrawRoute(wrapPanel, _appData.Route1, _appData.ToAddRoute);
        }


        #endregion
        
        private void ShowInclineItemCLick(object sender, RoutedEventArgs e)
        {
            var window = new Window();
            InclineEditor inclineEditor1 = new Controls.InclineEditor(_appData.Route1.Segments, _appData.Route1.Kilometers, _appData.Route1.PointOnTracks, _appData.Route1.Inclines);
            //inclineEditor1.pointOnTracks = PointOnTracks;
            window.Content = inclineEditor1;
            window.Show();
        }
        

        private void ExportInclinesToExcelButton_Click(object sender, RoutedEventArgs e)
        {
            DbRouteQuery.SaveInclinesToCsvFile(InclinesTabControl1.EgisToExportInclinesGrid.ItemsSource);
            //DbRouteQuery.SaveInclinesToCsvFile(EgisToExportInclinesGrid.Items);
            //DbRouteQuery.SaveInclinesToCsvFile(_appData.Route1.Inclines);
        }


        private void FreightSpeedRadioButton_Checked(object sender, RoutedEventArgs e) {_appData.SpeedKindToFind = 2;}
        private void PassSpeedRadioButton_Copy1_Checked(object sender, RoutedEventArgs e) { _appData.SpeedKindToFind = 1; }
        private void HighSpeedRadioButton_Checked(object sender, RoutedEventArgs e) { _appData.SpeedKindToFind = 90; }
        private void VeryHighSpeedRadioButton_Checked(object sender, RoutedEventArgs e) { _appData.SpeedKindToFind = 87; }
        private void EtrainSpeedRadioButton_Checked(object sender, RoutedEventArgs e) { _appData.SpeedKindToFind = 89; }
        private void PrigSpeedRadioButton_Checked(object sender, RoutedEventArgs e) { _appData.SpeedKindToFind = 4; }
        private void MvpsSpeedRadioButton_Checked(object sender, RoutedEventArgs e) { _appData.SpeedKindToFind = 3; }

        private void AddTrafficLightToAddList_Click(object sender, RoutedEventArgs e)
        {

            TrafficLight t = new TrafficLight();

            if (TrafficLightsTabControl1.EgisToExportTrafficLightsGrid.ItemsSource == _appData.EgisRoute1.TrafficLights)
            {_appData.EgisRoute1.TrafficLights.Add(t);}
            else if (TrafficLightsTabControl1.EgisToExportTrafficLightsGrid.ItemsSource == _appData.Route1.TrafficLights)
            { _appData.Route1.TrafficLights.Add(t);}
            //t.DicTrafficLightKindID = 20;
            TrafficLightsTabControl1.EgisToExportTrafficLightsGrid.Items.Refresh();
        }

        private void AddPointOnTrackButton1_Click(object sender, RoutedEventArgs e)
        {
            PointOnTrack p = new PointOnTrack();
            _appData.Route1.PointOnTracks.Add(p);

            p.DicPointOnTrackKindID = 25; // по умолчанию укспс
            _appData.PointOnTracksToShow = PointOnTrackTabControl1.ImportOptionsControl2.FilterPoints(_appData.RouteToShowInDataGrids.PointOnTracks).ToList();
            PointOnTrackTabControl1.PointOnTrackEditGrid.ItemsSource = _appData.PointOnTracksToShow;
            PointOnTrackTabControl1.PointOnTrackEditGrid.Items.Refresh();

        }

        private void InsertTrafficLightsToDb_button_Click(object sender, RoutedEventArgs e)
        {

            // сохранение tlispeedrestrictions cтранно
            // для чего сохраняет tlirestrictions но не светофоры?
            DbRouteQuery.InsertTrafficLightsToDb(_appData.ConnectString, TrafficLightsTabControl1.EgisToExportTrafficLightsGrid.ItemsSource);
            return;
        }
        private void SetAll4AbValue_button_Click(object sender, RoutedEventArgs e)
        {
            // применить четырёхзначную сигнализацию ко всем выбранным светофорам
            foreach (var item in TrafficLightsTabControl1.EgisToExportTrafficLightsGrid.SelectedItems)
            {
                TrafficLight t = (TrafficLight)item;
                if (t.EgisABValue == 244) t.EgisABValue = 245;
                
            }

            TrafficLightsTabControl1.EgisToExportTrafficLightsGrid.Items.Refresh();
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

        private void StationDataGridSourceRadioButtonEgis_Checked(object sender, RoutedEventArgs e)
        {
            _appData.RouteToShowInDataGrids = _appData.EgisRoute1;
            RefreshDataGridsItemsSources();
        }

        private void StationDataGridSourceRadioButtonDb_Checked(object sender, RoutedEventArgs e)
        {
            _appData.RouteToShowInDataGrids = _appData.Route1;
            RefreshDataGridsItemsSources();
        }
        private void StationDataGridSourceRadioButtonToAdd_Checked(object sender, RoutedEventArgs e)
        {
            _appData.RouteToShowInDataGrids = _appData.ToAddRoute;
            RefreshDataGridsItemsSources();
        }

        private void EgisPtGrid_SelectionChanged(object sender, SelectionChangedEventArgs e)
        {
            if (_appData.EgisPtNormsGridLock == true) return;

            var item = BrakeChecksTabControl1.EgisPtGrid.SelectedItem;
            BrakeCheckPlace bcp = (BrakeCheckPlace)item;
            BrakeChecksTabControl1.EgisPtNormsGrid.ItemsSource = bcp.BrakeCheckNormList;
            BrakeChecksTabControl1.EgisPtNormsGrid.Items.Refresh();
        }

        private void EgisToExportTrafficLightsGrid_SelectionChanged(object sender, SelectionChangedEventArgs e)
        {
            TrafficLightsTabControl1.trafficLightEditControlMenu.TrafficLight = (TrafficLight)TrafficLightsTabControl1.EgisToExportTrafficLightsGrid.SelectedItem;
            TrafficLightsTabControl1.trafficLightEditControlMenu.RefreshFromTrafficLight();
        }

        private void ImportInitialStationNamesToBaseButton_Click(object sender, RoutedEventArgs e)
        {
            List<Station> StationsToInsert=new List<Station>();

            foreach (var item in StationsTabControl1.EgisToExportStationsGrid.SelectedItems)
            {
                StationsToInsert.Add((Station)item);
            }

            DbRouteQuery.ImportInitialStationsToDb(_appData.ConnectString, StationsToInsert);
            
            // считываем данные из базы заново после сохранения
            ClearDataAndCanvas();
            LoadData(_appData.ConnectString, _appData.Route1);
            DrawRoute(wrapPanel, _appData.Route1, _appData.ToAddRoute);
            MessageBox.Show("ok");
        }

        private void DeleteNopointSign_Click(object sender, RoutedEventArgs e)
        {
            // удалить знаки С которые не удалились полностью чере редактор
            DbRouteQuery.DeleteNoPointSigns(_appData.ConnectString);
            
            ClearDataAndCanvas();
            LoadData(_appData.ConnectString, _appData.Route1);
            DrawRoute(wrapPanel, _appData.Route1, _appData.ToAddRoute);
            MessageBox.Show("ok");
        }


        private void DeleteNopointSign2_Click(object sender, RoutedEventArgs e)
        {
            // удалить непроставленные знаки С

            DbRouteQuery.DeleteNoFrameObjects(_appData.ConnectString, "TrafficSignal", 21,37);
            
            ClearDataAndCanvas();
            LoadData(_appData.ConnectString, _appData.Route1);
            DrawRoute(wrapPanel, _appData.Route1, _appData.ToAddRoute);
            MessageBox.Show("ok");
        }

        private void DelerteNoPointUksps_Click(object sender, RoutedEventArgs e)
        {
            // удалить непроставленные УКСПС
            DbRouteQuery.DeleteNoFrameObjects(_appData.ConnectString, "", 16, 25);

            ClearDataAndCanvas();
            LoadData(_appData.ConnectString, _appData.Route1);
            DrawRoute(wrapPanel, _appData.Route1, _appData.ToAddRoute);
            MessageBox.Show("ok");
        }

        private void DelerteNoPointKtsm_Click(object sender, RoutedEventArgs e)
        {
            // удалить непроставленные КТСМ
            DbRouteQuery.DeleteNoFrameObjects(_appData.ConnectString, "", 15, 24);

            ClearDataAndCanvas();
            LoadData(_appData.ConnectString, _appData.Route1);
            DrawRoute(wrapPanel, _appData.Route1, _appData.ToAddRoute);
            MessageBox.Show("ok");
        }

        private void DelerteNoPointCrossing_Click(object sender, RoutedEventArgs e)
        {
            // удалить непроставленные переезды

            DbRouteQuery.DeleteNoFrameObjects(_appData.ConnectString, "Crossing", 9, 23);

            ClearDataAndCanvas();
            LoadData(_appData.ConnectString, _appData.Route1);
            DrawRoute(wrapPanel, _appData.Route1, _appData.ToAddRoute);
            MessageBox.Show("ok");
        }

        private void DeleteAllInclines_Click(object sender, RoutedEventArgs e)
        {
            // удалить все уклоны
            string tok = "Incline";
            DbRouteQuery.DeleteAllObjectsByKind(_appData.ConnectString,tok, 10,32);

            ClearDataAndCanvas();
            LoadData(_appData.ConnectString, _appData.Route1);
            DrawRoute(wrapPanel, _appData.Route1, _appData.ToAddRoute);
            MessageBox.Show("Уклоны удалены");
        }

        private void FrogModelMenuItem_Click(object sender, RoutedEventArgs e)
        {
            DbRouteQuery.UpdateFrogModels(_appData.ConnectString);
        }

        private void AutoBlockFrequency1_Click(object sender, RoutedEventArgs e)
        {
            DbRouteQuery.UpdateAutoBlockFrequency(_appData.ConnectString, 1);
        }

        private void AutoBlockFrequency2_Click(object sender, RoutedEventArgs e)
        {
            DbRouteQuery.UpdateAutoBlockFrequency(_appData.ConnectString,2);
        }

        private void FillEmptySpeedsButton_Click(object sender, RoutedEventArgs e)
        {
            // заполнить ограничения скоростей на пустых сегментах
            foreach (Segment s in _appData.Route1.Segments)
            {
                List<SpeedRestriction> newSpeedRestrictionsList =
                    _appData.Route1.SpeedRestrictions.FindAll(x => x.Start.SegmentID == s.SegmentID);

                if (newSpeedRestrictionsList.Count == 0)
                {
                    SpeedRestriction spd = new SpeedRestriction(40, 0, 0);
                    spd.Start = new PointOnTrack(s.Start);
                    spd.End = new PointOnTrack(s.End);
                    spd.Start.DicPointOnTrackKindID = spd.End.DicPointOnTrackKindID = 2;

                    _appData.Route1.SpeedRestrictions.Add(spd);
                    _appData.Route1.PointOnTracks.Add(spd.Start);
                    _appData.Route1.PointOnTracks.Add(spd.End);
                }
            }


            RefreshSpeedDataGrid();
            RemoveAllSpeedControls();

            DbRouteDrawer routeDrawer = new DbRouteDrawer();
            routeDrawer.widtscale = _appData.Widtscale;
            routeDrawer.heighscale = _appData.Heighscale;
            routeDrawer.kscale = _appData.Kscale;
            routeDrawer.lscale = _appData.Lscale;

            routeDrawer.DrawSpeedrestrictions(wrapPanel, _appData.Route1, false);

        }

        private void PointOnTrackEditGrid_SelectionChanged(object sender, SelectionChangedEventArgs e)
        {
            if (_appData.EgisPtNormsGridLock == true) return;

            PointOnTrack item = (PointOnTrack)PointOnTrackTabControl1.PointOnTrackEditGrid.SelectedItem;
            if (item == null) return;

            // Очищаем контейнер перед добавлением нового контрола
            PointOnTrackTabControl1.PointOnTrackEditControlsContainer.Children.Clear();

            // В зависимости от типа точки создаем соответствующий контрол
            switch (item.DicPointOnTrackKindID)
            {
                case 1: // Светофор
                    var trafficLightControl = new TrafficLightEditControl();
                    trafficLightControl.TrafficLight = PointOnTrack.GetTrafficLightForPoint(item, _appData.Route1);
                    trafficLightControl.RefreshFromTrafficLight();
                    PointOnTrackTabControl1.PointOnTrackEditControlsContainer.Children.Add(trafficLightControl);
                    break;

                case 40: // Токораздел (точка смены рода тока)
                    var currentKindControl = new CurrentKindChangeEditControl();
                    currentKindControl.CurrentKindChange = PointOnTrack.GetCurrentKindChangeForPoint(item, _appData.Route1);
                    currentKindControl.RefreshFromCurrentKindChange();
                    PointOnTrackTabControl1.PointOnTrackEditControlsContainer.Children.Add(currentKindControl);
                    break;

                // Добавьте другие case для других типов контролов по необходимости
            }

            // Обновляем меню точки на пути (если нужно)
           if (_appData.RouteToShowInDataGrids !=null) PointOnTrackTabControl1.pointOnTrackMenuControl1._route = _appData.RouteToShowInDataGrids;
           PointOnTrackTabControl1.pointOnTrackMenuControl1.p = item;
           PointOnTrackTabControl1.pointOnTrackMenuControl1.MenuRefresh();
        }

        

        private void SaveToDbButton1_Click(object sender, RoutedEventArgs e)
        {
            List<PointOnTrack> emptylist = new();

            DbRouteDataExporter drde = new DbRouteDataExporter(_appData.ConnectString, _appData.Route1, _appData.Route1, emptylist);
            PointOnTrackTabControl1.ImportOptionsControl2.ApplyToCheckBoxList(drde._routeExportCheckBoxList);
            drde.AddTrackObjectsFromDbRouteToBase();

            ClearDataAndCanvas();
            LoadData(_appData.ConnectString, _appData.Route1);
            DrawRoute(wrapPanel, _appData.Route1, _appData.ToAddRoute);
        }

        
    }
}
