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
using WpfAapp04;
using WpfApp04.ViewModels;
using VideoLib;

namespace WpfApp04.Controls
{
    /// <summary>
    /// Interaction logic for EgisSearchControl.xaml
    /// </summary>
    public partial class EgisSearchControl : UserControl
    {

        private AppDbRouteContextData _appData;
        public AppDbRouteContextData AppData
        {
            get => _appData;
            set { _appData = value; }
        }

        public event KeyEventHandler StationToFindKeyDown;
        public event RoutedEventHandler EgisFindStationClicked;
        public event RoutedEventHandler EgisLoadDataClicked;
        public event RoutedEventHandler ShowEgisPreviewClicked;
        public event MouseButtonEventHandler EgisStationsGridDoubleClick;
        public event MouseButtonEventHandler EgisTrackGridDoubleClick;
        public event SelectionChangedEventHandler EgisTrackGridSelectionChanged;
        public event RoutedEventHandler MaintrackRadioButtonChecked;
        public event RoutedEventHandler SideTrackRadioButtonChecked;
        public event RoutedEventHandler UpUsageDirectionToFindRadioButtonChecked;
        public event RoutedEventHandler DownUsageDirectionToFindRadioButtonChecked;
        public event RoutedEventHandler FreightSpeedRadioButtonChecked;
        public event RoutedEventHandler PassSpeedRadioButtonChecked;
        public event RoutedEventHandler HighSpeedRadioButtonChecked;
        public event RoutedEventHandler VeryHighSpeedRadioButtonChecked;
        public event RoutedEventHandler EtrainSpeedRadioButtonChecked;
        public event RoutedEventHandler MvpsSpeedRadioButtonChecked;
        public event RoutedEventHandler PrigSpeedRadioButtonChecked;
        public event RoutedEventHandler StationDataGridSourceRadioButtonEgisChecked;
        public event RoutedEventHandler StationDataGridSourceRadioButtonDbChecked;
        public event RoutedEventHandler StationDataGridSourceRadioButtonToAddChecked;
        public event EventHandler DataGridSorceRadioButtonChanged;

        public EgisSearchControl()
        {
            InitializeComponent();
        }
        
        private void StaitonToFindTextBox_KeyDown(object sender, KeyEventArgs e)
        {
            if (e.Key == Key.Enter)
            {
                StaitonToFindTextBox.Text = DbRouteHelper.ConvertEngToRus(StaitonToFindTextBox.Text);
                EgisImporter.SelectStations(StaitonToFindTextBox.Text, _appData.EgisConnection, _appData.EgisSelectedStations);
                EgisStationsGrid.Items.Refresh();
                EgisStationsGrid.SelectedIndex = 0;
            }
            StationToFindKeyDown?.Invoke(sender, e);
        }

        private void EgisFindStationButton_Click(object sender, RoutedEventArgs e)
        {
            StaitonToFindTextBox.Text = DbRouteHelper.ConvertEngToRus(StaitonToFindTextBox.Text);
            EgisImporter.SelectStations(StaitonToFindTextBox.Text, _appData.EgisConnection, _appData.EgisSelectedStations);
            EgisStationsGrid.Items.Refresh();
            EgisStationsGrid.SelectedIndex = 0;

            EgisFindStationClicked?.Invoke(sender, e);
        }

        private void EgisLoadDataButton_Click(object sender, RoutedEventArgs e)
        {
            if ((Track)EgisTrackGrid.SelectedItem != null)
            {
                _appData.EgisSelectedTrack = (Track)EgisTrackGrid.SelectedItem;
                EgisImporter egisImporter = new EgisImporter(_appData);
                if (_appData.EgisSelectedTrack != null) { egisImporter.LoadEgisData(); }
                EgisTrackTextBlock.Text = _appData.EgisRoute1.Kilometers.Count.ToString() + "  " + _appData.EgisRoute1.PointOnTracks.Count.ToString();
                DataGridSorceRadioButtonChanged?.Invoke(sender, e);//чтобы обновились таблицы
                //EgisLoadDataClicked?.Invoke(sender, e);
            }
        }

        private void ShowEgisPreviewButton_Click(object sender, RoutedEventArgs e)
        {
            EgisPreview egisPreview = new EgisPreview();
            egisPreview.Title = _appData.EgisSelectedTrack?.TrackNumber;
            DbRouteDrawer.DrawRouteWayFromAppData(_appData, egisPreview.EgisCanvas ,_appData.EgisRoute1);
            egisPreview.Show();
            ShowEgisPreviewClicked?.Invoke(sender, e);
        }

        private void EgisStationsGrid_MouseDoubleClick(object sender, MouseButtonEventArgs e)
        {
            EgisSelectTrack();
            EgisStationsGridDoubleClick?.Invoke(sender, e);
        }
        private void MaintrackRadioButton_Checked(object sender, RoutedEventArgs e)
        {
            EgisSelectTrack();
            MaintrackRadioButtonChecked?.Invoke(sender, e);
        }

