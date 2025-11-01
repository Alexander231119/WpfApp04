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
using WpfApp04;
using Microsoft.VisualBasic;
using System.Collections.Specialized;
//using static System.Windows.Forms.VisualStyles.VisualStyleElement.TrayNotify;

namespace LampFrame01
{
    /// <summary>
    /// Interaction logic for MainWindow.xaml
    /// </summary>
    public partial class MainWindow : Window
    {
        DbRoute inputRoute = new DbRoute();
        DbRoute outputRoute = new DbRoute();


        List<TrafficLight> TrafficLights1 = new List<TrafficLight>();
        List<TrafficLight> TrafficLights2 = new List<TrafficLight>();



        string fileName1 = "";
        string fileName2 = "";

        string ConnectSrting0 = "Provider=Microsoft.ACE.OLEDB.12.0;Data Source=";
        public static string ConnectString1 = "";
        public static string ConnectString2 = "";

        private OleDbConnection myConnection;

        public MainWindow()
        {
            InitializeComponent();
            grid1.ItemsSource = TrafficLights1;
            grid2.ItemsSource = TrafficLights2;

            grid1.LoadingRow += Grid_LoadingRow;
            grid2.LoadingRow += Grid_LoadingRow;


            // Подписываемся на изменение коллекций
            ((INotifyCollectionChanged)grid1.Items).CollectionChanged += Grid_CollectionChanged;
            ((INotifyCollectionChanged)grid2.Items).CollectionChanged += Grid_CollectionChanged;
        }

        private void Grid_CollectionChanged(object sender, NotifyCollectionChangedEventArgs e)
        {
            if (e.Action == NotifyCollectionChangedAction.Remove)
            {
                // Была удалена строка
                RefreshAllRowColors();
            }
        }

        private void Grid_LoadingRow(object sender, DataGridRowEventArgs e)
        {
            var trafficLight = e.Row.DataContext as TrafficLight;
            if (trafficLight == null) return;

            var currentGrid = sender as DataGrid;
            var otherGrid = currentGrid == grid1 ? grid2 : grid1;

            // Получаем индекс текущей строки
            int currentIndex = currentGrid.Items.IndexOf(trafficLight);

            // Проверяем строку с таким же индексом в другой таблице
            if (currentIndex >= 0 && currentIndex < otherGrid.Items.Count)
            {
                var otherTrafficLight = otherGrid.Items[currentIndex] as TrafficLight;
                if (otherTrafficLight != null && otherTrafficLight.TrackObjectName == trafficLight.TrackObjectName)
                {
                    e.Row.Background = new SolidColorBrush(Colors.LightGreen);
                    return;
                }
            }

            // Если нет совпадения - белый цвет
            e.Row.Background = new SolidColorBrush(Colors.White);
        }

        // Метод для перекраски всех строк после удаления
        private void RefreshAllRowColors()
        {
            // Перекрашиваем grid1
            foreach (var item in grid1.Items)
            {
                var row = grid1.ItemContainerGenerator.ContainerFromItem(item) as DataGridRow;
                if (row != null)
                {
                    var trafficLight = item as TrafficLight;
                    int index = grid1.Items.IndexOf(item);

                    if (index >= 0 && index < grid2.Items.Count)
                    {
                        var otherTrafficLight = grid2.Items[index] as TrafficLight;
                        if (otherTrafficLight != null && otherTrafficLight.TrackObjectName == trafficLight.TrackObjectName)
                        {
                            row.Background = new SolidColorBrush(Colors.LightGreen);
                            continue;
                        }
                    }
                    row.Background = new SolidColorBrush(Colors.White);
                }
            }

            // Перекрашиваем grid2
            foreach (var item in grid2.Items)
            {
                var row = grid2.ItemContainerGenerator.ContainerFromItem(item) as DataGridRow;
                if (row != null)
                {
                    var trafficLight = item as TrafficLight;
                    int index = grid2.Items.IndexOf(item);

                    if (index >= 0 && index < grid1.Items.Count)
                    {
                        var otherTrafficLight = grid1.Items[index] as TrafficLight;
                        if (otherTrafficLight != null && otherTrafficLight.TrackObjectName == trafficLight.TrackObjectName)
                        {
                            row.Background = new SolidColorBrush(Colors.LightGreen);
                            continue;
                        }
                    }
                    row.Background = new SolidColorBrush(Colors.White);
                }
            }
        }

        // Вызывайте этот метод после удаления строки
        private void AfterDeleteRow()
        {
            RefreshAllRowColors();
        }

