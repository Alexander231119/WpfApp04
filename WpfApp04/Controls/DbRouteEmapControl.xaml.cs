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
using VideoLib;

namespace WpfApp04.Controls
{
    /// <summary>
    /// Interaction logic for DbRouteEmapControl.xaml
    /// </summary>
    public partial class DbRouteEmapControl : UserControl
    {
        bool listlock = false;
        public RoutesElectronicMap dbElectronicMap = new();
        public int mapId; 
        public int routeId;
        public Window _window;
        public event Action<int, int> RouteSelected;

        public List<Station> SelectedStations = new List<Station>();
        public List<PointOnTrack> FoundPointObjects = new List<PointOnTrack>(); // для поиска по имени обьекта
        public Station selectedStation;


        public DbRouteEmapControl()
        {
            InitializeComponent();
        }

        //public void UpdatedbElectronicMapListBox()
        //{

        //    listlock = true;
        //    dbElectronicMapListBox.Items.Clear();



        //    //заполнить таблицу ekklub
        //    foreach (var c in dbElectronicMap.RoutesEkklubsList)
        //    {

        //        //int index = c.Key;

        //        //string scarid = "";

        //        ListBoxItem item = new ListBoxItem
        //        {
        //            Tag = c.Key,
        //            Content = c.Key + "  ekklub" 
        //        };
        //        dbElectronicMapListBox.Items.Add(item);
        //    }
        //    dbElectronicMapListBox.Items.Refresh();
        //    listlock = false;
        //}
        public void UpdatedbElectronicMapListBox(Dictionary<int, Dictionary<int, DbRoute>> filteredRoutes = null)
        {
            listlock = true;
            dbElectronicMapListBox.Items.Clear();

            if (filteredRoutes == null)
            {
                // Показываем все маршруты, если фильтр не задан
                foreach (var c in dbElectronicMap.RoutesEkklubsList)
                {
                    ListBoxItem item = new ListBoxItem
                    {
                        Tag = c.Key,
                        Content = c.Key + "  ekklub"
                    };
                    dbElectronicMapListBox.Items.Add(item);
                }
            }
            else
            {
                // Показываем только отфильтрованные маршруты
                foreach (var ekklubId in filteredRoutes.Keys)
                {
                    ListBoxItem item = new ListBoxItem
                    {
                        Tag = ekklubId,
                        Content = ekklubId + "  ekklub"
                    };
                    dbElectronicMapListBox.Items.Add(item);
                }
            }

            dbElectronicMapListBox.Items.Refresh();
            listlock = false;
        }

        private void dbElectronicMapListBox_SelectionChanged(object sender, SelectionChangedEventArgs e)
        {
            // заполнить список dbEmapRoutesListBox

            listlock = true;
            dbEmapRoutesListBox.Items.Clear();

            var item = (ListBoxItem)dbElectronicMapListBox.SelectedItem;
            int id = Convert.ToInt32(item?.Tag);
            mapId = id;
            //dbElectronicMap.RoutesEkklubsList[9].RoutesList[2];
            if (item is null) return;
            foreach (var k in dbElectronicMap.RoutesEkklubsList[id].RoutesList)
            {
                ListBoxItem item1 = new ListBoxItem
                {
                    Tag = k.Key,
                    Content = k.Key + "  klubway"
                };
                dbEmapRoutesListBox.Items.Add(item1);
            }
            dbEmapRoutesListBox.Items.Refresh();

            listlock = false;
        }

        private void dbEmapRoutesListBox_SelectionChanged(object sender, SelectionChangedEventArgs e)
        {
            if (listlock == false)
            {
                var item = (ListBoxItem)dbEmapRoutesListBox.SelectedItem;
                routeId = Convert.ToInt32(item?.Tag);

                //_window.DrawRouteWay(waywrapPanel, dbElectronicMap.RoutesEkklubsList[mapId].RoutesList[routeId]);
                RouteSelected?.Invoke(mapId, routeId);
            }
        }

        private void StationToFindTextButton_Click(object sender, RoutedEventArgs e)
        {
            string searchText = StationToFindTextBox.Text.Trim().ToLower();
            SelectedStations.Clear();

            if (string.IsNullOrEmpty(searchText))
            {
                FoundStationsGrid.ItemsSource = null;
                return;
            }

            // Ищем станции по частичному совпадению имени
            foreach (var ekklubPair in dbElectronicMap.RoutesEkklubsList)
            {
                foreach (var routePair in ekklubPair.Value.RoutesList)
                {
                    var stations = routePair.Value.Stations
                        .Where(s => s.StationName.ToLower().Contains(searchText))
                        .ToList();

                    SelectedStations.AddRange(stations);
                }
            }

            // Удаляем дубликаты станций (если одна станция есть в нескольких маршрутах)
            SelectedStations = SelectedStations
                .GroupBy(s => s.StationName)
                .Select(g => g.First())
                .ToList();

            // Обновляем DataGrid
            FoundStationsGrid.ItemsSource = SelectedStations;
        }

