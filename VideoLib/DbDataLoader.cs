using System;
using System.Collections.Generic;
using System.Data.OleDb;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Controls;

namespace WpfApp04
{
    public class DbDataLoader
    {
        public OleDbConnection _connection;
        private readonly DbRoute _route;
        private readonly PointOnTrackComparer _pcr = new PointOnTrackComparer();
        private readonly SpeedComparerToshow _scts = new SpeedComparerToshow();
        private readonly InclineComparer _inclc = new InclineComparer();
        private readonly StationComparer _stationsByRoute = new StationComparer();

        public DbDataLoader(string connectionString, DbRoute route)
        {
            _connection = new OleDbConnection(connectionString);
            _route = route;
        }

        public void LoadData()
        {
            _connection.Open();
            try
            {
                LoadSegmentsFromBase();
                LoadPointOnTracksFromBase();

                LoadTrackObjectsFromBase();

                FillSegmentsStartEnd();
                _route.PointOnTracks.Sort(_pcr);
                LoadTracks();
                LoadStationsFromBase();
                LoadPlatformsFromBase();
                FillStationPointOntracks(_route.PointOnTracks, _route.Segments, _route.Stations);
                FillPlatformPointOntracks(_route.PointOnTracks, _route.Segments, _route.Platforms);
                _route.Stations.Sort(_stationsByRoute);
                LoadInclines();
                FillInclinesPointOntracks();
                FillInclinesElevation(_route.Inclines);
                foreach (PointOnTrack p in _route.PointOnTracks) { p.FillElevation(_route.Inclines); }
                foreach (PointOnTrack p in _route.PointOnTracks) { p.FillStation(_route.Stations); }
                LoadAlsDevice();
                LoadSpeedRestrictionsFromBase();
                FillSpeedRestrictionsPointOntracks();
                _route.SpeedRestrictions.Sort(_scts);
                LoadCrossingPiecesFromBase();
                FillCrossingPiecesPointOntracks();
                LoadTrafficLights();
                FillTrafficLightsPointOntracks();
                LoadTliRestrictions();

                LoadTrafficLightLamps(_connection, _route);


                LoadTrackCircuits();
                FillTrackCircuitsPointOntracks();
                FillTrackCircuitsAls();
                FillKilometers(_route.PointOnTracks, _route.Segments);
                LoadUncodedTracks();
                FillUncodedTracksPointOntracks();
                LoadTrafficSignals();
                FillTrafficSignalsPointOntracks();
                LoadCrossingsFromBase();
                LoadPt();
                LoadCurrentKindChange();
                FillUkspsKtsm();
                FillNeutralSections();


            }
            finally
            {
                _connection.Close();
            }
        }

        public void LoadSegmentsFromBase()
        {
            string query = "SELECT S.SegmentID, S.SegmentName, S.SegmentLength, S.TrackID, S.StationID, " +
                           "S.AutoBlockFrequency, PS.PredefinedRouteSegmentFromStartToEnd " +
                           "FROM Segment AS S " +
                           "LEFT JOIN PredefinedRouteSegment AS [PS] ON S.SegmentID = PS.SegmentID " +
                           "WHERE S.SegmentID IN (SELECT SegmentID FROM PredefinedRouteSegment)";

            using (var command = new OleDbCommand(query, _connection))
            using (var reader = command.ExecuteReader())
            {
                while (reader.Read())
                {
                    var s = new Segment(
                        Convert.ToDouble(reader[0]),
                        reader[1].ToString(),
                        Convert.ToDouble(reader[2]),
                        Convert.ToDouble(reader[3]),
                        Convert.ToDouble(reader[5]),
                        Convert.ToDouble(reader[6]));

                    if (reader[4].ToString() != "")
                    {
                        s.StationID = Convert.ToDouble(reader[4]);
                    }

                    _route.Segments.Add(s);
                }
            }
        }
        private void LoadPointOnTracksFromBase()
        {
            const string query = "SELECT PointOnTrackID, DicPointOnTrackKindID, TrackObjectID, SegmentID, " +
                                "PointOnTrackCoordinate, PointOnTrackKm, PointOnTrackPk, PointOnTrackM, PointOnTrackUsageDirection " +
                                "FROM PointOnTrack AS P " +
                                "WHERE P.SegmentID IN (SELECT SegmentID FROM PredefinedRouteSegment)";

            using (var command = new OleDbCommand(query, _connection))
            using (var reader = command.ExecuteReader())
            {
                while (reader.Read())
                {
                    var point = new PointOnTrack(
                        Convert.ToDouble(reader[0]),
                        Convert.ToDouble(reader[1]),
                        Convert.ToDouble(reader[2]),
                        Convert.ToDouble(reader[3]),
                        Convert.ToDouble(reader[4]),
                        reader[5].ToString(),
                        Convert.ToDouble(reader[6]),
                        Convert.ToDouble(reader[7]),
                        Convert.ToDouble(reader[8]));

                    point.RefreshRouteCoordinate(_route.Segments);
                    _route.PointOnTracks.Add(point);
                }
            }
        }

        private void LoadTrackObjectsFromBase()
        {
            const string query = "SELECT TrackObjectID, DicTrackObjectKindID, TrackObjectName FROM TrackObject";

            using (var command = new OleDbCommand(query, _connection))
            using (var reader = command.ExecuteReader())
            {
                while (reader.Read())
                {
                    var trackObject = new TrackObject();

                    trackObject.TrackObjectID = Convert.ToDouble(reader[0]);
                    trackObject.DicTrackObjectKindID = Convert.ToDouble(reader[1]);
                    trackObject.TrackObjectName = Convert.ToString(reader[2]);
                    
                    _route.TrackObjects.Add(trackObject);
                }
            }

        }