        private void OpenInputButton_Click(object sender, RoutedEventArgs e)
        {
            var openFileDialog = new OpenFileDialog { };
            var result = openFileDialog.ShowDialog();
            if (result != true) return;

                
            fileName1 = openFileDialog.FileName;


            ConnectString1 = ConnectSrting0 + fileName1 + ";";

            //TrafficLights1.Clear();
            //LoadTrafficlights(ConnectString1, TrafficLights1);
            //grid1.Items.Refresh();


            LoadRouteDataBase(inputRoute,ref fileName1,ref ConnectString1, grid1);
            Inputfilenametextblock.Text = fileName1;
        }

        private void OpenOutputButton_Click(object sender, RoutedEventArgs e)
        {
            var openFileDialog = new OpenFileDialog { };
            var result = openFileDialog.ShowDialog();
            if (result != true) return;

            
            fileName2 = openFileDialog.FileName;


            ConnectString2 = ConnectSrting0 + fileName2 + ";";

            // TrafficLights2.Clear();
            //LoadTrafficlights(ConnectString2, TrafficLights2);
            //grid2.Items.Refresh();

            LoadRouteDataBase(outputRoute,ref fileName2,ref ConnectString2, grid2);
            Outputfilenametextblock.Text = fileName2;
        }

        void LoadRouteDataBase(DbRoute route,ref string fileName,ref string connectstring, DataGrid trafficLightDataGrid )
        {
            //var openFileDialog = new OpenFileDialog { };
            //var result = openFileDialog.ShowDialog();
            //if (result != true) return;

            //fileName = openFileDialog.FileName;
            //connectstring = ConnectSrting0 + fileName + ";";


            route.DbRouteClear();
            DbDataLoader dbrouteLoader = new DbDataLoader(connectstring, route);
            dbrouteLoader.LoadData();

            trafficLightDataGrid.ItemsSource = route.TrafficLights;


            // сортировка таблиц по координате маршруту
            trafficLightDataGrid.Items.SortDescriptions.Clear();
            trafficLightDataGrid.Items.SortDescriptions.Add(new SortDescription("StartRouteCoordinate", ListSortDirection.Ascending));

            trafficLightDataGrid.Items.Refresh();

            //foreach (var item in trafficLightDataGrid.Items)
            //{

            //    var row = trafficLightDataGrid.ItemContainerGenerator.ContainerFromItem(item) as DataGridRow;
            //   if (row!=null) row.Background = new SolidColorBrush(Colors.LightGreen);
            //}

            Dispatcher.BeginInvoke(() =>
            {
                foreach (var item in trafficLightDataGrid.Items)
                {
                   // if (trafficLightDataGrid.ItemContainerGenerator.ContainerFromItem(item) is DataGridRow row)
                     //   row.Background = Brushes.LightGreen;
                }
            });
        }

