using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace WpfApp04
{
    public class FrameDistance
    {
        public int FilmID { get; set; }
        public double FrameTime { get; set; }
        public double Distance { get; set; }
        
    }

    public class FrameDistanceComparerByTime : IComparer<FrameDistance>
    {
        public int Compare(FrameDistance x, FrameDistance y)
        {
            if (x.FrameTime > y.FrameTime)
            {
                return 1;
            }
            else if (x.FrameTime < y.FrameTime)
            { return -1; }
            return 0;
        }
    }
}
