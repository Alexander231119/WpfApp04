using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace WpfApp04
{
    public class StationComparer : IComparer<Station>
    {
        public int Compare(Station x, Station y)
        {
            double xMidCoordinate = (x.Start.RouteCoordinate +x.End.RouteCoordinate)/2;
            double yMidCoordinate = (y.Start.RouteCoordinate+y.End.RouteCoordinate)/2;

            if (xMidCoordinate > yMidCoordinate) { return 1; }
            else if (xMidCoordinate < yMidCoordinate) { return -1; }
            
            
                return 0;
            

            

        }
    }
}
