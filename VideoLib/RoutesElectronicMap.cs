using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using WpfApp04;

namespace WpfApp04
{
    public class RoutesEkklubs // электронная карта Ekklubs преобразованная в список DbRoutes
    {
        public Dictionary<int, DbRoute> RoutesList = new();
    }

    public class RoutesElectronicMap // файл  с картами преобразованный в списки с DbRoutes
    {
        //public List<RoutesEkklubs> RoutesEkklubsList=new List<RoutesEkklubs>();
        public Dictionary<int, RoutesEkklubs> RoutesEkklubsList = new();


        // преобразование файла electronic map в dBroutes
        public void DbRouteFromEkRoute(ElectonicMap electonicMap)
        {
            //double maxLinAddr = 0;
            //double minLinAddr = 0;

            foreach (var ekklubPair in electonicMap.ekklubs) // электронная карта
            {
                var ekklub = ekklubPair.Value;
                RoutesEkklubs routesEkKlubs = new RoutesEkklubs();
                RoutesEkklubsList.Add((int)ekklubPair.Key, routesEkKlubs);

                // создать сегмент. длина сегмента одинакова для всех ways
                //Segment s1 = new Segment();


                //создать километровые столбы
                List<Kilometer> WayKilometers = new List<Kilometer>(); // список километров исходный общий для всех путей данной карты



                // Создать DbRoute для каждого way
                foreach (var wayPair in ekklub.ways) // ways
                {
                    

                    var way = wayPair.Value;
                    var id = wayPair.Key;

                    bool reverse = false;

                    double wayeven=0;//чётность пути
                    double dreverse=0;// (double)reverse


                    //  определение направления
                    if (ekklub.CoordChange != null) // Если есть атрибут CoordChange
                    {
                        //reverse = ekklub.CoordChange == 0;

                        wayeven = way.pos % 2; //чётность пути

                        dreverse = ekklub.CoordChange - wayeven; // (double)reverse

                        reverse = dreverse == 0;
                    }
                    

                    DbRoute dbRoute = new DbRoute();
                    routesEkKlubs.RoutesList.Add(id, dbRoute);

                    Segment s = new Segment();
                    s.SegmentLength = 0;


                    //s.PredefinedRouteSegmentFromStartToEnd=ekklub.CoordChange-1;
                    s.PredefinedRouteSegmentFromStartToEnd = 1;

                    // s.SegmentID = id;
                    s.SegmentID = 1;

                    dbRoute.Segments.Add(s);
                    // список километров общий для всех путей данной карты
                    //dbRoute.Kilometers = WayKilometers;


                    // определение длины сегмента и его координат
                    double minLinAddr = way.objs.Min(p => p.LinAddr);
                    double maxLinAddr = way.objs.Max(p => p.LinAddr);

                    s.Start.LinAddr = minLinAddr;
                    s.Start.LinAddToRailwayCoordinates();
                    s.Start.PointOnTrackCoordinate = 0;
                    s.Start.RouteCoordinate = 0;

                    s.SegmentLength = maxLinAddr - minLinAddr;

                    s.End.LinAddr = maxLinAddr;
                    s.End.LinAddToRailwayCoordinates();
                    s.End.PointOnTrackCoordinate = s.SegmentLength;
                    s.End.RouteCoordinate = s.SegmentLength;



                    foreach (var obj in way.objs) // преобразование обьектов
                    {

                        

                        PointOnTrack startP = new PointOnTrack();
                        PointOnTrack endP = new PointOnTrack();


                        if (reverse == true)
                        {
                            endP.LinAddr = obj.LinAddr;
                            startP.LinAddr = endP.LinAddr - obj.Len;
                        }
                        else
                        {
                            startP.LinAddr = obj.LinAddr;
                            endP.LinAddr = obj.LinAddr + obj.Len;
                        }

                        startP.PointOnTrackCoordinate = startP.LinAddr - s.Start.LinAddr;
                            endP.PointOnTrackCoordinate = endP.LinAddr - s.Start.LinAddr;

                            startP.RouteCoordinate = startP.PointOnTrackCoordinate;
                            endP.RouteCoordinate = endP.PointOnTrackCoordinate;

                            startP.LinAddToRailwayCoordinates();
                            startP.TrackObjectID = obj.id;
                            startP.SegmentID = s.SegmentID;

                            endP.LinAddToRailwayCoordinates();
                            endP.TrackObjectID = obj.id;
                            endP.SegmentID = s.SegmentID;

                            
                        
                        dbRoute.PointOnTracks.Add(startP);
                        dbRoute.PointOnTracks.Add(endP);




                        //if (endP.RouteCoordinate > s.SegmentLength) s.SegmentLength = endP.RouteCoordinate; 

                        if (obj.kTO == 0) // километровый столб
                        {
                            Kilometer k = new Kilometer();
                            k.TrackObjectID = obj.id;
                            k.Start = startP;
                            k.Km = k.Start.PointOnTrackKm;

                            k.End.PointOnTrackCoordinate = k.Start.PointOnTrackCoordinate + 1000;
                            k.End.RouteCoordinate = k.Start.RouteCoordinate + 1000;
                            k.Length = 1000;
                            //dbRoute.Kilometers.Add(k);
                            WayKilometers.Add(k);
                        }

                        if (obj.kTO == 1) // светофор
                        {
                            TrafficLight k = new TrafficLight();
                            k.Start = startP;


                            k.TrackObjectID = obj.id;
                            k.TrafficLightName = obj.Name;
                            k.EgisTrafficLightKindValue = Convert.ToDouble(obj.Props[1].value);
                            k.GetDicTrafficLightKindIDFromEgis();
                            //k.Start.PointOnTrackCoordinate = obj.LinAddr;
                            //k.Start.RouteCoordinate = k.Start.PointOnTrackCoordinate;
                            dbRoute.TrafficLights.Add(k);

                        }

                        if (obj.kTO == 2) // постоянное ограничение скорсти
                        {
                            SpeedRestriction k = new SpeedRestriction(80, 0, 0);
                            k.Start = startP;
                            k.End = endP;
                            k.Value = Convert.ToDouble(obj.Props[0].value);
                            k.TrackObjectID = obj.id;

                            dbRoute.SpeedRestrictions.Add(k);
                        }

                        if (obj.kTO == 28) // стрелка
                        {
                            CrossingPiece k = new CrossingPiece();
                            k.TrackObjectID = obj.id;
                            k.Start = startP;
                            k.CrossingPieceName = obj.Name;
                            dbRoute.CrossingPieces.Add(k);


                        }

                        if (obj.kTO == 8) // станция
                        {
                            Station k = new Station();
                            k.Start = startP;
                            k.End = endP;
                            k.StationName = obj.Name;
                            k.TrackObjectID = obj.id;
                            k.StationID = k.EgisStationID = obj.id;

                            dbRoute.Stations.Add(k);
                        }

                        // уклона
                        if (obj.kTO == 32 && obj.Props.Any(p => p.id == 21))
                        {
                            double inclinevalue = Convert.ToDouble(obj.Props.Find(p => p.id == 21).value) / 10000 * (dreverse * 2 - 1);

                            Incline k = new Incline();
                            k.Value = inclinevalue;
                            k.Start = startP;
                            k.End = endP;

                            dbRoute.Inclines.Add(k);
                        }

                    }


                }


                foreach (var _route in routesEkKlubs.RoutesList)
                {

                    // добавить серую полосу на месте рц
                    TrackCircuit t = new TrackCircuit(0, "Gray line", 0);
                    t.Start = new PointOnTrack(_route.Value.Segments[0].Start);
                    t.End = new PointOnTrack(_route.Value.Segments[0].End);
                    _route.Value.TrackCircuits.Add(t);
                    _route.Value.PointOnTracks.Add(t.Start);
                    _route.Value.PointOnTracks.Add(t.End);
                    //добавить километровые столбы в каждый DbRoute (way)
                    foreach (Kilometer k in WayKilometers)
                    {
                        Kilometer k1 = new Kilometer();
                        k1.Km = k.Km;
                        k1.Start.PointOnTrackKm = k.Start.PointOnTrackKm;
                        k1.Start.LinAddr = k.Start.LinAddr;
                        k1.Length = k.Length;
                        k1.Start.SegmentID = _route.Value.Segments[0].SegmentID;
                        k1.End.SegmentID = _route.Value.Segments[0].SegmentID;

                        // 
                        k1.Start.PointOnTrackCoordinate = k1.Start.LinAddr - _route.Value.Segments[0].Start.LinAddr;
                        k1.End.PointOnTrackCoordinate = k1.Start.PointOnTrackCoordinate + k1.Length;

                        k1.Start.RouteCoordinate = k1.Start.PointOnTrackCoordinate;
                        k1.End.RouteCoordinate = k1.End.PointOnTrackCoordinate;

                        //k1.Start.RefreshRouteCoordinate(_route.Value.Segments);
                        //k1.End.RefreshRouteCoordinate(_route.Value.Segments);

                        if (k1.Start.PointOnTrackCoordinate >= 0 && k1.Start.PointOnTrackCoordinate <= _route.Value.Segments[0].SegmentLength)
                        {
                            _route.Value.Kilometers.Add(k1);
                            _route.Value.PointOnTracks.Add(k1.Start);
                            _route.Value.PointOnTracks.Add(k1.End);
                        }
                    }

                    foreach (TrafficLight t1 in _route.Value.TrafficLights)
                    {
                        //определить станцию для станционных светофоров

                        if (t1.DicTrafficLightKindID == 1
                            || t1.DicTrafficLightKindID == 2
                            || t1.DicTrafficLightKindID == 3
                            || t1.DicTrafficLightKindID == 7
                            || t1.DicTrafficLightKindID == 8
                            || t1.DicTrafficLightKindID == 11
                            || t1.DicTrafficLightKindID == 14
                            || t1.DicTrafficLightKindID == 15
                            || t1.DicTrafficLightKindID == 16
                            || t1.DicTrafficLightKindID == 22
                           )
                        {
                            t1.Start.FillStationID(_route.Value.Stations, true);
                            t1.StationID = t1.Start.StationID;
                            t1.Station = t1.Start.station;
                            t1.Start.TrackObjectName = "свет. " + t1.TrafficLightName + " на ст. " + t1.Station;
                        }
                    }

                    DbDataLoader.FillInclinesElevation(_route.Value.Inclines);


                }




            }
        }
    }
}
