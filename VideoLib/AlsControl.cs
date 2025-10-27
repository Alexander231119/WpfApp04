using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace WpfApp04
{
    public class AlsControl
    {
        public double ALSControlID;
        public double ALSDeviceIDToControl;
        public double ALSDeviceIDInfoFrom;
        public double DicALSControlKindID;
        public double ALSControlUsageDirection;

        public AlsControl( double aLSControlID, 
            double aLSDeviceIDToControl, 
            double aLSDeviceIDInfoFrom,
            double dicALSControlKindID,
            double aLSControlUsageDirection)
        {
            this.ALSControlID = aLSControlID;
            this.ALSDeviceIDToControl = aLSDeviceIDToControl;
            this.ALSDeviceIDInfoFrom = aLSDeviceIDInfoFrom;
            this.DicALSControlKindID= dicALSControlKindID;
            this.ALSControlUsageDirection= aLSControlUsageDirection;
        }


    }
}
