using System;
using System.Collections.Generic;
using System.ComponentModel;
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
    /// Interaction logic for SpeedEditTabControl.xaml
    /// </summary>
    public partial class SpeedEditTabControl : UserControl
    {
        private AppDbRouteContextData _appData;
        public AppDbRouteContextData AppData
        {
            get => _appData;
            set
            {
                _appData = value;
            }
        }
        public event SelectionChangedEventHandler SpeedDataGridSelectionChanged;
        public event EventHandler<DataGridRowEditEndingEventArgs> SpeedDataGridRowEditEnding;
        public event RoutedEventHandler AddSpeedClicked;
        public event RoutedEventHandler DeleteSpeedClicked;
        public event RoutedEventHandler DeleteAllSpeedClicked;
        public event RoutedEventHandler RouteCoordinateCheckBoxChecked;
        public event RoutedEventHandler RouteCoordinateCheckBoxUnchecked;
        public event KeyEventHandler SegmentIdTextBoxKeyDown;
        public event RoutedEventHandler SetSpeedSegmentIdClicked;
        public event RoutedEventHandler FillEmptySpeedsClicked;
        public event RoutedEventHandler SaveSpeedClicked; // клик по кнопке Сохранить скорости

        //изменилась скорость при переходе на новую строку в случае если пользователь внёс изменения
        //универсальное событие которое вызывается если скорость была изменена
        // и если нужно перерисовать заново скорости в Canvas
        public event EventHandler SpeedDataGridSpeedChanged;

        public bool RouteCoordinateChecked
        {
            get => RouteCoordinateCheckBox?.IsChecked ?? false;
            set => RouteCoordinateCheckBox.IsChecked = value;
        }

        public string SegmentIdText
        {
            get => SegmentIdTextBox.Text;
            set => SegmentIdTextBox.Text = value;
        }

        public SpeedEditTabControl()
        {
            InitializeComponent();
        }

        private void SpeedDataGrid_SelectionChanged(object sender, SelectionChangedEventArgs e)
        {

            _appData.SpeedRestrictionToEdit= (SpeedRestriction)SpeedDataGrid.SelectedItem;
            SpeedRestriction item = (SpeedRestriction)SpeedDataGrid.SelectedItem;
            SegmentIdTextBox.Text = item?.Start.SegmentID.ToString();

            if (_appData.RowChanged == true)
            {
                //если ограничение скорости было изменено пользователем в таблице
                //обновить данные всех скоростей и перерисовать их заново

                foreach (SpeedRestriction s in _appData.Route1.SpeedRestrictions)
                {
                    s.Start.RefreshCoordinate(_appData.Route1.PointOnTracks, _appData.Route1.Segments);
                    s.Start.RefreshRouteCoordinate(_appData.Route1.Segments);
                    s.End.RefreshCoordinate(_appData.Route1.PointOnTracks, _appData.Route1.Segments);
                    s.End.RefreshRouteCoordinate(_appData.Route1.Segments);
                }

                _appData.Route1.SpeedRestrictions.Sort(_appData.Scts);

                //убрать и нарисовать ограничения скорости заново

                

                SpeedDataGridSpeedChanged?.Invoke(sender, e);

                //SpeedDataGrid.Items.Refresh();
                //SpeedDataGrid.Focus();
                _appData.RowChanged = false;
            }

            //SpeedDataGridSelectionChanged?.Invoke(sender, e);
        }

        private void SpeedDataGrid_RowEditEnding(object sender, DataGridRowEditEndingEventArgs e)
        {
            //если пльзователь внёс изменения в таблице ограничений скрости
            _appData.RowChanged = true;
            SpeedDataGridRowEditEnding?.Invoke(sender, e);
        }

        private void AddSpeedButton_Click(object sender, RoutedEventArgs e)
        {
            // добавить ограничение скорости
            if (_appData.Route1.Segments.Count > 0)
            {
                SpeedRestriction item = (SpeedRestriction)SpeedDataGrid.SelectedItem;

                SpeedRestriction spdin = null; // новое ограничение скорости которое добавляет пользователь

                if (item != null)
                {
                    spdin = new SpeedRestriction(item);
                }
                else
                {
                    spdin = new SpeedRestriction(80, 0, 0);
                }

                SpeedEditMenu menu = new SpeedEditMenu(spdin);

                menu.Station.Text = spdin.Station?.ToString();
                menu.SegmentId.Text = spdin.Start.SegmentID.ToString();
                menu.Value.Text = "";
                menu.StartKm.Text = "";
                menu.StartPk.Text = "";
                menu.StartM.Text = "";
                menu.EndKm.Text = "";
                menu.EndPk.Text = "";
                menu.EndM.Text = "";
                menu.StartRouteCoordinate.Text = "";
                menu.EndRouteCoordinate.Text = "";
                menu.Length.Text = "";


                int sindex = _appData.Route1.Segments.FindIndex(y => (y.SegmentID == spdin.Start.SegmentID));
                int tindex = -1;
                if (sindex >= 0)
                {
                    tindex = _appData.Route1.Tracks.FindIndex(x => (x.TrackID == _appData.Route1.Segments[sindex].TrackID));
                }
                else
                {
                    sindex = 0;
                    tindex = _appData.Route1.Tracks.FindIndex(x => (x.TrackID == _appData.Route1.Segments[sindex].TrackID));
                }

                if (tindex >= 0)
                {
                    menu.TrackName.Text = _appData.Route1.Tracks[tindex].TrackNumber.ToString() + " " + _appData.Route1.Tracks[tindex].TrackName;
                }


                if (menu.ShowDialog() == true)
                {
                    
                    spdin.Start.SegmentID = spdin.End.SegmentID = Convert.ToDouble(menu.SegmentId.Text);

                    spdin.Start.PointOnTrackKm = menu.StartKm.Text;
                    spdin.Start.RefreshCoordinate(_appData.Route1.PointOnTracks, _appData.Route1.Segments);
                    spdin.End.RefreshCoordinate(_appData.Route1.PointOnTracks, _appData.Route1.Segments);

                    spdin.Start.RefreshRouteCoordinate(_appData.Route1.Segments);
                    spdin.End.RefreshRouteCoordinate(_appData.Route1.Segments);
                    spdin.Value = Convert.ToDouble(menu.Value.Text);

                    _appData.Route1.PointOnTracks.Add(spdin.Start);
                    _appData.Route1.PointOnTracks.Add(spdin.End);
                    _appData.Route1.SpeedRestrictions.Add(spdin);

                    //_appData.Route1.SpeedRestrictions.Sort(_appData.Scts);
                    RefreshSpeedDataGrid();

                    //AddSpeedClicked?.Invoke(sender, e);
                    SpeedDataGridSpeedChanged?.Invoke(sender, e);
                }

            }

            
        }

        private void DeleteSpeedButton_Click(object sender, RoutedEventArgs e)
        {
            SpeedRestriction item = (SpeedRestriction)SpeedDataGrid.SelectedItem;
            _appData.RouteToShowInDataGrids.SpeedRestrictions.Remove(item);

            RefreshSpeedDataGrid();

            //DeleteSpeedClicked?.Invoke(sender, e);
            SpeedDataGridSpeedChanged?.Invoke(sender, e);
        }

        private void DeleteAllSpeedButton_Click(object sender, RoutedEventArgs e)
        {
            _appData.RouteToShowInDataGrids.SpeedRestrictions.Clear();
            RefreshSpeedDataGrid();
            //DeleteAllSpeedClicked?.Invoke(sender, e);
            SpeedDataGridSpeedChanged?.Invoke(sender, e);
        }

        private void RouteCoordinateCheckBox_Checked(object sender, RoutedEventArgs e)
        {
            SpeedDataGrid.Columns[11].Visibility = Visibility.Visible;
            RouteCoordinateCheckBoxChecked?.Invoke(sender, e);
        }

        private void RouteCoordinateCheckBox_Unchecked(object sender, RoutedEventArgs e)
        {
            SpeedDataGrid.Columns[11].Visibility = Visibility.Hidden;
            RouteCoordinateCheckBoxUnchecked?.Invoke(sender, e);
        }

        private void SegmentIdTextBox_KeyDown(object sender, KeyEventArgs e)
        {
            SegmentIdTextBoxKeyDown?.Invoke(sender, e);
        }

        private void SetSpeedSegmentIdButton_Click(object sender, RoutedEventArgs e)
        {
             SpeedRestriction item = (SpeedRestriction)SpeedDataGrid.SelectedItem;
             _appData.SpeedRestrictionToEdit = item;
            if (item != null)
            {

                item.Start.SegmentID = item.End.SegmentID = Convert.ToDouble(SegmentIdTextBox.Text);
                item.Start.RefreshCoordinate(_appData.Route1.PointOnTracks, _appData.Route1.Segments);
                item.Start.RefreshRouteCoordinate(_appData.Route1.Segments);
                item.End.RefreshCoordinate(_appData.Route1.PointOnTracks, _appData.Route1.Segments);
                item.End.RefreshRouteCoordinate(_appData.Route1.Segments);
            }
            SpeedDataGridSpeedChanged?.Invoke(sender, e);
            //SetSpeedSegmentIdClicked?.Invoke(sender, e);

        }

        private void FillEmptySpeedsButton_Click(object sender, RoutedEventArgs e)
        {
            // заполнить ограничения скоростей на пустых сегментах
            foreach (Segment s in _appData.Route1.Segments)
            {
                List<SpeedRestriction> newSpeedRestrictionsList =
                    _appData.Route1.SpeedRestrictions.FindAll(x => x.Start.SegmentID == s.SegmentID);

                if (newSpeedRestrictionsList.Count == 0)
                {
                    SpeedRestriction spd = new SpeedRestriction(40, 0, 0);
                    spd.Start = new PointOnTrack(s.Start);
                    spd.End = new PointOnTrack(s.End);
                    spd.Start.DicPointOnTrackKindID = spd.End.DicPointOnTrackKindID = 2;

                    _appData.Route1.SpeedRestrictions.Add(spd);
                    _appData.Route1.PointOnTracks.Add(spd.Start);
                    _appData.Route1.PointOnTracks.Add(spd.End);
                }
            }

            RefreshSpeedDataGrid();
            //FillEmptySpeedsClicked?.Invoke(sender, e);
            SpeedDataGridSpeedChanged?.Invoke(sender, e);
        }

        private void SaveSpeedButton_Click(object sender, RoutedEventArgs e)
        {
            DbRouteDataExporter.SaveSpeedRestrictions(_appData.ConnectString, _appData.Route1);

            

            //SaveSpeedClicked?.Invoke(sender, e);
            _appData.DbData_Changed();

            MessageBox.Show("Сохранены ограничения скорости \n всего: " +
                            _appData.Route1.SpeedRestrictions.Count.ToString(), "постоянные ограничения скорости");
        }

        void RefreshSpeedDataGrid()
        {
            int selectedrow = SpeedDataGrid.SelectedIndex;

            SpeedDataGrid.ItemsSource = null;
            SpeedDataGrid.ItemsSource = _appData.RouteToShowInDataGrids.SpeedRestrictions;
            SpeedDataGrid.Items.SortDescriptions.Clear();
            SpeedDataGrid.Items.SortDescriptions.Add(new SortDescription("StartRouteCoordinate", ListSortDirection.Ascending));
            SpeedDataGrid.Items.Refresh();

            SpeedDataGrid.SelectedIndex = selectedrow;

        }
    }
}