        private void FillSegmentsStartEnd()
        {
            foreach (var segment in _route.Segments)
            {
                int startIndex = _route.PointOnTracks.FindIndex(x =>
                    x.SegmentID == segment.SegmentID &&
                    x.PointOnTrackCoordinate == 0 &&
                    (x.DicPointOnTrackKindID == 27 || x.DicPointOnTrackKindID == 28 || x.DicPointOnTrackKindID == 29));

                int endIndex = _route.PointOnTracks.FindIndex(x =>
                    x.SegmentID == segment.SegmentID &&
                    x.PointOnTrackCoordinate == segment.SegmentLength &&
                    (x.DicPointOnTrackKindID == 27 || x.DicPointOnTrackKindID == 28 || x.DicPointOnTrackKindID == 29));

                if (startIndex >= 0)
                {
                    if (segment.PredefinedRouteSegmentFromStartToEnd == 1)
                        segment.Start = _route.PointOnTracks[startIndex];
                    else
                        segment.End = _route.PointOnTracks[startIndex];
                }

                if (endIndex >= 0)
                {
                    if (segment.PredefinedRouteSegmentFromStartToEnd == 1)
                        segment.End = _route.PointOnTracks[endIndex];
                    else
                        segment.Start = _route.PointOnTracks[endIndex];
                }
            }
        }

        private void LoadTracks()
        {
            const string query = "SELECT TrackID, TrackNumber, TrackEven, DicTrackKindID, TrackName FROM Track";

            using (var command = new OleDbCommand(query, _connection))
            using (var reader = command.ExecuteReader())
            {
                while (reader.Read())
                {
                    _route.Tracks.Add(new Track(
                        Convert.ToDouble(reader[0]),
                        reader[1].ToString(),
                        Convert.ToDouble(reader[2]),
                        Convert.ToDouble(reader[3]),
                        reader[4].ToString()));
                }
            }
        }

        private void LoadStationsFromBase()
        {
            const string query = "SELECT TrackObjectID, StationName FROM Station";

            using (var command = new OleDbCommand(query, _connection))
            using (var reader = command.ExecuteReader())
            {
                while (reader.Read())
                {
                    var station = new Station(reader[1].ToString())
                    {
                        Start = { TrackObjectID = Convert.ToDouble(reader[0]) },
                        End = { TrackObjectID = Convert.ToDouble(reader[0]) },
                        TrackObjectID = Convert.ToDouble(reader[0])
                    };
                    _route.Stations.Add(station);
                }
            }
        }

        private void LoadPlatformsFromBase()
        {
            const string query = "SELECT TrackObjectID, PlatformName FROM Platform";

            using (var command = new OleDbCommand(query, _connection))
            using (var reader = command.ExecuteReader())
            {
                while (reader.Read())
                {
                    var platform = new Platform
                    {
                        PlatformName = reader[1].ToString(),
                        Start = { TrackObjectID = Convert.ToDouble(reader[0]) },
                        End = { TrackObjectID = Convert.ToDouble(reader[0]) }
                    };
                    _route.Platforms.Add(platform);
                }
            }
        }
        private void LoadInclines()
        {
            const string query = "SELECT TrackObjectID, InclineValue FROM Incline";

            using (var command = new OleDbCommand(query, _connection))
            using (var reader = command.ExecuteReader())
            {
                while (reader.Read())
                {
                    var incline = new Incline(Convert.ToDouble(reader[1]))
                    {
                        Start = { TrackObjectID = Convert.ToDouble(reader[0]) },
                        End = { TrackObjectID = Convert.ToDouble(reader[0]) }
                    };
                    _route.Inclines.Add(incline);
                }
            }
        }
        private void LoadAlsDevice()
        {
            const string query = "SELECT ALSControlID, ALSDeviceIDToControl, ALSDeviceIDInfoFrom, " +
                                 "DicALSControlKindID, ALSControlUsageDirection FROM ALSControl";

            using (var command = new OleDbCommand(query, _connection))
            using (var reader = command.ExecuteReader())
            {
                while (reader.Read())
                {
                    _route.AlsControls.Add(new AlsControl(
                        Convert.ToDouble(reader[0]),
                        Convert.ToDouble(reader[1]),
                        Convert.ToDouble(reader[2]),
                        Convert.ToDouble(reader[3]),
                        Convert.ToDouble(reader[4])));
                }
            }
        }

        private void LoadSpeedRestrictionsFromBase()
        {
            const string query = "SELECT TrackObjectID, PermanentRestrictionSpeed, " +
                                 "PermRestrictionOnlyHeader, PermRestrictionForEmptyTrain " +
                                 "FROM PermanentRestriction";

            using (var command = new OleDbCommand(query, _connection))
            using (var reader = command.ExecuteReader())
            {
                while (reader.Read())
                {
                    var restriction = new SpeedRestriction(
                        Convert.ToDouble(reader[1]),
                        Convert.ToDouble(reader[2]),
                        Convert.ToDouble(reader[3]))
                    {
                        Start = { TrackObjectID = Convert.ToDouble(reader[0]) },
                        End = { TrackObjectID = Convert.ToDouble(reader[0]) }
                    };
                    _route.SpeedRestrictions.Add(restriction);
                }
            }
        }

        private void LoadCrossingPiecesFromBase()
        {
            const string query = "SELECT TrackObjectID, StationID, CrossingPieceName, " +
                                 "DicCrossingPieceKindID, CrossingPieceFrogModel " +
                                 "FROM CrossingPiece";

            using (var command = new OleDbCommand(query, _connection))
            using (var reader = command.ExecuteReader())
            {
                while (reader.Read())
                {
                    var crossingPiece = new CrossingPiece
                    {
                        TrackObjectID = Convert.ToDouble(reader[0]),
                        Start = { TrackObjectID = Convert.ToDouble(reader[0]) },
                        End = { TrackObjectID = Convert.ToDouble(reader[0]) },
                        CrossingPieceName = reader[2].ToString(),
                        DicCrossingPieceKindID = Convert.ToDouble(reader[3]),
                        CrossingPieceFrogModel = Convert.ToDouble(reader[4])
                    };

                    if (reader[1].ToString() != "")
                    {
                        crossingPiece.StationID = Convert.ToDouble(reader[1]);
                    }

                    _route.CrossingPieces.Add(crossingPiece);
                }
            }
        }

