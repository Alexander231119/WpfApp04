using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Media.Animation;

namespace WpfApp04
{
    public class TliRestriction
    {
        public double TrafficLightID;
        

        public TliRestrictionKind kind { get; set; }
        public ALSRouteKind routeKind { get; set; }
        public sbyte blockCount { get; set; }
        public short speed { get; set; }
        public sbyte showBlockCount { get; set; }
        public AutoBlockInternalControlCode autoBlockCode { get; set; }

        public TliRestriction()
        {
            this.routeKind = ALSRouteKind.Dummy;
            this.autoBlockCode = AutoBlockInternalControlCode.Dummy;
        }

        public TliRestriction(byte kind, ALSRouteKind routeKind, short blockCount, short speed, short showBlockCount, AutoBlockInternalControlCode autoBlockCode)
        {
            this.kind = (TliRestrictionKind)kind;
            this.routeKind = routeKind;
            this.blockCount = (sbyte)blockCount;
            this.speed = speed;
            this.showBlockCount = (sbyte)showBlockCount;
            this.autoBlockCode = autoBlockCode;
        }
        public TliRestriction(TliRestrictionKind kind, short blockCount, short speed)
        {
            this.kind = kind;
            routeKind = ALSRouteKind.Dummy;
            this.blockCount = blockCount < sbyte.MinValue ? sbyte.MinValue : blockCount > sbyte.MaxValue ? sbyte.MaxValue : (sbyte)blockCount;
            this.speed = speed;
            showBlockCount = blockCount < 0 ? (sbyte)0 : blockCount > sbyte.MaxValue ? sbyte.MaxValue : (sbyte)blockCount;
            autoBlockCode = AutoBlockInternalControlCode.Dummy;
        }
    }
    public enum TliRestrictionKind : byte
    {
        Cargo = 0,
        Pass = 1,
        Suburban = 2
    }
    public enum ALSRouteKind : byte
    {
        Dummy, // - 0
        /// <summary>
        /// Прямо
        /// </summary>
        Straight, // - 1
        /// <summary>
        /// Прямо, поворот на следующей стрелке, ограничение скорости 60
        /// </summary>
        StraightNextWithTurn, // - 2
        /// <summary>
        /// Поворот на ближайшей стрелке, ограничение скорости 60
        /// </summary>
        WithTurn, // - 3
        /// <summary>
        /// Поворот на ближайшей стрелке, ограничение скорости 80
        /// </summary>
        WithTurn80, // - 4
        /// <summary>
        /// Поворот на ближайшей стрелке, ограничение скорости 80,
        /// по следующей стрелке поворот с ограничением 60
        /// </summary>
        WithTurn80NextWithTurn, // - 5
        /// <summary>
        /// Поворот на ближайшей стрелке, ограничение скорости 120
        /// </summary>
        WithTurn120, // - 6
        /// <summary>
        /// Поворот на ближайшей стрелке, ограничение скорости 120, 
        /// по следующей стрелке поворот с ограничением 60
        /// </summary>
        WithTurn120NextWithTurn, // - 7
        /// <summary>
        /// 3 желтых
        /// </summary>
        ThreeYellow, // - 8
        /// <summary>
        /// Полуавтоблокировка прямо
        /// </summary>
        SemiAutoBlocking, // - 9
        /// <summary>
        /// Полуавтоблокировка с поворотом
        /// </summary>
        SemiAutoBlockingWithTurn, // - 10
        /// <summary>
        /// По локомотивному светофору
        /// </summary>
        DueToLocomotiveSvetofor, // - 11
        /// <summary>
        /// По неправильному пути
        /// </summary>
        WrongTrack, // - 12
        /// <summary>
        /// По некодированному пути
        /// </summary>
        NotCoded, // - 13
        /// <summary>
        /// Входной прямо
        /// </summary>
        EntranceOpenStraight, // - 14
        /// <summary>
        /// Входной с поворотом, ограничение скорости 60
        /// </summary>
        EntranceOpenWithTurn, // - 15
        /// <summary>
        /// Входной с поворотом, ограничение скорости 80
        /// </summary>
        EntranceOpenWithTurn80, // - 16
        /// <summary>
        /// Входной с поворотом, ограничение скорости 120
        /// </summary>
        EntranceOpenWithTurn120, // - 17
        /// <summary>
        /// Следующий маршрут всегда прямо
        /// </summary>
        TransmitOnlyStraight, // - 18
        /// <summary>
        /// Прямо, поворот на следующей стрелке, ограничение скорости 80
        /// </summary>
        StraightNextWithTurn80, // - 19
        /// <summary>
        /// Прямо, на следующей стрелке выход на неправильный путь
        /// </summary>
        StraightNextWrongTrack, // - 20
        /// <summary>
        /// Прямо, поворот на следующей стрелке, ограничение скорости 80
        /// </summary>
        WithTurnNextWrongTrack // - 21
    };
    public enum AutoBlockInternalControlCode : byte
    {
        /// <summary> 
        /// Отсутствие кода 
        /// </summary>
        [Description("Отсутствие кода")] None,
        /// <summary> 
        /// Зелёный 
        /// </summary>
        [Description("З")] Green,
        /// <summary> 
        /// Жёлтый 
        /// </summary>
        [Description("Ж")] Yellow,
        /// <summary> 
        /// Желтый с красным
        ///  </summary>
        [Description("КЖ")] YellowWithRed,
        /// <summary>
        /// Желтый после КЖ
        /// </summary>
        [Description("Ж после КЖ")] YellowAfterYellowWithRed,
        /// <summary>
        /// Желтый после желтого
        /// </summary>
        [Description("ОтсутсЖ после Жтвие кода")] YellowAfterYellow,
        /// <summary>
        /// Неопределенный
        /// </summary>
        [Description("Неопределенный")] Dummy = 255
    }

    

