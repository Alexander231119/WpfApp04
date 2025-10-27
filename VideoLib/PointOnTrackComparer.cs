using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace WpfApp04
{
    public class PointOnTrackComparer : IComparer<PointOnTrack>
    {
        public int Compare(PointOnTrack x, PointOnTrack y)
        {

            if (x.RouteCoordinate > y.RouteCoordinate) { return 1; }
            else if (x.RouteCoordinate < y.RouteCoordinate) { return -1; }
            else if (y.RouteCoordinate == x.RouteCoordinate)
            {
                if (x.SegmentID > y.SegmentID) { return 1; }
                else if (x.SegmentID < y.SegmentID) { return -1; }
            }

            return 0;

        }
    }
}
