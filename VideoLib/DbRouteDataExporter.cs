using System;
using System.Collections.Generic;
using System.Data.OleDb;
using System.Globalization;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Controls;
using System.Windows;

namespace WpfApp04
{
    public class DbRouteDataExporter
    {
        //private readonly OleDbConnection _connection;
        private readonly DbRoute _toAddRoute;
        private readonly DbRoute _targetRoute;
        private List<PointOnTrack> _pointOnTracksToAdd = new List<PointOnTrack>();
        string _connectstring = string.Empty;


        public RouteExportCheckBoxList _routeExportCheckBoxList = new();
        
        public DbRouteDataExporter(string connectString, DbRoute toAddRoute, DbRoute targetRoute, List<PointOnTrack> pointOnTracksToAdd)
        {

            _connectstring = connectString;
            //_connection = new OleDbConnection(connectString);
            
            _toAddRoute = toAddRoute;
            _targetRoute = targetRoute;
            _pointOnTracksToAdd = pointOnTracksToAdd;
        }

        public void AddTrackObjectsFromDbRouteToBase()
        {

            // Внести в базу данных импортированные обьекты Insert
            
                    

            //найти наибольший TrackObjectId
            double TrackObjectID;
            TrackObjectID = SelectMaxTrackObjectFromBase(_connectstring);

            OleDbConnection _myConnection;
            
            _myConnection = new OleDbConnection(_connectstring);
            _myConnection.Open();

            //найти наибольший PointOntrackID
            string query3 = "SELECT MAX(PointOnTrackID) FROM PointOnTrack";
            OleDbCommand command3 = new OleDbCommand(query3, _myConnection);
            OleDbDataReader reader3 = command3.ExecuteReader();
            reader3.Read();
            double PointOnTrackID = Convert.ToDouble(reader3[0]);
            reader3.Close();



            //удалить рельсовые цепи из базы

            if (_routeExportCheckBoxList._DeleteTrackCircuitsChickBox is true)
            {


                DbRouteQuery.DeleteAllObjectsByKindConnected(_myConnection,"TrackCircuit",22,38);


                //string query7 = "DELETE " +
                //"FROM TrackCircuit ";

                //string query71 = "DELETE FROM TrackObject " +
                //    "WHERE DicTrackObjectKindID = 22 ";

                //string query72 = "DELETE FROM PointOnTrack " +
                //    "WHERE DicPointOnTrackKindID = 38 ";
                //OleDbCommand command7 = new OleDbCommand(query7, _myConnection);
                //OleDbCommand command71 = new OleDbCommand(query71, _myConnection);
                //OleDbCommand command72 = new OleDbCommand(query72, _myConnection);

                //command7.ExecuteNonQuery();
                //command71.ExecuteNonQuery();
                //command72.ExecuteNonQuery();

            }

            if (_routeExportCheckBoxList._ImportInclinesCheckBox is true)
            {
                foreach (Incline s in _toAddRoute.Inclines)
                {

                    int sindex = _targetRoute.Segments.FindIndex(x => (x.SegmentID == s.Start.SegmentID));
                    var seg = _targetRoute.Segments[sindex];

                    int tindex = _targetRoute.Tracks.FindIndex(x => (x.TrackID == seg.TrackID));
                    var tr = _targetRoute.Tracks[tindex];

                    // внести в таблицу TrackObject
                    TrackObjectID += 1;
                    string TrackObjectName = "Уклон на пути " + tr.TrackNumber + " " + tr.TrackName + " на " +
                        s.Start.PointOnTrackKm + " км " + s.Start.PointOnTrackPk + " пк " + s.Start.PointOnTrackM + " м";

                    string query = "INSERT INTO TrackObject ( TrackObjectID, DicTrackObjectKindID, TrackObjectName) " +
                        "VALUES (" + TrackObjectID + ", 10, '" + TrackObjectName + "' )";
                    OleDbCommand command = new OleDbCommand(query, _myConnection);
                    command.ExecuteNonQuery();

                    // внести в таблицу Incline

                    double valuetoinsert;

                    if (seg.PredefinedRouteSegmentFromStartToEnd == -1)
                        valuetoinsert = s.Value * (-1);
                    else valuetoinsert = s.Value;

                    string query2 = "INSERT INTO Incline (TrackObjectID, InclineValue) VALUES ( " +
                        TrackObjectID + ", '" + valuetoinsert + "')";
                    OleDbCommand command2 = new OleDbCommand(query2, _myConnection);
                    command2.ExecuteNonQuery();

                    double PointOnTrackUsageDirection;
                    double predefinedRouteSegmentFromStartToEnd;




                    if (sindex >= 0)
                    {

                        predefinedRouteSegmentFromStartToEnd = _targetRoute.Segments[sindex].PredefinedRouteSegmentFromStartToEnd;

                        if (predefinedRouteSegmentFromStartToEnd == 1) // если на этом отрезки возрастает километраж
                        {
                            PointOnTrackID += 1;
                            PointOnTrackUsageDirection = 1;
                            InsertPointOntrack(TrackObjectID, 32, PointOnTrackID, s.Start, PointOnTrackUsageDirection, _myConnection);

                            PointOnTrackID += 1;
                            PointOnTrackUsageDirection = -1;
                            InsertPointOntrack(TrackObjectID, 32, PointOnTrackID, s.End, PointOnTrackUsageDirection, _myConnection);

                        }
                        else
                        {
                            PointOnTrackID += 1;
                            PointOnTrackUsageDirection = 1;
                            InsertPointOntrack(TrackObjectID, 32, PointOnTrackID, s.End, PointOnTrackUsageDirection, _myConnection);

                            PointOnTrackID += 1;
                            PointOnTrackUsageDirection = -1;
                            InsertPointOntrack(TrackObjectID, 32, PointOnTrackID, s.Start, PointOnTrackUsageDirection, _myConnection);

                        }
                    }

                }

            }

            if (_routeExportCheckBoxList._ImportSpeedCheckBox is true)
            {
                foreach (SpeedRestriction s in _toAddRoute.SpeedRestrictions)
                {

                    int sindex = _targetRoute.Segments.FindIndex(x => (x.SegmentID == s.Start.SegmentID));

                    var seg = _targetRoute.Segments[sindex];

                    int tindex = _targetRoute.Tracks.FindIndex(x => (x.TrackID == seg.TrackID));

                    var tr = _targetRoute.Tracks[tindex];

                    // внести в таблицу TrackObject
                    TrackObjectID += 1;
                    string TrackObjectName = "огр. " + s.Value + " км/ч на " + s.Start.PointOnTrackKm + " км "
                                             + s.Start.PointOnTrackPk + " пк " + s.Start.PointOnTrackM.ToString("G", CultureInfo.InvariantCulture) + " м";

                    string query = "INSERT INTO TrackObject ( TrackObjectID, DicTrackObjectKindID, TrackObjectName) " +
                                   "VALUES (" + TrackObjectID + ", 8, '" + TrackObjectName + "' )";
                    OleDbCommand command = new OleDbCommand(query, _myConnection);
                    command.ExecuteNonQuery();

                    // внести ограничения скоростей в таблицу PermanentRestriction
                    string query2 = "INSERT INTO PermanentRestriction (TrackObjectID, PermanentRestrictionSpeed, " +
                        "PermRestrictionOnlyHeader, PermRestrictionForEmptyTrain) VALUES ( " + TrackObjectID + ", "
                        + s.Value + ", " + s.PermRestrictionOnlyHeader + ", " + s.PermRestrictionForEmptyTrain + " )";
                    OleDbCommand command2 = new OleDbCommand(query2, _myConnection);
                    command2.ExecuteNonQuery();

                    // внести ограничения скоростей в таблицу PointOntrack
                    double PointOnTrackUsageDirection;
                    double predefinedRouteSegmentFromStartToEnd;




                    if (sindex >= 0)
                    {

                        predefinedRouteSegmentFromStartToEnd = _targetRoute.Segments[sindex].PredefinedRouteSegmentFromStartToEnd;

                        if (predefinedRouteSegmentFromStartToEnd == 1) // если на этом отрезки возрастает километраж
                        {
                            PointOnTrackID += 1;
                            PointOnTrackUsageDirection = 1;
                            InsertPointOntrack(TrackObjectID, 2, PointOnTrackID, s.Start, PointOnTrackUsageDirection, _myConnection);

                            PointOnTrackID += 1;
                            PointOnTrackUsageDirection = -1;
                            InsertPointOntrack(TrackObjectID, 2, PointOnTrackID, s.End, PointOnTrackUsageDirection, _myConnection);
                        }
                        else
                        {
                            PointOnTrackID += 1;
                            PointOnTrackUsageDirection = 1;
                            InsertPointOntrack(TrackObjectID, 2, PointOnTrackID, s.End, PointOnTrackUsageDirection, _myConnection);

                            PointOnTrackID += 1;
                            PointOnTrackUsageDirection = -1;
                            InsertPointOntrack(TrackObjectID, 2, PointOnTrackID, s.Start, PointOnTrackUsageDirection, _myConnection);
                        }
                    }

                }

            }

            if (_routeExportCheckBoxList._ImportPlatformsCheckBox is true)
            {
                foreach (Platform s in _toAddRoute.Platforms)
                {
                    // внести в таблицу TrackObject
                    TrackObjectID += 1;
                    string TrackObjectName = "платформа " + s.PlatformName;

                    string query = "INSERT INTO TrackObject ( TrackObjectID, DicTrackObjectKindID, TrackObjectName) " +
                        "VALUES (" + TrackObjectID + ", 2, '" + TrackObjectName + "' )";
                    OleDbCommand command = new OleDbCommand(query, _myConnection);
                    command.ExecuteNonQuery();

                    // внести в таблицу Platform для платформ вне станций
                    string query2 = "INSERT INTO Platform (TrackObjectID, PlatformName) VALUES ( " +
                        TrackObjectID + ", '" + s.PlatformName + "')";

                    // внести в таблицу Platform для станционных платформ
                    string query22 = "INSERT INTO Platform (TrackObjectID, PlatformName, StationID) VALUES ( " +
                                    TrackObjectID + ", '" + s.PlatformName + "', '" + s.StationID + "')";


                    OleDbCommand command2 = new OleDbCommand(query2, _myConnection);
                    OleDbCommand command22 = new OleDbCommand(query22, _myConnection);


                    if (s.StationID != 0 && s.StationID != null) { command22.ExecuteNonQuery(); }
                    else { command2.ExecuteNonQuery(); }

                    double PointOnTrackUsageDirection;
                    double predefinedRouteSegmentFromStartToEnd;


                    int sindex = _targetRoute.Segments.FindIndex(x => (x.SegmentID == s.Start.SegmentID));

                    if (sindex >= 0)
                    {

                        predefinedRouteSegmentFromStartToEnd = _targetRoute.Segments[sindex].PredefinedRouteSegmentFromStartToEnd;

                        if (predefinedRouteSegmentFromStartToEnd == 1) // если на этом отрезки возрастает километраж
                        {
                            PointOnTrackID += 1;
                            PointOnTrackUsageDirection = 1;
                            InsertPointOntrack(TrackObjectID, s.Start.DicPointOnTrackKindID, PointOnTrackID, s.Start, PointOnTrackUsageDirection, _myConnection);

                            PointOnTrackID += 1;
                            PointOnTrackUsageDirection = -1;
                            InsertPointOntrack(TrackObjectID, s.Start.DicPointOnTrackKindID, PointOnTrackID, s.End, PointOnTrackUsageDirection, _myConnection);
                            // добавить точку остановки первого вагона
                            PointOnTrackID += 1;
                            InsertPointOntrack(TrackObjectID, 26, PointOnTrackID, s.End, PointOnTrackUsageDirection, _myConnection);
                        }
                        else
                        {
                            PointOnTrackID += 1;
                            PointOnTrackUsageDirection = 1;
                            InsertPointOntrack(TrackObjectID, s.Start.DicPointOnTrackKindID, PointOnTrackID, s.End, PointOnTrackUsageDirection, _myConnection);

                            PointOnTrackID += 1;
                            PointOnTrackUsageDirection = -1;
                            InsertPointOntrack(TrackObjectID, s.Start.DicPointOnTrackKindID, PointOnTrackID, s.Start, PointOnTrackUsageDirection, _myConnection);//s.Start.DicPointOnTrackKindID = 9
                                                                                                                                                                 // добавить точку остановки первого вагона
                            PointOnTrackID += 1;
                            InsertPointOntrack(TrackObjectID, 26, PointOnTrackID, s.End, PointOnTrackUsageDirection, _myConnection);

                        }
                    }

                }

            }

            if (_routeExportCheckBoxList._ImportNeutralSectionsCheckBox is true)
            {
                foreach (NeutralSection s in _toAddRoute.NeutralSections)
                {

                    int sindex = _targetRoute.Segments.FindIndex(x => (x.SegmentID == s.Start.SegmentID));

                    var seg = _targetRoute.Segments[sindex];

                    int tindex = _targetRoute.Tracks.FindIndex(x => (x.TrackID == seg.TrackID));

                    var tr = _targetRoute.Tracks[tindex];

                    // внести в таблицу TrackObject
                    TrackObjectID += 1;
                    string TrackObjectName = "нейтральная вставка на пути " + tr.TrackNumber + " " + tr.TrackName + " на " +
                        s.StartPointOnTrackKm + " км " + s.StartPointOnTrackPk + " пк " + s.StartPointOnTrackM + " м";

                    string query = "INSERT INTO TrackObject ( TrackObjectID, DicTrackObjectKindID, TrackObjectName) " +
                        "VALUES (" + TrackObjectID + ", 19, '" + TrackObjectName + "' )";
                    OleDbCommand command = new OleDbCommand(query, _myConnection);
                    command.ExecuteNonQuery();

                    double PointOnTrackUsageDirection;
                    double predefinedRouteSegmentFromStartToEnd;

                    if (sindex >= 0)
                    {

                        predefinedRouteSegmentFromStartToEnd = _targetRoute.Segments[sindex].PredefinedRouteSegmentFromStartToEnd;

                        if (predefinedRouteSegmentFromStartToEnd == 1) // если на этом отрезки возрастает километраж
                        {
                            PointOnTrackID += 1;
                            PointOnTrackUsageDirection = 1;
                            InsertPointOntrack(TrackObjectID, s.Start.DicPointOnTrackKindID, PointOnTrackID, s.Start, PointOnTrackUsageDirection, _myConnection);

                            PointOnTrackID += 1;
                            PointOnTrackUsageDirection = -1;
                            InsertPointOntrack(TrackObjectID, s.Start.DicPointOnTrackKindID, PointOnTrackID, s.End, PointOnTrackUsageDirection, _myConnection);


                        }
                        else
                        {
                            PointOnTrackID += 1;
                            PointOnTrackUsageDirection = 1;
                            InsertPointOntrack(TrackObjectID, s.Start.DicPointOnTrackKindID, PointOnTrackID, s.End, PointOnTrackUsageDirection, _myConnection);

                            PointOnTrackID += 1;
                            PointOnTrackUsageDirection = -1;
                            InsertPointOntrack(TrackObjectID, s.Start.DicPointOnTrackKindID, PointOnTrackID, s.Start, PointOnTrackUsageDirection, _myConnection);//s.Start.DicPointOnTrackKindID = 9


                        }
                    }

                }

            }


            if ((_routeExportCheckBoxList._ImportUkspsCheckBox == true) && (_toAddRoute.UkspsList.Count > 0))
            {
                foreach (Uksps s in _toAddRoute.UkspsList)
                {

                    int sindex = _targetRoute.Segments.FindIndex(x => (x.SegmentID == s.Start.SegmentID));

                    var seg = _targetRoute.Segments[sindex];

                    int tindex = _targetRoute.Tracks.FindIndex(x => (x.TrackID == seg.TrackID));

                    var tr = _targetRoute.Tracks[tindex];

                    // внести в таблицу TrackObject
                    TrackObjectID += 1;
                    string TrackObjectName = "УКСПС  на пути " + tr.TrackNumber + " " + tr.TrackName + " на " +
                        s.StartPointOnTrackKm + " км " + s.StartPointOnTrackPk + " пк " + s.StartPointOnTrackM + " м";

                    string query = "INSERT INTO TrackObject ( TrackObjectID, DicTrackObjectKindID, TrackObjectName) " +
                        "VALUES (" + TrackObjectID + ", 16, '" + TrackObjectName + "' )";
                    OleDbCommand command = new OleDbCommand(query, _myConnection);
                    command.ExecuteNonQuery();



                    // внести  PointOntrack
                    double PointOnTrackUsageDirection;
                    double predefinedRouteSegmentFromStartToEnd;




                    if (sindex >= 0)
                    {

                        predefinedRouteSegmentFromStartToEnd = _targetRoute.Segments[sindex].PredefinedRouteSegmentFromStartToEnd;

                        if (predefinedRouteSegmentFromStartToEnd == 1) // если на этом отрезки возрастает километраж
                        {
                            PointOnTrackID += 1;
                            PointOnTrackUsageDirection = 0;
                            InsertPointOntrack(TrackObjectID, 25, PointOnTrackID, s.Start, PointOnTrackUsageDirection, _myConnection);

                        }
                        else
                        {
                            PointOnTrackID += 1;
                            PointOnTrackUsageDirection = 0;
                            InsertPointOntrack(TrackObjectID, 25, PointOnTrackID, s.Start, PointOnTrackUsageDirection, _myConnection);
                        }
                    }
                }
            }

            if ((_routeExportCheckBoxList._ImportKtsmCheckBox == true) && (_toAddRoute.KtsmList.Count > 0))
            {
                foreach (Ktsm s in _toAddRoute.KtsmList)
                {

                    int sindex = _targetRoute.Segments.FindIndex(x => (x.SegmentID == s.Start.SegmentID));
                    var seg = _targetRoute.Segments[sindex];
                    int tindex = _targetRoute.Tracks.FindIndex(x => (x.TrackID == seg.TrackID));
                    var tr = _targetRoute.Tracks[tindex];

                    // внести в таблицу TrackObject
                    TrackObjectID += 1;
                    string TrackObjectName = "КТСМ на пути " + tr.TrackNumber + " " + tr.TrackName + " на " +
                        s.StartPointOnTrackKm + " км " + s.StartPointOnTrackPk + " пк " + s.StartPointOnTrackM + " м";

                    string query = "INSERT INTO TrackObject ( TrackObjectID, DicTrackObjectKindID, TrackObjectName) " +
                        "VALUES (" + TrackObjectID + ", 15, '" + TrackObjectName + "' )";
                    OleDbCommand command = new OleDbCommand(query, _myConnection);
                    command.ExecuteNonQuery();

                    // внести  PointOntrack
                    double PointOnTrackUsageDirection;
                    double predefinedRouteSegmentFromStartToEnd;


                    if (sindex >= 0)
                    {
                        PointOnTrackID += 1;
                        PointOnTrackUsageDirection = 0;
                        InsertPointOntrack(TrackObjectID, 24, PointOnTrackID, s.Start, PointOnTrackUsageDirection, _myConnection);
                    }
                }
            }

            if ((_routeExportCheckBoxList._ImportTrafficLightsCheckBox == true) && (_toAddRoute.TrafficLights.Count > 0))
            {
                foreach (TrafficLight s in _toAddRoute.TrafficLights)
                {

                    // внести светофор в таблицу TrackObject
                    TrackObjectID += 1;

                    InsertTrafficlight(s, TrackObjectID, _myConnection);

                    if (s.TliRestrictions.Count > 0)
                    {
                        foreach (TliRestriction tli in s.TliRestrictions)
                        {
                            InsertTliRestriction(tli, TrackObjectID, _myConnection);
                        }
                    }

                    // внести  в таблицу PointOntrack
                    double PointOnTrackUsageDirection;
                    double predefinedRouteSegmentFromStartToEnd;

                    int sindex = _targetRoute.Segments.FindIndex(x => (x.SegmentID == s.Start.SegmentID));

                    if (sindex >= 0)
                    {

                        predefinedRouteSegmentFromStartToEnd = _targetRoute.Segments[sindex].PredefinedRouteSegmentFromStartToEnd;

                        if (predefinedRouteSegmentFromStartToEnd == 1) // если на этом отрезки возрастает километраж
                        {
                            PointOnTrackID += 1;
                            PointOnTrackUsageDirection = 1;
                            InsertPointOntrack(TrackObjectID, 1, PointOnTrackID, s.Start, PointOnTrackUsageDirection, _myConnection);

                        }
                        else
                        {
                            PointOnTrackID += 1;
                            PointOnTrackUsageDirection = -1;
                            InsertPointOntrack(TrackObjectID, 1, PointOnTrackID, s.Start, PointOnTrackUsageDirection, _myConnection);
                        }
                    }
                }
            }

            if ((_routeExportCheckBoxList._ImportSignsCheckBox == true) && (_toAddRoute.TrafficSignals.Count > 0))
            {
                foreach (TrafficSignal s in _toAddRoute.TrafficSignals)
                {

                    // внести сигнальный знак в таблицу TrackObject
                    int sindex = _targetRoute.Segments.FindIndex(x => (x.SegmentID == s.Start.SegmentID));
                    var seg = _targetRoute.Segments[sindex];
                    int tindex = _targetRoute.Tracks.FindIndex(x => (x.TrackID == seg.TrackID));
                    var tr = _targetRoute.Tracks[tindex];
                    string signName = "";

                    if (s.DicTrafficSignalKindID == 21)
                    { signName = "Подача свистка"; }


                    // внести в таблицу TrackObject
                    TrackObjectID += 1;
                    string TrackObjectName = signName + tr.TrackNumber + " " + tr.TrackName + " на " +
                        s.StartPointOnTrackKm + " км " + s.StartPointOnTrackPk + " пк " + s.StartPointOnTrackM + " м";

                    string query = "INSERT INTO TrackObject ( TrackObjectID, DicTrackObjectKindID, TrackObjectName) " +
                        "VALUES (" + TrackObjectID + ", 21, '" + TrackObjectName + "' )";
                    OleDbCommand command = new OleDbCommand(query, _myConnection);
                    command.ExecuteNonQuery();

                    // внести TrafficSignal
                    string query2 = "INSERT INTO TrafficSignal (TrackObjectID, DicTrafficSignalKindID) " +
                        "VALUES (" + TrackObjectID + ", '" + s.DicTrafficSignalKindID + "' )";
                    OleDbCommand command2 = new OleDbCommand(query2, _myConnection);
                    command2.ExecuteNonQuery();


                    // внести в таблицу PointOntrack
                    double PointOnTrackUsageDirection;
                    double predefinedRouteSegmentFromStartToEnd;


                    if (sindex >= 0)
                    {

                        predefinedRouteSegmentFromStartToEnd = _targetRoute.Segments[sindex].PredefinedRouteSegmentFromStartToEnd;

                        if (predefinedRouteSegmentFromStartToEnd == 1) // если на этом отрезки возрастает километраж
                        {
                            PointOnTrackID += 1;
                            PointOnTrackUsageDirection = 1;
                            InsertPointOntrack(TrackObjectID, 37, PointOnTrackID, s.Start, PointOnTrackUsageDirection, _myConnection);

                        }
                        else
                        {
                            PointOnTrackID += 1;
                            PointOnTrackUsageDirection = -1;
                            InsertPointOntrack(TrackObjectID, 37, PointOnTrackID, s.Start, PointOnTrackUsageDirection, _myConnection);
                        }
                    }
                }
            }

            if ((_routeExportCheckBoxList._ImportCurrentKindChangeCheckBox == true) && (_toAddRoute.CurrentKindChanges.Count > 0))
            {
                if(_toAddRoute == _targetRoute) { DbRouteQuery.DeleteAllObjectsByKindConnected(_myConnection,"CurrentKindChange",24,40);}

                ExportCurrentKindChanges(_myConnection, _toAddRoute, _targetRoute, ref TrackObjectID, ref PointOnTrackID);
            }


            if ((_routeExportCheckBoxList._ImportCrossingsCheckBox == true) && (_toAddRoute.Crossings.Count > 0))
            {
                foreach (Crossing s in _toAddRoute.Crossings)
                {

                    // внести переезд в таблицу TrackObject
                    int sindex = _targetRoute.Segments.FindIndex(x => (x.SegmentID == s.Start.SegmentID));
                    var seg = _targetRoute.Segments[sindex];
                    int tindex = _targetRoute.Tracks.FindIndex(x => (x.TrackID == seg.TrackID));
                    var tr = _targetRoute.Tracks[tindex];
                    string signName = "";

                    if (s.DicCrossingKindID == 1)
                    { signName = "Переезд охраняемый"; }
                    else
                    { signName = "Переезд неохраняемый"; }


                    // внести в таблицу TrackObject
                    TrackObjectID += 1;
                    string TrackObjectName = signName + tr.TrackNumber + " " + tr.TrackName + " на " +
                        s.StartPointOnTrackKm + " км " + s.StartPointOnTrackPk + " пк " + s.StartPointOnTrackM + " м";

                    string query = "INSERT INTO TrackObject ( TrackObjectID, DicTrackObjectKindID, TrackObjectName) " +
                        "VALUES (" + TrackObjectID + ", 9, '" + TrackObjectName + "' )";
                    OleDbCommand command = new OleDbCommand(query, _myConnection);
                    command.ExecuteNonQuery();

                    // внести Crossing
                    string query2 = "INSERT INTO Crossing (TrackObjectID, DicCrossingKindID, CanPutObstacle) " +
                        "VALUES (" + TrackObjectID + ", '" + s.DicCrossingKindID + "', 0)";
                    OleDbCommand command2 = new OleDbCommand(query2, _myConnection);
                    command2.ExecuteNonQuery();


                    // внести в таблицу PointOntrack
                    double PointOnTrackUsageDirection;
                    double predefinedRouteSegmentFromStartToEnd;


                    if (sindex >= 0)
                    {

                        predefinedRouteSegmentFromStartToEnd = _targetRoute.Segments[sindex].PredefinedRouteSegmentFromStartToEnd;

                        if (predefinedRouteSegmentFromStartToEnd == 1) // если на этом отрезки возрастает километраж
                        {
                            PointOnTrackID += 1;
                            PointOnTrackUsageDirection = 1;
                            InsertPointOntrack(TrackObjectID, 23, PointOnTrackID, s.Start, PointOnTrackUsageDirection, _myConnection);

                        }
                        else
                        {
                            PointOnTrackID += 1;
                            PointOnTrackUsageDirection = -1;
                            InsertPointOntrack(TrackObjectID, 23, PointOnTrackID, s.Start, PointOnTrackUsageDirection, _myConnection);
                        }
                    }
                }
            }

            if (_routeExportCheckBoxList._ImportStationBordersCheckBox == true)
            {
                foreach (PointOnTrack p in _pointOnTracksToAdd)
                {

                    int sindex = _targetRoute.Segments.FindIndex(x => (x.SegmentID == p.SegmentID));
                    // внести в таблицу PointOntrack
                    double PointOnTrackUsageDirection;
                    double predefinedRouteSegmentFromStartToEnd;

                    //if (sindex >= 0)
                    if (sindex >= 0 && p.DicPointOnTrackKindID == 8)
                    {
                        PointOnTrackID += 1;
                        InsertPointOntrack(p.TrackObjectID, p.DicPointOnTrackKindID, PointOnTrackID, p, p.PointOnTrackUsageDirection, _myConnection);

                    }
                }
            }

            _myConnection.Close();
            MessageBox.Show(

                "объекты добавлены " +
                " \n точки на пути: " + _pointOnTracksToAdd.Count.ToString() +
                " \n светофоры: " + _toAddRoute.TrafficLights.Count.ToString() +
                " \n платформы : " + _toAddRoute.Platforms.Count.ToString() +
                " \n сигнальные знаки : " + _toAddRoute.TrafficSignals.Count.ToString() +
                " \n переезды : " + _toAddRoute.Crossings.Count.ToString() +
                " \n укспс : " + _toAddRoute.UkspsList.Count.ToString() +
                " \n ктсм : " + _toAddRoute.KtsmList.Count.ToString() +
                " \n нейтральная вставка : " + _toAddRoute.NeutralSections.Count.ToString() +
                " \n скорости : " + _toAddRoute.SpeedRestrictions.Count.ToString() +
                " \n уклоны : " + _toAddRoute.Inclines.Count.ToString(), " AddMessageBox");
        }

