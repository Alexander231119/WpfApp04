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
using System.Windows.Media.Animation;
using Pen = System.Windows.Media.Pen;
using Brushes = System.Windows.Media.Brushes;
using System.IO;
using WpfAapp04;
using Microsoft.Extensions.Configuration;
//using VideoLib;



namespace WpfApp04
{
    /// <summary>
    /// Interaction logic for MainWindow.xaml
    /// </summary>
    public partial class MainWindow : Window
    {

        DbRoute route1 = new DbRoute();
        DbRoute toAddRoute = new DbRoute();
        DbRoute egisRoute1 = new DbRoute();
        DbRoute routeToShowInDataGrids = new DbRoute();

        private List<PointOnTrack> pointOnTracksToShow = new List<PointOnTrack>(); // списко трочек на пути для показа в таблице PointOnTrackEditGrid

        ElectonicMap map1 = new ElectonicMap();// файл электронной карты

        RoutesElectronicMap routesElectronicMap = new RoutesElectronicMap();// файл электронной карты сконвертированный в списки DbRoute

        public DbRoute ekDbRoute = new DbRoute(); // выбранная карта или pos в карте

        private List<Segment> segmentsSourseFromEgis = new List<Segment>();
        List<Segment> segmentsToFillFromEgis = new List<Segment>(); // сегменты в целевом маршруте для выбора для экспорта обьектов например route1
        
        private List<PointOnTrack> PointOnTracksToAdd = new List<PointOnTrack>(); // отдельный список точек на пути для добавления без создания новых обьектов

        static string egisconnectionString;
        SqlConnection egisconnection;// = new SqlConnection(egisconnectionString);

        List<Station> EgisSelectedStations= new List<Station>();// найденные станции
        List<Track> EgisSelectedTracks= new List<Track>(); // пути проходящие через станцию

        List<PointOnTrack> EgisFoundPointObjects = new List<PointOnTrack>(); // для поиска по имени обьекта
        
        Track egisSelectedTrack = new Track(); // выбранный путь
        
        double UsageDirectionToFind = 1; // направление для поиска обьектов возрастание или убывание
        private double SpeedKindToFind = 0; // вид движения для поиска скоростей и проб тормозов

        string fileName = "";
        string ConnectSrting1 = "Provider=Microsoft.ACE.OLEDB.12.0;Data Source=";
        public static string ConnectString = "";

        // масштаб и положение элементов
        public double heighscale { get; set; } = 1;
        public double widtscale = 0.1;

        public double segmentsBottom = 0;
        public double segmentsHeight = 20;

        public double kilometersBottom = 20;
        public double kilometersHeight = 30;

        public double pkLineBottom = 50;
        public double pkLineHeight = 10;

        public double inclineControlBottom = 60;
        public double inclineControlHeight = 240;// не используется

        public double floorBottom = 100;

        public double trafficSignalsBottom = 255;

        public double trackCircuitsBottom = 300;
        public double trackCircuitsHeight = 15;

        public double stationsBottom = 315;
        public double stationsHeight = 65;

        // высота отрисовки ограничений скорости
        public double speedBottom = 400;
        public double maxSpeed = 300;

        //для масштабирования уклонов
        public double kscale=1;
        public double lscale=1;
        public double maxElev=0;
        public double minElev=0; 

        
        //
        public double lastX = 0;
        public double lastY = 0;
        public bool rowchanged = false;
        private bool EgisPtNormsGridLock = false;

        private OleDbConnection myConnection;
        PointOnTrackComparer pcr = new PointOnTrackComparer();
        SpeedComparerToshow scts = new SpeedComparerToshow(); 
        InclineComparer inclc = new InclineComparer();
        StationComparer stationsByroute = new StationComparer();
        
