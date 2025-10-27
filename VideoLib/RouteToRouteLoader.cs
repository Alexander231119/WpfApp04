using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace WpfApp04
{
    public class RouteToRouteLoader
    {
        private readonly DbRoute _sourseRoute;
        private readonly DbRoute _routeToAdd;
        private readonly DbRoute _targetRoute;

        private List<Segment> _segmentsToFillFromEgis = new List<Segment>();
        public List<Segment> _segmentsSourseFromEgis = new List<Segment>();

        private List<PointOnTrack> _pointOnTracksToAdd = new List<PointOnTrack>();

        public RouteExportCheckBoxList _routeExportCheckBoxList = new();

        public RouteToRouteLoader(
            DbRoute sourseRoute, 
            DbRoute routeToAdd, 
            DbRoute targetRoute, 
            List<Segment> segmentsToFillFromEgis, 
            List<PointOnTrack> pointOnTracksToAdd)
        {
            _sourseRoute = sourseRoute;
            _routeToAdd = routeToAdd;
            _targetRoute = targetRoute;
            _segmentsToFillFromEgis=segmentsToFillFromEgis;
            _pointOnTracksToAdd=pointOnTracksToAdd;
        }
        //public void FillFromRouteToRoute(DbRoute _sourseRoute, DbRoute _routeToAdd, DbRoute _targetRoute, List<Segment> _segmentsToFillFromEgis)

        public void FillFromRouteToRoute()
        {
            // добавление обьектов из егис в ToAddRoute
            // _sourseRoute источник ,
            // _routeToAdd фиктивный маршрут, содержащий объекты которые будут добавлены,
            // _targetRoute - маршрут для определения координат
            // _segmentsToFillFromEgis список сегментов _targetRoute, в которые будут добавляться обьекты
            // _segmentsSourseFromEgis список сегментов маршрута _sourseRoute, из которых будут добавляться обьекты

            if (_segmentsToFillFromEgis.Count > 0)
            {
                foreach (Segment s in _segmentsToFillFromEgis)
                {
                    if (_routeExportCheckBoxList._ImportTrafficLightsCheckBox == true)
                    {
                        //добавить светофоры
                        foreach (TrafficLight t in _sourseRoute.TrafficLights)
                        {

                            TrafficLight t1 = new TrafficLight();
                            t1.Start = new PointOnTrack();

                            


                            t1.Start.SegmentID = s.SegmentID;
                            t1.Start.PointOnTrackKm = t.Start.PointOnTrackKm;
                            t1.Start.PointOnTrackPk = t.Start.PointOnTrackPk;
                            t1.Start.PointOnTrackM = t.Start.PointOnTrackM;
                            t1.Start.DicPointOnTrackKindID = 1;
                            t1.DicTrafficLightKindID = t.DicTrafficLightKindID;
                            t1.TrafficLightName = t.TrafficLightName;

                            foreach (TliRestriction tli in t.TliRestrictions)
                            {
                                t1.TliRestrictions.Add(tli);
                            }

                            t1.Start.RefreshCoordinate(_targetRoute.PointOnTracks, _targetRoute.Segments);


                            // создать условие, проверяющее t.SegmentID на предмет того что этот сегмент в списке segmentsSourseFromEgis
                            int sindex = _segmentsSourseFromEgis.FindIndex(x => x.SegmentID == t.Start.SegmentID);

                            if ((t1.Start.PointOnTrackCoordinate < s.SegmentLength) && (t1.Start.PointOnTrackCoordinate > 0) && sindex>=0)
                            {
                                t1.Start.RefreshRouteCoordinate(_targetRoute.Segments);
                                _routeToAdd.TrafficLights.Add(t1);
                                _targetRoute.PointOnTracks.Add(t1.Start);

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
                                    t1.Start.FillStationID(_targetRoute.Stations, true);
                                    t1.StationID = t1.Start.StationID;
                                    t1.Station = t1.Start.station;
                                }

                                // для входного светофора 
                                // совместить координату с началом станции

                                if (t1.DicTrafficLightKindID == 1 || t1.DicTrafficLightKindID == 14)
                                {
                                    Station st = _targetRoute.Stations.Find(x => x.TrackObjectID == t1.StationID);

                                    if (st != null)
                                    {
                                        t1.Start.PointOnTrackCoordinate = st.Start.PointOnTrackCoordinate;
                                        t1.Start.PointOnTrackKm = st.Start.PointOnTrackKm;
                                        t1.Start.PointOnTrackPk = st.Start.PointOnTrackPk;
                                        t1.Start.PointOnTrackM = st.Start.PointOnTrackM;
                                        t1.Start.RouteCoordinate = st.Start.RouteCoordinate;
                                    }
                                }
                            }
                        }

                        
                    }

                    if (_routeExportCheckBoxList._ImportSignsCheckBox == true)
                    {
                        //добавить сигнальные знаки
                        foreach (TrafficSignal t in _sourseRoute.TrafficSignals)
                        {

                            TrafficSignal t1 = new TrafficSignal();
                            t1.Start = new PointOnTrack();

                            t1.Start.SegmentID = s.SegmentID;
                            t1.Start.PointOnTrackKm = t.Start.PointOnTrackKm;
                            t1.Start.PointOnTrackPk = t.Start.PointOnTrackPk;
                            t1.Start.PointOnTrackM = t.Start.PointOnTrackM;
                            t1.Start.DicPointOnTrackKindID = 37;

                            t1.DicTrafficSignalKindID = t.DicTrafficSignalKindID;

                            t1.Start.RefreshCoordinate(_targetRoute.PointOnTracks, _targetRoute.Segments);

                            int sindex = _segmentsSourseFromEgis.FindIndex(x => x.SegmentID == t.Start.SegmentID);

                            if ((t1.Start.PointOnTrackCoordinate < s.SegmentLength) && (t1.Start.PointOnTrackCoordinate > 0) && sindex >= 0)
                            {
                                t1.Start.RefreshRouteCoordinate(_targetRoute.Segments);
                                _routeToAdd.TrafficSignals.Add(t1);
                                _targetRoute.PointOnTracks.Add(t1.Start);
                            }


                        }
                    }

                    if (_routeExportCheckBoxList._ImportCurrentKindChangeCheckBox == true)
                    {
                        //добавить точки смены рода тока
                        foreach (CurrentKindChange t in _sourseRoute.CurrentKindChanges)
                        {

                            CurrentKindChange t1 = new CurrentKindChange();
                            t1.Start = new PointOnTrack();

                            t1.Start.SegmentID = s.SegmentID;
                            t1.Start.PointOnTrackKm = t.Start.PointOnTrackKm;
                            t1.Start.PointOnTrackPk = t.Start.PointOnTrackPk;
                            t1.Start.PointOnTrackM = t.Start.PointOnTrackM;
                            t1.Start.DicPointOnTrackKindID = 40;

                            t1.CurrentKindNameLeft = t.CurrentKindNameLeft;
                            t1.CurrentKindNameRight = t.CurrentKindNameRight;

                            t1.DicCurrentKindIDLeft = t.DicCurrentKindIDLeft;
                            t1.DicCurrentKindIDRight = t.DicCurrentKindIDRight;

                            t1.Start.RefreshCoordinate(_targetRoute.PointOnTracks, _targetRoute.Segments);

                            int sindex = _segmentsSourseFromEgis.FindIndex(x => x.SegmentID == t.Start.SegmentID);

                            if ((t1.Start.PointOnTrackCoordinate < s.SegmentLength) && (t1.Start.PointOnTrackCoordinate > 0) && sindex >= 0)
                            {
                                t1.Start.RefreshRouteCoordinate(_targetRoute.Segments);
                                _routeToAdd.CurrentKindChanges.Add(t1);
                                _targetRoute.PointOnTracks.Add(t1.Start);
                            }


                        }
                    }

                    if (_routeExportCheckBoxList._ImportUkspsCheckBox == true)
                    {
                        //добавить укспс
                        foreach (Uksps t in _sourseRoute.UkspsList)
                        {

                            Uksps t1 = new Uksps();
                            t1.Start = new PointOnTrack();

                            t1.Start.SegmentID = s.SegmentID;
                            t1.Start.PointOnTrackKm = t.Start.PointOnTrackKm;
                            t1.Start.PointOnTrackPk = t.Start.PointOnTrackPk;
                            t1.Start.PointOnTrackM = t.Start.PointOnTrackM;
                            t1.Start.DicPointOnTrackKindID = 25;

                            t1.Start.RefreshCoordinate(_targetRoute.PointOnTracks, _targetRoute.Segments);

                            int sindex = _segmentsSourseFromEgis.FindIndex(x => x.SegmentID == t.Start.SegmentID);

                            if ((t1.Start.PointOnTrackCoordinate < s.SegmentLength) && (t1.Start.PointOnTrackCoordinate > 0) && sindex >= 0)
                            {
                                t1.Start.RefreshRouteCoordinate(_targetRoute.Segments);
                                _routeToAdd.UkspsList.Add(t1);
                                _targetRoute.PointOnTracks.Add(t1.Start);
                            }


                        }
                    }

                    if (_routeExportCheckBoxList._ImportKtsmCheckBox == true)
                    {
                        //добавить KTSM
                        foreach (Ktsm t in _sourseRoute.KtsmList)
                        {

                            Ktsm t1 = new Ktsm();
                            t1.Start = new PointOnTrack();

                            t1.Start.SegmentID = s.SegmentID;
                            t1.Start.PointOnTrackKm = t.Start.PointOnTrackKm;
                            t1.Start.PointOnTrackPk = t.Start.PointOnTrackPk;
                            t1.Start.PointOnTrackM = t.Start.PointOnTrackM;
                            t1.Start.DicPointOnTrackKindID = 24;

                            t1.Start.RefreshCoordinate(_targetRoute.PointOnTracks, _targetRoute.Segments);

                            int sindex = _segmentsSourseFromEgis.FindIndex(x => x.SegmentID == t.Start.SegmentID);

                            if ((t1.Start.PointOnTrackCoordinate < s.SegmentLength) && (t1.Start.PointOnTrackCoordinate > 0) && sindex >= 0)
                            {
                                t1.Start.RefreshRouteCoordinate(_targetRoute.Segments);
                                _routeToAdd.KtsmList.Add(t1);
                                _targetRoute.PointOnTracks.Add(t1.Start);
                            }


                        }
                    }

                    if (_routeExportCheckBoxList._ImportCrossingsCheckBox == true)
                    {
                        //добавить переезды
                        foreach (Crossing t in _sourseRoute.Crossings)
                        {

                            Crossing t1 = new Crossing();

                            t1.Start = new PointOnTrack();
                            t1.DicCrossingKindID = t.DicCrossingKindID;

                            t1.Start.SegmentID = s.SegmentID;
                            t1.Start.PointOnTrackKm = t.Start.PointOnTrackKm;
                            t1.Start.PointOnTrackPk = t.Start.PointOnTrackPk;
                            t1.Start.PointOnTrackM = t.Start.PointOnTrackM;
                            t1.Start.DicPointOnTrackKindID = 23;

                            t1.End.SegmentID = s.SegmentID;
                            t1.End.PointOnTrackKm = t.Start.PointOnTrackKm;
                            t1.End.PointOnTrackPk = t.Start.PointOnTrackPk;
                            t1.End.PointOnTrackM = t.Start.PointOnTrackM;
                            t1.End.DicPointOnTrackKindID = 23;

                            t1.Start.RefreshCoordinate(_targetRoute.PointOnTracks, _targetRoute.Segments);

                            int sindex = _segmentsSourseFromEgis.FindIndex(x => x.SegmentID == t.Start.SegmentID);

                            if ((t1.Start.PointOnTrackCoordinate < s.SegmentLength) && (t1.Start.PointOnTrackCoordinate > 0) && sindex >= 0)
                            {
                                t1.Start.RefreshRouteCoordinate(_targetRoute.Segments);
                                _routeToAdd.Crossings.Add(t1);
                                _targetRoute.PointOnTracks.Add(t1.Start);
                            }

                        }
                    }

                    if (_routeExportCheckBoxList._ImportStationBordersCheckBox is true)
                    {
                        //добавить границы станций
                        foreach (Station t in _sourseRoute.Stations)
                        {

                            Station t1 = _targetRoute.Stations.Find(x => x.StationName == t.StationName);
                            //Platform t1 = new Platform();

                            if (t1 != null)
                            {
                                PointOnTrack p1 = new PointOnTrack();

                                p1.SegmentID = s.SegmentID;
                                p1.PointOnTrackKm = t.Start.PointOnTrackKm;
                                p1.PointOnTrackPk = t.Start.PointOnTrackPk;
                                p1.PointOnTrackM = t.Start.PointOnTrackM;
                                p1.DicPointOnTrackKindID = 8;
                                p1.TrackObjectID = t1.TrackObjectID;
                                p1.PointOnTrackUsageDirection = t.Start.PointOnTrackUsageDirection;

                                PointOnTrack p2 = new PointOnTrack();

                                p2.SegmentID = s.SegmentID;
                                p2.PointOnTrackKm = t.End.PointOnTrackKm;
                                p2.PointOnTrackPk = t.End.PointOnTrackPk;
                                p2.PointOnTrackM = t.End.PointOnTrackM;
                                p2.DicPointOnTrackKindID = 8;
                                p2.TrackObjectID = t1.TrackObjectID;
                                p2.PointOnTrackUsageDirection = t.End.PointOnTrackUsageDirection;

                                p1.RefreshCoordinate(_targetRoute.PointOnTracks, _targetRoute.Segments);
                                p2.RefreshCoordinate(_targetRoute.PointOnTracks, _targetRoute.Segments);

                                if (s.PredefinedRouteSegmentFromStartToEnd == -1)
                                {
                                    PointOnTrack tt = new PointOnTrack();

                                    tt = p1;
                                    p1 = p2;
                                    p2 = tt;
                                }

                                int sindex = _segmentsSourseFromEgis.FindIndex(x => x.SegmentID == t.Start.SegmentID);

                                if ((p1.PointOnTrackCoordinate < s.SegmentLength) && (p1.PointOnTrackCoordinate > 0) && sindex >= 0)
                                {
                                    p1.RefreshRouteCoordinate(_targetRoute.Segments);
                                    t1.Start = p1;
                                    //PointOnTracks.Add(p1);
                                    _pointOnTracksToAdd.Add(p1);
                                }

                                int sindex2 = _segmentsSourseFromEgis.FindIndex(x => x.SegmentID == t.End.SegmentID);

                                if ((p2.PointOnTrackCoordinate < s.SegmentLength) && (p2.PointOnTrackCoordinate > 0) && sindex2 >= 0)
                                {
                                    p2.RefreshRouteCoordinate(_targetRoute.Segments);
                                    t1.End = p2;
                                    //PointOnTracks.Add(p2);
                                    _pointOnTracksToAdd.Add(p2);
                                }
                            }




                        }

                    }

                    if (_routeExportCheckBoxList._ImportPlatformsCheckBox is true)
                    {
                        //добавить платформы
                        foreach (Platform t in _sourseRoute.Platforms)
                        {
                            Platform t1 = new Platform();
                            t1.Start = new PointOnTrack();
                            t1.Start.SegmentID = s.SegmentID;
                            t1.Start.PointOnTrackKm = t.Start.PointOnTrackKm;
                            t1.Start.PointOnTrackPk = t.Start.PointOnTrackPk;
                            t1.Start.PointOnTrackM = t.Start.PointOnTrackM;
                            t1.Start.DicPointOnTrackKindID = 9;
                            t1.End.SegmentID = s.SegmentID;
                            t1.End.PointOnTrackKm = t.End.PointOnTrackKm;
                            t1.End.PointOnTrackPk = t.End.PointOnTrackPk;
                            t1.End.PointOnTrackM = t.End.PointOnTrackM;
                            t1.End.DicPointOnTrackKindID = 9;
                            t1.PlatformName = t.PlatformName;

                            t1.Start.RefreshCoordinate(_targetRoute.PointOnTracks, _targetRoute.Segments);
                            t1.End.RefreshCoordinate(_targetRoute.PointOnTracks, _targetRoute.Segments);

                            if ((t1.Start.PointOnTrackCoordinate < s.SegmentLength) &&
                                (t1.Start.PointOnTrackCoordinate > 0) &&
                                (t1.End.PointOnTrackCoordinate < s.SegmentLength) &&
                                (t1.End.PointOnTrackCoordinate > 0))
                            {
                                int sindex = _segmentsSourseFromEgis.FindIndex(x => x.SegmentID == t.Start.SegmentID);
                                if (sindex >= 0)
                                {
                                    if (s.PredefinedRouteSegmentFromStartToEnd * _segmentsSourseFromEgis[sindex]
                                            .PredefinedRouteSegmentFromStartToEnd == -1)
                                    {
                                        PointOnTrack tt = new PointOnTrack();
                                        tt = t1.Start;
                                        t1.Start = t1.End;
                                        t1.End = tt;
                                    }

                                    t1.Start.RefreshRouteCoordinate(_targetRoute.Segments);
                                    t1.End.RefreshRouteCoordinate(_targetRoute.Segments);
                                    _routeToAdd.Platforms.Add(t1);
                                    _targetRoute.PointOnTracks.Add(t1.Start);
                                    _targetRoute.PointOnTracks.Add(t1.End);

                                    // заполнить StationID определить на какой станции находится платформа
                                    t1.Start.FillStationID(_targetRoute.Stations, false);
                                    t1.StationID = t1.Start.StationID;
                                    t1.Station = t1.Start.station;
                                    t1.End.FillStationID(_targetRoute.Stations, false);
                                    t1.StationID = t1.Start.StationID;
                                    t1.Station = t1.Start.station;
                                }
                            }
                        }
                    }

                    if (_routeExportCheckBoxList._ImportNeutralSectionsCheckBox is true)
                    {
                        //добавить нейтр
                        foreach (NeutralSection t in _sourseRoute.NeutralSections)
                        {

                            NeutralSection t1 = new NeutralSection();
                            t1.Start = new PointOnTrack();

                            t1.Start.SegmentID = s.SegmentID;
                            t1.Start.PointOnTrackKm = t.Start.PointOnTrackKm;
                            t1.Start.PointOnTrackPk = t.Start.PointOnTrackPk;
                            t1.Start.PointOnTrackM = t.Start.PointOnTrackM;
                            t1.Start.DicPointOnTrackKindID = 35;

                            t1.End.SegmentID = s.SegmentID;
                            t1.End.PointOnTrackKm = t.End.PointOnTrackKm;
                            t1.End.PointOnTrackPk = t.End.PointOnTrackPk;
                            t1.End.PointOnTrackM = t.End.PointOnTrackM;
                            t1.End.DicPointOnTrackKindID = 35;


                            t1.Start.RefreshCoordinate(_targetRoute.PointOnTracks, _targetRoute.Segments);
                            t1.End.RefreshCoordinate(_targetRoute.PointOnTracks, _targetRoute.Segments);

                            int sindex = _segmentsSourseFromEgis.FindIndex(x => x.SegmentID == t.Start.SegmentID);

                            if ((t1.Start.PointOnTrackCoordinate < s.SegmentLength) && (t1.Start.PointOnTrackCoordinate > 0) &&
                                (t1.End.PointOnTrackCoordinate < s.SegmentLength) && (t1.End.PointOnTrackCoordinate > 0) && sindex >= 0)
                            {

                                if (s.PredefinedRouteSegmentFromStartToEnd * _segmentsSourseFromEgis[sindex].PredefinedRouteSegmentFromStartToEnd == -1)
                                {
                                    PointOnTrack tt = new PointOnTrack();

                                    tt = t1.Start;
                                    t1.Start = t1.End;
                                    t1.End = tt;
                                }

                                t1.Start.RefreshRouteCoordinate(_targetRoute.Segments);
                                t1.End.RefreshRouteCoordinate(_targetRoute.Segments);



                                _routeToAdd.NeutralSections.Add(t1);

                                _targetRoute.PointOnTracks.Add(t1.Start);
                                _targetRoute.PointOnTracks.Add(t1.End);


                                // знаки отключить включить ток


                                TrafficSignal ts1 = new TrafficSignal();
                                ts1.Start = new PointOnTrack(t1.Start);
                                ts1.Start.DicPointOnTrackKindID = 37;
                                ts1.End.DicPointOnTrackKindID = 37;
                                ts1.DicTrafficSignalKindID = 18;

                                ts1.Start.RouteCoordinate -= 50;
                                ts1.Start.RefreshKmPkM2(_targetRoute.PointOnTracks, _targetRoute.Segments);
                                ts1.Start.RefreshKmPkM(_targetRoute.PointOnTracks);

                                TrafficSignal ts2 = new TrafficSignal();
                                ts2.Start = new PointOnTrack(t1.End);
                                ts2.Start.DicPointOnTrackKindID = 37;
                                ts2.End.DicPointOnTrackKindID = 37;
                                ts2.DicTrafficSignalKindID = 19;

                                ts2.Start.RouteCoordinate += 80;
                                ts2.Start.RefreshKmPkM2(_targetRoute.PointOnTracks, _targetRoute.Segments);
                                ts2.Start.RefreshKmPkM(_targetRoute.PointOnTracks);

                                TrafficSignal ts3 = new TrafficSignal();
                                ts3.Start = new PointOnTrack(t1.End);
                                ts3.Start.DicPointOnTrackKindID = 37;
                                ts3.End.DicPointOnTrackKindID = 37;
                                ts3.DicTrafficSignalKindID = 20;

                                ts3.Start.RouteCoordinate += 210;
                                ts3.Start.RefreshKmPkM2(_targetRoute.PointOnTracks, _targetRoute.Segments);
                                ts3.Start.RefreshKmPkM(_targetRoute.PointOnTracks);

                                _routeToAdd.TrafficSignals.Add(ts1);
                                _routeToAdd.TrafficSignals.Add(ts2);
                                _routeToAdd.TrafficSignals.Add(ts3);


                            }
                        }
                    }



                    if (_routeExportCheckBoxList._ImportInclinesCheckBox is true)
                    {
                        //добавить уклоны
                        foreach (Incline t in _sourseRoute.Inclines)
                        {
                            Incline t1 = new Incline(t.Value);
                            //t1.Start = new PointOnTrack();

                            t1.Start.SegmentID = s.SegmentID;
                            t1.Start.PointOnTrackKm = t.Start.PointOnTrackKm;
                            t1.Start.PointOnTrackPk = t.Start.PointOnTrackPk;
                            t1.Start.PointOnTrackM = t.Start.PointOnTrackM;
                            t1.Start.DicPointOnTrackKindID = t.Start.DicPointOnTrackKindID;

                            t1.End.SegmentID = s.SegmentID;
                            t1.End.PointOnTrackKm = t.End.PointOnTrackKm;
                            t1.End.PointOnTrackPk = t.End.PointOnTrackPk;
                            t1.End.PointOnTrackM = t.End.PointOnTrackM;
                            t1.End.DicPointOnTrackKindID = t.Start.DicPointOnTrackKindID;

                            t1.Start.RefreshCoordinate(_targetRoute.PointOnTracks, _targetRoute.Segments);
                            t1.End.RefreshCoordinate(_targetRoute.PointOnTracks, _targetRoute.Segments);




                            


                            int sindex = _segmentsSourseFromEgis.FindIndex(x => x.SegmentID == t.Start.SegmentID);
                            if (sindex >= 0)
                            {

                                if (s.PredefinedRouteSegmentFromStartToEnd * _segmentsSourseFromEgis[sindex].PredefinedRouteSegmentFromStartToEnd == -1)
                                {
                                    PointOnTrack tt = new PointOnTrack();

                                    tt = t1.Start;
                                    t1.Start = t1.End;
                                    t1.End = tt;

                                    if (t1.Value != 0)
                                        t1.Value *= -1;
                                }






                                //если начало и конец уклона внутри сегмента
                                if ((t1.Start.PointOnTrackCoordinate <= s.SegmentLength) && (t1.Start.PointOnTrackCoordinate > 0) &&
                                    (t1.End.PointOnTrackCoordinate <= s.SegmentLength) && (t1.End.PointOnTrackCoordinate > 0))
                                {
                                    t1.Start.RefreshRouteCoordinate(_targetRoute.Segments);
                                    t1.End.RefreshRouteCoordinate(_targetRoute.Segments);

                                    _routeToAdd.Inclines.Add(t1);

                                    _targetRoute.PointOnTracks.Add(t1.Start);
                                    _targetRoute.PointOnTracks.Add(t1.End);
                                }
                                // если начало уклона внутри сегмента
                                else if ((t1.Start.PointOnTrackCoordinate <= s.SegmentLength) && (t1.Start.PointOnTrackCoordinate > 0))
                                    // &&   (t1.End.PointOnTrackCoordinate >= s.SegmentLength))
                                {
                                    t1.End = new PointOnTrack(s.End);
                                    t1.End.DicPointOnTrackKindID = 32;

                                    t1.Start.RefreshRouteCoordinate(_targetRoute.Segments);
                                    t1.End.RefreshRouteCoordinate(_targetRoute.Segments);

                                    _routeToAdd.Inclines.Add(t1);

                                    _targetRoute.PointOnTracks.Add(t1.Start);
                                    _targetRoute.PointOnTracks.Add(t1.End);
                                }
                                // если конец уклона внутри сегмента
                                else if (//(t1.Start.PointOnTrackCoordinate <= s.SegmentLength) && (t1.Start.PointOnTrackCoordinate > 0) &&
                                         (t1.End.PointOnTrackCoordinate <= s.SegmentLength) && (t1.End.PointOnTrackCoordinate > 0))
                                {

                                    t1.Start = new PointOnTrack(s.Start);
                                    t1.Start.DicPointOnTrackKindID = 32;

                                    t1.Start.RefreshRouteCoordinate(_targetRoute.Segments);
                                    t1.End.RefreshRouteCoordinate(_targetRoute.Segments);

                                    _routeToAdd.Inclines.Add(t1);

                                    _targetRoute.PointOnTracks.Add(t1.Start);
                                    _targetRoute.PointOnTracks.Add(t1.End);
                                }


                            }

                            

                        }
                    }

                    if (_routeExportCheckBoxList._ImportSpeedCheckBox is true)
                    {
                        //добавить скорости
                        foreach (SpeedRestriction t in _sourseRoute.SpeedRestrictions)
                        {

                            SpeedRestriction t1 = new SpeedRestriction(t.Value, 0, 0);
                            //t1.Start = new PointOnTrack();

                            t1.Start.SegmentID = s.SegmentID;
                            t1.Start.PointOnTrackKm = t.Start.PointOnTrackKm;
                            t1.Start.PointOnTrackPk = t.Start.PointOnTrackPk;
                            t1.Start.PointOnTrackM = t.Start.PointOnTrackM;
                            t1.Start.DicPointOnTrackKindID = t.Start.DicPointOnTrackKindID;

                            t1.End.SegmentID = s.SegmentID;
                            t1.End.PointOnTrackKm = t.End.PointOnTrackKm;
                            t1.End.PointOnTrackPk = t.End.PointOnTrackPk;
                            t1.End.PointOnTrackM = t.End.PointOnTrackM;
                            t1.End.DicPointOnTrackKindID = t.Start.DicPointOnTrackKindID;
                            t1.PermRestrictionForEmptyTrain = t.PermRestrictionForEmptyTrain;

                            t1.Start.RefreshCoordinate(_targetRoute.PointOnTracks, _targetRoute.Segments);
                            t1.End.RefreshCoordinate(_targetRoute.PointOnTracks, _targetRoute.Segments);

                            
                                

                                int sindex = _segmentsSourseFromEgis.FindIndex(x => x.SegmentID == t.Start.SegmentID);
                                if (sindex >= 0 ) 
                            {

                                if (s.PredefinedRouteSegmentFromStartToEnd* _segmentsSourseFromEgis[sindex].PredefinedRouteSegmentFromStartToEnd == -1)
                                {
                                    PointOnTrack tt = new PointOnTrack();

                                    tt = t1.Start;
                                    t1.Start = t1.End;
                                    t1.End = tt;

                                    //if (t1.Value != 0)
                                    // t1.Value *= -1;
                                }



                                //если начало и конец  внутри сегмента
                                if ((t1.Start.PointOnTrackCoordinate <= s.SegmentLength) && (t1.Start.PointOnTrackCoordinate > 0) &&
                                    (t1.End.PointOnTrackCoordinate <= s.SegmentLength) && (t1.End.PointOnTrackCoordinate > 0))
                                {
                                    t1.Start.RefreshRouteCoordinate(_targetRoute.Segments);
                                    t1.End.RefreshRouteCoordinate(_targetRoute.Segments);

                                    if (t1.ObjectLength != 0)
                                    {
                                        _routeToAdd.SpeedRestrictions.Add(t1);
                                        _targetRoute.PointOnTracks.Add(t1.Start);
                                        _targetRoute.PointOnTracks.Add(t1.End);
                                    }
                                }


                                // если начало  внутри сегмента
                                else if ((t1.Start.PointOnTrackCoordinate <= s.SegmentLength) && (t1.Start.PointOnTrackCoordinate > 0))
                                    // &&   (t1.End.PointOnTrackCoordinate >= s.SegmentLength))
                                {
                                    t1.End = new PointOnTrack(s.End);
                                    t1.End.DicPointOnTrackKindID = 32;

                                    t1.Start.RefreshRouteCoordinate(_targetRoute.Segments);
                                    t1.End.RefreshRouteCoordinate(_targetRoute.Segments);

                                    if (t1.ObjectLength != 0)
                                    {
                                        _routeToAdd.SpeedRestrictions.Add(t1);
                                        _targetRoute.PointOnTracks.Add(t1.Start);
                                        _targetRoute.PointOnTracks.Add(t1.End);
                                    }
                                }
                                // если конец внутри сегмента
                                else if (//(t1.Start.PointOnTrackCoordinate <= s.SegmentLength) && (t1.Start.PointOnTrackCoordinate > 0) &&
                                         (t1.End.PointOnTrackCoordinate <= s.SegmentLength) && (t1.End.PointOnTrackCoordinate > 0))
                                {

                                    t1.Start = new PointOnTrack(s.Start);
                                    t1.Start.DicPointOnTrackKindID = 32;

                                    t1.Start.RefreshRouteCoordinate(_targetRoute.Segments);
                                    t1.End.RefreshRouteCoordinate(_targetRoute.Segments);

                                    if (t1.ObjectLength != 0)
                                    {
                                        _routeToAdd.SpeedRestrictions.Add(t1);
                                        _targetRoute.PointOnTracks.Add(t1.Start);
                                        _targetRoute.PointOnTracks.Add(t1.End);
                                    }
                                }
                                // если начало перед сегментом а конец после сегмента



                            }



                        }
                    }
                }
            }
        }

    }
}
