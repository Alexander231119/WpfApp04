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
    /// Interaction logic for PointOnTrackMenuControl.xaml
    /// </summary>
    public partial class PointOnTrackMenuControl : UserControl
    {
        public PointOnTrack p;
        public DbRoute _route;
        public PointOnTrackMenuControl()
        {
            InitializeComponent();
            Loaded += PointOnTrackMenuControl_Loaded;
        }

        private void PointOnTrackMenuControl_Loaded(object sender, RoutedEventArgs e)
        {
            // Populate the ComboBox with all point types
            DicPointOntrackNameComboBox.ItemsSource = PointOnTrack.PointOnTrackNames
                .OrderBy(kvp => kvp.Key)
                .Select(kvp => new KeyValuePair<double, string>(kvp.Key, kvp.Value))
                .ToList();

            DicPointOntrackNameComboBox.DisplayMemberPath = "Value";
            DicPointOntrackNameComboBox.SelectedValuePath = "Key";

            DicPointOntrackNameComboBox.SelectionChanged += DicPointOntrackNameComboBox_SelectionChanged;
        }



        private void DicPointOntrackNameComboBox_SelectionChanged(object sender, SelectionChangedEventArgs e)
        {
            if (p != null && DicPointOntrackNameComboBox.SelectedValue is double selectedId)
            {
                // Удалить TrackObject
                switch (p.DicPointOnTrackKindID)
                {
                    case 40: // токораздел
                        CurrentKindChange t = PointOnTrack.GetCurrentKindChangeForPoint(p, _route);
                        _route.CurrentKindChanges.Remove(t);

                        break;
                    case 1: // светофор

                        break;
                }


                // создать новый TrackObject
                p.DicPointOnTrackKindID = selectedId;
                switch (p.DicPointOnTrackKindID)
                {
                    case 40: // токораздел
                        CurrentKindChange t = new CurrentKindChange();
                        t.Start = p;
                        _route.CurrentKindChanges.Add(t);

                        break;
                    case 1: // светофор

                        break;
                }

                MenuRefresh();
            }
        }
        public void MenuRefresh()
        {
            if (p == null) return;
            DicPointOnTrackKindTextBlock.Text = p.DicPointOnTrackKindID.ToString();
            SegmentIdTextBox.Text = p.SegmentID.ToString();

            // Set the selected item in ComboBox
            DicPointOntrackNameComboBox.SelectedValue = p.DicPointOnTrackKindID;
        }

        private void SetPointSegmentIdButton_Click(object sender, RoutedEventArgs e)
        {
            p.SegmentID= Convert.ToDouble(SegmentIdTextBox.Text);
        }
    }
}