        public static void InsertPointOntrack(double TrackObjectID, double DicPointOnTrackKindID, double PointOnTrackID, PointOnTrack p, double PointOnTrackUsageDirection, OleDbConnection _myConnection)
        {
            string query4 = "INSERT INTO PointOnTrack ( PointOnTrackID, DicPointOnTrackKindID, TrackObjectID, SegmentID, PointOnTrackCoordinate, PointOnTrackKm, PointOnTrackPk, PointOnTrackM, PointOnTrackUsageDirection ) " +
            "VALUES (" + PointOnTrackID + ", " + DicPointOnTrackKindID + ", " + TrackObjectID + ", " + p.SegmentID + ", " + p.PointOnTrackCoordinate.ToString("G", CultureInfo.InvariantCulture) + ", " + p.PointOnTrackKm + ", " + p.PointOnTrackPk.ToString("G", CultureInfo.InvariantCulture) + ", " + p.PointOnTrackM.ToString("G", CultureInfo.InvariantCulture) + ", " + PointOnTrackUsageDirection.ToString("G", CultureInfo.InvariantCulture) + " )";
            OleDbCommand command4 = new OleDbCommand(query4, _myConnection);
            command4.ExecuteNonQuery();
        }
        public static void InsertTrafficlight(TrafficLight s, double TrackObjectID, OleDbConnection _myConnection)
        {
            // внести светофор в таблицы TrackObjects и TrafficLight

            string TrackObjectName = "свет. " + s.TrafficLightName;

            string query = "INSERT INTO TrackObject ( TrackObjectID, DicTrackObjectKindID, TrackObjectName) " +
                           "VALUES (" + TrackObjectID + ", 5, '" + TrackObjectName + "' )";
            OleDbCommand command = new OleDbCommand(query, _myConnection);
            command.ExecuteNonQuery();



            // внести светофор в таблицу TrafficLight StationID для станционных светофоров
            if (s.DicTrafficLightKindID == 1
                || s.DicTrafficLightKindID == 2
                || s.DicTrafficLightKindID == 3
                || s.DicTrafficLightKindID == 7
                || s.DicTrafficLightKindID == 8
                || s.DicTrafficLightKindID == 11
                || s.DicTrafficLightKindID == 14
                || s.DicTrafficLightKindID == 15
                || s.DicTrafficLightKindID == 16
                || s.DicTrafficLightKindID == 22
               )
            {
                string query23 = "INSERT INTO TrafficLight (TrackObjectID, TrafficLightName, " +
                                 "DicKPTKindID, DicTrafficLightKindID, StationID, TrafficLightTransasID) VALUES ( " +
                                 TrackObjectID + ", '" + s.TrafficLightName + "', 2, " + s.DicTrafficLightKindID + ", " + s.StationID + ", " + TrackObjectID + " )";
                OleDbCommand command23 = new OleDbCommand(query23, _myConnection);
                command23.ExecuteNonQuery();
            }
            else
            {
                // внести светофор в таблицу TrafficLight для остальных светофоров

                string query2 = "INSERT INTO TrafficLight (TrackObjectID, TrafficLightName, " +
                                "DicKPTKindID, DicTrafficLightKindID, TrafficLightTransasID) VALUES ( " +
                                TrackObjectID + ", '" + s.TrafficLightName + "', 2, " + s.DicTrafficLightKindID + ", " + TrackObjectID + " )";
                OleDbCommand command2 = new OleDbCommand(query2, _myConnection);
                command2.ExecuteNonQuery();
            }
        }

