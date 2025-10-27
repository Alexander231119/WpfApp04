using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace WpfApp04
{
    public class UncodedTrack: TrackObject
    {
        
        
        public string UncodedTrackName = "";        
        
        public UncodedTrack(double trackObjectId, string uncodedTrackName)
        {
            TrackObjectID = trackObjectId;
            UncodedTrackName = uncodedTrackName;            
        }


    }
}
