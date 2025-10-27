using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace WpfApp04
{
    public class TrackObject
    {
        public PointOnTrack Start { get; set; }


        public double StartElevation
        {
            get => Start.Elevation;
            set => Start.Elevation = value;

        }
        public double EndElevation
        {
            get => End.Elevation;
            set => End.Elevation = value;

        }

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
        public double StartRouteCoordinate
        {
            get => Start.RouteCoordinate;
            set => Start.RouteCoordinate = value;
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

        public double EndSegment
        {
            get => Start.SegmentID;
            set => Start.SegmentID = value;
        }

        public double EndRouteCoordinate
        {
            get => End.RouteCoordinate;
            set => End.RouteCoordinate = value;
        }
        public double TrackObjectID { get; set; }
        public double DicTrackObjectKindID { get; set; }
        public string TrackObjectName { get; set; } = "";
        public string Station { get; set; } = "";
        public double StationID { get; set; }
        public string TrackNumber { get; set; } = "";
        public double EgisDicTrafficKind { get; set; }

        public double ObjectLength
        {
            get => (Math.Round((EndRouteCoordinate - StartRouteCoordinate), 2));
        }

        public TrackObject() 
        { 
        this.Start = new PointOnTrack();
        this.End = new PointOnTrack();  
        }

        public void RefreshStationMid(List<Station> stations)
        {
            //MidCoordinate
            double MidCoordinate = (Start.RouteCoordinate + End.RouteCoordinate) / 2;

            int sindex = stations.FindIndex(x => (x.Start.RouteCoordinate < MidCoordinate)
                                                 && (x.End.RouteCoordinate > MidCoordinate) ||
                                                 (x.Start.RouteCoordinate == MidCoordinate) ||
                                                 (x.End.RouteCoordinate == MidCoordinate)
            );

            int sindexminus = stations.FindLastIndex(z => (z.End.RouteCoordinate < MidCoordinate));
            int sindexplus = stations.FindIndex(y => (y.Start.RouteCoordinate > MidCoordinate));

            if (sindex >= 0)
            {
                if ((stations[sindex].Start.RouteCoordinate == MidCoordinate) ||
                    (stations[sindex].End.RouteCoordinate == MidCoordinate) ||
                    ((stations[sindex].Start.RouteCoordinate < MidCoordinate) && (stations[sindex].End.RouteCoordinate > MidCoordinate)))
                {
                    Station = stations[sindex].StationName;
                }
            }
            else if (
                (sindexminus >= 0) && (sindexplus >= 0) &&
                (stations[sindexminus].End.RouteCoordinate < MidCoordinate)
                && (stations[sindexplus].Start.RouteCoordinate > MidCoordinate))
            {
                Station = stations[sindexminus].StationName + " - " + stations[sindexplus].StationName;
            }
        }

        public string TrackToShow(List<Track> tracks, List<Segment> segments)
        {
            int sindex = segments.FindIndex((Segment) => Segment.SegmentID == Start.SegmentID);
            int tindex = tracks.FindIndex(x => (x.TrackID == segments[sindex].TrackID));
            string tracktoshow = tracks[tindex].TrackNumber + " " + tracks[tindex].TrackName;

            if (tracks[tindex].DicTrackKindID == 1)
            {
                //главный путь
                tracktoshow = tracks[tindex].TrackNumber + " гл.   " + tracks[tindex].TrackName;
            }
            else
            {
                //боковой путь
                tracktoshow = tracks[tindex].TrackNumber + " бок   " + tracks[tindex].TrackName;
            }

            TrackNumber = tracktoshow;
            return tracktoshow;
        }

        // поменять направление 
        public void StartToEnd()
        {
            string Km = Start.PointOnTrackKm;
            double Pk = Start.PointOnTrackPk;
            double M = Start.PointOnTrackM;
            double coordinate = Start.PointOnTrackCoordinate;

            Start.PointOnTrackKm = End.PointOnTrackKm;
            Start.PointOnTrackPk = End.PointOnTrackPk;
            Start.PointOnTrackM = End.PointOnTrackM;
            Start.PointOnTrackCoordinate = End.PointOnTrackCoordinate;

            End.PointOnTrackKm = Km;
            End.PointOnTrackPk = Pk;
            End.PointOnTrackM = M;
            End.PointOnTrackCoordinate = coordinate;

        }
    }

    public class RailBridge : TrackObject
    {
        public RailBridge()
        {
            this.Start = new PointOnTrack();
            this.End = new PointOnTrack();
        }
    }

    public class Tunnel : TrackObject
    {
        public Tunnel()
        {
            this.Start = new PointOnTrack();
            this.End = new PointOnTrack();
        }
    }

    public class CurrentKindChange : TrackObject
    {
        public double DicCurrentKindIDLeft { get; set; }
        public double DicCurrentKindIDRight { get; set; }

        public static readonly Dictionary<double, string> DicCurrentKindNames = new()
        {
            [1]= "переменный",
            [2]= "постоянный",
            [3]= "переключаемый",
            [4]= "Не электрофицирован",
            [5]= "Не определен"
        };
        public string CurrentKindNameLeft
        {
            get => DicCurrentKindNames.TryGetValue(DicCurrentKindIDLeft, out var name)
                ? name
                : string.Empty;
            set { } // Удалить, если не используется
        }
        public string CurrentKindNameRight
        {
            get => DicCurrentKindNames.TryGetValue(DicCurrentKindIDRight, out var name)
                ? name
                : string.Empty;
            set { } // Удалить, если не используется
        }

        public CurrentKindChange()
        {
            this.Start = new PointOnTrack();
            this.End = new PointOnTrack();
        }
    }

}