        public void LoadTrafficlights(string cstring, List<TrafficLight> TrafficLights)
        {
            myConnection = new OleDbConnection(cstring);
            myConnection.Open();

            
            string queryspeed =
                "SELECT TrafficLight.TrackObjectID, TrafficLightName, DicTrafficLightKindID, StationID, T.TrackObjectName " +
                "FROM TrafficLight " +
                "LEFT JOIN TrackObject AS [T] ON TrafficLight.TrackObjectID = T.TrackObjectID";


            OleDbCommand command6 = new OleDbCommand(queryspeed, myConnection);

            OleDbDataReader reader6 = command6.ExecuteReader();

            while (reader6.Read())
            {

                TrafficLight light = new TrafficLight();
                light.TrackObjectID = Convert.ToDouble(reader6[0]);
                light.Start.TrackObjectID = Convert.ToDouble(reader6[0]);

                light.TrafficLightName = reader6[1].ToString();
                light.DicTrafficLightKindID = Convert.ToDouble(reader6[2]);
                light.TrackObjectName = reader6[4].ToString();
                
                
                TrafficLights.Add(light);

                /*TrafficLights.Add(new TrafficLight(
                    Convert.ToDouble(reader6[0]),
                    reader6[1].ToString(),
                    Convert.ToDouble(reader6[2]))
                );
                TrafficLights[TrafficLights.Count - 1].Start.TrackObjectID = Convert.ToDouble(reader6[0]);
                */
            }
            reader6.Close();

            foreach (TrafficLight l in TrafficLights)
            {
                string querylightlamp =
                        "SELECT TrackObjectID, FilmID, FrameTime, DicLampUsageID, X, Y, DX, DY, DarkDX, DarkDY, A, R, G, B, LightInFilm " +
                        "FROM TrafficLightLampInFrame " +
                        "WHERE TrackObjectID = " + l.TrackObjectID.ToString("G", CultureInfo.InvariantCulture);
                
                OleDbCommand command7 = new OleDbCommand(querylightlamp, myConnection);

                OleDbDataReader reader7 = command7.ExecuteReader();

                while (reader7.Read())
                {

                    TrafficLightLampInFrame lightLampInFrame = new TrafficLightLampInFrame();
                    lightLampInFrame.TrackObjectID = Convert.ToDouble(reader7[0]);
                    lightLampInFrame.FilmID = Convert.ToDouble(reader7[1]); 
                    lightLampInFrame.FrameTime = Convert.ToDouble(reader7[2]);  
                    lightLampInFrame.DicLampUsageID = Convert.ToDouble(reader7[3]);
                    lightLampInFrame.X = Convert.ToDouble(reader7[4]);
                    lightLampInFrame.Y = Convert.ToDouble(reader7[5]);
                    lightLampInFrame.DX = Convert.ToDouble(reader7[6]);
                    lightLampInFrame.DY = Convert.ToDouble(reader7[7]);
                    lightLampInFrame.DarkDX = Convert.ToDouble(reader7[8]);
                    lightLampInFrame.DarkDY = Convert.ToDouble(reader7[9]);
                    lightLampInFrame.A = Convert.ToDouble(reader7[10]);
                    lightLampInFrame.R = Convert.ToDouble(reader7[11]);
                    lightLampInFrame.G = Convert.ToDouble(reader7[12]);
                    lightLampInFrame.B = Convert.ToDouble(reader7[13]);
                    lightLampInFrame.LightInFilm = Convert.ToBoolean(reader7[14]);

                    l.trafficLightLampInFrames.Add(lightLampInFrame);


                }

                string querylight =
                        "SELECT TrackObjectID, FilmID, FrameTime, Left, Top, Height, Width, Visible, BackgroundA, BackgroundR, BackgroundG, BackgroundB " +
                        "FROM TrafficLightInFrame " +
                        "WHERE TrackObjectID = " + l.TrackObjectID.ToString("G", CultureInfo.InvariantCulture);


                OleDbCommand command8 = new OleDbCommand(querylight, myConnection);

                OleDbDataReader reader8 = command8.ExecuteReader();

                while (reader8.Read())
                {

                    TrafficLightInFrame lightInFrame = new TrafficLightInFrame();

                    lightInFrame.TrackObjectID = Convert.ToDouble(reader8[0]);
                    lightInFrame.FilmID = Convert.ToDouble(reader8[1]);
                    lightInFrame.FrameTime = Convert.ToDouble(reader8[2]);
                    lightInFrame.Left = Convert.ToDouble(reader8[3]);
                    lightInFrame.Top = Convert.ToDouble(reader8[4]);
                    lightInFrame.Height = Convert.ToDouble(reader8[5]);
                    lightInFrame.Width = Convert.ToDouble(reader8[6]);
                    lightInFrame.Visible = Convert.ToBoolean(reader8[7]);
                    lightInFrame.BackgroundA = Convert.ToDouble(reader8[8]);
                    lightInFrame.BackgroundR = Convert.ToDouble(reader8[9]);
                    lightInFrame.BackgroundG = Convert.ToDouble(reader8[10]);
                    lightInFrame.BackgroundB = Convert.ToDouble(reader8[11]);

                    

                    l.trafficLightInFrames.Add(lightInFrame);


                }


                l.trafficLightInFramesCount = l.trafficLightInFrames.Count;
                l.trafficLightLampInFramesCount = l.trafficLightLampInFrames.Count;
            }

            myConnection.Close();

            DbRoute route1 = new DbRoute();
            route1.TrafficLights = TrafficLights;
            DbDataLoader dataLoader = new DbDataLoader(cstring, route1);

            dataLoader._connection.Open();
            dataLoader.LoadTliRestrictions();
            dataLoader._connection.Close();

            //foreach (TrafficLight t in TrafficLights)
            //{
              //  t.tliRestrictionsCount = t.TliRestrictions.Count;
            //}

        }

