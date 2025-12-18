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
    /// Interaction logic for ObjectSearchTabControl.xaml
    /// </summary>
    public partial class ObjectSearchTabControl : UserControl
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

        public event RoutedEventHandler EgisFindPointObjectsClicked;
        public event MouseButtonEventHandler EgisFoundPointObjectsGridDoubleClick;

        public string PointObjectToFindText
        {
            get => PointObjectToFindTextBox.Text;
            set => PointObjectToFindTextBox.Text = value;
        }

        public ObjectSearchTabControl()
        {
            InitializeComponent();
        }

        private void EgisFindPointObjectsButton_Click(object sender, RoutedEventArgs e)
        {
            //найти обьект в егис по названию обьекта и станции
            
            string EgisStationID;
            if (_appData.EgisSelectedStation == null)
            {
                EgisStationID = "";
            }
            else
            {
                EgisStationID = _appData.EgisSelectedStation.EgisStationID.ToString();
            }

            _appData.ObjectNameToFind = PointObjectToFindTextBox.Text;
            
            _appData.EgisSelectedTracks.Clear();
            _appData.EgisFoundPointObjects.Clear();

            EgisImporter.EgisFindPointObject(EgisStationID, _appData.ObjectNameToFind, _appData.StationNameToFind, _appData.EgisConnectionString, _appData.EgisSelectedTracks, _appData.EgisFoundPointObjects);

            //EgisSearchControl1.EgisTrackGrid.Items.Refresh();

            EgisFoundPointObjectsGrid.ItemsSource = _appData.EgisFoundPointObjects;
            EgisFoundPointObjectsGrid.Items.Refresh();

            EgisFindPointObjectsClicked?.Invoke(sender, e);
        }

        private void EgisFoundPointObjectsGrid_MouseDoubleClick(object sender, MouseButtonEventArgs e)
        {
            _appData.EgisFoundPointOnTrack = (PointOnTrack)EgisFoundPointObjectsGrid.SelectedItem;
            _appData.EgisSelectedTrack.TrackID = _appData.EgisFoundPointOnTrack.TrackID;

            EgisImporter egisImporter = new EgisImporter(_appData);
            if (_appData.EgisSelectedTrack != null) { egisImporter.LoadEgisData(); }

            EgisFoundPointObjectsGridDoubleClick?.Invoke(sender, e);
        }
    }
}
