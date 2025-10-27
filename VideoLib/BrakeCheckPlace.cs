using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using WpfApp04;

namespace WpfApp04
{
    public class BrakeCheckPlace : TrackObject
    {
        public double DicBrakeCheckKindID { get; set; }
        public double BrakeCheckPlaceDirection { get; set; }
        public double EgisDicBrakeCheckKindID { get; set; }

        public string DicBrakeCheckKindName
        {
            get
            {
                if (DicBrakeCheckKindID == 0) return "ПТ";
                else if (DicBrakeCheckKindID == 1) return "ЭПТ";
                else return "";
            }
            set
            {

            }
        }

        public List<BrakeCheckNorm> BrakeCheckNormList { get; set; } = new List<BrakeCheckNorm>();
        public BrakeCheckPlace()
        {
            this.Start = new PointOnTrack();
            this.End = new PointOnTrack();
        }

        public void GetDicBrakeCheckKindFromEgis()
        {
            if (EgisDicBrakeCheckKindID == 10) DicBrakeCheckKindID = 0; // пт
            else if (EgisDicBrakeCheckKindID == 11) DicBrakeCheckKindID = 1; // эпт
        }



        public void ReduceBrakeCheckNormList()
        {
            List<double> speedlist = new List<double>();
            List<double> pathlist = new List<double>();

            foreach (BrakeCheckNorm bcn in BrakeCheckNormList)
            {
                double _speed = speedlist.Find(x => x == bcn.BrakeCheckNormSpeed);
                if (_speed == 0)
                {
                    speedlist.Add(bcn.BrakeCheckNormSpeed);
                }
                double _path = pathlist.Find(x => x == bcn.BrakeCheckNormPath);
                if (_path == 0)
                {
                    pathlist.Add(bcn.BrakeCheckNormPath);
                }
            }

            BrakeCheckNormList.Clear();

            speedlist.Sort();
            pathlist.Sort();

            for (int i = 0; i < speedlist.Count; i++)
            {
                BrakeCheckNorm b = new BrakeCheckNorm();
                b.BrakeCheckPlaceID = TrackObjectID;
                b.BrakeCheckNormSpeed = speedlist[i];
                b.BrakeCheckNormPath = pathlist[i];

                BrakeCheckNormList.Add(b);
            }

        }

    }
}