        public MainWindow()
        {
            InitializeComponent();

            var config = ConfigLoader.Load();
            string connectionString = config.GetConnectionString("RailDB");
            egisconnectionString = connectionString;
            egisconnection = new SqlConnection(egisconnectionString);

            SpeedDataGrid.ItemsSource = route1.SpeedRestrictions;
            KmGrid.ItemsSource = route1.Kilometers;
            EgisStationsGrid.ItemsSource= EgisSelectedStations;
            StationDataGridSourceRadioButtonEgis.IsChecked = true;
            EgisTrackGrid.ItemsSource = EgisSelectedTracks;
            EgisKmGrid.ItemsSource = egisRoute1.Kilometers;
            EgisPlatformsGrid.ItemsSource = egisRoute1.Platforms;
            SegmentsToFillFromEgisGrid.ItemsSource = route1.Segments;
            EgisFoundPointObjectsGrid.ItemsSource = EgisFoundPointObjects;
            EgisToExportTrafficLightsGrid.ItemsSource = egisRoute1.TrafficLights;
            EgisToExportStationsGrid.ItemsSource = egisRoute1.Stations;
            EgisToExportInclinesGrid.ItemsSource = egisRoute1.Inclines;
            EgisPtGrid.ItemsSource = egisRoute1.BrakeCheckPlaces;

            DbRouteEmapControl_1.dbElectronicMap = routesElectronicMap;
            DbRouteEmapControl_1._window = this;
            DbRouteEmapControl_1.RouteSelected += OnRouteSelected;


            // В конструкторе MainWindow подписываемся на событие изменения фильтров 
            ImportOptionsControl2.FilterChanged += () =>
            {
                var source = StationDataGridSourceRadioButtonEgis.IsChecked == true ?
                    egisRoute1.PointOnTracks :
                    route1.PointOnTracks;

                pointOnTracksToShow = ImportOptionsControl2.FilterPoints(source).ToList();
                PointOnTrackEditGrid.ItemsSource = pointOnTracksToShow;
                PointOnTrackEditGrid.Items.Refresh();
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
            if (myConnection != null)
                myConnection.Close();
        }

        private void Test_Click(object sender, RoutedEventArgs e)
        {
            string errorMessage = "";
            foreach (var s in route1.SpeedRestrictions) 
            {
                errorMessage += s.Start.CheckCoordinate(route1.PointOnTracks, route1.Segments) + s.End.CheckCoordinate(route1.PointOnTracks, route1.Segments);
            }
            
            if ((route1.Segments.Count > 0)&&(route1.Stations.Count > 0)&&(route1.SpeedRestrictions.Count > 0)&&(route1.Kilometers.Count>0)) 
            {
                wrapPanel.Children.Clear();
                route1.SpeedRestrictions.Sort(scts);
                DrawRoute(wrapPanel, route1, toAddRoute);
            }
            if (errorMessage != "") MessageBox.Show(errorMessage);
        }


        private void CloseFile_Click(object sender, RoutedEventArgs e)
        {
            ClearDataAndCanvas();

            if (myConnection != null)
            myConnection.Close();
            Close();
        }

        private void ElectonicMap_menuItem_Click(object sender, RoutedEventArgs e)
        {

            string mapfilename;
            var openFileDialog = new OpenFileDialog { };
            var result = openFileDialog.ShowDialog();
            if (result != true) return;

            mapfilename = openFileDialog.FileName;

            map1 = map1.Load(mapfilename);
            //ekDbRoute.DbRouteClear();
            //ekDbRoute.DbRouteFromEkRoute(map1);


            routesElectronicMap.RoutesEkklubsList?.Clear();
            routesElectronicMap.DbRouteFromEkRoute(map1);

            //ekDbRoute = routesElectronicMap.RoutesEkklubsList[9].RoutesList[2];
            //ekDbRoute = routesElectronicMap.RoutesEkklubsList[9].RoutesList[DbRouteEmapControl_1.routeId];
            
            DbRouteEmapControl_1.UpdatedbElectronicMapListBox();
        }

        private void OpenForImport_menuItem_Click(object sender, RoutedEventArgs e)
        {
            
            var openFileDialog = new OpenFileDialog { };
            var result = openFileDialog.ShowDialog();
            if (result != true) return;

            string fileName2 = openFileDialog.FileName;
            //Title = fileName;
            string ConnectString2 = ConnectSrting1 + fileName2 + ";";

            EgisPtNormsGridLock = true;
            //ClearDataAndCanvas();
            egisRoute1.DbRouteClear();
            LoadData(ConnectString2, egisRoute1);

            
            EgisPlatformsGrid.Items.Refresh();
            EgisToExportInclinesGrid.ItemsSource = egisRoute1.Inclines;
            EgisToExportInclinesGrid.Items.Refresh();
            EgisToExportStationsGrid.Items.Refresh();
            EgisPtGrid.Items.Refresh();

            string message1 = "";
            message1 = egisRoute1.Kilometers.Count.ToString() + "  " + egisRoute1.PointOnTracks.Count.ToString();
            EgisTrackTextBlock.Text = message1;
            message1 = "";


            EgisPtNormsGridLock = false;
        }

        private void OpenFile_Click(object sender, RoutedEventArgs e)
        {
            var openFileDialog = new OpenFileDialog { };
                var result = openFileDialog.ShowDialog();
            if (result != true) return;

            fileName = openFileDialog.FileName;
            Title = fileName;
            ConnectString = ConnectSrting1 + fileName + ";";

            ClearDataAndCanvas();
            LoadData(ConnectString, route1);
            
            DrawRoute(wrapPanel, route1, toAddRoute);

            // сортировка таблицы по координате по маршруту

            SpeedDataGrid.Items.SortDescriptions.Clear();
            SpeedDataGrid.Items.SortDescriptions.Add(new SortDescription("StartRouteCoordinate", ListSortDirection.Ascending));
            //
            SpeedDataGrid.Items.Refresh();
            SpeedDataGrid.SelectedIndex = 0;            
            KmGrid.Items.Refresh();

            foreach (var item in KmGrid.Items)
            {
                Kilometer k = (Kilometer)item;
                if (k.Length > 1100 || k.Length < 950)
                {
                    int index = KmGrid.Items.IndexOf(item);

                    if (index >= 0)
                    {
                        IList itemsSource = KmGrid.ItemsSource as IList;
                        DataGridRow row = KmGrid.ItemContainerGenerator.ContainerFromItem(itemsSource[index]) as DataGridRow;
                        Brush b = new SolidColorBrush(Colors.Yellow);
                        if (row != null) row.Background = b;
                    }
                }
            }

            SegmentsToFillFromEgisGrid.Items.Refresh();
        }

        void ClearDataAndCanvas() 
        {
            wrapPanel.Children.Clear();
            wrapPanel.ClearVisuals();
            segmentsToFillFromEgis.Clear();
            route1.DbRouteClear();
            ClearToAddLists();
        }


        void ClearToAddLists()
        {
            toAddRoute.DbRouteClear();
            PointOnTracksToAdd.Clear();
        }

        #region Load & Fill Data

        void LoadData(string cstring, DbRoute route)
        {
            EgisPtNormsGridLock = true;
            
            // загрузить с использованием метода из отдельного файла
            DbDataLoader loader = new DbDataLoader(cstring, route);
            loader.LoadData();
            // для каждого км в маршруте из базы подписаться на событие для корректировки длины километра

            if (route == route1)
            {
                foreach (Kilometer k in route1.Kilometers) { k.KmLengthSet += KmLengthChangedPerform; }
            }
            
            
            EgisPtNormsGridLock = false;
        }

        #endregion

        #region Draw

        void DrawRoute(DrawingCanvas _canvas, DbRoute _route1, DbRoute _toAddRoute)
        {
            // для отображения двух маршрутов
            DbRouteDrawer routeDrawer = new DbRouteDrawer();
            routeDrawer.widtscale = widtscale;
            routeDrawer.heighscale = heighscale;
            //routeDrawer.maxSpeed = maxSpeed;
            //routeDrawer.segmentsBottom = segmentsBottom;
            //routeDrawer.segmentsHeight = segmentsHeight;
            //routeDrawer.kilometersBottom = kilometersBottom;
            //routeDrawer.kilometersHeight = kilometersHeight;
            //routeDrawer.pkLineBottom = pkLineBottom;
            //routeDrawer.pkLineHeight = pkLineHeight;
            //routeDrawer.inclineControlBottom = inclineControlBottom;
            //routeDrawer.floorBottom=floorBottom;
            routeDrawer.kscale=kscale;
            routeDrawer.lscale=lscale;

            routeDrawer.DrawRoute(_canvas, _route1, _toAddRoute);

            
        }

        public void DrawRouteWay(DrawingCanvas _canvas, DbRoute _route)
        {
            //для отображения одного маршррута
            DbRouteDrawer routeDrawer = new DbRouteDrawer();
            routeDrawer.widtscale = widtscale;
            routeDrawer.heighscale = heighscale;
            routeDrawer.kscale = kscale;
            routeDrawer.lscale = lscale;

            routeDrawer.DrawRouteWay(_canvas, _route);

        }
        #endregion

        #region Speedrestriction & DataPerform

        public void SaveSpeedrestrictions(string _connectstring)
        {
            
                DbRouteDataExporter.SaveSpeedRestrictions(ConnectString,route1);

                MessageBox.Show("Введены ограничения скорости \n всего: " +
                                route1.SpeedRestrictions.Count.ToString(), "постоянные ограничения скорости");
                // считываем данные из базы заново после сохранения
                ClearDataAndCanvas();
                LoadData(ConnectString, route1);
                DrawRoute(wrapPanel, route1, toAddRoute);
            
        }
        
        private void wrapPanel_MouseRightButtonDown(object sender, MouseButtonEventArgs e)
        {
            double mX = Mouse.GetPosition(wrapPanel).X;
            double mY = wrapPanel.ActualHeight - Mouse.GetPosition(wrapPanel).Y;

            lastX= mX;
            lastY= mY;
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
                widtscale = Convert.ToDouble(ScaletextBox.Text)/1000;
                wrapPanel.Children.Clear();
                DrawRoute(wrapPanel, route1, toAddRoute);
            }
        }