        public static void InsertTliRestriction(TliRestriction tli, double TrackObjectID, OleDbConnection _myConnection)
        {

            string querycode = "INSERT INTO TrafficLightSpeedRestriction ( " +
                               "TrafficLightID" +
                               ", RestrictionKind" +
                               ", BlockCount" +
                               ", Speed" +
                               ", ShowBlockCount" +
                               ", AutoBlockCode" +
                               " ) " +
                               "VALUES ("
                               + TrackObjectID + ", "
                               + (int)tli.kind + ", "
                               + tli.blockCount + ", "
                               + tli.speed + ", "
                               + tli.showBlockCount
                               + ", "
                               + (int)tli.autoBlockCode
                               + " )";

            string querynocode = "INSERT INTO TrafficLightSpeedRestriction ( " +
                                 "TrafficLightID" +
                                 ", RestrictionKind" +
                                 ", BlockCount" +
                                 ", Speed" +
                                 ", ShowBlockCount" +
                                 " ) " +
                                 "VALUES ("
                                 + TrackObjectID + ", "
                                 + (int)tli.kind + ", "
                                 + tli.blockCount + ", "
                                 + tli.speed + ", "
                                 + tli.showBlockCount
                                 + " )";
            string query;

            if (tli.autoBlockCode == AutoBlockInternalControlCode.Dummy)
            {
                query = querynocode;
            }
            else
            {
                query = querycode;
            }

            OleDbCommand command = new OleDbCommand(query, _myConnection);
            command.ExecuteNonQuery();
        }

