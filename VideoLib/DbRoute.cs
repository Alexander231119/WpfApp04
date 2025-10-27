using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace WpfApp04
{
    
    
    public class DbRoute
    {
        public List<SpeedRestriction> SpeedRestrictions = new List<SpeedRestriction>();
        public List<PointOnTrack> PointOnTracks = new List<PointOnTrack>();
        public List<Segment> Segments = new List<Segment>();
        public List<Station> Stations = new List<Station>();
        public List<Kilometer> Kilometers = new List<Kilometer>();
        public List<Track> Tracks = new List<Track>(); // список путей
        public List<Incline> Inclines = new List<Incline>();
        public List<TrafficLight> TrafficLights = new List<TrafficLight>();
        public List<TrackCircuit> TrackCircuits = new List<TrackCircuit>();
        public List<AlsControl> AlsControls = new List<AlsControl>();
        public List<UncodedTrack> UncodedTracks = new List<UncodedTrack>();
        public List<Uksps> UkspsList = new List<Uksps>();
        public List<Ktsm> KtsmList = new List<Ktsm>();
        public List<CrossingPiece> CrossingPieces = new List<CrossingPiece>();
        public List<Platform> Platforms = new List<Platform>();
        public List<TrafficSignal> TrafficSignals = new List<TrafficSignal>();
        public List<Crossing> Crossings = new List<Crossing>();
        public List<NeutralSection> NeutralSections = new List<NeutralSection>();
        public List<RailBridge> RailBridges = new List<RailBridge>();
        public List<Tunnel> Tunnels = new List<Tunnel>();
        public List<BrakeCheckPlace> BrakeCheckPlaces = new List<BrakeCheckPlace>();
        public List<CurrentKindChange> CurrentKindChanges = new List<CurrentKindChange>();


        public DbRoute()
        {}

        public void DbRouteClear()
        {
            Segments.Clear();
            Stations.Clear();
            PointOnTracks.Clear();
            SpeedRestrictions.Clear();
            Kilometers.Clear();
            Tracks.Clear();
            Inclines.Clear();
            TrafficLights.Clear();
            TrackCircuits.Clear();
            AlsControls.Clear();
            UncodedTracks.Clear();
            CrossingPieces.Clear();
            Platforms.Clear();
            UkspsList.Clear();
            KtsmList.Clear();
            TrafficSignals.Clear();
            Crossings.Clear();
            NeutralSections.Clear();
            BrakeCheckPlaces.Clear();
            RailBridges.Clear();
            Tunnels.Clear();
            CurrentKindChanges.Clear();
        }
        
    }

    

}
