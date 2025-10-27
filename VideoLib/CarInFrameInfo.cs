using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace VideoLib
{
    public class VideoCar
    {
        public int crossingId { get; set; }
        public int id { get; set; }
        public List<CarInFrameInfo> frameInfos = new();
        public float cameraAngle { get; set; }
    }

    public class CarComparerById : IComparer<VideoCar>
    {
        public int Compare(VideoCar x, VideoCar y)
        {
            if (x.id > y.id)
            {
                return 1;
            }
            else if (x.id < y.id) 
            { return -1; }
            return 0;
        }
    }

    public class CarInFrameInfo
    {
        public int videoNumber;
        public int frameNumber { get; set; }
        public float positionX { get; set; }
        public float positionY { get; set; }
        public float positionZ { get; set; }
        public float rotationYaw { get; set; }
        public float visibility { get; set; }
        public float cameraAngle { get; set; }
        public float rotationPitch { get; set; } = 0;
        public float rotationRoll { get; set; } = 0;
    }



    public class CarInFrameInfoComparer : IComparer<CarInFrameInfo>
    {
        public int Compare(CarInFrameInfo x, CarInFrameInfo y)
        {
            if (x.frameNumber > y.frameNumber)
            {
                return 1;
            }
            else if (x.frameNumber < y.frameNumber) { return -1;}
            return 0;
        }
    }
}