        public static double SelectMaxTrackObjectFromBaseConnection(OleDbConnection _connection)
        {
            //найти наибольший TrackObjectId OleDbConnection
            string query1 = "SELECT MAX(TrackObjectID) FROM TrackObject";
            OleDbCommand command1 = new OleDbCommand(query1, _connection);
            OleDbDataReader reader = command1.ExecuteReader();
            reader.Read();
            double TrackObjectID;
            if (reader[0].ToString() != "")
            {
                TrackObjectID = Convert.ToDouble(reader[0]);
            }
            else
            {
                TrackObjectID = 0;
            }
            reader.Close();
            return TrackObjectID;
        }

        public static double SelectMaxTrackObjectFromBase(string connectionString)
        {
            //найти наибольший TrackObjectId  connectionString
            double trackObjectId = 0;

            using (OleDbConnection connection = new OleDbConnection(connectionString))
            {
                string query = "SELECT MAX(TrackObjectID) FROM TrackObject";
                OleDbCommand command = new OleDbCommand(query, connection);

                try
                {
                    connection.Open();
                    object result = command.ExecuteScalar();  // ExecuteScalar возвращает первый столбец первой строки

                    if (result != null && result != DBNull.Value)
                    {
                        trackObjectId = Convert.ToDouble(result);
                    }
                }
                catch (Exception ex)
                {
                    // Логирование ошибки (по желанию)
                    Console.WriteLine($"Ошибка при получении максимального TrackObjectID: {ex.Message}");
                    trackObjectId = 0;  // Возвращаем 0 в случае ошибки
                }
            }

            return trackObjectId;
        }

