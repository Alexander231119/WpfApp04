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
    /// Interaction logic for SpeedEditMenu.xaml
    /// </summary>
    public partial class SpeedEditMenu : Window
    {
        public SpeedRestrictionControl speedRestrictionControl;
        public SpeedRestriction spdin;
        public bool deleteAt = false;


        public SpeedEditMenu(SpeedRestriction spdin)
        {
            this.spdin = spdin;            
            InitializeComponent();
                        
        }

        private void SpeedOk_Click(object sender, RoutedEventArgs e)
        {

            spdin.Value = Convert.ToDouble(Value.Text);

            spdin.Start.PointOnTrackKm = StartKm.Text;
            spdin.Start.PointOnTrackPk = Convert.ToDouble( StartPk.Text);
            spdin.Start.PointOnTrackM = Convert.ToDouble( StartM.Text);
            

            spdin.End.PointOnTrackKm = EndKm.Text;
            spdin.End.PointOnTrackPk = Convert.ToDouble( EndPk.Text);
            spdin.End.PointOnTrackM = Convert.ToDouble( EndM.Text);
            
            spdin.ForEmptytrainBool = (bool)ForEmptytrainCheckBox.IsChecked;
            spdin.OnlyHeaderBool = (bool)OnlyHeaderCheckBox.IsChecked;
            
            
            //speedRestrictionControl.spdin.Value = Convert.ToDouble(Value.Text);

            //speedRestrictionControl.spdin.Start.PointOnTrackKm = StartKm.Text;
            //speedRestrictionControl.spdin.Start.PointOnTrackPk = Convert.ToDouble( StartPk.Text);
            //speedRestrictionControl.spdin.Start.PointOnTrackM = Convert.ToDouble( StartM.Text);
            

            //speedRestrictionControl.spdin.End.PointOnTrackKm = EndKm.Text;
            //speedRestrictionControl.spdin.End.PointOnTrackPk = Convert.ToDouble( EndPk.Text);
            //speedRestrictionControl.spdin.End.PointOnTrackM = Convert.ToDouble( EndM.Text);
            
            //speedRestrictionControl.spdin.ForEmptytrainBool = (bool)ForEmptytrainCheckBox.IsChecked;
            //speedRestrictionControl.spdin.OnlyHeaderBool = (bool)OnlyHeaderCheckBox.IsChecked;

            //if (spdin.PermRestrictionForEmptyTrain == 1)
            //{
            //    speedRestrictionControl.rectangle1.Fill.Opacity = 0;
            //    speedRestrictionControl.rectangle3.Fill.Opacity = 1;
            //    //speedRestrictionControl.rectangle2.Opacity = 0;
            //    speedRestrictionControl.rectangle2.Visibility = Visibility.Collapsed;
            //}
            //else
            //{
            //    speedRestrictionControl.rectangle1.Fill.Opacity = 1;
            //    speedRestrictionControl.rectangle3.Fill.Opacity = 0;
            //    speedRestrictionControl.rectangle2.Opacity = 1;
            //    speedRestrictionControl.rectangle2.Visibility = Visibility.Visible;
            //}


            //
            DialogResult = true;
            Close();
        }

        private void OnlyHeader_Checked(object sender, RoutedEventArgs e)
        {
           // speedRestrictionControl.spdin.PermRestrictionOnlyHeader = -1;
        }

        private void OnlyHeader_Unchecked(object sender, RoutedEventArgs e)
        {
           // speedRestrictionControl.spdin.PermRestrictionOnlyHeader = 0;
        }

        private void Delete_Click(object sender, RoutedEventArgs e)
        {
            deleteAt = true;
            DialogResult = true;
            Close();
        }
    }
}
