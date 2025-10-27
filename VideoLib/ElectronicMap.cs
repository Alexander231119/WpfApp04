using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Xml.Linq;
//using System.Text.Encoding.CodePages;

namespace WpfApp04
{
    


    public class ElectonicMap
    {
        // todo не используем
        public List<MainTrackRoute> mainTracks;
        public List<SideKlubRoute> sideTracks;
        //--
        public int dir_code;
        public string num;
        public DateTime date;
        public Dictionary<int, ekklub> ekklubs = new();



        public ElectonicMap Load(string xmlName)
        {
            

            var map = new ElectonicMap();
            var xDoc = XDocument.Load(xmlName);
            if (xDoc.Root == null) return null;
            // Создаем и заполняем ElectonicMap
            map.num = (string)xDoc.Root.Attribute("num");
            map.date = DateTime.Parse(xDoc.Root.Attribute("date")?.Value);
            map.dir_code = (int)xDoc.Root.Attribute("dir_code");
            map.ekklubs = xDoc.Descendants("ekklub")
                .Select(
                    ekklub => new ekklub
                    {
                        pos = (int)ekklub.Attribute("pos"),
                        CoordChange = (int)ekklub.Attribute("CoordChange"),
                        id = (int)ekklub.Attribute("id"),
                        ways = ekklub.Descendants("way")
                            .ToDictionary(
                                way => (int)way.Attribute("pos"),
                                way => new KlubWay
                                {
                                    pos = (int)way.Attribute("pos"),
                                    objs = way.Descendants("obj")
                                        .Select(
                                            obj => new KlubObj
                                            {
                                                id = obj.Attribute("id") != null ? (int)obj.Attribute("id") : 0,
                                                kTO = (int)obj.Attribute("kTO"),
                                                LinAddr = (int)obj.Attribute("LinAddr"),// + DeltaKlubMap,
                                                Len = obj.Attribute("Len") != null ? (int)obj.Attribute("Len") : 0,
                                                Lat = obj.Attribute("Lat") != null ? (long)obj.Attribute("Lat") : 0,
                                                Lon = obj.Attribute("Lon") != null ? (long)obj.Attribute("Lon") : 0,
                                                Props = obj.Descendants("prop")
                                                    .Select(
                                                        prop => new KlubObjProp
                                                        {
                                                            id = (int)prop.Attribute("id"),
                                                            value = (string)prop.Attribute("value")
                                                        }).ToList()
                                            }).ToList()
                                })
                    }).ToDictionary(e => e.pos);
            return map;
        }

    }
    /// <summary>
    /// Электронная карта участка
    /// </summary>
    public class ekklub
    {
        public int pos;
        public int CoordChange;
        public int id;
        public KlubObj start => ways[0].objs.First();
        public KlubObj end => ways[0].objs.Last();
        public Dictionary<int, KlubWay> ways;
        public bool In(double coordinate) => coordinate >= start.LinAddr && coordinate <= end.LinAddr;
    }
    /// <summary>
    /// Путь
    /// </summary>
    public class KlubWay
    {
        public int pos;
        public List<KlubObj> objs;
    }
    /// <summary>
    /// Объекты
    /// </summary>
    public class KlubObj
    {
        public int id;
        public int kTO; // Тип объекта
        public int LinAddr; // Линейная координата
        public int Len; // Длина
        public long Lat; // Широта
        public long Lon; // Долгота
        public List<KlubObjProp> Props;
        public string Name => Props.FirstOrDefault(p => p.id == 2)?.value;
        public int SpeedLimit =>
            int.TryParse(Props.FirstOrDefault(p => p.id == 17)?.value, out var result)
                ? result : 0;
        public override string ToString() => $"{kTO} {LinAddr} {Name}";
    }
    /// <summary>
    /// Свойства объектов
    /// </summary>
    public class KlubObjProp
    {
        public int id;
        public string value;
    }
    /// <summary>
    /// Главный путь (идет на протяжении всей карты)
    /// </summary>
    public class MainTrackRoute
    {
        public string trackName;
        //public List<KlubTarget> klubTargets;
    }
    /// <summary>
    /// Боковой путь (станционный)
    /// </summary>
    public class SideKlubRoute
    {
        public string stationName;
        public string main; // Track.number
        public string side; // Track.number
        public double delta; // todo пока не используем
        //public List<KlubTarget> klubTargets;
    }

    
}
