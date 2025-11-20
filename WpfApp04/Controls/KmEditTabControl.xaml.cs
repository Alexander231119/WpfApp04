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
        public List<Kilometer> SelectedKilometersToEdit = new List<Kilometer>();
        public bool EgisPtNormsGridLock;

        public DbRoute Route1 = new DbRoute();
        public DbRoute EgisRoute1 = new DbRoute();
        public string ConnectString = String.Empty;



        //public event SelectionChangedEventHandler KmGridSelectionChanged;
        public event RoutedEventHandler DbKmSetLengthClicked;
        public event RoutedEventHandler DbKmSegmentGroupSetLengthClicked;
        public event RoutedEventHandler SetKmGroupLengthWithEgisClicked;

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
            //KmGridSelectionChanged?.Invoke(sender, e);
            // при изменении выделения в таблице KmGrid

            // километры которые выбрал пользователь очистить список
            SelectedKilometersToEdit.Clear();

            if (EgisPtNormsGridLock == true) return;

            Kilometer item = (Kilometer)KmGrid.SelectedItem;

            KmTextBlock1.Text = "Км " + item?.Km +
                                                  "\nНачало: Segment: "
                                                  + item?.Start.SegmentID.ToString() + " "
                                                  + item?.Start.DicPointOnTrackKindName + " "
                                                  + item?.Start.PointOnTrackKm + "-"
                                                  + item?.Start.PointOnTrackPk.ToString() + "-"
                                                  + item?.Start.PointOnTrackM.ToString() + " "
                                                  + "TrackObject: " + item?.Start.TrackObjectID.ToString() + " "
                                                  + "PointOntrack: " + item?.Start.PointOntrackID.ToString() + " "

                                                  + "\nКонец: Segment: "
                                                  + item?.End.SegmentID.ToString() + " "
                                                  + item?.End.DicPointOnTrackKindName + " "
                                                  + item?.End.PointOnTrackKm + "-"
                                                  + item?.End.PointOnTrackPk.ToString() + "-"
                                                  + item?.End.PointOnTrackM.ToString() + " "
                                                  + "TrackObject: " + item?.End.TrackObjectID.ToString() + " "
                                                  + "PointOntrack: " + item?.End.PointOntrackID.ToString() + " "
                ;

            DbKmTextBox.Text = item?.Length.ToString();


            // если выбрано несколько километров посчитать среднюю длину



            foreach (var km_item in KmGrid.SelectedItems)
            {
                Kilometer k = (Kilometer)km_item;
                SelectedKilometersToEdit.Add(k);
            }

            //int selectedkmsCount = KmEditTabControl1.KmGrid.SelectedItems.Count;
            //int selectedkmsCount = selectedKilometersToEdit.Count;

            if (SelectedKilometersToEdit.Count > 0)
            {
                double SelectedKmsTotalLength = 0;

                for (int i = 0; i < SelectedKilometersToEdit.Count; i++)
                {
                    SelectedKmsTotalLength += SelectedKilometersToEdit[i].Length;
                }

                double avgLength = SelectedKmsTotalLength / SelectedKilometersToEdit.Count;
                DbKmTextBox.Text = Math.Round(avgLength, 2).ToString();
            }
        }

        private void DbKmSetLengthButton_Click(object sender, RoutedEventArgs e)
        {
            DbKmSetLengthClicked?.Invoke(sender, e);
        }

        private void DbKmSegmentGroupSetLengthButton_Click(object sender, RoutedEventArgs e)
        {
            DbKmSegmentGroupSetLengthClicked?.Invoke(sender, e);
        }

        private void SetKmGroupLengthWithEgisButton_Click(object sender, RoutedEventArgs e)
        {

            List<Kilometer> egisSourseKmlist = new List<Kilometer>();
            double egisKmtotalLength = 0; // суммарная длина соотвесттвующих километров из егис
            double selectedKmsTotalLength = 0; // суммарная длина выбранных км из базы

            for (int i = 0; i < SelectedKilometersToEdit.Count; i++)
            {
                Kilometer EgisKm = EgisRoute1.Kilometers.Find(x => x.Km == SelectedKilometersToEdit[i].Km);
                egisSourseKmlist.Add(EgisKm);

                selectedKmsTotalLength += SelectedKilometersToEdit[i].Length;
                egisKmtotalLength += EgisKm.Length;
            }

            for (int i = 0; i < SelectedKilometersToEdit.Count; i++)
            {
                SelectedKilometersToEdit[i].Length = egisSourseKmlist[i].Length * (selectedKmsTotalLength / egisKmtotalLength);
                DbRouteQuery.KmLengthSetPerform(ConnectString, SelectedKilometersToEdit[i], Route1);
            }

            SetKmGroupLengthWithEgisClicked?.Invoke(sender, e);
        }
    }
}