        private void ObjectToFindTextButton_Click(object sender, RoutedEventArgs e)
        {
            string searchText = ObjectToFindTextBox.Text.Trim().ToLower();
            FoundPointObjects.Clear();

            if (string.IsNullOrEmpty(searchText))
            {
                EgisFoundPointObjectsGrid.ItemsSource = null;
                UpdatedbElectronicMapListBox();
                return;
            }

            // Получаем выбранную станцию (если есть)
            selectedStation = FoundStationsGrid.SelectedItem as Station;

            // Собираем все маршруты, где есть совпадения
            var matchingRoutes = new Dictionary<int, Dictionary<int, DbRoute>>();

            foreach (var ekklubPair in dbElectronicMap.RoutesEkklubsList)
            {
                int ekklubId = ekklubPair.Key;
                var routesDict = new Dictionary<int, DbRoute>();

                foreach (var routePair in ekklubPair.Value.RoutesList)
                {
                    int routeId = routePair.Key;
                    var route = routePair.Value;

                    // Находим соответствующие объекты
                    var matchingTrafficLights = route.TrafficLights
                        .Where(t => t.TrafficLightName.ToLower().Contains(searchText) &&
                                   (selectedStation == null || t.Station == selectedStation.StationName))
                        .Select(t => t.Start);

                    var matchingCrossingPieces = route.CrossingPieces
                        .Where(c => c.CrossingPieceName.ToLower().Contains(searchText) &&
                                   (selectedStation == null || c.Station == selectedStation.StationName))
                        .Select(c => c.Start);

                    var matchingObjects = matchingTrafficLights.Concat(matchingCrossingPieces).ToList();

                    if (matchingObjects.Any())
                    {
                        FoundPointObjects.AddRange(matchingObjects);
                        routesDict.Add(routeId, route);
                    }
                }

                if (routesDict.Count > 0)
                {
                    matchingRoutes.Add(ekklubId, routesDict);
                }
            }

            // Обновляем DataGrid с найденными объектами
            EgisFoundPointObjectsGrid.ItemsSource = FoundPointObjects;
            EgisFoundPointObjectsGrid.Items.Refresh();


            // Обновляем ListBox с найденными маршрутами
            UpdateListBoxesWithFilteredRoutes(matchingRoutes);
        }

        private void UpdateListBoxesWithFilteredRoutes(Dictionary<int, Dictionary<int, DbRoute>> matchingRoutes)
        {
            listlock = true;
            dbElectronicMapListBox.Items.Clear();
            dbEmapRoutesListBox.Items.Clear();

            // Заполняем список электронных карт (ekklubs)
            foreach (var ekklubId in matchingRoutes.Keys)
            {
                ListBoxItem item = new ListBoxItem
                {
                    Tag = ekklubId,
                    Content = ekklubId + "  ekklub"
                };
                dbElectronicMapListBox.Items.Add(item);
            }

            dbElectronicMapListBox.Items.Refresh();
            listlock = false;

            // Если есть только один ekklub, автоматически выбираем его
            if (matchingRoutes.Count == 1)
            {
                dbElectronicMapListBox.SelectedIndex = 0;
            }
        }

        private void FoundStationsGrid_MouseDoubleClick(object sender, MouseButtonEventArgs e)
        {
            if (FoundStationsGrid.SelectedItem is Station selectedStation)
            {
                // Фильтруем маршруты, содержащие выбранную станцию
                var filteredRoutes = new Dictionary<int, Dictionary<int, DbRoute>>();

                foreach (var ekklubPair in dbElectronicMap.RoutesEkklubsList)
                {
                    int ekklubId = ekklubPair.Key;
                    var routesDict = new Dictionary<int, DbRoute>();

                    foreach (var routePair in ekklubPair.Value.RoutesList)
                    {
                        if (routePair.Value.Stations.Any(s => s.StationName == selectedStation.StationName))
                        {
                            routesDict.Add(routePair.Key, routePair.Value);
                        }
                    }

                    if (routesDict.Count > 0)
                    {
                        filteredRoutes.Add(ekklubId, routesDict);
                    }
                }

                UpdateListBoxesWithFilteredRoutes(filteredRoutes);
            }
        }

        private void EgisFoundPointObjectsGrid_MouseDoubleClick(object sender, MouseButtonEventArgs e)
        {
            if (EgisFoundPointObjectsGrid.SelectedItem is PointOnTrack selectedPoint)
            {
                // Находим маршруты, содержащие этот объект
                var matchingRoutes = new Dictionary<int, Dictionary<int, DbRoute>>();

                foreach (var ekklubPair in dbElectronicMap.RoutesEkklubsList)
                {
                    int ekklubId = ekklubPair.Key;
                    var routesDict = new Dictionary<int, DbRoute>();

                    foreach (var routePair in ekklubPair.Value.RoutesList)
                    {
                        int routeId = routePair.Key;
                        var route = routePair.Value;

                        // Проверяем, есть ли в маршруте объект с таким TrackObjectID
                        bool hasTrafficLight = route.TrafficLights.Any(light =>
                            light.TrackObjectID == selectedPoint.TrackObjectID);

                        bool hasCrossingPiece = route.CrossingPieces.Any(piece =>
                            piece.TrackObjectID == selectedPoint.TrackObjectID);

                        bool hasObject = hasTrafficLight || hasCrossingPiece;

                        if (hasObject)
                        {
                            routesDict.Add(routeId, route);
                        }
                    }

                    if (routesDict.Count > 0)
                    {
                        matchingRoutes.Add(ekklubId, routesDict);
                    }
                }

                // Обновляем ListBox с найденными маршрутами
                UpdateListBoxesWithFilteredRoutes(matchingRoutes);
            }
        }
    }
}