        private void LoadTrafficLights()
        {
            const string query = "SELECT TrackObjectID, TrafficLightName, " +
                                 "DicTrafficLightKindID, StationID " +
                                 "FROM TrafficLight";

            using (var command = new OleDbCommand(query, _connection))
            using (var reader = command.ExecuteReader())
            {
                while (reader.Read())
                {
                    var trafficLight = new TrafficLight(
                        Convert.ToDouble(reader[0]),
                        reader[1].ToString(),
                        Convert.ToDouble(reader[2]))
                    {
                        Start = { TrackObjectID = Convert.ToDouble(reader[0]) }
                    };

                    if (reader[3].ToString() != "")
                    {
                        trafficLight.StationID = Convert.ToDouble(reader[3]);
                    }

                    _route.TrafficLights.Add(trafficLight);
                }
            }
        }

        public static void LoadTrafficLightLamps(OleDbConnection connection, DbRoute route)
        {
            string queryTrafficLightLampInFrame =
                "SELECT TrackObjectID, FilmID, FrameTime, DicLampUsageID, X, Y, DX, DY, DarkDX, DarkDY, A, R, G, B, LightInFilm " +
                "FROM TrafficLightLampInFrame";

            OleDbCommand command7 = new OleDbCommand(queryTrafficLightLampInFrame, connection);

            OleDbDataReader reader7 = command7.ExecuteReader();

            while (reader7.Read())
            {

                TrafficLightLampInFrame lightLampInFrame = new TrafficLightLampInFrame();
                lightLampInFrame.TrackObjectID = Convert.ToDouble(reader7[0]);
                lightLampInFrame.FilmID = Convert.ToDouble(reader7[1]);
                lightLampInFrame.FrameTime = Convert.ToDouble(reader7[2]);
                lightLampInFrame.DicLampUsageID = Convert.ToDouble(reader7[3]);
                lightLampInFrame.X = Convert.ToDouble(reader7[4]);
                lightLampInFrame.Y = Convert.ToDouble(reader7[5]);
                lightLampInFrame.DX = Convert.ToDouble(reader7[6]);
                lightLampInFrame.DY = Convert.ToDouble(reader7[7]);
                lightLampInFrame.DarkDX = Convert.ToDouble(reader7[8]);
                lightLampInFrame.DarkDY = Convert.ToDouble(reader7[9]);
                lightLampInFrame.A = Convert.ToDouble(reader7[10]);
                lightLampInFrame.R = Convert.ToDouble(reader7[11]);
                lightLampInFrame.G = Convert.ToDouble(reader7[12]);
                lightLampInFrame.B = Convert.ToDouble(reader7[13]);
                lightLampInFrame.LightInFilm = Convert.ToBoolean(reader7[14]);

                TrafficLight l = route.TrafficLights.Find(l => l.TrackObjectID == lightLampInFrame.TrackObjectID);

                if (l != null)
                {
                    l.trafficLightLampInFrames.Add(lightLampInFrame);
                    l.trafficLightLampInFramesCount = l.trafficLightLampInFrames.Count;
                }
                
            }

            string queryTrafficLightInFrame =
                "SELECT TrackObjectID, FilmID, FrameTime, Left, Top, Height, Width, Visible, BackgroundA, BackgroundR, BackgroundG, BackgroundB " +
                "FROM TrafficLightInFrame";


            OleDbCommand command8 = new OleDbCommand(queryTrafficLightInFrame, connection);

            OleDbDataReader reader8 = command8.ExecuteReader();

            while (reader8.Read())
            {

                TrafficLightInFrame lightInFrame = new TrafficLightInFrame();

                lightInFrame.TrackObjectID = Convert.ToDouble(reader8[0]);
                lightInFrame.FilmID = Convert.ToDouble(reader8[1]);
                lightInFrame.FrameTime = Convert.ToDouble(reader8[2]);
                lightInFrame.Left = Convert.ToDouble(reader8[3]);
                lightInFrame.Top = Convert.ToDouble(reader8[4]);
                lightInFrame.Height = Convert.ToDouble(reader8[5]);
                lightInFrame.Width = Convert.ToDouble(reader8[6]);
                lightInFrame.Visible = Convert.ToBoolean(reader8[7]);
                lightInFrame.BackgroundA = Convert.ToDouble(reader8[8]);
                lightInFrame.BackgroundR = Convert.ToDouble(reader8[9]);
                lightInFrame.BackgroundG = Convert.ToDouble(reader8[10]);
                lightInFrame.BackgroundB = Convert.ToDouble(reader8[11]);

                TrafficLight l = route.TrafficLights.Find(l => l.TrackObjectID == lightInFrame.TrackObjectID);

                l.trafficLightInFrames.Add(lightInFrame);
                l.trafficLightInFramesCount = l.trafficLightInFrames.Count;

            }


        }

        private void LoadCurrentKindChange()
        {
            const string query = "SELECT TrackObjectID, DicCurrentKindIDLeft, DicCurrentKindIDRight " +
                                 "FROM CurrentKindChange";

            try
            {


                using (var command = new OleDbCommand(query, _connection))
                using (var reader = command.ExecuteReader())
                {

                    while (reader.Read())
                    {
                        CurrentKindChange t = new CurrentKindChange();
                        if (reader[0].ToString() != "") t.TrackObjectID = Convert.ToDouble(reader[0]);
                        if (reader[1].ToString() != "") t.DicCurrentKindIDLeft = Convert.ToDouble(reader[1]);
                        if (reader[2].ToString() != "") t.DicCurrentKindIDRight = Convert.ToDouble(reader[2]);

                        _route.CurrentKindChanges.Add(t);

                        //int index = _route.PointOnTracks.FindIndex(x => x.TrackObjectID == t.Start.TrackObjectID);
                        //if (index >= 0)
                        //{
                        //    t.Start = _route.PointOnTracks[index];
                        //    t.Start.RefreshRouteCoordinate(_route.Segments);
                        //}

                    }

                }
            }
            catch (Exception ex)
            {
                // Логирование ошибки при необходимости
            }

            foreach (CurrentKindChange t in _route.CurrentKindChanges)
            {
                int index = _route.PointOnTracks.FindIndex(x => (x.TrackObjectID == t.TrackObjectID));
                if (index >= 0)
                {
                    t.Start = _route.PointOnTracks[index];
                    t.Start.RefreshRouteCoordinate(_route.Segments);
                }
            }

        }


