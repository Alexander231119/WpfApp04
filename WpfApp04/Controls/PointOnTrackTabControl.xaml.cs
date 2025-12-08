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
    /// Interaction logic for PointOnTrackTabControl.xaml
    /// </summary>
    public partial class PointOnTrackTabControl : UserControl
    {
        private AppDbRouteContextData _appData;
        public AppDbRouteContextData AppData
        {
            get => _appData;
            set { _appData = value; }
        }
        public event SelectionChangedEventHandler PointOnTrackEditGridSelectionChanged;
        public event RoutedEventHandler SaveToDbClicked;
        public event RoutedEventHandler AddPointOnTrackClicked;

        public PointOnTrackMenuControl PointOnTrackMenuControl => pointOnTrackMenuControl1;
        public ImportOptionsControl ImportOptionsControl => ImportOptionsControl2;
        //public DataGrid PointOnTrackEditGrid => PointOnTrackEditGrid;
        //public Grid PointOnTrackEditControlsContainer => PointOnTrackEditControlsContainer;

        // для прямого доступа из вне контрола
        //public DataGrid PointOnTrackGrid => PointOnTrackEditGrid;
        //public Grid EditControlsContainer => PointOnTrackEditControlsContainer;

        public PointOnTrackTabControl()
        {
            InitializeComponent();
        }

        private void PointOnTrackEditGrid_SelectionChanged(object sender, SelectionChangedEventArgs e)
        {
            if (_appData.EgisPtNormsGridLock == true) return;

            PointOnTrack item = (PointOnTrack)PointOnTrackEditGrid.SelectedItem;
            if (item == null) return;

            // Очищаем контейнер перед добавлением нового контрола
            PointOnTrackEditControlsContainer.Children.Clear();

            // В зависимости от типа точки создаем соответствующий контрол
            switch (item.DicPointOnTrackKindID)
            {
                case 1: // Светофор
                    var trafficLightControl = new TrafficLightEditControl();
                    trafficLightControl.TrafficLight = PointOnTrack.GetTrafficLightForPoint(item, _appData.Route1);
                    trafficLightControl.RefreshFromTrafficLight();
                    PointOnTrackEditControlsContainer.Children.Add(trafficLightControl);
                    break;

                case 40: // Токораздел 
                    var currentKindControl = new CurrentKindChangeEditControl();
                    currentKindControl.CurrentKindChange = PointOnTrack.GetCurrentKindChangeForPoint(item, _appData.Route1);
                    currentKindControl.RefreshFromCurrentKindChange();
                    PointOnTrackEditControlsContainer.Children.Add(currentKindControl);
                    break;
                case 54: // точка смены рода тока
                    var currentKindControl1 = new CurrentKindChangeEditControl();
                    currentKindControl1.CurrentKindChange = PointOnTrack.GetCurrentKindChangeForPoint(item, _appData.Route1);
                    currentKindControl1.RefreshFromCurrentKindChange();
                    PointOnTrackEditControlsContainer.Children.Add(currentKindControl1);
                    break;
                    // Добавьте другие case для других типов контролов по необходимости
            }

            // Обновляем меню точки на пути (если нужно)
            if (_appData.RouteToShowInDataGrids != null) pointOnTrackMenuControl1._route = _appData.RouteToShowInDataGrids;

            pointOnTrackMenuControl1.p = item;
            pointOnTrackMenuControl1.MenuRefresh();
            PointOnTrackEditGridSelectionChanged?.Invoke(sender, e);
        }

        private void SaveToDbButton1_Click(object sender, RoutedEventArgs e)
        {
            List<PointOnTrack> emptylist = new();

            DbRouteDataExporter drde = new DbRouteDataExporter(_appData.ConnectString, _appData.Route1, _appData.Route1, emptylist);
            ImportOptionsControl2.ApplyToCheckBoxList(drde._routeExportCheckBoxList);

            // нужно дописать метод для удаления обьектов выбранной категории из базы
            // drde.DeleteObjectsFromDb();
            

            // экспорт обьектов
            drde.AddTrackObjectsFromDbRouteToBase();

            _appData.DbData_Changed();
            //SaveToDbClicked?.Invoke(sender, e);
        }

        private void AddPointOnTrackButton1_Click(object sender, RoutedEventArgs e)
        {
            PointOnTrack p = new PointOnTrack();
            _appData.Route1.PointOnTracks.Add(p);

            p.DicPointOnTrackKindID = 25; // по умолчанию укспс
            _appData.PointOnTracksToShow = ImportOptionsControl2.FilterPoints(_appData.RouteToShowInDataGrids.PointOnTracks).ToList();
            PointOnTrackEditGrid.ItemsSource = _appData.PointOnTracksToShow;
            PointOnTrackEditGrid.Items.Refresh();
            AddPointOnTrackClicked?.Invoke(sender, e);
        }
    }
}