        //void InsertTrafficLightLampInFrame(
        // double TrackObjectID,
        // double FilmID,
        // double FrameTime,
        // double DicLampUsageID,
        // double X,
        // double Y,
        // double DX,
        // double DY,
        // double DarkDX,
        // double DarkDY,
        // double A,
        // double R,
        // double G,
        // double B,
        // bool LightInFilm,
        //OleDbConnection _myConnection)
        //{
        //    string query4 = "INSERT INTO TrafficLightLampInFrame ( TrackObjectID, FilmID, FrameTime, DicLampUsageID, X, Y, DX, DY, DarkDX, DarkDY, A, R, G, B, LightInFilm ) " +
        //    "VALUES (" + TrackObjectID 
        //    + ", " + FilmID 
        //    + ", " + FrameTime.ToString("G", CultureInfo.InvariantCulture)
        //    + ", " + DicLampUsageID 
        //    + ", " + X.ToString("G", CultureInfo.InvariantCulture)
        //    + ", " + Y.ToString("G", CultureInfo.InvariantCulture)
        //    + ", " + DX.ToString("G", CultureInfo.InvariantCulture)
        //    + ", " + DY.ToString("G", CultureInfo.InvariantCulture)
        //    + ", " + DarkDX.ToString("G", CultureInfo.InvariantCulture)
        //    + ", " + DarkDY.ToString("G", CultureInfo.InvariantCulture)
        //    + ", " + A
        //    + ", " + R
        //    + ", " + G
        //    + ", " + B
        //    + ", " + LightInFilm + " )";
        //    OleDbCommand command4 = new OleDbCommand(query4, _myConnection);
        //    command4.ExecuteNonQuery();
        //}

        //void InsertTrafficLightInFrame(
        // double TrackObjectID,
        // double FilmID,
        // double FrameTime,
        // double Left,
        // double Top,
        // double Height,
        // double Width,
        // bool Visible,
        // double BackgroundA,
        // double BackgroundR,
        // double BackgroundG,
        // double BackgroundB,
        // OleDbConnection _myConnection
        //    )
        //{

        //    string query40 = "INSERT INTO TrafficLightInFrame ( TrackObjectID, FilmID, FrameTime, [Top], Visible ) " +
        //    "VALUES (" + TrackObjectID
        //    + ", " + FilmID
        //    + ", " + FrameTime.ToString("G", CultureInfo.InvariantCulture)
        //    + ", " + Left.ToString("G", CultureInfo.InvariantCulture)
        //    + ", -1 )";

        //    string query4 = "INSERT INTO TrafficLightInFrame ( TrackObjectID, FilmID, FrameTime, [Left], [Top], [Height], [Width], Visible, BackgroundA, BackgroundR, BackgroundG, BackgroundB ) " +
        //    "VALUES (" + TrackObjectID
        //    + ", " + FilmID
        //    + ", " + FrameTime.ToString("G", CultureInfo.InvariantCulture)
        //    + ", " + Left.ToString("G", CultureInfo.InvariantCulture)
        //    + ", " + Top.ToString("G", CultureInfo.InvariantCulture)
        //    + ", " + Height.ToString("G", CultureInfo.InvariantCulture)
        //    + ", " + Width.ToString("G", CultureInfo.InvariantCulture)
        //    + ", " + Visible
        //    + ", " + BackgroundA
        //    + ", " + BackgroundR
        //    + ", " + BackgroundG
        //    + ", " + BackgroundB
        //    + " )";
        //    OleDbCommand command5 = new OleDbCommand(query4, _myConnection);
        //    command5.ExecuteNonQuery();
        //}


        private void ExportTliButton_Click(object sender, RoutedEventArgs e)
        {
            myConnection = new OleDbConnection(ConnectString2);
            myConnection.Open();
            //foreach (var item1 in TrafficLights1)
            foreach (var item1 in outputRoute.TrafficLights)
            {

                int index = inputRoute.TrafficLights.IndexOf(item1);
                TrafficLight trafficLight1 = (TrafficLight)item1;

                var item2 = outputRoute.TrafficLights[index];
                TrafficLight trafficLight2 = (TrafficLight)item2;
                

                foreach (TliRestriction tllf in trafficLight1.TliRestrictions)
                {
                    DbRouteDataExporter.InsertTliRestriction(tllf, trafficLight2.TrackObjectID, myConnection);
                }
                

            }
            myConnection.Close();
            MessageBox.Show("ok");

            //TrafficLights1.Clear();
            //LoadTrafficlights(ConnectString1, TrafficLights1);
            grid1.Items.Refresh();

            //TrafficLights2.Clear();
            //LoadTrafficlights(ConnectString2, TrafficLights2);
            LoadRouteDataBase(outputRoute, ref fileName2, ref ConnectString2, grid2);
            grid2.Items.Refresh();

            
        }