        public void LoadTliRestrictions()
        {
            const string query = "SELECT TrafficLightID, RestrictionKind, RouteKind, " +
                                "BlockCount, Speed, ShowBlockCount, AutoBlockCode " +
                                "FROM TrafficLightSpeedRestriction";

            try
            {
                using (var command = new OleDbCommand(query, _connection))
                using (var reader = command.ExecuteReader())
                {
                    var tliRestrictions = new List<TliRestriction>();

                    while (reader.Read())
                    {
                        var tli = new TliRestriction();
                        if (reader[0].ToString() != "") tli.TrafficLightID = Convert.ToDouble(reader[0]);
                        if (reader[1].ToString() != "") tli.kind = (TliRestrictionKind)Convert.ToInt32(reader[1]);
                        tli.routeKind = ALSRouteKind.Dummy;
                        if (reader[3].ToString() != "") tli.blockCount = (sbyte)Convert.ToDouble(reader[3]);
                        if (reader[4].ToString() != "") tli.speed = (short)Convert.ToDouble(reader[4]);
                        if (reader[5].ToString() != "") tli.showBlockCount = (sbyte)Convert.ToInt32(reader[5]);
                        if (reader[6].ToString() != "") { tli.autoBlockCode = (AutoBlockInternalControlCode)Convert.ToInt32(reader[6]); }
                        else
                        {
                            tli.autoBlockCode = AutoBlockInternalControlCode.Dummy;
                        }

                        tliRestrictions.Add(tli);
                    }

                    foreach (var tli in tliRestrictions)
                    {
                        var trafficLight = _route.TrafficLights.Find(x => x.TrackObjectID == tli.TrafficLightID);
                        trafficLight?.TliRestrictions.Add(tli);
                    }
                }
            }
            catch (Exception ex)
            {
                // Логирование ошибки при необходимости
                Console.WriteLine(ex.Message);
            }
        }

        private void LoadTrackCircuits()
        {
            const string query = "SELECT TrackObjectID, TrackCircuitName, " +
                                "StationID, TrackCircuitContinueThroughBound " +
                                "FROM TrackCircuit";

            using (var command = new OleDbCommand(query, _connection))
            using (var reader = command.ExecuteReader())
            {
                while (reader.Read())
                {
                    var trackCircuit = new TrackCircuit(
                        Convert.ToDouble(reader[0]),
                        reader[1].ToString(),
                        Convert.ToDouble(reader[3]));

                    if (reader[2].ToString() != "")
                    {
                        trackCircuit.StationID = Convert.ToDouble(reader[2]);
                    }

                    _route.TrackCircuits.Add(trackCircuit);
                }
            }
        }

        private void LoadUncodedTracks()
        {
            const string query = "SELECT TrackObjectID, UncodedTrackName " +
                                "FROM UncodedTrack";

            using (var command = new OleDbCommand(query, _connection))
            using (var reader = command.ExecuteReader())
            {
                while (reader.Read())
                {
                    _route.UncodedTracks.Add(new UncodedTrack(
                        Convert.ToDouble(reader[0]),
                        reader[1].ToString()));
                }
            }
        }

        private void LoadTrafficSignals()
        {
            const string query = "SELECT TrackObjectID, DicTrafficSignalKindID " +
                                "FROM TrafficSignal";

            using (var command = new OleDbCommand(query, _connection))
            using (var reader = command.ExecuteReader())
            {
                while (reader.Read())
                {
                    var trafficSignal = new TrafficSignal
                    {
                        TrackObjectID = Convert.ToDouble(reader[0]),
                        DicTrafficSignalKindID = Convert.ToDouble(reader[1]),
                        Start = { TrackObjectID = Convert.ToDouble(reader[0]) }
                    };

                    _route.TrafficSignals.Add(trafficSignal);
                }
            }
        }

        private void LoadCrossingsFromBase()
        {
            const string query = "SELECT TrackObjectID, DicCrossingKindID " +
                                "FROM Crossing";

            using (var command = new OleDbCommand(query, _connection))
            using (var reader = command.ExecuteReader())
            {
                while (reader.Read())
                {
                    var crossing = new Crossing
                    {
                        TrackObjectID = Convert.ToDouble(reader[0]),
                        DicCrossingKindID = Convert.ToDouble(reader[1]),
                        Start = { TrackObjectID = Convert.ToDouble(reader[0]) }
                    };

                    _route.Crossings.Add(crossing);
                }
            }

            foreach (Crossing t in _route.Crossings)
            {
                int index = _route.PointOnTracks.FindIndex(x => (x.TrackObjectID == t.Start.TrackObjectID));
                if (index >= 0)
                {
                    t.Start = _route.PointOnTracks[index];
                    t.Start.RefreshRouteCoordinate(_route.Segments);
                }
            }
        }

        

