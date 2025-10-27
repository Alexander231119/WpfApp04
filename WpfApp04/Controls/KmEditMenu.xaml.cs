using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using System.Text;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Data;
using System.Windows.Documents;
using System.Windows.Input;
using System.Windows.Media;
using System.Windows.Media.Imaging;
using System.Windows.Shapes;

namespace WpfApp04
{
    /// <summary>
    /// Interaction logic for KmEditMenu.xaml
    /// </summary>
    public partial class KmEditMenu : Window
    {

        //public delegate void KmHandler(Kilometer k);
        //public event KmHandler? KmLengthSet;


        //public KmControl kmControl;
        public Kilometer klm;

        public KmEditMenu(Kilometer _klm)
        {
            this.klm = _klm;
            InitializeComponent();
        }

        private void SetLengthButton_Click(object sender, RoutedEventArgs e)
        {
            klm.Length = Convert.ToDouble(KmLengthTextBox.Text);

            klm.KmLengthBeenSet();
            DialogResult = true;
            Close();
        }
    }
}
