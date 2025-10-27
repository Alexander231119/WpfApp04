using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace WpfApp04
{
    public class Crossing : TrackObject
    {
        public double DicCrossingKindID;
        public double EgisDicCrossingKindID;
        
        public Crossing()
        {
            this.Start = new PointOnTrack();
            this.End = new PointOnTrack();
        }
    }
}
