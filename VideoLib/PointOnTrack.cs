using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows;

namespace WpfApp04
{
    public class PointOnTrack
    {
        public double PointOntrackID;
        public string PointOnTrackKm { get; set; } = "";
        public double PointOnTrackPk { get; set; }
        public double PointOnTrackM { get; set; }
        public double DicPointOnTrackKindID;
        public double LinAddr { get; set; }
        public double PointOnTrackLongitude { get; set; }
        public double PointOnTrackLatitude { get; set; }


        public double EgisDicPointOnTrackKindID;
        public double TrackObjectID;
        public double SegmentID { get; set; }
        public double PointOnTrackCoordinate;
        public double PointOnTrackUsageDirection;
        public string station = "";
        public double RouteCoordinate { get; set; }
        public double Elevation = 0;
        public double FilmFrameTime { get; set; }

        public string TrackObjectName { set; get; } = "";
        public string TrackNumber { get; set; } = "";        
        public string TrackName { set; get; } = "";
        public double TrackID { get; set; }

        public double StationID { get; set; }

        public static readonly Dictionary<double, string> PointOnTrackNames = new()
        {
            [0] = "Километровый столб",
            [27] = "Тупик",
            [28] = "Стрелка со стороны остряка",
            [29] = "Стрелка против остряка",
            [1] = "Светофор",
            [2] = "Граница постоянного ограничения",
            [6]= "Место проверки тормозов, граница",
            [8]= "Граница станции",
            [9]= "Граница платформы",
            [10] = "Ось станции",
            [23] = "Переезд",
            [24] = "ДИСК",
            [25] = "УКСПС",
            [26] = "Точка остановки первого вагона",
            [31] = "Граница кривой",
            [32] = "Граница уклона",
            [33] = "Граница моста",
            [34] = "Граница туннеля",
            [35] = "Граница нейтральной вставки",
            [36] = "Граница некодированного пути",
            [37] = "Сигнальный знак",
            [38] = "Граница рельсовой цепи",
            [39] = "Путепровод",
            [40] = "Токораздел"
        };

        public string DicPointOnTrackKindName
        {
            get => PointOnTrackNames.TryGetValue(DicPointOnTrackKindID, out var name)
                ? name
                : string.Empty;
            set { } // Удалить, если не используется
        }
        
        // конструктор предложенный создать автоматически
        public PointOnTrack()
        {
        }

        // конструктор
        public PointOnTrack(double PointOnTrackId, double DicPointOnTrackKindID, double TrackObjectID, double SegmentID, double PointOnTrackCoordinate,
            string PointOnTrackKm, double PointOnTrackPk, double PointOnTrackM,
            double PointOnTrackUsageDirection)
        {
            this.PointOntrackID = PointOnTrackId;
            this.DicPointOnTrackKindID = DicPointOnTrackKindID;
            this.TrackObjectID = TrackObjectID;
            this.SegmentID = SegmentID;
            this.PointOnTrackCoordinate = PointOnTrackCoordinate;
            this.PointOnTrackKm = PointOnTrackKm;
            this.PointOnTrackPk = PointOnTrackPk;
            this.PointOnTrackM = PointOnTrackM;
            this.PointOnTrackUsageDirection = PointOnTrackUsageDirection;
        }