    public class TrafficLight : TrackObject
    {

        private static readonly Dictionary<double, string> TrafficLightNames = new Dictionary<double, string>
        {
            {0, "проходной 3-значная"},
            {1, "входной 3-значная"},
            {2, "выходной 3-значная"},
            {3, "маршрутный 3-значная"},
            {4, "прикрытия"},
            {5, "заградительный"},
            {6, "предупредительный"},
            {7, "повторительный"},
            {8, "маневровый"},
            {9, "горочный"},
            {10, "проходной 4-значная"},
            {11, "выходной 4-значная"},
            {12, "предвходной 3-значная"},
            {13, "предвходной 4-значная"},
            {14, "входной 4-значная"},
            {15, "маршрутный 4-значная"},
            {16, "входной с неправильного пути"},
            {20, "сигнальная точка тональной АЛС"},
            {21, "входной с коротким блок-участком"},
            {22, "выходной полуавтоблокировка"},
            {23, "выходной без желтого огня"},
            {24, "предвходной с коротким блок-участком"},
            {25, "входной, проследуемый в маневровом режиме"},
            {26, "маршрутный, проследуемый в маневровом режиме"},
            {27, "выходной, проследуемый в маневровом режиме"}
        };

        public double TrackObjectID { get; set; }
        public string TrackObjectName { get; set; } = "";
        public string TrafficLightName { get; set; } = "";
        public double DicTrafficLightKindID { get; set; }
        public double EgisTrafficLightKindValue { get; set; } // track object property value DicTrackObjectPropertyKindID = 9

        public double EgisABValue
        {
            get
            {
                if (DicTrafficLightKindID == 0 
                    ||DicTrafficLightKindID == 1 
                    || DicTrafficLightKindID == 2 
                    || DicTrafficLightKindID == 3 
                    || DicTrafficLightKindID == 12) return 244;
                else if (DicTrafficLightKindID == 10 
                         || DicTrafficLightKindID == 14 
                         || DicTrafficLightKindID == 11 
                         || DicTrafficLightKindID == 15
                         || DicTrafficLightKindID == 13) return 245;
                else return 0;
            }
            set
            {
                if (value == 244)
                {
                    if (DicTrafficLightKindID == 10) DicTrafficLightKindID = 0;
                    if (DicTrafficLightKindID == 14) DicTrafficLightKindID = 1;
                    if (DicTrafficLightKindID == 11) DicTrafficLightKindID = 2;
                    if (DicTrafficLightKindID == 15) DicTrafficLightKindID = 3;
                    if (DicTrafficLightKindID == 13) DicTrafficLightKindID = 12;
                }
                if (value == 245)
                {
                    if (DicTrafficLightKindID == 0) DicTrafficLightKindID = 10;
                    if (DicTrafficLightKindID == 1) DicTrafficLightKindID = 14;
                    if (DicTrafficLightKindID == 2) DicTrafficLightKindID = 11;
                    if (DicTrafficLightKindID == 3) DicTrafficLightKindID = 15;
                    if (DicTrafficLightKindID == 12) DicTrafficLightKindID = 13;
                }
                OnPropertyChanged(nameof(EgisABValue));
            }
        }
        // 243 - паб, 244 - 3х значная, итд
        public event PropertyChangedEventHandler PropertyChanged;
        
        protected void OnPropertyChanged(string name) =>
            PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(name));

        public List<TliRestriction> TliRestrictions = new List<TliRestriction>();

        public List<TrafficLightInFrame> trafficLightInFrames = new List<TrafficLightInFrame>();
        public double trafficLightInFramesCount {  get; set; }

        public List<TrafficLightLampInFrame> trafficLightLampInFrames = new List<TrafficLightLampInFrame>();
        public double trafficLightLampInFramesCount
        {
            get;
            set;
        }

        public double tliRestrictionsCount
        {
            get
            {
             return TliRestrictions.Count;
            } 
            set {  }
        }

        public string DicTrafficLightKindName
        {
            get => TrafficLightNames.TryGetValue(DicTrafficLightKindID, out var name)
                ? name
                : string.Empty;
            
            set { }
        }

        public TrafficLight()
        {
            this.Start = new PointOnTrack();
        }

        public TrafficLight( double trackObjectId, string trafficLigtName, double dicTrafficLightKindID)
        {
            this.Start = new PointOnTrack();
            this.TrackObjectID = trackObjectId;            
            this.TrafficLightName = trafficLigtName;
            this.DicTrafficLightKindID = dicTrafficLightKindID;
            
        }
        
        //соответстиве типов светофоров Егис и БД
        private static readonly Dictionary<double, double> TrafficLightKindMapping = new Dictionary<double, double>
        {
            {21, 1},
            {22, 2},
            {23, 3},
            {24, 0},
            {25, 4},
            {26, 5},
            {27, 6},
            {28, 7},
            {29, 8},
            {30, 9}
        };

        public void GetDicTrafficLightKindIDFromEgis()
        {
            if (TrafficLightKindMapping.TryGetValue(EgisTrafficLightKindValue, out double mappedValue))
            {
                DicTrafficLightKindID = mappedValue;
            }
            // Для default-case ничего не делаем (оставляем текущее значение)
        }
    }
}