        public static double SelectMaxTrackObjectFromBaseConnected(OleDbConnection connection)
        {
            //найти наибольший TrackObjectId  connectionString
            double trackObjectId = 0;

            
                string query = "SELECT MAX(TrackObjectID) FROM TrackObject";
                OleDbCommand command = new OleDbCommand(query, connection);

                try
                {
                    connection.Open();
                    object result = command.ExecuteScalar();  // ExecuteScalar возвращает первый столбец первой строки

                    if (result != null && result != DBNull.Value)
                    {
                        trackObjectId = Convert.ToDouble(result);
                    }
                }
                catch (Exception ex)
                {
                    // Логирование ошибки (по желанию)
                    Console.WriteLine($"Ошибка при получении максимального TrackObjectID: {ex.Message}");
                    trackObjectId = 0;  // Возвращаем 0 в случае ошибки
                }
            

            return trackObjectId;
        }

        public static double SelectMaxPointOntrackFromBaseConnected(OleDbConnection connection)
        {
            //найти наибольший pointOnTrackId исполльзуя OleDbConnection
            double pointOnTrackId = 0;


            string query = "SELECT MAX(PointOnTrackID) FROM PointOnTrack";
            OleDbCommand command = new OleDbCommand(query, connection);

            try
            {
                connection.Open();
                object result = command.ExecuteScalar();  // ExecuteScalar возвращает первый столбец первой строки

                if (result != null && result != DBNull.Value)
                {
                    pointOnTrackId = Convert.ToDouble(result);
                }
            }
            catch (Exception ex)
            {
                // Логирование ошибки (по желанию)
                Console.WriteLine($"Ошибка при получении максимального pointOnTrackId: {ex.Message}");
                pointOnTrackId = 0;  // Возвращаем 0 в случае ошибки
            }


            return pointOnTrackId;
        }

