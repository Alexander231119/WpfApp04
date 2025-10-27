using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace WpfApp04
{
    public class Uksps : TrackObject
    {
        public Uksps()
        {
            this.Start = new PointOnTrack();
            this.End = new PointOnTrack();
        }
    }
}
