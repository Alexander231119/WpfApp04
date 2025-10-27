using System;
using System.Collections.Generic;
using System.Data.Common;
using System.Data.OleDb;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace WpfApp04
{
    public class Segment
    {
        public double SegmentID { get; set; }
        public string SegmentName { get; set; } = "";
        public double SegmentLength;
        public double TrackID;
        public double PredefinedRouteSegmentFromStartToEnd;
        public double StationID;
        public double ALSDirection = 1;
        public double AutoBlockFrequency;
        public double EgisStationID { get; set; }

        public PointOnTrack Start { get; set; }

        public string StartPointOnTrackKm
        {
            get => Start.PointOnTrackKm;
            set => Start.PointOnTrackKm = value;
        }

        public double StartPointOnTrackPk
        {
            get => Start.PointOnTrackPk;
            set => Start.PointOnTrackPk = value;
        }

        public double StartPointOnTrackM
        {
            get => Start.PointOnTrackM;
            set => Start.PointOnTrackM = value;
        }
        //
        public double StartSegment
        {
            get => Start.SegmentID;
            set => Start.SegmentID = value;
        }

        public PointOnTrack End { get; set; }

        public string EndPointOnTrackKm
        {
            get => End.PointOnTrackKm;
            set => End.PointOnTrackKm = value;
        }

        public double EndPointOnTrackPk
        {
            get => End.PointOnTrackPk;
            set => End.PointOnTrackPk = value;
        }

        public double EndPointOnTrackM
        {
            get => End.PointOnTrackM;
            set => End.PointOnTrackM = value;
        }

        //конструктор
        public Segment()
        {
            Start = new PointOnTrack();
            End = new PointOnTrack();
        }
        public Segment(
            double SegmentID,
            string SegmentName,
            double SegmentLength,
            double TrackID,
            //double StationID,
            double ALSDirection,
            double PredefinedRouteSegmentFromStartToEnd)
        {
            this.SegmentID = SegmentID;
            this.SegmentName = SegmentName;
            this.SegmentLength = SegmentLength;
            this.TrackID = TrackID;
            //this.StationID = StationID;
            this.ALSDirection = ALSDirection;

            this.PredefinedRouteSegmentFromStartToEnd = PredefinedRouteSegmentFromStartToEnd;
        }
     
    }
}
