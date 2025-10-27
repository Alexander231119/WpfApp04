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

namespace WpfApp04
{
    /// <summary>
    /// Interaction logic for UserControl1.xaml
    /// </summary>
    public partial class userControl : UserControl
    {
        public string name;
        public string description;
        public userControl()
        {
            InitializeComponent();
        }

        private void rectangle_MouseRightButtonDown(object sender, MouseButtonEventArgs e)
        {
            string message = name + "\n" + description;
            MessageBox.Show(message);
        }
    }
}
