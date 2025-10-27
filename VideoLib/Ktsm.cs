using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace WpfApp04
{
    public class Ktsm : TrackObject
    {
        public Ktsm()
        {
            this.Start = new PointOnTrack();
            this.End = new PointOnTrack();
        }
    }
}
