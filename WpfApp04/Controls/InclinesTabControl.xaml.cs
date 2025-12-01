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
    /// Interaction logic for InclinesTabControl.xaml
    /// </summary>
    public partial class InclinesTabControl : UserControl
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


        public event RoutedEventHandler ExportInclinesToExcelClicked;

        public InclinesTabControl()
        {
            InitializeComponent();
        }

        private void ExportInclinesToExcelButton_Click(object sender, RoutedEventArgs e)
        {
            DbRouteQuery.SaveInclinesToCsvFile(EgisToExportInclinesGrid.ItemsSource);
            ExportInclinesToExcelClicked?.Invoke(sender, e);
        }
    }
}
