using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Media.Converters;

namespace WpfApp04
{
    public class SpeedRestriction : TrackObject
    {
        public double Value { get; set; }
        public double PermRestrictionOnlyHeader { get; set; }
        public double PermRestrictionForEmptyTrain { get; set; }
        public double DicTrafficKind { get; set; } 
        // егис 
        //1 пассажирское 
        //2 грузовое
        //3 МВПС
        //4 пригородное
        //5  маневровое
        //6 хозяйственное
        //87 высокоскоростное
        //88 грузовое с порожними вагонами
        //89 электропоезда
        //90 скоростное

        public bool OnlyHeaderBool
        {
            get
            {
                if (PermRestrictionOnlyHeader == 0)
                    return false;
                else
                    return true;
            }
            set
            {
                if (value == true)
                {
                    PermRestrictionOnlyHeader = 1;
                }
                else
                {
                    PermRestrictionOnlyHeader = 0;
                }
            }
        }

        public bool ForEmptytrainBool
        {
            get
            {
                if (PermRestrictionForEmptyTrain == 0)
                    return false;
                else
                    return true;
            }
            set
            {
                if (value == true)
                {
                    PermRestrictionForEmptyTrain = 1;
                }
                else
                {
                    PermRestrictionForEmptyTrain = 0;
                }
            }
        }

        //конструктор
        public SpeedRestriction(PointOnTrack Start, PointOnTrack End, double Value)
        {
            this.Start = Start;
            this.End = End;
            this.Value = Value;
        }

        //конструктор
        public SpeedRestriction(PointOnTrack Start, PointOnTrack End, double Value, string station)
        {
            this.Start = Start;
            this.End = End;
            this.Value = Value;
            this.Station = station;
        }

        public SpeedRestriction(double Value, double PermRestrictionOnlyHeader, double PermRestrictionForEmptyTrain)
        {
            this.Start = new PointOnTrack();
            this.End = new PointOnTrack();
            this.Value = Value;
            this.PermRestrictionOnlyHeader = PermRestrictionOnlyHeader;
            this.PermRestrictionForEmptyTrain = PermRestrictionForEmptyTrain;
        }

        public SpeedRestriction(SpeedRestriction s)
        {
            this.Start = new PointOnTrack();
            this.End = new PointOnTrack();
            this.Start.SegmentID = this.End.SegmentID = s.Start.SegmentID;
            this.Station = s.Station;
            this.TrackNumber = s.TrackNumber;
        }

        
    }
}
