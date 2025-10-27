using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace WpfApp04
{
    public class Track
    {
        public double TrackID { get; set; }
        public string TrackNumber { get; set; } = "";
        public double TrackEven;
        public double DicTrackKindID;
        public string TrackName { get; set; } = "";
        

        public Track()
        {
            
        }

        public Track(
            double TrackID,
            string TrackNumber,
            double TrackEven,
            double DicTrackKindID,
            string TrackName)
        {
            this.TrackID = TrackID;
            this.TrackNumber = TrackNumber;
            this.TrackEven = TrackEven;
            this.DicTrackKindID = DicTrackKindID;
            this.TrackName = TrackName;
        }

    }
}