        public PointOnTrack(PointOnTrack point)
        {
            this.DicPointOnTrackKindID = point.DicPointOnTrackKindID;
            this.SegmentID = point.SegmentID;
            this.PointOnTrackCoordinate = point.PointOnTrackCoordinate;
            this.PointOnTrackKm = point.PointOnTrackKm;
            this.PointOnTrackPk = point.PointOnTrackPk;
            this.PointOnTrackM = point.PointOnTrackM;
            this.PointOnTrackUsageDirection = point.PointOnTrackUsageDirection;
            this.station = point.station;
            this.RouteCoordinate = point.RouteCoordinate;
        }
        //соответствие PointOnTrackKind ЕГИС (слева) и Mdb (справа)
        private static readonly Dictionary<double, double> EgisToPointOnTrackKindMapping = new()
        {
            [0] = 0,
            [1] = 1,
            [2] = 2,
            [4] = 38,
            [6] = 6,
            [8] = 8,
            [9] = 9,
            [10] = 10,
            [23] = 23,
            [24] = 24,
            [25] = 25,
            [26] = 26,
            [27] = 27,
            [28] = 28,
            [31] = 31,
            [32] = 32,
            [33] = 33, // мост dictrackobjectkind 17егис 17база
            [34] = 34,
            [35] = 35,
            [38] = 2,  // скорость по предупреждению
            [39] = 37, // нт
            [40] = 37, // кт
            [48] = 37, // граница опасного места
            [49] = 24, // ось ктсм
            [57] = 24, // граница ктсм
            [55] = 37  // свисток
        };

        public void GetPoinOntrackKindFromEgis()
        {
            if (EgisToPointOnTrackKindMapping.TryGetValue(EgisDicPointOnTrackKindID, out var mappedValue))
            {
                DicPointOnTrackKindID = mappedValue;
            }
        }

        public void LinAddToRailwayCoordinates()
        {
            PointOnTrackKm = (Math.Floor(LinAddr / 1000) + 1).ToString();
            PointOnTrackPk = Math.Floor((LinAddr % 1000) / 100) + 1;
            PointOnTrackM = (LinAddr % 100);
        }


        public void RefreshRouteCoordinate(List<Segment> segments)
        {

            int sindex = segments.FindIndex((Segment) => Segment.SegmentID == SegmentID);

            if (sindex >= 0)
            {
                if (segments[sindex].PredefinedRouteSegmentFromStartToEnd == 1)
                {
                    this.RouteCoordinate = this.PointOnTrackCoordinate;
                }
                else
                {
                    this.RouteCoordinate = segments[sindex].SegmentLength - this.PointOnTrackCoordinate;
                }
            }
                        
            if (sindex >= 1)
            {
                for (int i = 1; i <= sindex; i++)
                {
                    RouteCoordinate += segments[sindex - i].SegmentLength;
                }
            }
            
            RouteCoordinate = Math.Round(RouteCoordinate, 2);
        }

        // вычислить PointOntrackCoordinate по железнодорожным координатам с известным SegmentID
        public void RefreshCoordinate(List<PointOnTrack> points, List<Segment> segments)
        {
            string km = PointOnTrackKm;
            double segmentid = SegmentID;
            PointOnTrack p = new PointOnTrack();

            int index = points.FindIndex(x => (x.PointOnTrackKm == km) && (x.SegmentID == segmentid)
            &&((x.DicPointOnTrackKindID == 0) || (x.DicPointOnTrackKindID == 27) || (x.DicPointOnTrackKindID == 28) || (x.DicPointOnTrackKindID == 29)));

            int sindex = segments.FindIndex((Segment) => Segment.SegmentID == SegmentID);


            if (index >= 0)
            {
                PointOnTrackCoordinate = points[index].PointOnTrackCoordinate
                                         - (points[index].PointOnTrackPk - 1) * 100 - points[index].PointOnTrackM
                                         + (PointOnTrackPk - 1) * 100 + PointOnTrackM;
            }
            else
            {
                if (sindex < 0)
                { 
                  //  MessageBox.Show("Некорректный SegmentID ", "   "); 
                }
                else
                {
                  //  MessageBox.Show("КМ " + PointOnTrackKm + " не соответствует SegmentID " + SegmentID.ToString(),
                  //      "    ");
                }
            }

            //округлить до сотых
            PointOnTrackCoordinate = Math.Round(PointOnTrackCoordinate, 2);
            // если координата близка к нулю
            if (PointOnTrackCoordinate < 0.001 && PointOnTrackCoordinate > -0.001)
                PointOnTrackCoordinate = 0;
        }

