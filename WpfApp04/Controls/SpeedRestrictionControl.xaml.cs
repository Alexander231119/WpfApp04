using System;
using System.Collections.Generic;
using System.Data.Common;
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
    /// Interaction logic for SpeedRestrictionControl.xaml
    /// </summary>
    public partial class SpeedRestrictionControl : UserControl
    {
        
        public delegate void SpeedHandler(double speedindex);
        public event SpeedHandler? SpeedChanged;

        public SpeedRestriction spdin;
        public double TrackObjectID;
        public double heightscale;
        public List<Segment> segments;
        public List<Track> tracks;
        public List<PointOnTrack> pointOnTracks;
        public List<SpeedRestriction> speedRestrictions;
        public double widtscale;
        Canvas _wrapPanel;
        internal DataGrid dataGrid;

        public SpeedRestrictionControl
            (
            double heighscale, 
            double widtscale, 
            SpeedRestriction spdin, 
            List<Segment> segments,
            List<PointOnTrack> pointOnTracks,
            List<SpeedRestriction> speedRestrictions,
            List<Track> tracks,
            Canvas _wrapPanel
            )
        {
            this.pointOnTracks= pointOnTracks;
            this.segments = segments;
            this.spdin = spdin;
            this.heightscale = heighscale;
            this.widtscale = widtscale;
            this.tracks = tracks;
            this.speedRestrictions = speedRestrictions;
            this._wrapPanel = _wrapPanel;


            InitializeComponent();
        }

        private void rectangle1_MouseRightButtonDown(object sender, MouseButtonEventArgs e)
        {
            if (dataGrid != null)
            {
                dataGrid.SelectedItem = spdin;
                dataGrid.ScrollIntoView(spdin);
                dataGrid.Focus();
            }

            EditSpeedRestriction();
        }

        private void EditSpeedRestriction()
        {
            SpeedEditMenu menu = new SpeedEditMenu(spdin);
            menu.speedRestrictionControl = this;

            menu.Value.Text = spdin.Value.ToString();
            menu.StartKm.Text = spdin.Start.PointOnTrackKm;
            menu.StartPk.Text = spdin.Start.PointOnTrackPk.ToString();
            menu.StartM.Text = spdin.Start.PointOnTrackM.ToString();
            menu.EndKm.Text = spdin.End.PointOnTrackKm;
            menu.EndPk.Text = spdin.End.PointOnTrackPk.ToString();
            menu.EndM.Text = spdin.End.PointOnTrackM.ToString();
            menu.StartRouteCoordinate.Text = spdin.Start.RouteCoordinate.ToString();
            menu.EndRouteCoordinate.Text = spdin.End.RouteCoordinate.ToString();
            menu.Length.Text = (Math.Round((spdin.End.RouteCoordinate - spdin.Start.RouteCoordinate), 2)).ToString();
            menu.OnlyHeaderCheckBox.IsChecked = spdin.OnlyHeaderBool;
            menu.ForEmptytrainCheckBox.IsChecked = spdin.ForEmptytrainBool;
            menu.SegmentId.Text = spdin.Start.SegmentID.ToString();
            menu.TrackName.Text = TrackToShow();
            menu.Station.Text = spdin.Station.ToString();


            if (menu.ShowDialog() == true)
            {
                int index = speedRestrictions.IndexOf(spdin);
                //rectangle1.Height = Convert.ToDouble(menu.Value.Text) * heightscale;
                //rectangle2.Height = (200 - Convert.ToDouble(menu.Value.Text))*heightscale;


                if (menu.deleteAt == true)
                {

                    speedRestrictions.RemoveAt(index);
                    //var cindex = _wrapPanel.Children.IndexOf(this);


                    _wrapPanel.Children.Remove(this);

                }
                else
                {

                spdin.Start.PointOnTrackKm = menu.StartKm.Text;
                spdin.Start.RefreshCoordinate(pointOnTracks, segments);
                //spdin.Start.RefreshKmPkM(pointOnTracks); // пересчёт жд координаты
                spdin.End.RefreshCoordinate(pointOnTracks, segments);
                //spdin.End.RefreshKmPkM(pointOnTracks); // пересчёт жд координаты
                spdin.Start.RefreshRouteCoordinate(segments);
                spdin.End.RefreshRouteCoordinate(segments);
                spdin.Value = Convert.ToDouble(menu.Value.Text);
                Canvas.SetLeft(this, spdin.Start.RouteCoordinate * widtscale);
                this.rectangle1.Width = Math.Abs(spdin.End.RouteCoordinate - spdin.Start.RouteCoordinate) * widtscale;
                this.rectangle1.Height = spdin.Value * heightscale;
                this.rectangle2.Width = this.rectangle1.Width;
                this.rectangle2.Height = (200 - spdin.Value) * heightscale;

                if (spdin.ForEmptytrainBool == true)
                { 
                    this.rectangle2.Visibility = Visibility.Collapsed;
                    rectangle2.Opacity = 0;
                    rectangle1.Fill.Opacity = 0;
                    rectangle3.Fill.Opacity = 1;
                }
                if(spdin.ForEmptytrainBool == false)
                {  
                    this.rectangle2.Visibility = Visibility.Visible;
                    rectangle2.Opacity = 1;
                    rectangle1.Fill.Opacity = 1;
                    rectangle3.Fill.Opacity = 0;
                }

                this.rectangle3.Width = this.rectangle1.Width;
                this.rectangle3.Height = this.rectangle1.Height;

                    if (this.rectangle1.Width > 20)
                        { this.Value.Text = spdin.Value.ToString(); }
                    else { this.Value.Text = ""; }


                
                    SpeedChanged?.Invoke(index);
                }
                

            }
        }

        private string TrackToShow()
        {
            int sindex = segments.FindIndex(x => (x.SegmentID == spdin.Start.SegmentID));
            int tindex=-1;
            if (sindex >=0)  tindex = tracks.FindIndex(y => (y.TrackID == segments[sindex].TrackID));

            string trackmain;
            if (tindex >= 0 && sindex >= 0)
            {
                if (tracks[tindex].DicTrackKindID == 1)
                {
                    trackmain = "гл ";
                }
                else
                {
                    trackmain = "бок";
                }

                return tracks[tindex].TrackNumber + " " + trackmain + " " + tracks[tindex].TrackName;
            }
            else return "";
        }

        private void rectangle1_MouseEnter(object sender, MouseEventArgs e)
        {
           // rectangle1.Fill.Opacity = 0.6;
        }

        private void rectangle1_MouseLeave(object sender, MouseEventArgs e)
        {
           // rectangle1.Fill.Opacity = 1;
        }

        private void rectangle1_MouseLeftButtonDown(object sender, MouseButtonEventArgs e)
        {
            if (dataGrid != null)
            {
                dataGrid.SelectedItem = spdin;
                dataGrid.ScrollIntoView(spdin);
                dataGrid.Focus();
            }
            
            
        }


    }
}
