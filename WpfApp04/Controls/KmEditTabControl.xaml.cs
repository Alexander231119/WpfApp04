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
    /// Interaction logic for KmEditTabControl.xaml
    /// </summary>
    public partial class KmEditTabControl : UserControl
    {
        public event SelectionChangedEventHandler KmGridSelectionChanged;
        public event RoutedEventHandler DbKmSetLengthClicked;
        public event RoutedEventHandler DbKmSegmentGroupSetLengthClicked;

        public string KmTextBlock1Text
        {
            get => KmTextBlock1.Text;
            set => KmTextBlock1.Text = value;
        }

        public string DbKmTextBoxText
        {
            get => DbKmTextBox.Text;
            set => DbKmTextBox.Text = value;
        }

        public KmEditTabControl()
        {
            InitializeComponent();
        }

        private void KmGrid_SelectionChanged(object sender, SelectionChangedEventArgs e)
        {
            KmGridSelectionChanged?.Invoke(sender, e);
        }

        private void DbKmSetLengthButton_Click(object sender, RoutedEventArgs e)
        {
            DbKmSetLengthClicked?.Invoke(sender, e);
        }

        private void DbKmSegmentGroupSetLengthButton_Click(object sender, RoutedEventArgs e)
        {
            DbKmSegmentGroupSetLengthClicked?.Invoke(sender, e);
        }
    }
}
