using Microsoft.DirectX.Direct3D;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace WpfApp04
{
    public class Incline:TrackObject
    {
        
        public double Value { get; set; }
        
        public Incline( double value)
        {
            this.Start = new PointOnTrack();
            this.End = new PointOnTrack();
            this.Value = value;
        }

        public Incline()
        {
            this.Start = new PointOnTrack();
            this.End = new PointOnTrack();
            this.Value = 0;
        }
    }
}
