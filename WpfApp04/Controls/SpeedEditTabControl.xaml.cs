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
    /// Interaction logic for SpeedEditTabControl.xaml
    /// </summary>
    public partial class SpeedEditTabControl : UserControl
    {
        public event SelectionChangedEventHandler SpeedDataGridSelectionChanged;
        public event EventHandler<DataGridRowEditEndingEventArgs> SpeedDataGridRowEditEnding;
        public event RoutedEventHandler AddSpeedClicked;
        public event RoutedEventHandler DeleteSpeedClicked;
        public event RoutedEventHandler DeleteAllSpeedClicked;
        public event RoutedEventHandler RouteCoordinateCheckBoxChecked;
        public event RoutedEventHandler RouteCoordinateCheckBoxUnchecked;
        public event KeyEventHandler SegmentIdTextBoxKeyDown;
        public event RoutedEventHandler SetSpeedSegmentIdClicked;
        public event RoutedEventHandler FillEmptySpeedsClicked;
        public event RoutedEventHandler SaveSpeedClicked;

        public bool RouteCoordinateChecked
        {
            get => RouteCoordinateCheckBox?.IsChecked ?? false;
            set => RouteCoordinateCheckBox.IsChecked = value;
        }

        public string SegmentIdText
        {
            get => SegmentIdTextBox.Text;
            set => SegmentIdTextBox.Text = value;
        }

        public SpeedEditTabControl()
        {
            InitializeComponent();
        }

        private void SpeedDataGrid_SelectionChanged(object sender, SelectionChangedEventArgs e)
        {
            SpeedDataGridSelectionChanged?.Invoke(sender, e);
        }

        private void SpeedDataGrid_RowEditEnding(object sender, DataGridRowEditEndingEventArgs e)
        {
            SpeedDataGridRowEditEnding?.Invoke(sender, e);
        }

        private void AddSpeedButton_Click(object sender, RoutedEventArgs e)
        {
            AddSpeedClicked?.Invoke(sender, e);
        }

        private void DeleteSpeedButton_Click(object sender, RoutedEventArgs e)
        {
            DeleteSpeedClicked?.Invoke(sender, e);
        }

        private void DeleteAllSpeedButton_Click(object sender, RoutedEventArgs e)
        {
            DeleteAllSpeedClicked?.Invoke(sender, e);
        }

        private void RouteCoordinateCheckBox_Checked(object sender, RoutedEventArgs e)
        {
            RouteCoordinateCheckBoxChecked?.Invoke(sender, e);
        }

        private void RouteCoordinateCheckBox_Unchecked(object sender, RoutedEventArgs e)
        {
            RouteCoordinateCheckBoxUnchecked?.Invoke(sender, e);
        }

        private void SegmentIdTextBox_KeyDown(object sender, KeyEventArgs e)
        {
            SegmentIdTextBoxKeyDown?.Invoke(sender, e);
        }

        private void SetSpeedSegmentIdButton_Click(object sender, RoutedEventArgs e)
        {
            SetSpeedSegmentIdClicked?.Invoke(sender, e);
        }

        private void FillEmptySpeedsButton_Click(object sender, RoutedEventArgs e)
        {
            FillEmptySpeedsClicked?.Invoke(sender, e);
        }

        private void SaveSpeedButton_Click(object sender, RoutedEventArgs e)
        {
            SaveSpeedClicked?.Invoke(sender, e);
        }
    }
}
