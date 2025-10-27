using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace WpfApp04
{
    public class Platform : TrackObject
    {

        public string PlatformName { get; set; } = "";
        public double StationID;

        public Platform()
        {
            this.Start = new PointOnTrack();
            this.End = new PointOnTrack();
        }


        public void RenameStationaryPlatform()
        {
            if ((
                    PlatformName == "1"
                    || PlatformName == "2"
                    || PlatformName == "3"
                    || PlatformName == "4"
                    || PlatformName == "5"
                    || PlatformName == "6"
                    || PlatformName == "7"
                    || PlatformName == "8"
                    || PlatformName == "9"
                ) && (Start.station != ""))
            {
                PlatformName = Start.station;
            }

            if (PlatformName.Length > 1)
            {
                //s.PlatformName = s.PlatformName.Substring(0, 1).ToUpper() + s.PlatformName.Substring(1).ToLower();

                //PlatformName = CapitalizeAllWords(s.PlatformName);
            }
        }

    }

    

}