        public static void ExportCurrentKindChanges(OleDbConnection _myConnection, DbRoute _toAddRoute, DbRoute _targetRoute,ref double TrackObjectID,ref double PointOnTrackID)
        {

            //double TrackObjectID = SelectMaxTrackObjectFromBaseConnected(_myConnection);
            //double PointOnTrackID = SelectMaxPointOntrackFromBaseConnected(_myConnection);


            foreach (CurrentKindChange s in _toAddRoute.CurrentKindChanges)
            {
                // точка смены рода тока
                // создать таблицу CurrentKindChange
                try
                {
                    string query72 = "CREATE TABLE CurrentKindChange(TrackObjectID int NOT NULL PRIMARY KEY, DicCurrentKindIDLeft int NOT NULL, DicCurrentKindIDRight int NOT NULL );";
                    OleDbCommand command72 = new OleDbCommand(query72, _myConnection);
                    command72.ExecuteNonQuery();
                }
                catch (Exception ex)
                {
                    MessageBox.Show(ex.Message);
                }

                // внести в таблицу TrackObject

                int sindex = _targetRoute.Segments.FindIndex(x => (x.SegmentID == s.Start.SegmentID));
                var seg = _targetRoute.Segments[sindex];
                int tindex = _targetRoute.Tracks.FindIndex(x => (x.TrackID == seg.TrackID));
                var tr = _targetRoute.Tracks[tindex];



                // внести в таблицу TrackObject
                TrackObjectID += 1;
                string TrackObjectName = "Токораздел на пути " + tr.TrackNumber + " " + tr.TrackName + " на " +
                                         s.StartPointOnTrackKm + " км " + s.StartPointOnTrackPk + " пк " + s.StartPointOnTrackM + " м";

                string query = "INSERT INTO TrackObject ( TrackObjectID, DicTrackObjectKindID, TrackObjectName) " +
                    "VALUES (" + TrackObjectID + ", 24, '" + TrackObjectName + "' )";
                OleDbCommand command = new OleDbCommand(query, _myConnection);
                command.ExecuteNonQuery();

                // внести CurrentKindChange
                string query2 = "INSERT INTO CurrentKindChange (TrackObjectID, DicCurrentKindIDLeft, DicCurrentKindIDRight) " +
                    "VALUES (" + TrackObjectID + ", '" + s.DicCurrentKindIDLeft + "', '" + s.DicCurrentKindIDRight + "' )";
                OleDbCommand command2 = new OleDbCommand(query2, _myConnection);
                command2.ExecuteNonQuery();


                double PointOnTrackUsageDirection;
                double predefinedRouteSegmentFromStartToEnd;


                if (sindex >= 0)
                {

                    predefinedRouteSegmentFromStartToEnd = _targetRoute.Segments[sindex].PredefinedRouteSegmentFromStartToEnd;

                    if (predefinedRouteSegmentFromStartToEnd == 1) // если на этом отрезки возрастает километраж
                    {
                        PointOnTrackID += 1;
                        PointOnTrackUsageDirection = 1;
                        InsertPointOntrack(TrackObjectID, 40, PointOnTrackID, s.Start, PointOnTrackUsageDirection, _myConnection);

                    }
                    else
                    {
                        PointOnTrackID += 1;
                        PointOnTrackUsageDirection = -1;
                        InsertPointOntrack(TrackObjectID, 40, PointOnTrackID, s.Start, PointOnTrackUsageDirection, _myConnection);
                    }
                }
            }
        }