        // вычислить жд координату по RouteCoordinate
        public void RefreshKmPkM2(List<PointOnTrack> points, List<Segment>segments)
        {
            
            //найти сегмент на котором находится точка
            double segmentslength = 0;

            for (int i = 0; i < segments.Count; i++)
            {
                PointOnTrackCoordinate = RouteCoordinate - segmentslength;

                segmentslength += segments[i].SegmentLength;
                SegmentID = segments[i].SegmentID;

               if (segments[i].PredefinedRouteSegmentFromStartToEnd == -1)
               { PointOnTrackCoordinate = segments[i].SegmentLength - PointOnTrackCoordinate; }

                if (segmentslength >= RouteCoordinate) break;
            }


        }

        // вычислить жд координату по PointOnTrackCoordinate
        public void RefreshKmPkM(List<PointOnTrack> points)
        {
            int index1 = points.FindIndex(
                x => (x.PointOnTrackCoordinate <= PointOnTrackCoordinate) 
                && (x.SegmentID == SegmentID)
                        && (
                            (x.DicPointOnTrackKindID == 0) || 
                            (x.DicPointOnTrackKindID == 27) || 
                            (x.DicPointOnTrackKindID == 28) || 
                            (x.DicPointOnTrackKindID == 29)
                           )
                     );
            int index2 = points.FindLastIndex(
                x => (x.PointOnTrackCoordinate <= PointOnTrackCoordinate)
                && (x.SegmentID == SegmentID)
                        && (
                            (x.DicPointOnTrackKindID == 0) ||
                            (x.DicPointOnTrackKindID == 27) ||
                            (x.DicPointOnTrackKindID == 28) ||
                            (x.DicPointOnTrackKindID == 29)
                           )
                     );

            int index;

            

            if ( Math.Abs(PointOnTrackCoordinate - points[index1].PointOnTrackCoordinate) < Math.Abs(PointOnTrackCoordinate - points[index2].PointOnTrackCoordinate))
            {
                index = index1;
            }
            else
            {
                index = index2;
            }

            //index = Math.Max(index1, index2);

            string Km = points[index].PointOnTrackKm;

            double M = (PointOnTrackCoordinate - points[index].PointOnTrackCoordinate +(points[index1].PointOnTrackPk-1)*100+ points[index1].PointOnTrackM) % 100; // остаток от деления на 100

            double Pk = ((PointOnTrackCoordinate - points[index].PointOnTrackCoordinate + (points[index1].PointOnTrackPk - 1) * 100 + points[index1].PointOnTrackM) - M) / 100 + 1;
            // double Pk = ((PointOnTrackCoordinate - points[index].PointOnTrackCoordinate) - PointOnTrackM) / 100 + 1;

            string message = PointOnTrackKm 
                + " " 
                + PointOnTrackPk.ToString() 
                + " " 
                + PointOnTrackM.ToString()
                + " - "
                + PointOnTrackCoordinate.ToString() +"\n";

            message += points[index].PointOnTrackKm + " " 
                + points[index].PointOnTrackPk.ToString() 
                + " " 
                + points[index].PointOnTrackM.ToString()
                + " - "
                + points[index].PointOnTrackCoordinate.ToString() + "\n";
            
            PointOnTrackKm = Km;
            PointOnTrackM = Math.Round(M,2);
            PointOnTrackPk = Math.Round(Pk, 0);

            message += PointOnTrackKm + " " 
                + PointOnTrackPk.ToString() + " " 
                + PointOnTrackM.ToString() + " - "
                + PointOnTrackCoordinate.ToString() + "\n";
            
            //MessageBox.Show(message);
        }

