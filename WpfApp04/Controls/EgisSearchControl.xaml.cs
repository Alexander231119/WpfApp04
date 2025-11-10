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

namespace WpfApp04.Controls
{
    /// <summary>
    /// Interaction logic for EgisSearchControl.xaml
    /// </summary>
    public partial class EgisSearchControl : UserControl
    {

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

        public EgisSearchControl()
        {
            InitializeComponent();
        }
        
        private void StaitonToFindTextBox_KeyDown(object sender, KeyEventArgs e)
        {
            StationToFindKeyDown?.Invoke(sender, e);
        }

        private void EgisFindStationButton_Click(object sender, RoutedEventArgs e)
        {
            EgisFindStationClicked?.Invoke(sender, e);
        }

        private void EgisLoadDataButton_Click(object sender, RoutedEventArgs e)
        {
            EgisLoadDataClicked?.Invoke(sender, e);
        }

        private void ShowEgisPreviewButton_Click(object sender, RoutedEventArgs e)
        {
            ShowEgisPreviewClicked?.Invoke(sender, e);
        }

        private void EgisStationsGrid_MouseDoubleClick(object sender, MouseButtonEventArgs e)
        {
            EgisStationsGridDoubleClick?.Invoke(sender, e);
        }

        private void EgisTrackGrid_MouseDoubleClick(object sender, MouseButtonEventArgs e)
        {
            EgisTrackGridDoubleClick?.Invoke(sender, e);
        }

        private void EgisTrackGrid_SelectionChanged(object sender, SelectionChangedEventArgs e)
        {
            EgisTrackGridSelectionChanged?.Invoke(sender, e);
        }

        private void MaintrackRadioButton_Checked(object sender, RoutedEventArgs e)
        {
            MaintrackRadioButtonChecked?.Invoke(sender, e);
        }

        private void SideTrackRadioButton_Checked(object sender, RoutedEventArgs e)
        {
            SideTrackRadioButtonChecked?.Invoke(sender, e);
        }

        private void UpUsageDirectionToFindRadioButton_Checked(object sender, RoutedEventArgs e)
        {
            UpUsageDirectionToFindRadioButtonChecked?.Invoke(sender, e);
        }

        private void DownUsageDirectionToFindRadioButton_Checked(object sender, RoutedEventArgs e)
        {
            DownUsageDirectionToFindRadioButtonChecked?.Invoke(sender, e);
        }

        private void FreightSpeedRadioButton_Checked(object sender, RoutedEventArgs e)
        {
            FreightSpeedRadioButtonChecked?.Invoke(sender, e);
        }

        private void PassSpeedRadioButton_Copy1_Checked(object sender, RoutedEventArgs e)
        {
            PassSpeedRadioButtonChecked?.Invoke(sender, e);
        }

        private void HighSpeedRadioButton_Checked(object sender, RoutedEventArgs e)
        {
            HighSpeedRadioButtonChecked?.Invoke(sender, e);
        }

        private void VeryHighSpeedRadioButton_Checked(object sender, RoutedEventArgs e)
        {
            VeryHighSpeedRadioButtonChecked?.Invoke(sender, e);
        }

        private void EtrainSpeedRadioButton_Checked(object sender, RoutedEventArgs e)
        {
            EtrainSpeedRadioButtonChecked?.Invoke(sender, e);
        }

        private void MvpsSpeedRadioButton_Checked(object sender, RoutedEventArgs e)
        {
            MvpsSpeedRadioButtonChecked?.Invoke(sender, e);
        }

        private void PrigSpeedRadioButton_Checked(object sender, RoutedEventArgs e)
        {
            PrigSpeedRadioButtonChecked?.Invoke(sender, e);
        }

        private void StationDataGridSourceRadioButtonEgis_Checked(object sender, RoutedEventArgs e)
        {
            StationDataGridSourceRadioButtonEgisChecked?.Invoke(sender, e);
        }

        private void StationDataGridSourceRadioButtonDb_Checked(object sender, RoutedEventArgs e)
        {
            StationDataGridSourceRadioButtonDbChecked?.Invoke(sender, e);
        }

        private void StationDataGridSourceRadioButtonToAdd_Checked(object sender, RoutedEventArgs e)
        {
            StationDataGridSourceRadioButtonToAddChecked?.Invoke(sender, e);
        }

    }
}
