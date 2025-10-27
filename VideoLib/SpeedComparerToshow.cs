using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace WpfApp04
{
    public class SpeedComparerToshow : IComparer<SpeedRestriction>
    {

        public int Compare(SpeedRestriction x, SpeedRestriction y)
        {
            if( Math.Abs(x.PermRestrictionForEmptyTrain) > Math.Abs(y.PermRestrictionForEmptyTrain)) { return 1; }
            else if(Math.Abs(x.PermRestrictionForEmptyTrain) < Math.Abs(y.PermRestrictionForEmptyTrain)) { return -1; }
            else if (Math.Abs(x.PermRestrictionForEmptyTrain) == Math.Abs(y.PermRestrictionForEmptyTrain)) 
            { 
            if (x.Value > y.Value) { return -1; }
            else if (y.Value > x.Value) { return 1; }
            else if (x.Value==y.Value)
                {
                    if (Math.Abs(x.End.PointOnTrackCoordinate - x.Start.PointOnTrackCoordinate) >
                        Math.Abs(y.End.PointOnTrackCoordinate - y.Start.PointOnTrackCoordinate))
                    { return -1; }
                    else if (Math.Abs(x.End.PointOnTrackCoordinate - x.Start.PointOnTrackCoordinate) <
                        Math.Abs(y.End.PointOnTrackCoordinate - y.Start.PointOnTrackCoordinate))
                    { return 1; }
                }
            }
            return 0;
        }
    }
}
