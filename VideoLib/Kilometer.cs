using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace WpfApp04
{
    public class Kilometer : TrackObject
    {
        
        public string Km { get; set; } = "";
        public double Length { get; set; }

        public delegate void KmHandler(Kilometer k);
        public event KmHandler? KmLengthSet;


        public Kilometer()
        {
            this.Start = new PointOnTrack();
            this.End = new PointOnTrack();
            this.Km = "";
        }


        public void KmLengthBeenSet()
        {
            KmLengthSet?.Invoke(this);
        }
    }
}