        private void SideTrackRadioButton_Checked(object sender, RoutedEventArgs e)
        {
            EgisSelectTrack();
            SideTrackRadioButtonChecked?.Invoke(sender, e);
        }

        private void EgisTrackGrid_MouseDoubleClick(object sender, MouseButtonEventArgs e)
        {
            _appData.EgisSelectedTrack = (Track)EgisTrackGrid.SelectedItem;
            EgisTrackGridDoubleClick?.Invoke(sender, e);
        }
        private void EgisTrackGrid_SelectionChanged(object sender, SelectionChangedEventArgs e)
        {
            _appData.EgisSelectedTrack = (Track)EgisTrackGrid.SelectedItem;
            EgisTrackGridSelectionChanged?.Invoke(sender, e);
        }
        
        private void UpUsageDirectionToFindRadioButton_Checked(object sender, RoutedEventArgs e)
        {
            _appData.UsageDirectionToFind = 1;
            UpUsageDirectionToFindRadioButtonChecked?.Invoke(sender, e);
        }

        private void DownUsageDirectionToFindRadioButton_Checked(object sender, RoutedEventArgs e)
        {
            _appData.UsageDirectionToFind = -1;
            DownUsageDirectionToFindRadioButtonChecked?.Invoke(sender, e);
        }

        private void FreightSpeedRadioButton_Checked(object sender, RoutedEventArgs e)
        {
            _appData.SpeedKindToFind = 2;
            FreightSpeedRadioButtonChecked?.Invoke(sender, e);
        }

        private void PassSpeedRadioButton_Copy1_Checked(object sender, RoutedEventArgs e)
        {
            _appData.SpeedKindToFind = 1;
            PassSpeedRadioButtonChecked?.Invoke(sender, e);
        }

        private void HighSpeedRadioButton_Checked(object sender, RoutedEventArgs e)
        {
            _appData.SpeedKindToFind = 90;
            HighSpeedRadioButtonChecked?.Invoke(sender, e);
        }

        private void VeryHighSpeedRadioButton_Checked(object sender, RoutedEventArgs e)
        {
            _appData.SpeedKindToFind = 87;
            VeryHighSpeedRadioButtonChecked?.Invoke(sender, e);
        }

        private void EtrainSpeedRadioButton_Checked(object sender, RoutedEventArgs e)
        {
            _appData.SpeedKindToFind = 89;
            EtrainSpeedRadioButtonChecked?.Invoke(sender, e);
        }

        private void MvpsSpeedRadioButton_Checked(object sender, RoutedEventArgs e)
        {
            _appData.SpeedKindToFind = 3;
            MvpsSpeedRadioButtonChecked?.Invoke(sender, e);
        }

        private void PrigSpeedRadioButton_Checked(object sender, RoutedEventArgs e)
        {
            _appData.SpeedKindToFind = 4;
            PrigSpeedRadioButtonChecked?.Invoke(sender, e);
        }

        private void StationDataGridSourceRadioButtonEgis_Checked(object sender, RoutedEventArgs e)
        {
            _appData.RouteToShowInDataGrids = _appData.EgisRoute1;
            //StationDataGridSourceRadioButtonEgisChecked?.Invoke(sender, e);
            DataGridSorceRadioButtonChanged?.Invoke(sender, e);
        }

        private void StationDataGridSourceRadioButtonDb_Checked(object sender, RoutedEventArgs e)
        {
            _appData.RouteToShowInDataGrids = _appData.Route1;
            //StationDataGridSourceRadioButtonDbChecked?.Invoke(sender, e);
            DataGridSorceRadioButtonChanged?.Invoke(sender, e);
        }

        private void StationDataGridSourceRadioButtonToAdd_Checked(object sender, RoutedEventArgs e)
        {
            _appData.RouteToShowInDataGrids = _appData.ToAddRoute;
            //StationDataGridSourceRadioButtonToAddChecked?.Invoke(sender, e);
            DataGridSorceRadioButtonChanged?.Invoke(sender,e);
        }

        private void StaitonToFindTextBox_TextChanged(object sender, TextChangedEventArgs e)
        {
            _appData.StationNameToFind = StaitonToFindTextBox.Text;
        }

        private void EgisStationsGrid_SelectionChanged(object sender, SelectionChangedEventArgs e)
        {
            _appData.EgisSelectedStation = (Station)EgisStationsGrid.SelectedItem;
        }

        void EgisSelectTrack()
        {
            
            _appData.EgisSelectedStation = (Station)EgisStationsGrid.SelectedItem;
            EgisImporter.SelectTrack(_appData.EgisSelectedStation, (bool)MaintrackRadioButton.IsChecked, _appData.EgisConnection, _appData.EgisSelectedTracks);
            EgisTrackGrid.Items.Refresh();
        }
    }
}
