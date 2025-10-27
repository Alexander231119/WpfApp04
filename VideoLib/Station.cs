using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace WpfApp04
{
    public class Station : TrackObject
    {
        
        public string StationName { get; set; } = "";

        public double EgisStationID { get; set; }
        
        public Station()
        {
            this.Start = new PointOnTrack();
            this.End = new PointOnTrack();
        }

        public Station(string StationName)
        {
            this.Start = new PointOnTrack();
            this.End = new PointOnTrack();
            this.StationName = StationName;
        }


    }
}