        private void LoadPt()
        {
            const string queryBrakeCheck = "SELECT B.TrackObjectID, B.DicBrakeCheckKindID, " +
                                         "B.BrakeCheckPlaceDirection, T.TrackObjectName " +
                                         "FROM BrakeCheckPlace AS B " +
                                         "LEFT JOIN TrackObject AS [T] ON B.TrackObjectID = T.TrackObjectID";

            const string queryBrakeCheckNorm = "SELECT BrakeCheckPlaceID, BrakeCheckNormSpeed, " +
                                             "BrakeCheckNormPath FROM BrakeCheckNorm";

            // Загрузка мест проверки тормозов
            using (var command = new OleDbCommand(queryBrakeCheck, _connection))
            using (var reader = command.ExecuteReader())
            {
                while (reader.Read())
                {
                    var brakeCheck = new BrakeCheckPlace
                    {
                        TrackObjectID = Convert.ToDouble(reader[0]),
                        DicBrakeCheckKindID = Convert.ToDouble(reader[1]),
                        BrakeCheckPlaceDirection = Convert.ToDouble(reader[2]),
                        TrackObjectName = reader[3].ToString(),
                        Start = { TrackObjectID = Convert.ToDouble(reader[0]) }
                    };

                    _route.BrakeCheckPlaces.Add(brakeCheck);
                }
            }

            // Загрузка норм проверки тормозов
            using (var command = new OleDbCommand(queryBrakeCheckNorm, _connection))
            using (var reader = command.ExecuteReader())
            {
                while (reader.Read())
                {
                    var norm = new BrakeCheckNorm
                    {
                        BrakeCheckPlaceID = Convert.ToDouble(reader[0]),
                        BrakeCheckNormSpeed = Convert.ToDouble(reader[1]),
                        BrakeCheckNormPath = Convert.ToDouble(reader[2])
                    };

                    var brakeCheck = _route.BrakeCheckPlaces.Find(x => x.TrackObjectID == norm.BrakeCheckPlaceID);
                    brakeCheck?.BrakeCheckNormList.Add(norm);
                }
            }

            // Связывание точек с местами проверки тормозов
            foreach (var brakeCheck in _route.BrakeCheckPlaces)
            {
                int index = _route.PointOnTracks.FindIndex(x => x.TrackObjectID == brakeCheck.TrackObjectID);
                if (index >= 0)
                {
                    brakeCheck.Start = _route.PointOnTracks[index];
                    brakeCheck.Start.RefreshRouteCoordinate(_route.Segments);
                }
            }
        }

        public void FillSpeedRestrictionsPointOntracks()
        {
            foreach (SpeedRestriction s in _route.SpeedRestrictions)
            {
                // находим точки соответствующие началу и концу ограничения
                int index = _route.PointOnTracks.FindIndex(x => (x.TrackObjectID == s.Start.TrackObjectID) && (x.PointOnTrackUsageDirection == 1));
                int index2 = _route.PointOnTracks.FindIndex(x => (x.TrackObjectID == s.End.TrackObjectID) && (x.PointOnTrackUsageDirection == -1));

                int sindex;

                if (index >= 0 && index2 >= 0)
                {
                    sindex = _route.Segments.FindIndex(x => (x.SegmentID == _route.PointOnTracks[index].SegmentID));

                    if (sindex >= 0)
                    {

                        if (_route.Segments[sindex].PredefinedRouteSegmentFromStartToEnd == 1)
                        {
                            s.Start = _route.PointOnTracks[index];
                            s.End = _route.PointOnTracks[index2];

                        }
                        else if (_route.Segments[sindex].PredefinedRouteSegmentFromStartToEnd == -1)
                        {
                            s.End = _route.PointOnTracks[index];
                            s.Start = _route.PointOnTracks[index2];
                        }
                    }
                }

                s.TrackToShow(_route.Tracks, _route.Segments);

                s.Start.RefreshRouteCoordinate(_route.Segments);
                s.End.RefreshRouteCoordinate(_route.Segments);
                s.RefreshStationMid(_route.Stations);
            }
        }

        private void FillCrossingPiecesPointOntracks()
        {
            foreach (var s in _route.CrossingPieces)
            {
                int index = _route.PointOnTracks.FindIndex(x => x.TrackObjectID == s.TrackObjectID);
                int index2 = _route.PointOnTracks.FindLastIndex(x => x.TrackObjectID == s.TrackObjectID);

                if (index >= 0)
                {
                    s.Start = _route.PointOnTracks[index];
                    s.Station = s.Start.station;
                }

                if (index2 >= 0)
                {
                    s.End = _route.PointOnTracks[index2];
                    s.Station = s.End.station;
                }

                if (s.Start != null)
                {
                    s.Start.RefreshRouteCoordinate(_route.Segments);
                }

                if (s.End != null)
                {
                    s.End.RefreshRouteCoordinate(_route.Segments);
                }
            }
        }

        private void FillTrafficLightsPointOntracks()
        {
            foreach (var t in _route.TrafficLights)
            {
                int index = _route.PointOnTracks.FindIndex(x => x.TrackObjectID == t.Start.TrackObjectID);
                if (index >= 0)
                {
                    t.Start = _route.PointOnTracks[index];
                    t.Start.RefreshRouteCoordinate(_route.Segments);
                }

                TrackObject trackobject = _route.TrackObjects.Find(y => y.TrackObjectID == t.TrackObjectID);
                if (trackobject !=null)
                {
                    t.TrackObjectName = trackobject.TrackObjectName;
                }
            }
        }

        private void FillTrackCircuitsPointOntracks()
        {
            foreach (var s in _route.TrackCircuits)
            {
                int index = _route.PointOnTracks.FindIndex(x => x.TrackObjectID == s.TrackObjectID);
                int index2 = _route.PointOnTracks.FindLastIndex(x => x.TrackObjectID == s.TrackObjectID);

                if (index >= 0 && index2 >= 0)
                {
                    s.Start = _route.PointOnTracks[index];
                    s.End = _route.PointOnTracks[index2];
                }

                if (s.Start != null && s.End != null)
                {
                    s.Start.RefreshRouteCoordinate(_route.Segments);
                    s.End.RefreshRouteCoordinate(_route.Segments);
                }
            }
        }

        private void FillTrackCircuitsAls()
        {
            foreach (var t in _route.TrackCircuits)
            {
                int alsindex = _route.AlsControls.FindIndex(x => t.TrackObjectID == x.ALSDeviceIDInfoFrom);
                if (alsindex >= 0)
                {
                    t.TrafficLightId = _route.AlsControls[alsindex].ALSDeviceIDToControl;
                }

                int trafficLightIndex = _route.TrafficLights.FindIndex(x => t.TrafficLightId == x.TrackObjectID);
                if (trafficLightIndex >= 0)
                {
                    t.TrafficLightName = _route.TrafficLights[trafficLightIndex].TrafficLightName;
                }
            }
        }

