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


            LoadRouteDataBase(outputRoute,ref fileName2,ref ConnectString2, grid2);
            Outputfilenametextblock.Text = fileName2;
        }

        void LoadRouteDataBase(DbRoute route,ref string fileName,ref string connectstring, DataGrid trafficLightDataGrid )
        {
            

            route.DbRouteClear();
            DbDataLoader dbrouteLoader = new DbDataLoader(connectstring, route);
            dbrouteLoader.LoadData();

            trafficLightDataGrid.ItemsSource = route.TrafficLights;


            // сортировка таблиц по координате маршруту
            trafficLightDataGrid.Items.SortDescriptions.Clear();
            trafficLightDataGrid.Items.SortDescriptions.Add(new SortDescription("StartRouteCoordinate", ListSortDirection.Ascending));

            trafficLightDataGrid.Items.Refresh();

            
            Dispatcher.BeginInvoke(() =>
            {
                foreach (var item in trafficLightDataGrid.Items)
                {
                   // if (trafficLightDataGrid.ItemContainerGenerator.ContainerFromItem(item) is DataGridRow row)
                     //   row.Background = Brushes.LightGreen;
                }
            });
        }
        
        private void ExportTliButton_Click(object sender, RoutedEventArgs e)
        {
            myConnection = new OleDbConnection(ConnectString2);
            myConnection.Open();
            
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

            
            grid1.Items.Refresh();

            
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
                

                int index = grid1.Items.IndexOf(griditem);
                TrafficLight trafficLight1 = (TrafficLight)griditem;

                var item2 = grid2.Items[index]; 
                TrafficLight trafficLight2 = ( TrafficLight )item2;

                foreach (TrafficLightLampInFrame tllf in trafficLight1.trafficLightLampInFrames )
                {
                        DbRouteDataExporter.InsertTrafficLightLampInFrame(trafficLight2.TrackObjectID,tllf, myConnection);
                }

                foreach (TrafficLightInFrame tlf in trafficLight1.trafficLightInFrames )
                {
                    DbRouteDataExporter.InsertTrafficLightInFrame(trafficLight2.TrackObjectID, tlf, myConnection);
                }
            }
            myConnection.Close();

            MessageBox.Show("ok");

            
            grid1.Items.Refresh();
            
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