        private void SaveSpeedsClick(object sender, RoutedEventArgs e)
        {
            SaveSpeedrestrictions(ConnectString);
        }

        private void SaveSpeedButton_Click(object sender, RoutedEventArgs e)
        {
            SaveSpeedrestrictions(ConnectString);
        }

        private void ExportSpeedsClick(object sender, RoutedEventArgs e)
        {
            var openFileDialog2 = new OpenFileDialog { };
            var result = openFileDialog2.ShowDialog();
            if (result != true) return;

            string fileName2 = openFileDialog2.FileName;
            //Title = fileName;
            string ConnectString2 = ConnectSrting1 + fileName2 + ";";


            SaveSpeedrestrictions(ConnectString2);

        }

        //
        void SpeedChangedPerform(double index)
        {
            //MessageBox.Show("SpeedChangedPerform index"+ index.ToString());
            //wrapPanel.Children.Clear();
            route1.SpeedRestrictions.Sort(scts);
            //DrawRoute(wrapPanel,route1,toAddRoute);
            SpeedDataGrid.Items.Refresh();
            RemoveAllSpeedControls();

            DbRouteDrawer routeDrawer = new DbRouteDrawer();
            routeDrawer.widtscale = widtscale;
            routeDrawer.heighscale = heighscale;
            routeDrawer.kscale = kscale;
            routeDrawer.lscale = lscale;

            routeDrawer.DrawSpeedrestrictions(wrapPanel, route1, false);
            routeDrawer.DrawSpeedrestrictions(wrapPanel, toAddRoute, true);

            //DrawSpeedrestrictions(wrapPanel, route1.SpeedRestrictions,false);
            //DrawSpeedrestrictions(wrapPanel, toAddRoute.SpeedRestrictions,true);



        }


        private void DeleteSpeedButton_Click(object sender, RoutedEventArgs e)
        {
            SpeedRestriction item = (SpeedRestriction)SpeedDataGrid.SelectedItem;
            route1.SpeedRestrictions.Remove(item);
            RefreshSpeedDataGrid();
            //wrapPanel.Children.Clear();
            //DrawRoute(wrapPanel,route1,toAddRoute);
            RemoveAllSpeedControls();

            DbRouteDrawer routeDrawer = new DbRouteDrawer();
            routeDrawer.widtscale = widtscale;
            routeDrawer.heighscale = heighscale;
            routeDrawer.kscale = kscale;
            routeDrawer.lscale = lscale;

            routeDrawer.DrawSpeedrestrictions(wrapPanel, route1, false);

        }

        void RefreshSpeedDataGrid() 
        {
            int selectedrow = SpeedDataGrid.SelectedIndex;

            SpeedDataGrid.ItemsSource = null;
            SpeedDataGrid.ItemsSource = route1.SpeedRestrictions;
            SpeedDataGrid.Items.SortDescriptions.Clear();
            SpeedDataGrid.Items.SortDescriptions.Add(new SortDescription("StartRouteCoordinate", ListSortDirection.Ascending));
            SpeedDataGrid.Items.Refresh();

            SpeedDataGrid.SelectedIndex = selectedrow;

        }

        private void AddSpeedButton_Click(object sender, RoutedEventArgs e)
        {
            if (route1.Segments.Count > 0) 
            { 
            MenuAddSpeedFromCell();
            }
        }

        void MenuAddSpeedFromCell()
        {
            SpeedRestriction item = (SpeedRestriction)SpeedDataGrid.SelectedItem;

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


                int sindex = route1.Segments.FindIndex(y => (y.SegmentID == spdin.Start.SegmentID));
                int tindex = -1;
                if (sindex >= 0)
                {
                    tindex = route1.Tracks.FindIndex(x => (x.TrackID == route1.Segments[sindex].TrackID));
                }
                else
                {
                    sindex = 0;
                    tindex = route1.Tracks.FindIndex(x => (x.TrackID == route1.Segments[sindex].TrackID));
                }

                if (tindex >= 0)
                {
                    menu.TrackName.Text = route1.Tracks[tindex].TrackNumber.ToString() + " " + route1.Tracks[tindex].TrackName;
                }

            
                if (menu.ShowDialog() == true)
                {

                //rectangle1.Height = Convert.ToDouble(menu.Value.Text) * heightscale;
                //rectangle2.Height = (200 - Convert.ToDouble(menu.Value.Text))*heightscale;
                    spdin.Start.SegmentID = spdin.End.SegmentID = Convert.ToDouble(menu.SegmentId.Text);

                    spdin.Start.PointOnTrackKm = menu.StartKm.Text;
                    spdin.Start.RefreshCoordinate(route1.PointOnTracks, route1.Segments);
                    spdin.End.RefreshCoordinate(route1.PointOnTracks, route1.Segments);

                    spdin.Start.RefreshRouteCoordinate(route1.Segments);
                    spdin.End.RefreshRouteCoordinate(route1.Segments);
                    spdin.Value = Convert.ToDouble(menu.Value.Text);

                    route1.PointOnTracks.Add(spdin.Start);
                    route1.PointOnTracks.Add(spdin.End);
                    route1.SpeedRestrictions.Add(spdin);

                    RefreshSpeedDataGrid();

                    //wrapPanel.Children.Clear();
                    RemoveAllSpeedControls();

                    route1.SpeedRestrictions.Sort(scts);
                    route1.PointOnTracks.Sort(pcr);

                    DbRouteDrawer routeDrawer = new DbRouteDrawer();
                    routeDrawer.widtscale = widtscale;
                    routeDrawer.heighscale = heighscale;
                    routeDrawer.kscale = kscale;
                    routeDrawer.lscale = lscale;

                    routeDrawer.DrawSpeedrestrictions(wrapPanel, route1, false);
                }
            
        }