        private void FillUncodedTracksPointOntracks()
        {
            foreach (var s in _route.UncodedTracks)
            {
                int index = _route.PointOnTracks.FindIndex(x => x.TrackObjectID == s.TrackObjectID);
                int index2 = _route.PointOnTracks.FindLastIndex(x => x.TrackObjectID == s.TrackObjectID);

                if (index >= 0 && index2 >= 0)
                {
                    s.Start = _route.PointOnTracks[index];
                    s.End = _route.PointOnTracks[index2];
                }

                if (s.Start != null && s.End != null)
                {
                    s.Start.RefreshRouteCoordinate(_route.Segments);
                    s.End.RefreshRouteCoordinate(_route.Segments);
                }
            }
        }

        private void FillTrafficSignalsPointOntracks()
        {
            foreach (var t in _route.TrafficSignals)
            {
                int index = _route.PointOnTracks.FindIndex(x => x.TrackObjectID == t.Start.TrackObjectID);
                if (index >= 0)
                {
                    t.Start = _route.PointOnTracks[index];
                    t.Start.RefreshRouteCoordinate(_route.Segments);
                }
            }
        }

        private void FillUkspsKtsm()
        {
            foreach (var p in _route.PointOnTracks)
            {
                if (p.DicPointOnTrackKindID == 25) // УКСПС
                {
                    _route.UkspsList.Add(new Uksps { Start = p });
                }
                else if (p.DicPointOnTrackKindID == 24) // КТСМ
                {
                    _route.KtsmList.Add(new Ktsm { Start = p });
                }
            }
        }

        private void FillNeutralSections()
        {
            foreach (var p in _route.PointOnTracks)
            {
                if (p.DicPointOnTrackKindID == 35) // Нейтральная вставка
                {
                    var section = _route.NeutralSections.Find(x => x.TrackObjectID == p.TrackObjectID);
                    var segment = _route.Segments.Find(y => y.SegmentID == p.SegmentID);

                    if (section == null)
                    {
                        section = new NeutralSection { TrackObjectID = p.TrackObjectID };
                        _route.NeutralSections.Add(section);
                    }

                    if (p.PointOnTrackUsageDirection * segment.PredefinedRouteSegmentFromStartToEnd == 1)
                    {
                        section.Start = p;
                    }
                    else
                    {
                        section.End = p;
                    }
                }
            }
        }

        public void FillKilometers(List<PointOnTrack> _PointOnTracks, List<Segment> _Segments)
        {
            foreach (PointOnTrack p in _PointOnTracks)
            {
                if ((p.DicPointOnTrackKindID == 0)
                    || (p.DicPointOnTrackKindID == 27)
                    || (p.DicPointOnTrackKindID == 28)
                    || (p.DicPointOnTrackKindID == 29))
                {
                    // int index2 = PointOnTracks.FindIndex(x => (x.PointOnTrackCoordinate > p.PointOnTrackCoordinate) &&
                    int index2 = _PointOnTracks.FindIndex(x => (x.RouteCoordinate > p.RouteCoordinate) && (x.SegmentID == p.SegmentID)
                    && (
                       (x.DicPointOnTrackKindID == 0)
                    || (x.DicPointOnTrackKindID == 27)
                    || (x.DicPointOnTrackKindID == 28)
                    || (x.DicPointOnTrackKindID == 29)
                    ));
                    int sindex = _Segments.FindIndex(x => (x.SegmentID == p.SegmentID));

                    if ((index2 >= 0) && (p.RouteCoordinate != _PointOnTracks[index2].RouteCoordinate))
                    {


                        if (_Segments[sindex].PredefinedRouteSegmentFromStartToEnd == 1)
                        {
                            Kilometer Kmtoadd = new Kilometer();

                            Kmtoadd.Start = p;
                            Kmtoadd.End = _PointOnTracks[index2];
                            Kmtoadd.Km = p.PointOnTrackKm;
                            _route.Kilometers.Add(Kmtoadd);
                        }
                        else if (_Segments[sindex].PredefinedRouteSegmentFromStartToEnd == -1)
                        {
                            Kilometer Kmtoadd = new Kilometer();
                            Kmtoadd.Start = p;
                            Kmtoadd.End = _PointOnTracks[index2];
                            Kmtoadd.Km = _PointOnTracks[index2].PointOnTrackKm;
                            _route.Kilometers.Add(Kmtoadd);
                        }
                    }
                }

            }

            bool dkm = true;
            while (dkm == true)
            {
                dkm = UniteNextKm();
            }

            foreach (Kilometer k in _route.Kilometers)
            {
                //k.KmLengthSet += KmLengthChangedPerform;
                // присваивается Km.Length

                k.Length = Math.Round((k.End.RouteCoordinate - k.Start.RouteCoordinate), 2);
            }




        }
        public bool UniteNextKm()
        {
            //для километров, переходящих через стрелку не меняющую километраж
            foreach (Kilometer k in _route.Kilometers)
            {
                int index = _route.Kilometers.IndexOf(k);// идекс текущего километра
                int index1 = _route.Kilometers.FindIndex(x => (x.Start.RouteCoordinate == k.End.RouteCoordinate));// найти индекс следующего километра

                int sindex = _route.Segments.FindIndex(s => s.SegmentID == k.End.SegmentID); // идекс сегмента конца текущего километра
                int sindex1;


                if (index1 >= 0) // если существует следующий км
                {
                    sindex1 = _route.Segments.FindIndex(s => s.SegmentID == _route.Kilometers[index1].Start.SegmentID); // найти индекс сегмента начала следующего километра



                    if (
                          (index1 > 0) // если существует следующий км
                          &&
                          (
                             (_route.Kilometers[index1].Start.PointOnTrackKm == _route.Kilometers[index].End.PointOnTrackKm) // если его координаты начала совпадают с концом текущего
                              && (_route.Kilometers[index1].Start.PointOnTrackPk == _route.Kilometers[index].End.PointOnTrackPk)
                              && (_route.Kilometers[index1].Start.PointOnTrackM == _route.Kilometers[index].End.PointOnTrackM)
                              && (_route.Kilometers[index1].Start.SegmentID != _route.Kilometers[index].End.SegmentID)// это разные сегменты
                              && (_route.Segments[sindex1].PredefinedRouteSegmentFromStartToEnd == _route.Segments[sindex].PredefinedRouteSegmentFromStartToEnd) // на следующем сегменте километраж в том же направлении 
                          )
                        ||
                    // если начало следующего км и конец текущего это одна и та же точка и это остряк стрелки
                          (k.End == _route.Kilometers[index1].Start && (k.End.DicPointOnTrackKindID == 28 || k.End.DicPointOnTrackKindID == 29))
                       )

                    {
                        k.End = _route.Kilometers[index1].End;
                        _route.Kilometers.RemoveAt(index1);
                        return true;
                    }
                }
            }
            return false;
        }

