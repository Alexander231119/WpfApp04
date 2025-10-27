using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace WpfApp04
{
    public class InclineComparer : IComparer<Incline>
    {
        public int Compare(Incline x, Incline y)
        {
             
                if (x.Start.RouteCoordinate > y.Start.RouteCoordinate) { return 1; }
                else if (x.Start.RouteCoordinate < y.Start.RouteCoordinate) { return -1; }
            
            return 0;
        }

    }
}
