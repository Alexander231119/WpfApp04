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
    /// Interaction logic for TabImportControl.xaml
    /// </summary>
    public partial class TabImportControl : UserControl
    {
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
            AddSegmentsToFillFromEgisClicked?.Invoke(sender, e);
        }

        private void FillFromEgisButton_Click(object sender, RoutedEventArgs e)
        {
            FillFromEgisClicked?.Invoke(sender, e);
        }

        private void InsertFromEgisToBaseButton_Click(object sender, RoutedEventArgs e)
        {
            InsertFromEgisToBaseClicked?.Invoke(sender, e);
        }

        private void ClearToAddListsButtony_Click(object sender, RoutedEventArgs e)
        {
            ClearToAddListsClicked?.Invoke(sender, e);
        }
    }
}