        public void FillElevation ( List<Incline> inclineList)
        {
            int inclindex = inclineList.FindIndex(x => (x.Start.RouteCoordinate<RouteCoordinate )
            &&(x.End.RouteCoordinate> RouteCoordinate)||
            (x.Start.RouteCoordinate == RouteCoordinate)||
            (x.End.RouteCoordinate == RouteCoordinate)
            );

            if (inclindex >= 0)
            {
                if (inclineList[inclindex].Start.RouteCoordinate == RouteCoordinate)
                {
                    Elevation = inclineList[inclindex].Start.Elevation;
                }
                else if (inclineList[inclindex].End.RouteCoordinate == RouteCoordinate)
                {
                    Elevation = inclineList[inclindex].End.Elevation;
                }
                else
                {
                    Elevation = ((inclineList[inclindex].End.Elevation - inclineList[inclindex].Start.Elevation) /
                        (inclineList[inclindex].End.RouteCoordinate - inclineList[inclindex].Start.RouteCoordinate)) *
                        (RouteCoordinate - inclineList[inclindex].Start.RouteCoordinate) + inclineList[inclindex].Start.Elevation;
                }

            }
        }

        // название станции или перегона
        public void FillStation(List<Station> stations)
        {
            

            int sindex = stations.FindIndex(x => (x.Start.RouteCoordinate < RouteCoordinate)
            && (x.End.RouteCoordinate > RouteCoordinate) ||
            (x.Start.RouteCoordinate == RouteCoordinate) ||
            (x.End.RouteCoordinate == RouteCoordinate)
            );

            int sindexminus = stations.FindLastIndex(x => (x.End.RouteCoordinate < RouteCoordinate));
            int sindexplus = stations.FindIndex(x => (x.Start.RouteCoordinate > RouteCoordinate));

            if (sindex >= 0)
            {
                if ((stations[sindex].Start.RouteCoordinate == RouteCoordinate)||
                    (stations[sindex].End.RouteCoordinate == RouteCoordinate)||
                    ((stations[sindex].Start.RouteCoordinate < RouteCoordinate)&& (stations[sindex].End.RouteCoordinate > RouteCoordinate)))
                {
                    this.station = stations[sindex].StationName;
                }
            }
            else if (
                (sindexplus >= 0) && (sindexminus >= 0)
               && (stations[sindexminus].End.RouteCoordinate < RouteCoordinate)
                    && (stations[sindexplus].Start.RouteCoordinate > RouteCoordinate)
                     
                    )
            {
                    station = stations[sindexminus].StationName+ " - " + stations[sindexplus].StationName;
            }
                        

        }

        // id станции
        public void FillStationID(List<Station> stations, bool toNextStation)
        {
            
            int sindex = stations.FindIndex(x => (x.Start.RouteCoordinate < RouteCoordinate)
            && (x.End.RouteCoordinate > RouteCoordinate) ||
            (x.Start.RouteCoordinate == RouteCoordinate) ||
            (x.End.RouteCoordinate == RouteCoordinate)
            );

            int sindexminus = stations.FindLastIndex(x => (x.End.RouteCoordinate < RouteCoordinate));
            int sindexplus = stations.FindIndex(x => (x.Start.RouteCoordinate > RouteCoordinate));

            if (sindex >= 0)// точка внутри границ станции
            {
                if ((stations[sindex].Start.RouteCoordinate == RouteCoordinate) ||
                    (stations[sindex].End.RouteCoordinate == RouteCoordinate) ||
                    ((stations[sindex].Start.RouteCoordinate < RouteCoordinate) && (stations[sindex].End.RouteCoordinate > RouteCoordinate)))
                {
                    this.StationID = stations[sindex].TrackObjectID;
                    this.station = stations[sindex].StationName;
                }
            }
            else if (
                (sindexplus >= 0) && (sindexminus >= 0)
               && (stations[sindexminus].End.RouteCoordinate < RouteCoordinate)
               && (stations[sindexplus].Start.RouteCoordinate > RouteCoordinate)
               && toNextStation
                    )// точка между станциями, ставим Id следующей станции (подходит для входных светофоров)
            {
                this.StationID = stations[sindexplus].TrackObjectID;
                this.station = stations[sindexplus].StationName;
            }
        }