        private void ExportButton_Click(object sender, RoutedEventArgs e)
        {
            myConnection = new OleDbConnection(ConnectString2);
            myConnection.Open();
            //foreach ( var item1 in TrafficLights1 ) 
            foreach (var griditem in grid1.Items)
            { 
                

                //int index = TrafficLights1.IndexOf( item1 );

                int index = grid1.Items.IndexOf(griditem);
                TrafficLight trafficLight1 = (TrafficLight)griditem;//item1;

                var item2 = grid2.Items[index]; // TrafficLights2[index];
                TrafficLight trafficLight2 = ( TrafficLight )item2;

                

                foreach (TrafficLightLampInFrame tllf in trafficLight1.trafficLightLampInFrames )
                {
                        //InsertTrafficLightLampInFrame(
                        //   trafficLight2.TrackObjectID,
                        //   tllf.FilmID,
                        //   tllf.FrameTime,
                        //   tllf.DicLampUsageID,
                        //   tllf.X,
                        //   tllf.Y,
                        //   tllf.DX,
                        //   tllf.DY,
                        //   tllf.DarkDX,
                        //   tllf.DarkDY,
                        //   tllf.A,
                        //   tllf.R,
                        //   tllf.G,
                        //   tllf.B,
                        //   tllf.LightInFilm,
                        //   myConnection
                        //    );

                        //TrafficLightLampInFrame tllf2 = new TrafficLightLampInFrame();
                        //tllf2.TrackObjectID = trafficLight2.TrackObjectID;

                        DbRouteDataExporter.InsertTrafficLightLampInFrame(trafficLight2.TrackObjectID,tllf, myConnection);
                            
                }

                foreach (TrafficLightInFrame tlf in trafficLight1.trafficLightInFrames )
                {
                    //InsertTrafficLightInFrame(
                    //   trafficLight2.TrackObjectID,
                    //    tlf.FilmID,
                    //    tlf.FrameTime,
                    //    tlf.Left,
                    //    tlf.Top,
                    //    tlf.Height,
                    //    tlf.Width,
                    //    tlf.Visible,
                    //    tlf.BackgroundA,
                    //    tlf.BackgroundR,
                    //    tlf.BackgroundG,
                    //    tlf.BackgroundB,
                    //   myConnection
                    //    );
                    

                    DbRouteDataExporter.InsertTrafficLightInFrame(trafficLight2.TrackObjectID, tlf, myConnection);
                }

            }
            myConnection.Close();




            MessageBox.Show("ok");

            //TrafficLights1.Clear();
            //LoadTrafficlights(ConnectString1, TrafficLights1);
            grid1.Items.Refresh();

            //TrafficLights2.Clear();
            //LoadTrafficlights(ConnectString2, TrafficLights2);
            LoadRouteDataBase(outputRoute,ref fileName2,ref ConnectString2, grid2);
            grid2.Items.Refresh();
        }

        private void MoveFramesButton_Click(object sender, RoutedEventArgs e)
        {

            myConnection = new OleDbConnection(ConnectString2);
            myConnection.Open();

            string ddistancestring = MoveFramesTextBox.Text.Replace(".", ",");

            double ddistance = Convert.ToDouble(ddistancestring);

            string ddistanceSign;
            if (ddistance >= 0)
            {
                ddistanceSign = "+";
            }
            else
            {
                ddistanceSign = "-";
            }

            foreach (TrafficLight t in TrafficLights2)
            {
                string query =
                    "UPDATE TrafficLightInFrame " +
                    "SET FrameTime = FrameTime " + ddistanceSign + " " + Math.Abs(ddistance).ToString("G", CultureInfo.InvariantCulture) + " " +
                    "WHERE TrafficLightInFrame.TrackObjectID = " + t.TrackObjectID.ToString("G", CultureInfo.InvariantCulture) + " ";

                string query2 =
                    "UPDATE TrafficLightLampInFrame " +
                    "SET FrameTime = FrameTime " + ddistanceSign + " " + Math.Abs(ddistance).ToString("G", CultureInfo.InvariantCulture) + " " +
                    "WHERE TrafficLightLampInFrame.TrackObjectID = " + t.TrackObjectID.ToString("G", CultureInfo.InvariantCulture) + " ";

                OleDbCommand command4 = new OleDbCommand(query, myConnection);
                OleDbDataReader reader = command4.ExecuteReader();

                OleDbCommand command5 = new OleDbCommand(query2, myConnection);
                OleDbDataReader reader5 = command5.ExecuteReader();

            }
            myConnection.Close();
            MessageBox.Show("ok");
        }
        
    }
}