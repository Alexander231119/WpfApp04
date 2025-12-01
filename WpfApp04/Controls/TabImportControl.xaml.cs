using System;
using System.Collections;
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
    /// Interaction logic for TabImportControl.xaml
    /// </summary>
    public partial class TabImportControl : UserControl
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
        public event RoutedEventHandler AddSegmentsToFillFromEgisClicked;
        public event RoutedEventHandler FillFromEgisClicked;
        public event RoutedEventHandler InsertFromEgisToBaseClicked;
        public event RoutedEventHandler ClearToAddListsClicked;

        public bool DeleteTrackCircuits
        {
            get => DeleteTrackCircuitsChickBox?.IsChecked ?? false;
            set => DeleteTrackCircuitsChickBox.IsChecked = value;
        }

        public bool KmShorten
        {
            get => KmShortenCheckBox?.IsChecked ?? false;
            set => KmShortenCheckBox.IsChecked = value;
        }

        public string SegmentsToFillText
        {
            get => SegmentsToFillFromEgisTextBlock.Text;
            set => SegmentsToFillFromEgisTextBlock.Text = value;
        }

        public string SegmentsSourceText
        {
            get => SegmentsSourceFromEgisTextBlock.Text;
            set => SegmentsSourceFromEgisTextBlock.Text = value;
        }

        public TabImportControl()
        {
            InitializeComponent();
        }

        private void AddSegmentsToFillFromEgisButton_Click(object sender, RoutedEventArgs e)
        {
            //кнопка добавить сегменты в список сегментов для импорта
            IList<Segment> targetCollection;
            TextBlock targetTextBlock;
            string message = "";

            if (SegmentsToFillFromEgisGrid.ItemsSource == _appData.Route1.Segments)
            {
                targetCollection = _appData.SegmentsToFillFromEgis;
                targetTextBlock = SegmentsToFillFromEgisTextBlock;
            }
            else if (SegmentsToFillFromEgisGrid.ItemsSource == _appData.EgisRoute1.Segments)
            {
                targetCollection = _appData.SegmentsSourseFromEgis;
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

            AddSegmentsToFillFromEgisClicked?.Invoke(sender, e);
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

            routeLoader._routeExportCheckBoxList._DeleteTrackCircuitsChickBox = DeleteTrackCircuitsChickBox.IsChecked ?? false;
            ImportOptionsControl1.ApplyToCheckBoxList(routeLoader._routeExportCheckBoxList);

            routeLoader.FillFromRouteToRoute();

            
            FillFromEgisClicked?.Invoke(sender, e);
        }

        private void InsertFromEgisToBaseButton_Click(object sender, RoutedEventArgs e)
        {
            DbRouteDataExporter drde = new DbRouteDataExporter(_appData.ConnectString, _appData.ToAddRoute, _appData.Route1, _appData.PointOnTracksToAdd);

            drde._routeExportCheckBoxList._DeleteTrackCircuitsChickBox = DeleteTrackCircuitsChickBox.IsChecked ?? false;
            ImportOptionsControl1.ApplyToCheckBoxList(drde._routeExportCheckBoxList);
            drde.AddTrackObjectsFromDbRouteToBase();
            SegmentsSourceFromEgisTextBlock.Text = "";
            SegmentsToFillFromEgisTextBlock.Text = "";

            InsertFromEgisToBaseClicked?.Invoke(sender, e);
        }

        private void ClearToAddListsButtony_Click(object sender, RoutedEventArgs e)
        {
            _appData.ToAddRoute.DbRouteClear();
            _appData.PointOnTracksToAdd.Clear();

            ClearToAddListsClicked?.Invoke(sender, e);
        }
    }
}