        private void DeleteAllSpeedButton_Click(object sender, RoutedEventArgs e)
        {
            route1.SpeedRestrictions.Clear();
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
            SpeedDataGrid.Columns[11].Visibility= Visibility.Visible;
        }

        private void RouteCoordinateCheckBox_Unchecked(object sender, RoutedEventArgs e)
        {
            SpeedDataGrid.Columns[11].Visibility = Visibility.Hidden;
        }

        

        private void SegmentIdTextBox_KeyDown(object sender, KeyEventArgs e)
        {
            if (e.Key == Key.Enter)
            {
                

            }
        }

        private void SpeedDataGrid_SelectionChanged(object sender, SelectionChangedEventArgs e)
        {
            SpeedRestriction item = (SpeedRestriction)SpeedDataGrid.SelectedItem;
            SegmentIdTextBox.Text = item?.Start.SegmentID.ToString();

            if (rowchanged == true)
            {

                foreach (SpeedRestriction s in route1.SpeedRestrictions) 
                {
                    s.Start.RefreshCoordinate(route1.PointOnTracks, route1.Segments);
                    s.Start.RefreshRouteCoordinate(route1.Segments);
                    s.End.RefreshCoordinate(route1.PointOnTracks, route1.Segments);
                    s.End.RefreshRouteCoordinate(route1.Segments);
                }
                                                
                //wrapPanel.Children.Clear();
                RemoveAllSpeedControls();

                route1.SpeedRestrictions.Sort(scts);

                //DrawRoute(wrapPanel,route1,toAddRoute);


                DbRouteDrawer routeDrawer = new DbRouteDrawer();
                routeDrawer.widtscale = widtscale;
                routeDrawer.heighscale = heighscale;
                routeDrawer.kscale = kscale;
                routeDrawer.lscale = lscale;

                routeDrawer.DrawSpeedrestrictions(wrapPanel, route1, false);

                //SpeedDataGrid.Items.Refresh();
                //SpeedDataGrid.Focus();
                rowchanged = false;
            }
        }

        private void SpeedDataGrid_RowEditEnding(object sender, DataGridRowEditEndingEventArgs e)
        {
            rowchanged = true;
        }
        
        private void SetSpeedSegmentIdButton_Click(object sender, RoutedEventArgs e)
        {
            SpeedRestriction item = (SpeedRestriction)SpeedDataGrid.SelectedItem;
            if (item != null)
            {

                item.Start.SegmentID = item.End.SegmentID = Convert.ToDouble(SegmentIdTextBox.Text);
                item.Start.RefreshCoordinate(route1.PointOnTracks, route1.Segments);
                item.Start.RefreshRouteCoordinate(route1.Segments);
                item.End.RefreshCoordinate(route1.PointOnTracks, route1.Segments);
                item.End.RefreshRouteCoordinate(route1.Segments);
            }
        }


        private void KmGrid_SelectionChanged(object sender, SelectionChangedEventArgs e)
        {
            if (EgisPtNormsGridLock == true) return;

            Kilometer item = (Kilometer)KmGrid.SelectedItem;

            KmTextBlock1.Text = "Км " + item?.Km +
                                "\nНачало: Segment: "
                                + item?.Start.SegmentID.ToString() + " "
                                + item?.Start.DicPointOnTrackKindName + " "
                                + item?.Start.PointOnTrackKm + "-"
                                + item?.Start.PointOnTrackPk.ToString() + "-"
                                + item?.Start.PointOnTrackM.ToString() + " "
                                + "TrackObject: " + item?.Start.TrackObjectID.ToString() + " "
                                + "PointOntrack: " + item?.Start.PointOntrackID.ToString() + " "

                                + "\nКонец: Segment: "
                                + item?.End.SegmentID.ToString() + " "
                                + item?.End.DicPointOnTrackKindName + " "
                                + item?.End.PointOnTrackKm + "-"
                                + item?.End.PointOnTrackPk.ToString() + "-"
                                + item?.End.PointOnTrackM.ToString() + " "
                                + "TrackObject: " + item?.End.TrackObjectID.ToString() + " "
                                + "PointOntrack: " + item?.End.PointOntrackID.ToString() + " "
                ;

            DbKmTextBox.Text = item?.Length.ToString();

        }

        public void KmLengthChangedPerform(Kilometer k)
        {
            DbRouteQuery.KmLengthSetPerform(ConnectString,k,route1);

            MessageBox.Show("Изменения внесены в " + fileName);
            ClearDataAndCanvas();
            LoadData(ConnectString, route1);
            DrawRoute(wrapPanel, route1, toAddRoute);

        }


        private void DbKmSetLengthButton_Click(object sender, RoutedEventArgs e)
        {
            Kilometer klm = (Kilometer)KmGrid.SelectedItem;
            if (klm is null) return; 
            klm.Length = Convert.ToDouble(DbKmTextBox.Text);
            klm.KmLengthBeenSet();
                //
        }

