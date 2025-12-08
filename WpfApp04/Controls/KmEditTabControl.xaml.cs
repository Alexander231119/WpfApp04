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
    /// Interaction logic for KmEditTabControl.xaml
    /// </summary>
    public partial class KmEditTabControl : UserControl
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

        //public List<Kilometer> SelectedKilometersToEdit = new List<Kilometer>();
        //public bool EgisPtNormsGridLock;

            //public DbRoute Route1 = new DbRoute();
        //public DbRoute EgisRoute1 = new DbRoute();
        //public string ConnectString = String.Empty;



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
            _appData.SelectedKilometersToEdit.Clear();

            if (_appData.EgisPtNormsGridLock == true) return;

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
                _appData.SelectedKilometersToEdit.Add(k);
            }

            //int selectedkmsCount = KmEditTabControl1.KmGrid.SelectedItems.Count;
            //int selectedkmsCount = selectedKilometersToEdit.Count;

            if (_appData.SelectedKilometersToEdit.Count > 0)
            {
                double SelectedKmsTotalLength = 0;

                for (int i = 0; i < _appData.SelectedKilometersToEdit.Count; i++)
                {
                    SelectedKmsTotalLength += _appData.SelectedKilometersToEdit[i].Length;
                }

                double avgLength = SelectedKmsTotalLength / _appData.SelectedKilometersToEdit.Count;
                DbKmTextBox.Text = Math.Round(avgLength, 2).ToString();
            }
        }

        private void DbKmSetLengthButton_Click(object sender, RoutedEventArgs e)
        {
            // задать длину для выбранного километра

            Kilometer klm = (Kilometer)KmGrid.SelectedItem;
            if (klm is null) return;
            klm.Length = Convert.ToDouble(DbKmTextBox.Text);


            if (klm.Start.SegmentID == klm.End.SegmentID)
            {
                DbRouteQuery.KmLengthSetPerform(_appData.ConnectString, klm, _appData.Route1);
                MessageBox.Show("Изменения внесены в " + _appData.FileName);
            }


            //DbKmSetLengthClicked?.Invoke(sender, e);
            _appData.DbData_Changed();
        }

        private void DbKmSegmentGroupSetLengthButton_Click(object sender, RoutedEventArgs e)
        {
            //задать общую длину для выбранных километров

            if ((Kilometer)KmGrid.SelectedItem is null) return;

            double klmLength = Convert.ToDouble(DbKmTextBox.Text);

            foreach (Kilometer k in _appData.SelectedKilometersToEdit)
            {
                if (k.Start.SegmentID == k.End.SegmentID)
                    k.Length = klmLength;
                DbRouteQuery.KmLengthSetPerform(_appData.ConnectString, k, _appData.Route1);
            }

            //DbKmSegmentGroupSetLengthClicked?.Invoke(sender, e);
            _appData.DbData_Changed();
        }

        private void SetKmGroupLengthWithEgisButton_Click(object sender, RoutedEventArgs e)
        {

            List<Kilometer> egisSourseKmlist = new List<Kilometer>();
            double egisKmtotalLength = 0; // суммарная длина соотвесттвующих километров из егис
            double selectedKmsTotalLength = 0; // суммарная длина выбранных км из базы

            for (int i = 0; i < _appData.SelectedKilometersToEdit.Count; i++)
            {
                Kilometer EgisKm = _appData.EgisRoute1.Kilometers.Find(x => x.Km == _appData.SelectedKilometersToEdit[i].Km);
                egisSourseKmlist.Add(EgisKm);

                selectedKmsTotalLength += _appData.SelectedKilometersToEdit[i].Length;
                egisKmtotalLength += EgisKm.Length;
            }

            for (int i = 0; i < _appData.SelectedKilometersToEdit.Count; i++)
            {
                _appData.SelectedKilometersToEdit[i].Length = egisSourseKmlist[i].Length * (selectedKmsTotalLength / egisKmtotalLength);
                
                DbRouteQuery.KmLengthSetPerform(_appData.ConnectString, _appData.SelectedKilometersToEdit[i], _appData.Route1);
            }

            //SetKmGroupLengthWithEgisClicked?.Invoke(sender, e);
            _appData.DbData_Changed();
            //KmGrid.Items.Refresh();
        }
    }
}