        public void FillStationPointOntracks(List<PointOnTrack> _PointOnTracks, List<Segment> _Segments, List<Station> _Stations)
        {
            foreach (Station s in _Stations)
            {
                int index = _PointOnTracks.FindIndex(x => ((x.TrackObjectID == s.Start.TrackObjectID) && (x.PointOnTrackUsageDirection == 1)));
                int index2 = _PointOnTracks.FindIndex(x => ((x.TrackObjectID == s.End.TrackObjectID) && (x.PointOnTrackUsageDirection == -1)));
                int index3 = _PointOnTracks.FindLastIndex(x => ((x.TrackObjectID == s.Start.TrackObjectID) && (x.PointOnTrackUsageDirection == 1)));
                int index4 = _PointOnTracks.FindLastIndex(x => ((x.TrackObjectID == s.Start.TrackObjectID) && (x.PointOnTrackUsageDirection == -1)));

                int sindex;
                int sindex2;
                // если есть две точки станции 1 и -1
                if ((index >= 0) && (index2 >= 0))
                {
                    sindex = _Segments.FindIndex(x => (x.SegmentID == _PointOnTracks[index].SegmentID));
                    sindex2 = _Segments.FindIndex(x => (x.SegmentID == _PointOnTracks[index2].SegmentID));
                    //1
                    if ((_Segments[sindex].PredefinedRouteSegmentFromStartToEnd == 1) &&
                        (_Segments[sindex2].PredefinedRouteSegmentFromStartToEnd == 1))
                    {
                        s.Start = (_PointOnTracks[index]);
                        s.End = (_PointOnTracks[index2]);
                    }
                    //2
                    else if ((_Segments[sindex].PredefinedRouteSegmentFromStartToEnd == -1) &&
                             (_Segments[sindex2].PredefinedRouteSegmentFromStartToEnd == -1))
                    {
                        s.Start = (_PointOnTracks[index2]);
                        s.End = (_PointOnTracks[index]);
                    }
                }
                //3 если две точки -1
                else if ((index < 0) && (index3 < 0) && (index2 >= 0) && (index4 >= 0) && (index2 != index4))
                {
                    s.Start = (_PointOnTracks[index2]);
                    s.End = (_PointOnTracks[index4]);
                }
                //4 если две точки 1
                else if ((index >= 0) && (index3 >= 0) && (index2 < 0) && (index4 < 0) && (index != index3))
                {
                    s.Start = (_PointOnTracks[index]);
                    s.End = (_PointOnTracks[index3]);
                }
                //5,8 одна точка -1
                else if ((index < 0) && (index2 >= 0) && (index4 == index2))
                {
                    sindex2 = _Segments.FindIndex(x => (x.SegmentID == _PointOnTracks[index2].SegmentID));
                    //5
                    if (_Segments[sindex2].PredefinedRouteSegmentFromStartToEnd == 1)
                    {
                        s.Start = (_PointOnTracks[0]);

                        s.Start.RouteCoordinate = 0;
                        s.End = (_PointOnTracks[index2]);
                    }
                    //8
                    else if (_Segments[sindex2].PredefinedRouteSegmentFromStartToEnd == -1)
                    {
                        s.Start = (_PointOnTracks[index2]);
                        s.End = (_PointOnTracks[_PointOnTracks.Count() - 1]);
                    }

                }
                // 6,7 одна точка 1
                else if ((index >= 0) && (index2 < 0) && (index == index3))
                {
                    sindex = _Segments.FindIndex(x => (x.SegmentID == _PointOnTracks[index].SegmentID));
                    //6
                    if (_Segments[sindex].PredefinedRouteSegmentFromStartToEnd == 1)
                    {
                        s.Start = (_PointOnTracks[index]);
                        s.End = (_PointOnTracks[_PointOnTracks.Count() - 1]);
                    }
                    //7
                    else if (_Segments[sindex].PredefinedRouteSegmentFromStartToEnd == -1)
                    {
                        s.Start = (_PointOnTracks[0]);
                        s.End = (_PointOnTracks[index]);
                    }
                }


            }


        }

