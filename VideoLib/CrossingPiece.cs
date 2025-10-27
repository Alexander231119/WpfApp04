using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace WpfApp04
{
    public class CrossingPiece : TrackObject
    {
        // стрелка
        public string CrossingPieceName = "";
        public double DicCrossingPieceKindID;
        public double CrossingPieceFrogModel;
        
        public CrossingPiece()
        {
            this.Start = new PointOnTrack();
            this.End = new PointOnTrack();
        }
    }
}