        // проверить координаты 
        public string CheckCoordinate(List<PointOnTrack> kmpoints, List<Segment> segments)
        {
            int kmindex = kmpoints.FindIndex(x => (x.PointOnTrackKm == PointOnTrackKm) && (x.SegmentID == SegmentID) 
            && ((x.DicPointOnTrackKindID == 0) || (x.DicPointOnTrackKindID == 27) || (x.DicPointOnTrackKindID == 28) || (x.DicPointOnTrackKindID == 29)));



            int sindex = segments.FindIndex((Segment) => Segment.SegmentID == SegmentID);

            string message = "";
            int nextkmindex;
            if (segments[sindex].PredefinedRouteSegmentFromStartToEnd == 1)
                nextkmindex = kmindex + 1;
            else
                nextkmindex = kmindex - 1;

            if (kmindex < 0)
            {
                message = "КМ " + PointOnTrackKm + " не соответствует SegmentID " + SegmentID.ToString() + " \n";
            }
            else if (
                nextkmindex < kmpoints.Count()
                && nextkmindex >= 0
                && (((PointOnTrackPk - kmpoints[kmindex].PointOnTrackPk) * 100 + PointOnTrackM - kmpoints[kmindex].PointOnTrackM)
                    >= (kmpoints[nextkmindex].PointOnTrackCoordinate - kmpoints[kmindex].PointOnTrackCoordinate))
                && kmpoints[kmindex].SegmentID == kmpoints[nextkmindex].SegmentID
                && kmpoints[kmindex].PointOnTrackKm != kmpoints[nextkmindex].PointOnTrackKm)
            {
                double value1 = (PointOnTrackPk - kmpoints[kmindex].PointOnTrackPk) * 100 + PointOnTrackM - kmpoints[kmindex].PointOnTrackM;
                double value2 = kmpoints[nextkmindex].PointOnTrackCoordinate - kmpoints[kmindex].PointOnTrackCoordinate;
                message = " ошибочная координата:  \n" +
                          PointOnTrackKm + "км " + PointOnTrackPk.ToString() + "пк " + PointOnTrackM.ToString() + "м  " + "SegmentID " + SegmentID.ToString() + " " +
                          value1.ToString() + " > " +
                          value2.ToString() +
                          " \n"; //если жд координата за пределами данного километра
            }

            if (sindex < 0)
            {
                message = PointOnTrackKm + "км " + PointOnTrackPk.ToString() + "пк " + PointOnTrackM.ToString() + "м  " +
                          "SegmentID " + SegmentID.ToString() + " вне маршрута \n";
            }

            else if (PointOnTrackCoordinate > segments[sindex].SegmentLength)
            {
                message = "Координата точки больше длины сегмента: \n" +
                          PointOnTrackKm + "км " + PointOnTrackPk.ToString() + "пк " + PointOnTrackM.ToString() + "м  " +
                          PointOnTrackCoordinate.ToString() + " > " + segments[sindex].SegmentLength.ToString() + " " +
                          Math.Round(PointOnTrackCoordinate - segments[sindex].SegmentLength, 3).ToString() + "\n";
            }
            else if (PointOnTrackCoordinate < 0)
            {
                message = "Отрицательная координата: \n" +
                          PointOnTrackKm + "км " + PointOnTrackPk.ToString() + "пк " + PointOnTrackM.ToString() + "м  " +
                          PointOnTrackCoordinate.ToString() + "\n";
            }

            return message;
        }

        public static TrafficLight GetTrafficLightForPoint(PointOnTrack point, DbRoute _route)
        {
            // Находим светофор, соответствующий выбранной точке
            return _route.TrafficLights.FirstOrDefault(tl =>
                tl.Start.PointOntrackID == point.PointOntrackID ||
                tl.End.PointOntrackID == point.PointOntrackID);
        }
        public static CurrentKindChange GetCurrentKindChangeForPoint(PointOnTrack point, DbRoute _route)
        {
            // Находим точку смены рода тока, соответствующую выбранной точке
            return _route.CurrentKindChanges.FirstOrDefault(ck =>
                ck.Start.PointOntrackID == point.PointOntrackID);
        }

    }
}
