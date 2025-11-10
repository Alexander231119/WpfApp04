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
    /// Interaction logic for PointOnTrackTabControl.xaml
    /// </summary>
    public partial class PointOnTrackTabControl : UserControl
    {
        public event SelectionChangedEventHandler PointOnTrackEditGridSelectionChanged;
        public event RoutedEventHandler SaveToDbClicked;
        public event RoutedEventHandler AddPointOnTrackClicked;

        public PointOnTrackMenuControl PointOnTrackMenuControl => pointOnTrackMenuControl1;
        public ImportOptionsControl ImportOptionsControl => ImportOptionsControl2;
        //public DataGrid PointOnTrackEditGrid => PointOnTrackEditGrid;
        //public Grid PointOnTrackEditControlsContainer => PointOnTrackEditControlsContainer;

        // для прямого доступа из вне контрола
        public DataGrid PointOnTrackGrid => PointOnTrackEditGrid;
        public Grid EditControlsContainer => PointOnTrackEditControlsContainer;

        public PointOnTrackTabControl()
        {
            InitializeComponent();
        }

        private void PointOnTrackEditGrid_SelectionChanged(object sender, SelectionChangedEventArgs e)
        {
            PointOnTrackEditGridSelectionChanged?.Invoke(sender, e);
        }

        private void SaveToDbButton1_Click(object sender, RoutedEventArgs e)
        {
            SaveToDbClicked?.Invoke(sender, e);
        }

        private void AddPointOnTrackButton1_Click(object sender, RoutedEventArgs e)
        {
            AddPointOnTrackClicked?.Invoke(sender, e);
        }
    }
}