        public void FillPlatformPointOntracks(List<PointOnTrack> _PointOnTracks, List<Segment> _Segments, List<Platform> _Platforms)
        {
            foreach (Platform s in _Platforms)
            {
                int index = _PointOnTracks.FindIndex(x => ((x.TrackObjectID == s.Start.TrackObjectID) && (x.DicPointOnTrackKindID == 9) && (x.PointOnTrackUsageDirection == 1)));
                int index2 = _PointOnTracks.FindIndex(x => ((x.TrackObjectID == s.End.TrackObjectID) && (x.DicPointOnTrackKindID == 9) && (x.PointOnTrackUsageDirection == -1)));
                int index3 = _PointOnTracks.FindLastIndex(x => ((x.TrackObjectID == s.Start.TrackObjectID) && (x.DicPointOnTrackKindID == 9) && (x.PointOnTrackUsageDirection == 1)));
                int index4 = _PointOnTracks.FindLastIndex(x => ((x.TrackObjectID == s.Start.TrackObjectID) && (x.DicPointOnTrackKindID == 9) && (x.PointOnTrackUsageDirection == -1)));

                int sindex;
                int sindex2;

                // если есть две точки станции 1 и -1
                if ((index >= 0) && (index2 >= 0))
                {
                    sindex = _Segments.FindIndex(x => (x.SegmentID == _PointOnTracks[index].SegmentID));
                    sindex2 = _Segments.FindIndex(x => (x.SegmentID == _PointOnTracks[index2].SegmentID));
                    //1
                    if ((_Segments[sindex].PredefinedRouteSegmentFromStartToEnd == 1) &&
                        (_Segments[sindex2].PredefinedRouteSegmentFromStartToEnd == 1))
                    {
                        s.Start = (_PointOnTracks[index]);
                        s.End = (_PointOnTracks[index2]);
                    }
                    //2
                    else if ((_Segments[sindex].PredefinedRouteSegmentFromStartToEnd == -1) &&
                             (_Segments[sindex2].PredefinedRouteSegmentFromStartToEnd == -1))
                    {
                        s.Start = (_PointOnTracks[index2]);
                        s.End = (_PointOnTracks[index]);
                    }
                }
                //3 если две точки -1
                else if ((index < 0) && (index3 < 0) && (index2 >= 0) && (index4 >= 0) && (index2 != index4))
                {
                    s.Start = (_PointOnTracks[index2]);
                    s.End = (_PointOnTracks[index4]);
                }
                //4 если две точки 1
                else if ((index >= 0) && (index3 >= 0) && (index2 < 0) && (index4 < 0) && (index != index3))
                {
                    s.Start = (_PointOnTracks[index]);
                    s.End = (_PointOnTracks[index3]);
                }
                //5,8 одна точка -1
                else if ((index < 0) && (index2 >= 0) && (index4 == index2))
                {
                    sindex2 = _Segments.FindIndex(x => (x.SegmentID == _PointOnTracks[index2].SegmentID));
                    //5
                    if (_Segments[sindex2].PredefinedRouteSegmentFromStartToEnd == 1)
                    {

                        //s.Start = new PointOnTrack();
                        s.Start = (_PointOnTracks[0]);

                        s.Start.RouteCoordinate = 0;
                        s.End = (_PointOnTracks[index2]);
                    }
                    //8
                    else if (_Segments[sindex2].PredefinedRouteSegmentFromStartToEnd == -1)
                    {
                        s.Start = (_PointOnTracks[index2]);
                        s.End = (_PointOnTracks[_PointOnTracks.Count() - 1]);
                    }

                }
                // 6,7 одна точка 1
                else if ((index >= 0) && (index2 < 0) && (index == index3))
                {
                    sindex = _Segments.FindIndex(x => (x.SegmentID == _PointOnTracks[index].SegmentID));
                    //6
                    if (_Segments[sindex].PredefinedRouteSegmentFromStartToEnd == 1)
                    {
                        s.Start = (_PointOnTracks[index]);
                        s.End = (_PointOnTracks[_PointOnTracks.Count() - 1]);
                    }
                    //7
                    else if (_Segments[sindex].PredefinedRouteSegmentFromStartToEnd == -1)
                    {
                        s.Start = (_PointOnTracks[0]);
                        s.End = (_PointOnTracks[index]);
                    }
                }
            }
        }


        public void FillInclinesPointOntracks()
        {
            foreach (Incline s in _route.Inclines)
            {
                // находим точки соответствующие началу и концу ограничения
                int index = _route.PointOnTracks.FindIndex(x => (x.TrackObjectID == s.Start.TrackObjectID) && (x.PointOnTrackUsageDirection == 1));
                int index2 = _route.PointOnTracks.FindIndex(x => (x.TrackObjectID == s.End.TrackObjectID) && (x.PointOnTrackUsageDirection == -1));

                int sindex;

                if (index >= 0 && index2 >= 0)
                {
                    sindex = _route.Segments.FindIndex(x => (x.SegmentID == _route.PointOnTracks[index].SegmentID));

                    if (sindex >= 0)
                    {

                        if (_route.Segments[sindex].PredefinedRouteSegmentFromStartToEnd == 1)
                        {
                            s.Start = _route.PointOnTracks[index];
                            s.End = _route.PointOnTracks[index2];
                        }
                        else if (_route.Segments[sindex].PredefinedRouteSegmentFromStartToEnd == -1)
                        {
                            s.End = (_route.PointOnTracks[index]);
                            s.Start = (_route.PointOnTracks[index2]);

                            if (s.Value != 0)
                                s.Value *= -1;
                        }
                    }
                }
                s.Start.RefreshRouteCoordinate(_route.Segments);
                s.End.RefreshRouteCoordinate(_route.Segments);
            }

            _route.Inclines.Sort(_inclc);
        }
        public static void FillInclinesElevation(List<Incline> _Inclines)//, double maxElev, double minElev, double kscale, double lscale)
        {
            double maxElev = 0;
            double minElev = 0;
            double _kscale = 1;
            double _lscale = 1;


            for (int i = 0; i < _Inclines.Count; i++)
            {
                _Inclines[i].End.Elevation = _Inclines[i].Start.Elevation
                    + _Inclines[i].Value * Math.Abs(_Inclines[i].End.RouteCoordinate - _Inclines[i].Start.RouteCoordinate) / 1000;
                if (i < (_Inclines.Count - 1))
                {
                    _Inclines[i + 1].Start.Elevation = _Inclines[i].End.Elevation;
                }
                maxElev = Math.Max(maxElev, _Inclines[i].End.Elevation);
                minElev = Math.Min(minElev, _Inclines[i].End.Elevation);
            }


            if ((maxElev - minElev) != 0)
            {
                _kscale = 100 / (maxElev - minElev);
            }


            _lscale = 50 - _kscale * maxElev;
        }
    }
}