        public static void SaveSpeedRestrictions(string connectstring, DbRoute route)
        {
            //сохранить в базу только ограничения скорости

            string _connectstring = connectstring;

            OleDbConnection _myConnection;

            _myConnection = new OleDbConnection(_connectstring);
            _myConnection.Open();
            //удалить ограничения скорости из базы

            string query7 = "DELETE " +
                            "FROM PermanentRestriction ";

            string query71 = "DELETE FROM TrackObject " +
                             "WHERE DicTrackObjectKindID = 8 ";

            string query72 = "DELETE FROM PointOnTrack " +
                             "WHERE DicPointOnTrackKindID = 2 ";
            OleDbCommand command7 = new OleDbCommand(query7, _myConnection);
            OleDbCommand command71 = new OleDbCommand(query71, _myConnection);
            OleDbCommand command72 = new OleDbCommand(query72, _myConnection);

            command7.ExecuteNonQuery();
            command71.ExecuteNonQuery();
            command72.ExecuteNonQuery();

            //}
            //if ((myConnection != null) && (SpeedRestrictions.Count() > 0))
            //{

            if (route.SpeedRestrictions.Count > 0)
            {

                //найти наибольший TrackObjectId
                string query1 = "SELECT MAX(TrackObjectID) FROM TrackObject";
                OleDbCommand command1 = new OleDbCommand(query1, _myConnection);
                OleDbDataReader reader = command1.ExecuteReader();
                reader.Read();
                double TrackObjectID = Convert.ToDouble(reader[0]);
                reader.Close();

                //найти наибольший PointOntrackID
                string query3 = "SELECT MAX(PointOnTrackID) FROM PointOnTrack";
                OleDbCommand command3 = new OleDbCommand(query3, _myConnection);
                OleDbDataReader reader3 = command3.ExecuteReader();
                reader3.Read();
                double PointOnTrackID = Convert.ToDouble(reader3[0]);
                reader3.Close();



                foreach (SpeedRestriction s in route.SpeedRestrictions)
                {

                    // внести ограничения скорости в таблицу TrackObject
                    TrackObjectID += 1;
                    string TrackObjectName = "огр. " + s.Value + " км/ч на " + s.Start.PointOnTrackKm + " км "
                                             + s.Start.PointOnTrackPk + " пк " +
                                             s.Start.PointOnTrackM.ToString("G", CultureInfo.InvariantCulture) + " м";

                    string query = "INSERT INTO TrackObject ( TrackObjectID, DicTrackObjectKindID, TrackObjectName) " +
                                   "VALUES (" + TrackObjectID + ", 8, '" + TrackObjectName + "' )";
                    OleDbCommand command = new OleDbCommand(query, _myConnection);
                    command.ExecuteNonQuery();

                    // внести ограничения скоростей в таблицу PermanentRestriction
                    string query2 = "INSERT INTO PermanentRestriction (TrackObjectID, PermanentRestrictionSpeed, " +
                                    "PermRestrictionOnlyHeader, PermRestrictionForEmptyTrain) VALUES ( " +
                                    TrackObjectID + ", "
                                    + s.Value + ", " + s.PermRestrictionOnlyHeader + ", " +
                                    s.PermRestrictionForEmptyTrain + " )";
                    OleDbCommand command2 = new OleDbCommand(query2, _myConnection);
                    command2.ExecuteNonQuery();

                    // внести ограничения скоростей в таблицу PointOntrack
                    double PointOnTrackUsageDirection;
                    double predefinedRouteSegmentFromStartToEnd;


                    int sindex = route.Segments.FindIndex(x => (x.SegmentID == s.Start.SegmentID));

                    if (sindex >= 0)
                    {

                        predefinedRouteSegmentFromStartToEnd =
                            route.Segments[sindex].PredefinedRouteSegmentFromStartToEnd;

                        if (predefinedRouteSegmentFromStartToEnd == 1) // если на этом отрезки возрастает километраж
                        {
                            PointOnTrackID += 1;
                            PointOnTrackUsageDirection = 1;
                            DbRouteDataExporter.InsertPointOntrack(TrackObjectID, 2, PointOnTrackID, s.Start,
                                PointOnTrackUsageDirection, _myConnection);

                            PointOnTrackID += 1;
                            PointOnTrackUsageDirection = -1;
                            DbRouteDataExporter.InsertPointOntrack(TrackObjectID, 2, PointOnTrackID, s.End,
                                PointOnTrackUsageDirection, _myConnection);
                        }
                        else
                        {
                            PointOnTrackID += 1;
                            PointOnTrackUsageDirection = 1;
                            DbRouteDataExporter.InsertPointOntrack(TrackObjectID, 2, PointOnTrackID, s.End,
                                PointOnTrackUsageDirection, _myConnection);

                            PointOnTrackID += 1;
                            PointOnTrackUsageDirection = -1;
                            DbRouteDataExporter.InsertPointOntrack(TrackObjectID, 2, PointOnTrackID, s.Start,
                                PointOnTrackUsageDirection, _myConnection);
                        }
                    }
                }
            }
        }



    }
}
