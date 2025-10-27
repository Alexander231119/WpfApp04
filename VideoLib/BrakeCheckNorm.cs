using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace WpfApp04
{
    public class BrakeCheckNorm
    {
        public double BrakeCheckPlaceID { get; set; }
        public double BrakeCheckNormSpeed { get; set; }
        public double BrakeCheckNormPath { get; set; }

        //DicBrakeCheckKindID DicBrakeCheckKindName
        //0	ПТ
        //1	ЭПТ
        //2	ПТ зимняя
        //3	ЭПТ зимняя
        //4	ПТ дополнительная
        //5	ЭПТ дополнительная

        public BrakeCheckNorm() { }
    }
}
