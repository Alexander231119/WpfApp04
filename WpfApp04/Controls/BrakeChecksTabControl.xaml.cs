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
    /// Interaction logic for BrakeChecksTabControl.xaml
    /// </summary>
    public partial class BrakeChecksTabControl : UserControl
    {
        private AppDbRouteContextData _appData;
        public AppDbRouteContextData AppData
        {
            get => _appData;
            set
            {
                _appData = value;
                // Подписываемся на изменения если нужно
            }
        }

        public event SelectionChangedEventHandler EgisPtGridSelectionChanged;


        public BrakeChecksTabControl()
        {
            InitializeComponent();
        }

        private void EgisPtGrid_SelectionChanged(object sender, SelectionChangedEventArgs e)
        {
            if (_appData.EgisPtNormsGridLock == true) return;

            var item = EgisPtGrid.SelectedItem;
            BrakeCheckPlace bcp = (BrakeCheckPlace)item;
            EgisPtNormsGrid.ItemsSource = bcp.BrakeCheckNormList;
            EgisPtNormsGrid.Items.Refresh();


            EgisPtGridSelectionChanged?.Invoke(sender, e);
        }
    }
}
