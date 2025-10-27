using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace WpfApp04
{
    public class TrafficSignal : TrackObject
    {
        public double DicTrafficSignalKindID;

        public TrafficSignal()
        {
            this.Start = new PointOnTrack();
            this.End = new PointOnTrack();
        }

    }
}