        private void DbKmSegmentGroupSetLengthButton_Click(object sender, RoutedEventArgs e)
        {
            Kilometer klm = (Kilometer)KmGrid.SelectedItem;
            if (klm is null) return;


            //double klmLength = 995;
            double klmLength = Convert.ToDouble(DbKmTextBox.Text);

            double segmentid = klm.Start.SegmentID;
            double kindex = route1.Kilometers.IndexOf(klm);



                //выбор километров с длиной 1000 метров в данном сегменте
            List<Kilometer> KilometersToEdit = route1.Kilometers.FindAll(k =>
                k.Start.RouteCoordinate>=klm.Start.RouteCoordinate&&
                k.Length == 1000 && 
                k.Start.SegmentID == segmentid && 
                k.End.SegmentID == segmentid &&
                k.Start.SegmentID == k.End.SegmentID &&
                k.Start.DicPointOnTrackKindID == 0 && k.End.DicPointOnTrackKindID == 0);



                // километры которые выбрал пользователь
            List<Kilometer> selectedKilometersToEdit = new List<Kilometer>();

            foreach (var item in KmGrid.SelectedItems)
            {
                Kilometer k = (Kilometer)item;
                selectedKilometersToEdit.Add(k);
            }

            int KilometersToEditCount = KilometersToEdit.Count;
            int selectedKilometersToEditCount = selectedKilometersToEdit.Count;

            foreach (Kilometer k in selectedKilometersToEdit)
            {

                if (k.Start.SegmentID == k.End.SegmentID)
                    k.Length = klmLength;
                    DbRouteQuery.KmLengthSetPerform(ConnectString,k,route1);

            }

            ClearDataAndCanvas();
            LoadData(ConnectString, route1);
            DrawRoute(wrapPanel, route1, toAddRoute);

            //Kilometer k1 = route1.Kilometers.Find(k1 => (k1.Length == 1000)&&(k1.Start.SegmentID == segmentid));


            //if (k1 is null) return;

            //while (k1 != null)
            //{
            //    if (k1.Start.SegmentID == segmentid && k1.Length == 1000)
            //    {
            //        klm.Length = klmLength;
            //        klm.KmLengthBeenSet();
            //    }
            //}




            //foreach (Kilometer k in route1.Kilometers)
            //{
            //    if (k.Start.SegmentID == segmentid && k.Length == 1000)
            //    {
            //        klm.Length = klmLength;
            //        klm.KmLengthBeenSet();
            //    }
            //}

        }

        #endregion

        #region Егис

        private void EgisConnectMenuItem_Click(object sender, RoutedEventArgs e)
        {
            string sql = "SELECT StationID, StationName FROM Station WHERE StationName like '%никель-мурманский%'";
            try
            {
                egisconnection.Open();
                
            }
            catch (Exception ex)
            {
                MessageBox.Show(ex.Message);
            }
        }

        private void EgisDisconnectMenuItem_Click(object sender, RoutedEventArgs e)
        {
            if (egisconnection != null)
                egisconnection.Close();
        }
        private void EgisFindStationButton_Click(object sender, RoutedEventArgs e)
        {
            StaitonToFindTextBox.Text = DbRouteHelper.ConvertEngToRus(StaitonToFindTextBox.Text);
            EgisSelectStations();

        }
        void EgisSelectStations()
        {
            EgisImporter.SelectStations(StaitonToFindTextBox.Text, egisconnection, EgisSelectedStations);
            EgisStationsGrid.Items.Refresh();
            EgisStationsGrid.SelectedIndex = 0;
        }
        void EgisSelectTrack()
        {
            Station item = (Station)EgisStationsGrid.SelectedItem;
            EgisImporter.SelectTrack(item,(bool)MaintrackRadioButton.IsChecked, egisconnection, EgisSelectedTracks);
            EgisTrackGrid.Items.Refresh();
        }

        //найти обьект в егис по названию обьекта и станции
        void EgisFindPointObject()
        {
            Station item = (Station)EgisStationsGrid.SelectedItem;
            string EgisStationID;
            if (item == null)
            {
                EgisStationID = "";
            }
            else
            {
                EgisStationID = item.EgisStationID.ToString();
            }
            string ObjectNameToFind = PointObjectToFindTextBox.Text;
            string stationnametofind = StaitonToFindTextBox.Text;

            EgisSelectedTracks.Clear();
            EgisFoundPointObjects.Clear();

            EgisImporter.EgisFindPointObject(EgisStationID, ObjectNameToFind, stationnametofind, egisconnectionString, EgisSelectedTracks, EgisFoundPointObjects);
            EgisTrackGrid.Items.Refresh();
            EgisFoundPointObjectsGrid.Items.Refresh();
        }

        
        
        void LoadEgisData() 
        {
            //egisSelectedTrack = (Track)EgisTrackGrid.SelectedItem;


            EgisImporter egisImporter = new EgisImporter(egisconnectionString,egisRoute1) ;
            egisImporter.EgisSelectedTrack = egisSelectedTrack;
            egisImporter._speedKindToFind = SpeedKindToFind;
            egisImporter._usageDirectionToFind = UsageDirectionToFind;


            if (egisSelectedTrack != null)
            {
                EgisPtNormsGridLock = true;
                egisImporter.LoadEgisData();

                EgisPlatformsGrid.Items.Refresh();
                EgisToExportInclinesGrid.ItemsSource = egisRoute1.Inclines;
                EgisToExportInclinesGrid.Items.Refresh();
                EgisToExportStationsGrid.Items.Refresh();
                EgisPtGrid.Items.Refresh();

                string message1 = "";
                message1 = egisRoute1.Kilometers.Count.ToString() + "  " + egisRoute1.PointOnTracks.Count.ToString();
                EgisTrackTextBlock.Text = message1;
                message1 = "";
                EgisPtNormsGridLock = false;
            }

        }

        


        private void EmapShowMenuItem_Click(object sender, RoutedEventArgs e)
        {
            if (routesElectronicMap.RoutesEkklubsList.Count == 0) return;
            ekDbRoute = routesElectronicMap.RoutesEkklubsList[DbRouteEmapControl_1.mapId].RoutesList[DbRouteEmapControl_1.routeId];
            EgisPreview egisPreview = new EgisPreview();
            DrawRouteWay(egisPreview.EgisCanvas, ekDbRoute);
            egisPreview.Show();
        }
        
        private void ShowEgisPreviewButton_Click(object sender, RoutedEventArgs e)
        {
            EgisPreview egisPreview = new EgisPreview();
            egisPreview.Title = egisSelectedTrack?.TrackNumber;

            DrawRouteWay(egisPreview.EgisCanvas, egisRoute1);
            egisPreview.Show();
        }

        private void EgisTrackGrid_SelectionChanged(object sender, SelectionChangedEventArgs e)
        {
                   

        }

