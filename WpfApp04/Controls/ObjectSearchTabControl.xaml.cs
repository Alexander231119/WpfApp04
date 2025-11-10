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
    /// Interaction logic for ObjectSearchTabControl.xaml
    /// </summary>
    public partial class ObjectSearchTabControl : UserControl
    {
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
            EgisFindPointObjectsClicked?.Invoke(sender, e);
        }

        private void EgisFoundPointObjectsGrid_MouseDoubleClick(object sender, MouseButtonEventArgs e)
        {
            EgisFoundPointObjectsGridDoubleClick?.Invoke(sender, e);
        }
    }
}
