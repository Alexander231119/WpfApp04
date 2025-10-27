using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace WpfApp04
{
    public class TrackCircuit : TrackObject
    {
        

        public string TrackCircuitName = "";
        
        public double TrackCircuitContinueThroughBound;
        public double TrafficLightId { get; set; }
        public string TrafficLightName { get; set; } = "";
        

        public TrackCircuit(double trackObjectId, string trackCircuitName, double trackCircuitContinueThroughBound)
        {
            TrackObjectID = trackObjectId;
            TrackCircuitName = trackCircuitName;
            TrackCircuitContinueThroughBound = trackCircuitContinueThroughBound;
        }
    }
}