        private void EgisTrackGrid_MouseDoubleClick(object sender, MouseButtonEventArgs e)
        {
            egisSelectedTrack = (Track)EgisTrackGrid.SelectedItem;
        }

        private void DownUsageDirectionToFindRadioButton_Checked(object sender, RoutedEventArgs e)
        {
            UsageDirectionToFind = -1;
        }

        private void UpUsageDirectionToFindRadioButton_Checked(object sender, RoutedEventArgs e)
        {
            UsageDirectionToFind = 1;
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
            
            if ((Track)EgisTrackGrid.SelectedItem != null)
            {
                egisSelectedTrack = (Track)EgisTrackGrid.SelectedItem;
                LoadEgisData();
            }
        }

        private void StaitonToFindTextBox_KeyDown(object sender, KeyEventArgs e)
        {
            if (e.Key == Key.Enter)
            {
                //EgisSelectStations();
                EgisImporter.SelectStations(StaitonToFindTextBox.Text, egisconnection, EgisSelectedStations);
                EgisStationsGrid.Items.Refresh();
                EgisStationsGrid.SelectedIndex = 0;

            }
        }

        private void AddSegmentsToFillFromEgisButton_Click(object sender, RoutedEventArgs e)
        {
            IList<Segment> targetCollection;
            TextBlock targetTextBlock;
            string message = "";

            if (SegmentsToFillFromEgisGrid.ItemsSource == route1.Segments)
            {
                targetCollection = segmentsToFillFromEgis;
                targetTextBlock = SegmentsToFillFromEgisTextBlock;
            }
            else if (SegmentsToFillFromEgisGrid.ItemsSource == egisRoute1.Segments)
            {
                targetCollection = segmentsSourseFromEgis;
                targetTextBlock = SegmentsSourceFromEgisTextBlock;
            }
            else
            {
                return; // Неизвестный источник данных
            }

            targetCollection.Clear();

            if (SegmentsToFillFromEgisGrid.SelectedItems.Count > 0)
            {
                foreach (var item in SegmentsToFillFromEgisGrid.SelectedItems)
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
            if (segmentsSourseFromEgis.Count > 0)
            {
                SourceSegmentsCollection = segmentsSourseFromEgis;
            }
            else
            {
                SourceSegmentsCollection = egisRoute1.Segments;
            }

            RouteToRouteLoader routeLoader = new RouteToRouteLoader(egisRoute1, toAddRoute, route1, segmentsToFillFromEgis, PointOnTracksToAdd);

            if (segmentsSourseFromEgis.Count > 0)
            {
                routeLoader._segmentsSourseFromEgis = segmentsSourseFromEgis;
            }
            else
            {
                routeLoader._segmentsSourseFromEgis = egisRoute1.Segments;
            }

            ApplyImportCheckBoxes(routeLoader._routeExportCheckBoxList); 

            routeLoader.FillFromRouteToRoute();

            wrapPanel.Children.Clear();
            DrawRoute(wrapPanel,route1,toAddRoute);

            EgisStationsGrid.Items.Refresh();
            EgisToExportTrafficLightsGrid.Items.Refresh();

        }
        
        private void InsertFromEgisToBaseButton_Click(object sender, RoutedEventArgs e)
        {
            DbRouteDataExporter drde = new DbRouteDataExporter(ConnectString, toAddRoute, route1, PointOnTracksToAdd);

            ApplyImportCheckBoxes(drde._routeExportCheckBoxList);
            drde.AddTrackObjectsFromDbRouteToBase();

            ClearDataAndCanvas();
            LoadData(ConnectString, route1);
            DrawRoute(wrapPanel, route1, toAddRoute);

        }

        public void ApplyImportCheckBoxes(RouteExportCheckBoxList _list)
        {
            _list._DeleteTrackCircuitsChickBox = DeleteTrackCircuitsChickBox.IsChecked ?? false;
            ImportOptionsControl1.ApplyToCheckBoxList(_list);
        }
        
        private void EgisFindPointObjectsButton_Click(object sender, RoutedEventArgs e)
        {
            EgisFindPointObject();
        }

        private void EgisFoundPointObjectsGrid_MouseDoubleClick(object sender, MouseButtonEventArgs e)
        {
            PointOnTrack p = (PointOnTrack)EgisFoundPointObjectsGrid.SelectedItem;
            egisSelectedTrack.TrackID = p.TrackID;
            LoadEgisData();
        }

        private void ClearToAddListsButtony_Click(object sender, RoutedEventArgs e)
        {
            ClearToAddLists();
            wrapPanel.Children.Clear();
            DrawRoute(wrapPanel, route1, toAddRoute);
        }


        #endregion
        
        private void ShowInclineItemCLick(object sender, RoutedEventArgs e)
        {
            var window = new Window();
            InclineEditor inclineEditor1 = new Controls.InclineEditor(route1.Segments, route1.Kilometers, route1.PointOnTracks, route1.Inclines);
            //inclineEditor1.pointOnTracks = PointOnTracks;
            window.Content = inclineEditor1;
            window.Show();
        }
        

        private void ExportInclinesToExcelButton_Click(object sender, RoutedEventArgs e)
        {
            DbRouteQuery.SaveInclinesToCsvFile(EgisToExportInclinesGrid.ItemsSource);
            //DbRouteQuery.SaveInclinesToCsvFile(EgisToExportInclinesGrid.Items);
            //DbRouteQuery.SaveInclinesToCsvFile(route1.Inclines);
        }


        private void FreightSpeedRadioButton_Checked(object sender, RoutedEventArgs e) {SpeedKindToFind = 2;}
        private void PassSpeedRadioButton_Copy1_Checked(object sender, RoutedEventArgs e) { SpeedKindToFind = 1; }
        private void HighSpeedRadioButton_Checked(object sender, RoutedEventArgs e) { SpeedKindToFind = 90; }
        private void VeryHighSpeedRadioButton_Checked(object sender, RoutedEventArgs e) { SpeedKindToFind = 87; }
        private void EtrainSpeedRadioButton_Checked(object sender, RoutedEventArgs e) { SpeedKindToFind = 89; }
        private void PrigSpeedRadioButton_Checked(object sender, RoutedEventArgs e) { SpeedKindToFind = 4; }
        private void MvpsSpeedRadioButton_Checked(object sender, RoutedEventArgs e) { SpeedKindToFind = 3; }

        private void AddTrafficLightToAddList_Click(object sender, RoutedEventArgs e)
        {

            TrafficLight t = new TrafficLight();

            if (EgisToExportTrafficLightsGrid.ItemsSource == egisRoute1.TrafficLights)
            {egisRoute1.TrafficLights.Add(t);}
            else if (EgisToExportTrafficLightsGrid.ItemsSource == route1.TrafficLights)
            { route1.TrafficLights.Add(t);}
            //t.DicTrafficLightKindID = 20;
            EgisToExportTrafficLightsGrid.Items.Refresh();
        }

        private void AddPointOnTrackButton1_Click(object sender, RoutedEventArgs e)
        {
            PointOnTrack p = new PointOnTrack();
            route1.PointOnTracks.Add(p);

            p.DicPointOnTrackKindID = 25; // по умолчанию укспс
            pointOnTracksToShow = ImportOptionsControl2.FilterPoints(routeToShowInDataGrids.PointOnTracks).ToList();
            PointOnTrackEditGrid.ItemsSource = pointOnTracksToShow;
            PointOnTrackEditGrid.Items.Refresh();

        }

        private void InsertTrafficLightsToDb_button_Click(object sender, RoutedEventArgs e)
        {

            // сохранение tlispeedrestrictions cтранно
            // для чего сохраняет tlirestrictions но не светофоры?
            DbRouteQuery.InsertTrafficLightsToDb(ConnectString, EgisToExportTrafficLightsGrid.ItemsSource);
            return;
        }
        private void SetAll4AbValue_button_Click(object sender, RoutedEventArgs e)
        {
            // применить четырёхзначную сигнализацию ко всем выбранным светофорам
            foreach (var item in EgisToExportTrafficLightsGrid.SelectedItems)
            {
                TrafficLight t = (TrafficLight)item;
                if (t.EgisABValue == 244) t.EgisABValue = 245;
                
            }

            EgisToExportTrafficLightsGrid.Items.Refresh();
        }

        private void RefreshDataGridsItemsSources()
        {
            EgisPtNormsGridLock = true;

            // Применяем фильтр
            pointOnTracksToShow = ImportOptionsControl2.FilterPoints(routeToShowInDataGrids.PointOnTracks).ToList();
            PointOnTrackEditGrid.ItemsSource = pointOnTracksToShow;
            PointOnTrackEditGrid.Items.Refresh();

            EgisToExportStationsGrid.ItemsSource = routeToShowInDataGrids.Stations;
            EgisToExportStationsGrid.Items.Refresh();
            EgisToExportTrafficLightsGrid.ItemsSource = routeToShowInDataGrids.TrafficLights;
            EgisToExportTrafficLightsGrid.Items.Refresh();
            EgisToExportInclinesGrid.ItemsSource = routeToShowInDataGrids.Inclines;
            EgisToExportInclinesGrid.Items.Refresh();
            //EgisKmGrid.ItemsSource = Kilometers;
            //EgisKmGrid.Items.Refresh();
            EgisPlatformsGrid.ItemsSource = routeToShowInDataGrids.Platforms;
            EgisPlatformsGrid.Items.Refresh();
            SpeedDataGrid.ItemsSource = routeToShowInDataGrids.SpeedRestrictions;
            SpeedDataGrid.Items.Refresh();
            SpeedDataGrid.Items.SortDescriptions.Clear();
            SpeedDataGrid.Items.SortDescriptions.Add(new SortDescription("StartRouteCoordinate", ListSortDirection.Ascending));
            EgisPtGrid.ItemsSource = routeToShowInDataGrids.BrakeCheckPlaces;
            EgisPtGrid.Items.Refresh();
            SegmentsToFillFromEgisGrid.ItemsSource = routeToShowInDataGrids.Segments;
            SegmentsToFillFromEgisGrid.Items.Refresh();



            EgisPtNormsGridLock = false;
        }

        private void StationDataGridSourceRadioButtonEgis_Checked(object sender, RoutedEventArgs e)
        {
            routeToShowInDataGrids = egisRoute1;
            RefreshDataGridsItemsSources();
        }

        private void StationDataGridSourceRadioButtonDb_Checked(object sender, RoutedEventArgs e)
        {
            routeToShowInDataGrids = route1;
            RefreshDataGridsItemsSources();
        }
        private void StationDataGridSourceRadioButtonToAdd_Checked(object sender, RoutedEventArgs e)
        {
            routeToShowInDataGrids = toAddRoute;
            RefreshDataGridsItemsSources();
        }

        private void EgisPtGrid_SelectionChanged(object sender, SelectionChangedEventArgs e)
        {
            if (EgisPtNormsGridLock == true) return;

            var item = EgisPtGrid.SelectedItem;
            BrakeCheckPlace bcp = (BrakeCheckPlace)item;
            EgisPtNormsGrid.ItemsSource = bcp.BrakeCheckNormList;
            EgisPtNormsGrid.Items.Refresh();
        }

        private void EgisToExportTrafficLightsGrid_SelectionChanged(object sender, SelectionChangedEventArgs e)
        {
            trafficLightEditControlMenu.TrafficLight = (TrafficLight)EgisToExportTrafficLightsGrid.SelectedItem;
            trafficLightEditControlMenu.RefreshFromTrafficLight();
        }

        private void ImportInitialStationNamesToBaseButton_Click(object sender, RoutedEventArgs e)
        {
            List<Station> StationsToInsert=new List<Station>();

            foreach (var item in EgisToExportStationsGrid.SelectedItems)
            {
                StationsToInsert.Add((Station)item);
            }

            DbRouteQuery.ImportInitialStationsToDb(ConnectString, StationsToInsert);
            
            // считываем данные из базы заново после сохранения
            ClearDataAndCanvas();
            LoadData(ConnectString, route1);
            DrawRoute(wrapPanel, route1, toAddRoute);
            MessageBox.Show("ok");
        }

        private void DeleteNopointSign_Click(object sender, RoutedEventArgs e)
        {
            // удалить знаки С которые не удалились полностью чере редактор
            DbRouteQuery.DeleteNoPointSigns(ConnectString);
            
            ClearDataAndCanvas();
            LoadData(ConnectString, route1);
            DrawRoute(wrapPanel, route1, toAddRoute);
            MessageBox.Show("ok");
        }


        private void DeleteNopointSign2_Click(object sender, RoutedEventArgs e)
        {
            // удалить непроставленные знаки С

            DbRouteQuery.DeleteNoFrameObjects(ConnectString, "TrafficSignal", 21,37);
            
            ClearDataAndCanvas();
            LoadData(ConnectString, route1);
            DrawRoute(wrapPanel, route1, toAddRoute);
            MessageBox.Show("ok");
        }

        private void DelerteNoPointUksps_Click(object sender, RoutedEventArgs e)
        {
            // удалить непроставленные УКСПС
            DbRouteQuery.DeleteNoFrameObjects(ConnectString, "", 16, 25);

            ClearDataAndCanvas();
            LoadData(ConnectString, route1);
            DrawRoute(wrapPanel, route1, toAddRoute);
            MessageBox.Show("ok");
        }

        private void DelerteNoPointKtsm_Click(object sender, RoutedEventArgs e)
        {
            // удалить непроставленные КТСМ
            DbRouteQuery.DeleteNoFrameObjects(ConnectString, "", 15, 24);

            ClearDataAndCanvas();
            LoadData(ConnectString, route1);
            DrawRoute(wrapPanel, route1, toAddRoute);
            MessageBox.Show("ok");
        }

        private void DelerteNoPointCrossing_Click(object sender, RoutedEventArgs e)
        {
            // удалить непроставленные переезды

            DbRouteQuery.DeleteNoFrameObjects(ConnectString, "Crossing", 9, 23);

            ClearDataAndCanvas();
            LoadData(ConnectString, route1);
            DrawRoute(wrapPanel, route1, toAddRoute);
            MessageBox.Show("ok");
        }

        private void DeleteAllInclines_Click(object sender, RoutedEventArgs e)
        {
            // удалить все уклоны
            string tok = "Incline";
            DbRouteQuery.DeleteAllObjectsByKind(ConnectString,tok, 10,32);

            ClearDataAndCanvas();
            LoadData(ConnectString, route1);
            DrawRoute(wrapPanel, route1, toAddRoute);
            MessageBox.Show("Уклоны удалены");
        }

        private void FrogModelMenuItem_Click(object sender, RoutedEventArgs e)
        {
            DbRouteQuery.UpdateFrogModels(ConnectString);
        }

        private void AutoBlockFrequency1_Click(object sender, RoutedEventArgs e)
        {
            DbRouteQuery.UpdateAutoBlockFrequency(ConnectString, 1);
        }

        private void AutoBlockFrequency2_Click(object sender, RoutedEventArgs e)
        {
            DbRouteQuery.UpdateAutoBlockFrequency(ConnectString,2);
        }

        private void FillEmptySpeedsButton_Click(object sender, RoutedEventArgs e)
        {
            // заполнить ограничения скоростей на пустых сегментах
            foreach (Segment s in route1.Segments)
            {
                List<SpeedRestriction> newSpeedRestrictionsList =
                    route1.SpeedRestrictions.FindAll(x => x.Start.SegmentID == s.SegmentID);

                if (newSpeedRestrictionsList.Count == 0)
                {
                    SpeedRestriction spd = new SpeedRestriction(40, 0, 0);
                    spd.Start = new PointOnTrack(s.Start);
                    spd.End = new PointOnTrack(s.End);
                    spd.Start.DicPointOnTrackKindID = spd.End.DicPointOnTrackKindID = 2;

                    route1.SpeedRestrictions.Add(spd);
                    route1.PointOnTracks.Add(spd.Start);
                    route1.PointOnTracks.Add(spd.End);
                }
            }


            RefreshSpeedDataGrid();
            RemoveAllSpeedControls();

            DbRouteDrawer routeDrawer = new DbRouteDrawer();
            routeDrawer.widtscale = widtscale;
            routeDrawer.heighscale = heighscale;
            routeDrawer.kscale = kscale;
            routeDrawer.lscale = lscale;

            routeDrawer.DrawSpeedrestrictions(wrapPanel, route1, false);

        }

        private void PointOnTrackEditGrid_SelectionChanged(object sender, SelectionChangedEventArgs e)
        {
            if (EgisPtNormsGridLock == true) return;

            PointOnTrack item = (PointOnTrack)PointOnTrackEditGrid.SelectedItem;
            if (item == null) return;

            // Очищаем контейнер перед добавлением нового контрола
            PointOnTrackEditControlsContainer.Children.Clear();

            // В зависимости от типа точки создаем соответствующий контрол
            switch (item.DicPointOnTrackKindID)
            {
                case 1: // Светофор
                    var trafficLightControl = new TrafficLightEditControl();
                    trafficLightControl.TrafficLight = PointOnTrack.GetTrafficLightForPoint(item,route1);
                    trafficLightControl.RefreshFromTrafficLight();
                    PointOnTrackEditControlsContainer.Children.Add(trafficLightControl);
                    break;

                case 40: // Токораздел (точка смены рода тока)
                    var currentKindControl = new CurrentKindChangeEditControl();
                    currentKindControl.CurrentKindChange = PointOnTrack.GetCurrentKindChangeForPoint(item, route1);
                    currentKindControl.RefreshFromCurrentKindChange();
                    PointOnTrackEditControlsContainer.Children.Add(currentKindControl);
                    break;

                // Добавьте другие case для других типов контролов по необходимости
            }

            // Обновляем меню точки на пути (если нужно)
           if (routeToShowInDataGrids !=null) pointOnTrackMenuControl1._route = routeToShowInDataGrids;
            pointOnTrackMenuControl1.p = item;
            pointOnTrackMenuControl1.MenuRefresh();
        }

        

        private void SaveToDbButton1_Click(object sender, RoutedEventArgs e)
        {
            List<PointOnTrack> emptylist = new();

            DbRouteDataExporter drde = new DbRouteDataExporter(ConnectString, route1, route1, emptylist);
            ImportOptionsControl2.ApplyToCheckBoxList(drde._routeExportCheckBoxList);
            drde.AddTrackObjectsFromDbRouteToBase();

            ClearDataAndCanvas();
            LoadData(ConnectString, route1);
            DrawRoute(wrapPanel, route1, toAddRoute);
        }

        
    }
}
