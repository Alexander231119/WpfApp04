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
using System.Windows.Shapes;

namespace WpfApp04
{
    /// <summary>
    /// Interaction logic for EgisPreview.xaml
    /// </summary>
    public partial class EgisPreview : Window
    {
        public EgisPreview()
        {
            InitializeComponent();

            //var matrix  = new Matrix();
           // matrix.Translate(-1000, 0);
           // EgisCanvas.RenderTransform = new MatrixTransform(matrix);

        }
    }
}
