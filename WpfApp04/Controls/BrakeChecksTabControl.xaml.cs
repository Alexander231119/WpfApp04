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
    /// Interaction logic for BrakeChecksTabControl.xaml
    /// </summary>
    public partial class BrakeChecksTabControl : UserControl
    {
        public event SelectionChangedEventHandler EgisPtGridSelectionChanged;

        public BrakeChecksTabControl()
        {
            InitializeComponent();
        }

        private void EgisPtGrid_SelectionChanged(object sender, SelectionChangedEventArgs e)
        {
            EgisPtGridSelectionChanged?.Invoke(sender, e);
        }
    }
}
