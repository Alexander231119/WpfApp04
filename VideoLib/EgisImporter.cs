using System;
using System.Collections.Generic;
using System.Data.SqlClient;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows;
using WpfAapp04;

namespace WpfApp04
{
    public class EgisImporter
    {
        private SqlConnection _egisConnection;
        private DbRoute _egisRoute;
        public double _usageDirectionToFind = 1;
        public double _speedKindToFind = 0;



        //public List<Station> EgisSelectedStations { get; private set; } = new List<Station>();
        //public List<Track> EgisSelectedTracks { get; private set; } = new List<Track>();
        //public List<PointOnTrack> EgisFoundPointObjects { get; private set; } = new List<PointOnTrack>();

        public Track EgisSelectedTrack { get; set; } = new Track();

        public EgisImporter(string connectionString, DbRoute route)
        {
            _egisConnection = new SqlConnection(connectionString);
            _egisRoute = route;
        }

        public void LoadEgisData()
        {

            _egisRoute.DbRouteClear();

            LoadEgisSegments();
            _egisRoute.PointOnTracks.Sort(new PointOnTrackComparer()); // с этим работает нормально
            LoadEgisStations();
            LoadEgisInclines();
            if (_speedKindToFind != 0) LoadEgisSpeedrestrictions();
            LoadEgisTrafficLights();
            LoadEgisTrts();
            LoadEgisKmPoints();
            LoadEgisCrossingPieces();
            LoadEgisShortObjects();
            LoadEgisCrossings();
            LoadEgisLongObjects();
            LoadEgisPt();
            
            foreach (BrakeCheckPlace _brakeCheckPlace in _egisRoute.BrakeCheckPlaces)
            {
                _brakeCheckPlace.ReduceBrakeCheckNormList();
            }

            foreach (PointOnTrack p in _egisRoute.PointOnTracks) { p.RefreshRouteCoordinate(_egisRoute.Segments); }

            _egisRoute.PointOnTracks.Sort(new PointOnTrackComparer());// без этого работает

            FillEgisKilometers(_egisRoute.PointOnTracks, _egisRoute.Segments);
            
            DbDataLoader.FillInclinesElevation(_egisRoute.Inclines);

            
            foreach (PointOnTrack p in _egisRoute.PointOnTracks) { p.FillElevation(_egisRoute.Inclines); }
            foreach (PointOnTrack p in _egisRoute.PointOnTracks) { p.FillStation(_egisRoute.Stations); }

            FindEndStations(); // найти станции без границ

            _egisRoute.SpeedRestrictions.Sort(new SpeedComparerToshow());

            foreach (SpeedRestriction s in _egisRoute.SpeedRestrictions)
            {
                s.RefreshStationMid(_egisRoute.Stations);
            }

            //переименовать станционные платформы
            foreach (Platform s in _egisRoute.Platforms)
            {
                if ((
                    s.PlatformName == "1"
                    || s.PlatformName == "2"
                    || s.PlatformName == "3"
                    || s.PlatformName == "4"
                    || s.PlatformName == "5"
                    || s.PlatformName == "6"
                    || s.PlatformName == "7"
                    || s.PlatformName == "8"
                    || s.PlatformName == "9"
                    ) && (s.Start.station != ""))
                {
                    s.PlatformName = s.Start.station;
                }

                if (s.PlatformName.Length > 1)
                {
                    s.PlatformName = s.PlatformName.Substring(0, 1).ToUpper() + s.PlatformName.Substring(1).ToLower();
                    s.PlatformName = DbRouteHelper.CapitalizeAllWords(s.PlatformName);
                }
            }
        }

        public static void SelectStations(string stationName, SqlConnection connection, List<Station> _EgisSelectedStations)
        {
            SqlConnection _egisConnection1 = new SqlConnection();
            if (connection != null)
            {  _egisConnection1 = connection;}

            string sql = "SELECT StationID, StationName FROM Station WHERE StationName like '%" + stationName + "%'";

            try
            {
                _egisConnection1.Open();
                SqlCommand command = new SqlCommand(sql, _egisConnection1);
                SqlDataReader reader = command.ExecuteReader();

                _EgisSelectedStations.Clear();

                while (reader.Read())
                {
                    Station s = new Station(reader[1].ToString());
                    s.EgisStationID = Convert.ToDouble(reader[0]);
                    _EgisSelectedStations.Add(s);
                }
            }
            catch (Exception ex)
            {
                MessageBox.Show(ex.Message);
            }
            finally
            {
                _egisConnection1.Close();
            }
        }

        public static void SelectTrack(Station station, bool isMainTrack, SqlConnection connection, List<Track> _EgisSelectedTracks)
        {

            SqlConnection _egisConnection1 = new SqlConnection();
            if (connection != null)
            { _egisConnection1 = connection; }


            if (station != null)
            {
                double trackobjectIdToFind = station.EgisStationID;

                try
                {
                    _egisConnection1.Open();

                    

                    string sql = isMainTrack
                        ? "SELECT T.TrackObjectName, S.SegmentName, S.TrackID, Tr.TrackNumber, Tr.TrackName " +
                          "FROM TrackObject AS T " +
                          "LEFT JOIN PointOnTrack AS [PP] ON PP.TrackObjectID=T.TrackObjectID " +
                          "LEFT JOIN Segment AS [S] ON S.SegmentID=PP.SegmentID " +
                          "LEFT JOIN Track AS [Tr] ON Tr.TrackID=S.TrackID " +
                          "WHERE T.TrackObjectID=" + trackobjectIdToFind
                        : "SELECT T.TrackID, T.DicTrackKindID, T.TrackNumber, T.TrackName " +
                          "FROM Track AS T " +
                          "WHERE TrackName like '%" + station.StationName.Split('[')[0] + "%'";

                    SqlCommand command = new SqlCommand(sql, _egisConnection1);
                    SqlDataReader reader = command.ExecuteReader();

                    _EgisSelectedTracks.Clear();

                    while (reader.Read())
                    {
                        Track t = new Track();
                        t.TrackID = Convert.ToDouble(isMainTrack ? reader[2] : reader[0]);
                        t.TrackNumber = reader[isMainTrack ? 3 : 2].ToString() + " " + reader[isMainTrack ? 4 : 3].ToString();
                        _EgisSelectedTracks.Add(t);
                    }
                }
                catch (Exception ex)
                {
                    MessageBox.Show(ex.Message);
                }
                finally
                {
                    _egisConnection1.Close();
                }
            }
        }

        //найти обьект в егис по названию обьекта и станции
        public static void EgisFindPointObject(
            string _egisStationId, 
            string _objectNameToFind, 
            string _stationNameToFind, 
            string _egisConnectionString,
            List<Track> _EgisSelectedTracks,
            List<PointOnTrack> _EgisFoundPointObjects)
        {


        SqlConnection egisConnection = new SqlConnection(_egisConnectionString);

        string sql =

                    "SELECT T.TrackObjectName, TP1.TrackObjectPropertyValue, Track.TrackID, Track.TrackNumber, Track.TrackName, PP.PointOnTrackKm, PP.PointOnTrackPk, PP.PointOnTrackM " +
                    "FROM TrackObject AS T " +
                    "join TrackObjectProperty TP1 on TP1.TrackObjectID = T.TrackObjectID and TP1.DicTrackObjectPropertyKindID = 2 " +
                    "join TrackObjectProperty TP2 on TP2.TrackObjectID = T.TrackObjectID and TP2.DicTrackObjectPropertyKindID = 3 " +
                    "join PointOnTrack PP " +
                    "on PP.TrackObjectID = T.TrackObjectID " +
                    "join Segment S " +
                    "on S.SegmentID=PP.SegmentID " +
                    "join Track " +
                    "on Track.TrackID=S.TrackID " +
                    //"and Track.DicTrackKindID = 1 " +
                    //"and Track.DicTrackKindID NOT IN (1) " +
                    "where TP1.TrackObjectPropertyValue like '%" + _objectNameToFind + "%' ";

            if ((_egisStationId != "") && (_stationNameToFind != ""))
            {
                sql += "and TP2.TrackObjectPropertyValue = " + _egisStationId;
            }

            try
            {


                egisConnection.Open();
                //поиск путей

                SqlCommand command1 = new SqlCommand(sql, egisConnection);
                SqlDataReader reader = command1.ExecuteReader();

                
                //EgisFoundPointObjectsGrid.Items.Refresh();

                while (reader.Read())
                {

                    Track t = new Track();

                    t.TrackID = Convert.ToDouble(reader[2]);
                    t.TrackNumber = reader[3].ToString() + " " + reader[4].ToString();
                    _EgisSelectedTracks.Add(t);

                    PointOnTrack p = new PointOnTrack();

                    p.TrackObjectName = reader[0].ToString();
                    p.TrackNumber = reader[3].ToString() + " " + reader[4].ToString();
                    p.TrackID = Convert.ToDouble(reader[2]);
                    p.PointOnTrackKm = reader[5].ToString();
                    p.PointOnTrackPk = Convert.ToDouble(reader[6]);
                    p.PointOnTrackM = Convert.ToDouble(reader[7]);

                    _EgisFoundPointObjects.Add(p);

                    //MessageBox.Show(s.StationName +"  "+ s.EgisStationID.ToString());
                }

                


            }
            catch (Exception ex)
            {
                MessageBox.Show(ex.Message);
            }
            finally
            {
                egisConnection.Close();

            }
            
        }

        public void LoadEgisSegments()
        {
            //Track item = (Track)EgisTrackGrid.SelectedItem;

            double EgisTrackID = EgisSelectedTrack.TrackID;
            if (EgisSelectedTrack != null)
            {
                //  EgisTrackID = item.TrackID;
            }
            else
            {

            }
            // по убыванию
            string sql1 = "SELECT S.SegmentID, S.SegmentName, S.SegmentLength, S.TrackID, S.StationID " +
                          "FROM Segment S " +
                          "join PointOnTrack PP on PP.SegmentID = S.SegmentID " +
                          "and PP.DicPointOnTrackKindID in (22, 27, 28) " +
                          "and PP.PointOnTrackCoordinate = 0 " +
                          "WHERE S.TrackID = " + EgisTrackID.ToString() + " " +
                          "order by cast (PP.PointOnTrackKm as int) * 10000 + PP.PointOnTrackPk * 100 + PP.PointOnTrackM, PP.PointOnTrackCoordinate asc";

            //по возрастанию
            string sql2 = "SELECT S.SegmentID, S.SegmentName, S.SegmentLength, S.TrackID, S.StationID " +
                        "FROM Segment S " +
                        "join PointOnTrack PP on PP.SegmentID = S.SegmentID " +
                        "and PP.DicPointOnTrackKindID in (22, 27, 28) " +
                        "and PP.PointOnTrackCoordinate = 0 " +
                        "WHERE S.TrackID = " + EgisTrackID.ToString() + " " +
                        "order by cast (PP.PointOnTrackKm as int) * 10000 + PP.PointOnTrackPk * 100 + PP.PointOnTrackM, PP.PointOnTrackCoordinate desc";

            try
            {

                _egisConnection.Open();

                string sql = "";
                
                sql = sql2;
                
                SqlCommand command = new SqlCommand(sql, _egisConnection);
                SqlDataReader reader = command.ExecuteReader();

                //EgisSegments.Clear();

                while (reader.Read())
                {

                    Segment s = new Segment();
                    
                    s.PredefinedRouteSegmentFromStartToEnd = 1;
                    

                    s.SegmentID = Convert.ToDouble(reader[0]);
                    s.SegmentName = reader[1].ToString();
                    s.SegmentLength = Convert.ToDouble(reader[2]);
                    s.TrackID = Convert.ToDouble(reader[3]);
                    

                    string e = reader[4].ToString();
                    if (e != "")
                    {
                        s.StationID = Convert.ToDouble(e);
                    }


                    _egisRoute.Segments.Add(s);

                    
                }

            }
            catch (Exception ex)
            {
                MessageBox.Show(ex.Message);
            }
            finally
            {
                _egisConnection.Close();

            }
        }

        public void LoadEgisLongObjects()
        {
            string sql = "select PP.DicPointOnTrackKindID, PP.TrackObjectID, PP.SegmentID, PP.PointOnTrackUsageDirection, PP.PointOnTrackCoordinate, PP.PointOnTrackKm, PP.PointOnTrackPk, PP.PointOnTrackM, T.TrackObjectName " +
                "from TrackObject T " +
                "join PointOnTrack PP " +
                " on PP.TrackObjectID = T.TrackObjectID and PP.DicPointOnTrackKindID in (35, 34, 33) " +
                "join Segment S " +
                "on S.SegmentID = PP.SegmentID " +
                //"join TrackObjectProperty TP1 on TP1.TrackObjectID = T.TrackObjectID and TP1.DicTrackObjectPropertyKindID = 2 " +
                //"right join TrackObjectProperty TP3 on TP3.TrackObjectID = T.TrackObjectID and TP3.DicTrackObjectPropertyKindID = 3 " + // ссылка на станцию 
                //"where T.DicTrackObjectKindID in (1) " +  // and T.TrackObjectID in (293326, 288878) " + // id станции
                "and S.TrackID = " + EgisSelectedTrack.TrackID.ToString() + " " +
                "order by cast (PP.PointOnTrackKm as int) * 10000 + PP.PointOnTrackPk * 100 + PP.PointOnTrackM, PP.PointOnTrackCoordinate desc";



            try
            {

                _egisConnection.Open();

                SqlCommand command = new SqlCommand(sql, _egisConnection);
                SqlDataReader reader = command.ExecuteReader();



                while (reader.Read())
                {

                    PointOnTrack p = new PointOnTrack();
                    p.EgisDicPointOnTrackKindID = Convert.ToDouble(reader[0]);
                    p.TrackObjectID = Convert.ToDouble(reader[1]);
                    p.SegmentID = Convert.ToDouble(reader[2]);
                    p.PointOnTrackUsageDirection = Convert.ToDouble(reader[3]);
                    p.PointOnTrackCoordinate = Convert.ToDouble(reader[4]);
                    p.PointOnTrackKm = reader[5].ToString();
                    p.PointOnTrackPk = Convert.ToDouble(reader[6]);
                    p.PointOnTrackM = Convert.ToDouble(reader[7]);

                    p.GetPoinOntrackKindFromEgis();
                    _egisRoute.PointOnTracks.Add(p);


                    if (p.EgisDicPointOnTrackKindID == 35)
                    {
                        NeutralSection s = new NeutralSection();
                        s.TrackObjectID = s.Start.TrackObjectID = p.TrackObjectID;
                        s.End.TrackObjectID = p.TrackObjectID;
                        int index = _egisRoute.NeutralSections.FindIndex(y => (y.TrackObjectID == s.TrackObjectID));
                        if (index < 0)
                        {
                            if (p.PointOnTrackUsageDirection == 1) { s.Start = p; }
                            if (p.PointOnTrackUsageDirection == -1) { s.End = p; }
                            _egisRoute.NeutralSections.Add(s);
                        }
                        if (index >= 0)
                        {
                            if (p.PointOnTrackUsageDirection == 1) { _egisRoute.NeutralSections[index].Start = p; }
                            if (p.PointOnTrackUsageDirection == -1) { _egisRoute.NeutralSections[index].End = p; }
                        }
                    }
                    else if (p.EgisDicPointOnTrackKindID == 33)
                    {
                        RailBridge s = new RailBridge();
                        s.TrackObjectID = s.Start.TrackObjectID = p.TrackObjectID;
                        s.End.TrackObjectID = p.TrackObjectID;
                        int index = _egisRoute.RailBridges.FindIndex(y => (y.TrackObjectID == s.TrackObjectID));
                        if (index < 0)
                        {
                            if (p.PointOnTrackUsageDirection == 1) { s.Start = p; }
                            if (p.PointOnTrackUsageDirection == -1) { s.End = p; }
                            _egisRoute.RailBridges.Add(s);
                        }
                        if (index >= 0)
                        {
                            if (p.PointOnTrackUsageDirection == 1) { _egisRoute.RailBridges[index].Start = p; }
                            if (p.PointOnTrackUsageDirection == -1) { _egisRoute.RailBridges[index].End = p; }
                        }
                    }
                    else if (p.EgisDicPointOnTrackKindID == 34)
                    {
                        Tunnel s = new Tunnel();
                        s.TrackObjectID = s.Start.TrackObjectID = p.TrackObjectID;
                        s.End.TrackObjectID = p.TrackObjectID;
                        int index = _egisRoute.Tunnels.FindIndex(y => (y.TrackObjectID == s.TrackObjectID));
                        if (index < 0)
                        {
                            if (p.PointOnTrackUsageDirection == 1) { s.Start = p; }
                            if (p.PointOnTrackUsageDirection == -1) { s.End = p; }
                            _egisRoute.Tunnels.Add(s);
                        }
                        if (index >= 0)
                        {
                            if (p.PointOnTrackUsageDirection == 1) { _egisRoute.Tunnels[index].Start = p; }
                            if (p.PointOnTrackUsageDirection == -1) { _egisRoute.Tunnels[index].End = p; }
                        }
                    }
                }


            }
            catch (Exception ex)
            {
                MessageBox.Show(ex.Message);
            }
            finally
            {
                _egisConnection.Close();

            }
        }

        public void LoadEgisStations() // загрузить станции и платформы
        {

            string sql = "select PP.DicPointOnTrackKindID, PP.TrackObjectID, PP.SegmentID, PP.PointOnTrackUsageDirection, PP.PointOnTrackCoordinate, PP.PointOnTrackKm, PP.PointOnTrackPk, PP.PointOnTrackM, T.TrackObjectName, TP1.TrackObjectPropertyValue, TP1.TrackObjectPropertyValue " +
                "from TrackObject T " +
                "join PointOnTrack PP " +
                " on PP.TrackObjectID = T.TrackObjectID and PP.DicPointOnTrackKindID in (8, 9) " +
                "join Segment S " +
                "on S.SegmentID = PP.SegmentID " +
                "join TrackObjectProperty TP1 on TP1.TrackObjectID = T.TrackObjectID and TP1.DicTrackObjectPropertyKindID = 2 " +
                //"right join TrackObjectProperty TP3 on TP3.TrackObjectID = T.TrackObjectID and TP3.DicTrackObjectPropertyKindID = 3 " + // ссылка на станцию 
                //"where T.DicTrackObjectKindID in (1) " +  // and T.TrackObjectID in (293326, 288878) " + // id станции
                "and S.TrackID = " + EgisSelectedTrack.TrackID.ToString() + " " +
                "order by cast (PP.PointOnTrackKm as int) * 10000 + PP.PointOnTrackPk * 100 + PP.PointOnTrackM, PP.PointOnTrackCoordinate desc";



            try
            {

                _egisConnection.Open();

                SqlCommand command = new SqlCommand(sql, _egisConnection);
                SqlDataReader reader = command.ExecuteReader();



                while (reader.Read())
                {

                    PointOnTrack p = new PointOnTrack();
                    p.EgisDicPointOnTrackKindID = Convert.ToDouble(reader[0]);
                    p.TrackObjectID = Convert.ToDouble(reader[1]);
                    p.SegmentID = Convert.ToDouble(reader[2]);
                    p.PointOnTrackUsageDirection = Convert.ToDouble(reader[3]);
                    p.PointOnTrackCoordinate = Convert.ToDouble(reader[4]);
                    p.PointOnTrackKm = reader[5].ToString();
                    p.PointOnTrackPk = Convert.ToDouble(reader[6]);
                    p.PointOnTrackM = Convert.ToDouble(reader[7]);

                    p.GetPoinOntrackKindFromEgis();
                    _egisRoute.PointOnTracks.Add(p);


                    if (p.EgisDicPointOnTrackKindID == 8) //добавить станцию
                    {
                        Station s = new Station();


                        s.Start.TrackObjectID = p.TrackObjectID;
                        s.End.TrackObjectID = p.TrackObjectID;

                        s.EgisStationID = Convert.ToDouble(reader[1]);
                        s.StationName = reader[9].ToString();

                        int index = _egisRoute.Stations.FindIndex(y => (y.EgisStationID == s.EgisStationID));

                        if (index < 0)
                        {
                            if (p.PointOnTrackUsageDirection == 1) { s.Start = p; }
                            if (p.PointOnTrackUsageDirection == -1) { s.End = p; }

                            _egisRoute.Stations.Add(s);
                        }
                        if (index >= 0)
                        {
                            if (p.PointOnTrackUsageDirection == 1) { _egisRoute.Stations[index].Start = p; }
                            if (p.PointOnTrackUsageDirection == -1) { _egisRoute.Stations[index].End = p; }

                        }


                        //s.StationName = s.StationName.ToLower();


                        s.StationName = DbRouteHelper.CapitalizeAllWords(s.StationName);



                    }
                    else if (p.EgisDicPointOnTrackKindID == 9)// добавить платформу
                    {
                        Platform s = new Platform();


                        s.Start.TrackObjectID = p.TrackObjectID;
                        s.End.TrackObjectID = p.TrackObjectID;

                        s.TrackObjectID = Convert.ToDouble(reader[1]);
                        s.PlatformName = reader[9].ToString();



                        int index1 = _egisRoute.Platforms.FindIndex(y => (y.TrackObjectID == s.TrackObjectID));

                        if (index1 < 0)
                        {
                            if (p.PointOnTrackUsageDirection == 1) { s.Start = p; }
                            if (p.PointOnTrackUsageDirection == -1) { s.End = p; }

                            _egisRoute.Platforms.Add(s);
                        }
                        else if (index1 >= 0)
                        {
                            if (p.PointOnTrackUsageDirection == 1) { _egisRoute.Platforms[index1].Start = p; }
                            if (p.PointOnTrackUsageDirection == -1) { _egisRoute.Platforms[index1].End = p; }

                        }
                    }
                }
            }
            catch (Exception ex)
            {
                MessageBox.Show(ex.Message);
            }
            finally
            {
                _egisConnection.Close();

            }
        }



        public void LoadEgisCrossings()
        {
            string sql = "SELECT T.TrackObjectID, T.TrackObjectName, " +
                "PP1.PointOnTrackKm, PP1.PointOnTrackPk, PP1.PointOnTrackM, PP1.PointOnTrackCoordinate, PP1.SegmentID, " +
                "PP2.PointOnTrackKm, PP2.PointOnTrackPk, PP2.PointOnTrackM, PP2.PointOnTrackCoordinate, PP2.SegmentID, " +
                "PV.TrackObjectPropertyValue " +
            "from TrackObject T " +
            "join TrackObjectProperty PV " +
            "on PV.TrackObjectID = T.TrackObjectID " +
            "and PV.DicTrackObjectPropertyKindID = 20 " +
            "join PointOnTrack PP1 " +
            "on PP1.TrackObjectID = T.TrackObjectID " +
            "and PP1.PointOnTrackUsageDirection = 1 " +
            "join PointOnTrack PP2 " +
            "on PP2.TrackObjectID = T.TrackObjectID " +
            "and PP2.PointOnTrackUsageDirection = -1  " +
            "join Segment S " +
            "on S.SegmentID = PP1.SegmentID " +
            "where S.TrackID = " + EgisSelectedTrack.TrackID.ToString() + " " +
            "and T.DicTrackObjectKindID = 9 " +
            "order by cast(PP1.PointOnTrackKm as int) * 10000 + PP1.PointOnTrackPk * 100 + PP1.PointOnTrackM, PP1.PointOnTrackCoordinate desc ";


            try
            {

                _egisConnection.Open();

                SqlCommand command = new SqlCommand(sql, _egisConnection);
                SqlDataReader reader = command.ExecuteReader();



                while (reader.Read())
                {

                    Crossing c = new Crossing();
                    c.TrackObjectID = Convert.ToDouble(reader[0]);

                    c.Start.PointOnTrackKm = reader[2].ToString();
                    c.Start.PointOnTrackPk = Convert.ToDouble(reader[3]);
                    c.Start.PointOnTrackM = Convert.ToDouble(reader[4]);
                    c.Start.PointOnTrackCoordinate = Convert.ToDouble(reader[5]);
                    c.Start.SegmentID = Convert.ToDouble(reader[6]);
                    c.Start.PointOnTrackUsageDirection = 1;


                    c.Start.EgisDicPointOnTrackKindID = 23;
                    c.Start.DicPointOnTrackKindID = 23;

                    c.End.PointOnTrackKm = reader[7].ToString();
                    c.End.PointOnTrackPk = Convert.ToDouble(reader[8]);
                    c.End.PointOnTrackM = Convert.ToDouble(reader[9]);
                    c.End.PointOnTrackCoordinate = Convert.ToDouble(reader[10]);
                    c.End.SegmentID = Convert.ToDouble(reader[11]);
                    c.End.PointOnTrackUsageDirection = -1;


                    c.End.EgisDicPointOnTrackKindID = 23;
                    c.End.DicPointOnTrackKindID = 23;

                    c.EgisDicCrossingKindID = Convert.ToDouble(reader[12]);

                    if (c.EgisDicCrossingKindID == 16)
                    { c.DicCrossingKindID = 1; }
                    else { c.DicCrossingKindID = 2; }


                    _egisRoute.PointOnTracks.Add(c.Start);
                    _egisRoute.PointOnTracks.Add(c.End);

                    _egisRoute.Crossings.Add(c);
                }
            }
            catch (Exception ex)
            {
                MessageBox.Show(ex.Message);
            }
            finally
            {
                _egisConnection.Close();
            }

        }

        public void LoadEgisPt()
        {
            string sql = "SELECT T.TrackObjectID, T.TrackObjectName, " +
                         "PP1.PointOnTrackKm, PP1.PointOnTrackPk, PP1.PointOnTrackM, PP1.PointOnTrackCoordinate, PP1.SegmentID, " +
                         "PP2.PointOnTrackKm, PP2.PointOnTrackPk, PP2.PointOnTrackM, PP2.PointOnTrackCoordinate, PP2.SegmentID, " +
                         "PV.TrackObjectPropertyValue, " + //12
                         "PV2.TrackObjectPropertyValue, " + //13
                         "PV3.TrackObjectPropertyValue, " + //14
                         "PV4.TrackObjectPropertyValue, " + //15
                         "PV5.TrackObjectPropertyValue " + //16
                         "from TrackObject T " +

                         "left join TrackObjectProperty PV " +
                         "on PV.TrackObjectID = T.TrackObjectID " +
                         "and PV.DicTrackObjectPropertyKindID = 15 " + //12 скорость

                         "left join TrackObjectProperty PV2 " +
                         "on PV2.TrackObjectID = T.TrackObjectID " +
                         "and PV2.DicTrackObjectPropertyKindID = 16 " + //13 норма пути

                         "left join TrackObjectProperty PV3 " +
                         "on PV3.TrackObjectID = T.TrackObjectID " +
                         "and PV3.DicTrackObjectPropertyKindID = 1 " + //14 вид движения

                         "left join TrackObjectProperty PV4 " +
                         "on PV4.TrackObjectID = T.TrackObjectID " +
                         "and PV4.DicTrackObjectPropertyKindID = 14 " + //15 направление использования

                         "left join TrackObjectProperty PV5 " +
                         "on PV5.TrackObjectID = T.TrackObjectID " +
                         "and PV5.DicTrackObjectPropertyKindID = 13 " + //16 тип проверки 10 - пт 11 - эпт

                         "join PointOnTrack PP1 " +
                         "on PP1.TrackObjectID = T.TrackObjectID " +
                         "and PP1.PointOnTrackUsageDirection = 1 " +
                         "join PointOnTrack PP2 " +
                         "on PP2.TrackObjectID = T.TrackObjectID " +
                         "and PP2.PointOnTrackUsageDirection = -1  " +

                         "join Segment S " +
                         "on S.SegmentID = PP1.SegmentID " +
                         "where S.TrackID = " + EgisSelectedTrack.TrackID.ToString() + " " +
                         "and T.DicTrackObjectKindID = 7 " +
                         "order by cast(PP1.PointOnTrackKm as int) * 10000 + PP1.PointOnTrackPk * 100 + PP1.PointOnTrackM, PP1.PointOnTrackCoordinate desc ";

            try
            {

                _egisConnection.Open();

                SqlCommand command = new SqlCommand(sql, _egisConnection);
                SqlDataReader reader = command.ExecuteReader();



                while (reader.Read())
                {

                    BrakeCheckNorm npd = new BrakeCheckNorm();
                    npd.BrakeCheckPlaceID = Convert.ToDouble(reader[0]);
                    npd.BrakeCheckNormSpeed = Convert.ToDouble(reader[12]);
                    npd.BrakeCheckNormPath = Convert.ToDouble(reader[13]);

                    BrakeCheckPlace bf = _egisRoute.BrakeCheckPlaces.Find(x => x.TrackObjectID == npd.BrakeCheckPlaceID);

                    if (bf != null)
                    {
                        bf.BrakeCheckNormList.Add(npd);
                    }
                    else
                    {
                        BrakeCheckPlace spd = new BrakeCheckPlace();

                        spd.BrakeCheckNormList.Add(npd);

                        spd.TrackObjectID = Convert.ToDouble(reader[0]);
                        spd.TrackObjectName = reader[1].ToString();
                        spd.Start.PointOnTrackKm = reader[2].ToString();
                        spd.Start.PointOnTrackPk = Convert.ToDouble(reader[3]);
                        spd.Start.PointOnTrackM = Convert.ToDouble(reader[4]);
                        spd.Start.PointOnTrackCoordinate = Convert.ToDouble(reader[5]);
                        spd.Start.SegmentID = Convert.ToDouble(reader[6]);
                        spd.Start.PointOnTrackUsageDirection = 1;

                        spd.Start.DicPointOnTrackKindID = 6;
                        spd.Start.EgisDicPointOnTrackKindID = 6;

                        spd.End.PointOnTrackKm = reader[7].ToString();
                        spd.End.PointOnTrackPk = Convert.ToDouble(reader[8]);
                        spd.End.PointOnTrackM = Convert.ToDouble(reader[9]);
                        spd.End.PointOnTrackCoordinate = Convert.ToDouble(reader[10]);
                        spd.End.SegmentID = Convert.ToDouble(reader[11]);
                        spd.End.PointOnTrackUsageDirection = -1;

                        spd.End.DicPointOnTrackKindID = 6;
                        spd.End.EgisDicPointOnTrackKindID = 6;

                        spd.EgisDicTrafficKind = Convert.ToDouble(reader[14]);


                        if (reader[15].ToString() != "")
                        {
                            spd.BrakeCheckPlaceDirection = Convert.ToDouble(reader[15]);

                        }


                        if (reader[16].ToString() != "")
                        {
                            spd.EgisDicBrakeCheckKindID = Convert.ToDouble(reader[16]);

                            spd.GetDicBrakeCheckKindFromEgis();

                            //if (spd.EgisDicBrakeCheckKindID == 10) spd.DicBrakeCheckKindID = 0; // пт
                            //else if (spd.EgisDicBrakeCheckKindID == 11) spd.DicBrakeCheckKindID = 1; // эпт


                        }
                        if ((spd.EgisDicTrafficKind == _speedKindToFind || _speedKindToFind == 0) && (spd.BrakeCheckPlaceDirection * -1 != _usageDirectionToFind))
                        {
                            _egisRoute.BrakeCheckPlaces.Add(spd);

                            _egisRoute.PointOnTracks.Add(spd.Start);
                            _egisRoute.PointOnTracks.Add(spd.End);
                        }
                    }

                }


            }
            catch (Exception ex)
            {
                MessageBox.Show(ex.Message);
            }
            finally
            {
                _egisConnection.Close();
            }
        }

        public void LoadEgisSpeedrestrictions()
        {
            string sql = "SELECT T.TrackObjectID, T.TrackObjectName, " +
                "PP1.PointOnTrackKm, PP1.PointOnTrackPk, PP1.PointOnTrackM, PP1.PointOnTrackCoordinate, PP1.SegmentID, " +
                "PP2.PointOnTrackKm, PP2.PointOnTrackPk, PP2.PointOnTrackM, PP2.PointOnTrackCoordinate, PP2.SegmentID, " +
                "PV.TrackObjectPropertyValue, " +
                "PV2.TrackObjectPropertyValue, " +
                "PV3.TrackObjectPropertyValue, " +
                "PV4.TrackObjectPropertyValue " +
            "from TrackObject T " +

            "join TrackObjectProperty PV " +
            "on PV.TrackObjectID = T.TrackObjectID " +
            "and PV.DicTrackObjectPropertyKindID = 17 " + // ограничивающая скорость

            "join TrackObjectProperty PV2 " +
            "on PV2.TrackObjectID = T.TrackObjectID " +
            "and PV2.DicTrackObjectPropertyKindID = 1 " + // вид движения

            "left join TrackObjectProperty PV3 " +
            "on PV3.TrackObjectID = T.TrackObjectID " +
            "and PV3.DicTrackObjectPropertyKindID = 79 " + //направление действия ограничения

            "left join TrackObjectProperty PV4 " +
            "on PV4.TrackObjectID = T.TrackObjectID " +
            "and PV4.DicTrackObjectPropertyKindID = 19 " + // порожнего поезда

            "join PointOnTrack PP1 " +
            "on PP1.TrackObjectID = T.TrackObjectID " +
            "and PP1.PointOnTrackUsageDirection = 1 " +
            "join PointOnTrack PP2 " +
            "on PP2.TrackObjectID = T.TrackObjectID " +
            "and PP2.PointOnTrackUsageDirection = -1  " +

            "join Segment S " +
            "on S.SegmentID = PP1.SegmentID " +
            "where S.TrackID = " + EgisSelectedTrack.TrackID.ToString() + " " +
            "and T.DicTrackObjectKindID = 8 " +
            "order by cast(PP1.PointOnTrackKm as int) * 10000 + PP1.PointOnTrackPk * 100 + PP1.PointOnTrackM, PP1.PointOnTrackCoordinate desc ";


            try
            {

                _egisConnection.Open();

                SqlCommand command = new SqlCommand(sql, _egisConnection);
                SqlDataReader reader = command.ExecuteReader();



                while (reader.Read())
                {
                    SpeedRestriction spd = new SpeedRestriction(Convert.ToDouble(reader[12]), 0, 0);



                    spd.TrackObjectID = Convert.ToDouble(reader[0]);
                    spd.TrackObjectName = reader[1].ToString();
                    spd.Start.PointOnTrackKm = reader[2].ToString();
                    spd.Start.PointOnTrackPk = Convert.ToDouble(reader[3]);
                    spd.Start.PointOnTrackM = Convert.ToDouble(reader[4]);
                    spd.Start.PointOnTrackCoordinate = Convert.ToDouble(reader[5]);
                    spd.Start.SegmentID = Convert.ToDouble(reader[6]);
                    spd.Start.PointOnTrackUsageDirection = 1;

                    spd.Start.DicPointOnTrackKindID = 2;
                    spd.Start.EgisDicPointOnTrackKindID = 2;

                    spd.End.PointOnTrackKm = reader[7].ToString();
                    spd.End.PointOnTrackPk = Convert.ToDouble(reader[8]);
                    spd.End.PointOnTrackM = Convert.ToDouble(reader[9]);
                    spd.End.PointOnTrackCoordinate = Convert.ToDouble(reader[10]);
                    spd.End.SegmentID = Convert.ToDouble(reader[11]);
                    spd.End.PointOnTrackUsageDirection = -1;

                    spd.End.DicPointOnTrackKindID = 2;
                    spd.End.EgisDicPointOnTrackKindID = 2;

                    spd.DicTrafficKind = Convert.ToDouble(reader[13]);

                    double speeddirection = 0;
                    // 101 - по возрастанию 102 по убыванию DicTrackObjectPropertyKindID=79
                    if (reader[14].ToString() != "")
                    {
                        if (Convert.ToDouble(reader[14]) == 101) speeddirection = 1;
                        else if (Convert.ToDouble(reader[14]) == 102) speeddirection = -1;
                    }

                    if (reader[15].ToString() != "")
                    {
                        double PValue = Convert.ToDouble(reader[15]);
                        if (PValue == 55 || PValue == 103 || PValue == 226)
                        {
                            spd.PermRestrictionForEmptyTrain = 1;
                        }
                    }




                    if ((spd.DicTrafficKind == _speedKindToFind || _speedKindToFind == 0) && speeddirection * -1 != _usageDirectionToFind)
                    {
                        _egisRoute.PointOnTracks.Add(spd.Start);
                        _egisRoute.PointOnTracks.Add(spd.End);

                        _egisRoute.SpeedRestrictions.Add(spd);
                    }



                }

                //EgisSpeedRestrictions.Sort(scts);
            }
            catch (Exception ex)
            {
                MessageBox.Show(ex.Message);
            }
            finally
            {
                _egisConnection.Close();

            }
        }

        public void LoadEgisInclines()
        {


            string sql = "SELECT T.TrackObjectID, T.TrackObjectName, " +
                "PP1.PointOnTrackKm, PP1.PointOnTrackPk, PP1.PointOnTrackM, PP1.PointOnTrackCoordinate, PP1.SegmentID, " +
                "PP2.PointOnTrackKm, PP2.PointOnTrackPk, PP2.PointOnTrackM, PP2.PointOnTrackCoordinate, PP2.SegmentID, " +
                "replace(PV.TrackObjectPropertyValue,'.',',') incline " +
            "from TrackObject T " +
            "join TrackObjectProperty PV " +
            "on PV.TrackObjectID = T.TrackObjectID " +
            "and PV.DicTrackObjectPropertyKindID = 21 " +
            "join PointOnTrack PP1 " +
            "on PP1.TrackObjectID = T.TrackObjectID " +
            "and PP1.PointOnTrackUsageDirection = 1 " +
            "join PointOnTrack PP2 " +
            "on PP2.TrackObjectID = T.TrackObjectID " +
            "and PP2.PointOnTrackUsageDirection = -1  " +
            "join Segment S " +
            "on S.SegmentID = PP1.SegmentID " +
            "where S.TrackID = " + EgisSelectedTrack.TrackID.ToString() + " " +
            "and T.DicTrackObjectKindID = 10 " +
            "order by cast(PP1.PointOnTrackKm as int) * 10000 + PP1.PointOnTrackPk * 100 + PP1.PointOnTrackM, PP1.PointOnTrackCoordinate desc ";


            try
            {

                _egisConnection.Open();

                SqlCommand command = new SqlCommand(sql, _egisConnection);
                SqlDataReader reader = command.ExecuteReader();



                while (reader.Read())
                {

                    Incline incline = new Incline(Convert.ToDouble(reader[12]));

                    incline.TrackObjectID = Convert.ToDouble(reader[0]);
                    incline.TrackObjectName = reader[1].ToString();
                    incline.Start.PointOnTrackKm = reader[2].ToString();
                    incline.Start.PointOnTrackPk = Convert.ToDouble(reader[3]);
                    incline.Start.PointOnTrackM = Convert.ToDouble(reader[4]);
                    incline.Start.PointOnTrackCoordinate = Convert.ToDouble(reader[5]);
                    incline.Start.SegmentID = Convert.ToDouble(reader[6]);
                    incline.Start.PointOnTrackUsageDirection = 1;
                    incline.Start.DicPointOnTrackKindID = 32;
                    incline.Start.EgisDicPointOnTrackKindID = 32;
                    incline.End.PointOnTrackKm = reader[7].ToString();
                    incline.End.PointOnTrackPk = Convert.ToDouble(reader[8]);
                    incline.End.PointOnTrackM = Convert.ToDouble(reader[9]);
                    incline.End.PointOnTrackCoordinate = Convert.ToDouble(reader[10]);
                    incline.End.SegmentID = Convert.ToDouble(reader[11]);
                    incline.End.PointOnTrackUsageDirection = -1;
                    incline.End.DicPointOnTrackKindID = 32;
                    incline.End.EgisDicPointOnTrackKindID = 32;

                    //incline.Start.GetPoinOntrackKindFromEgis();
                    //incline.End.GetPoinOntrackKindFromEgis();



                    _egisRoute.PointOnTracks.Add(incline.Start);
                    _egisRoute.PointOnTracks.Add(incline.End);

                    _egisRoute.Inclines.Add(incline);




                }


            }
            catch (Exception ex)
            {
                MessageBox.Show(ex.Message);
            }
            finally
            {
                _egisConnection.Close();

            }
        }

        //загрузить укспс ктсм свисток
        public void LoadEgisShortObjects()
        {

            string sql =

            "select T.TrackObjectID, S.SegmentID, " +
            "PP.PointOnTrackUsageDirection, PP.PointOnTrackKm, PP.PointOnTrackPk, PP.PointOnTrackM, PP.PointOnTrackCoordinate, " +
            "PP.DicPointOnTrackKindID " +
            //"(TP1.TrackObjectPropertyValue)name, (TP2.TrackObjectPropertyValue)dictypeID " +
            "from TrackObject T " +
            "join PointOnTrack PP " +
            "on PP.TrackObjectID = T.TrackObjectID " +
            "and PP.DicPointOnTrackKindID in (25, 24, 49, 55) " +  // 25-укспс 24-диск 49-ктсм 55-свисток
            "join Segment S " +
            "on S.SegmentID = PP.SegmentID " +
            //"join TrackObjectProperty TP1 on TP1.TrackObjectID = T.TrackObjectID and TP1.DicTrackObjectPropertyKindID = 2 " +
            //"left join TrackObjectProperty TP2 on TP2.TrackObjectID = T.TrackObjectID and TP2.DicTrackObjectPropertyKindID = 9 " +
            "where S.TrackID = " + EgisSelectedTrack.TrackID.ToString() + " " +
            //"and T.DicTrackObjectKindID in (16) " +
            //"and PP.PointOnTrackUsageDirection = " + UsageDirectionToFind.ToString() + " " +
            "and PP.PointOnTrackUsageDirection in (0, " + _usageDirectionToFind.ToString() + ") " +
            "order by cast(PP.PointOnTrackKm as int) * 10000 + PP.PointOnTrackPk * 100 + PP.PointOnTrackM, PP.PointOnTrackCoordinate desc";



            try
            {

                _egisConnection.Open();

                SqlCommand command = new SqlCommand(sql, _egisConnection);
                SqlDataReader reader = command.ExecuteReader();



                while (reader.Read())
                {


                    PointOnTrack p = new PointOnTrack();



                    p.TrackObjectID = Convert.ToDouble(reader[0]);
                    p = new PointOnTrack();
                    p.SegmentID = Convert.ToDouble(reader[1]);
                    p.PointOnTrackUsageDirection = Convert.ToDouble(reader[2]);


                    p.PointOnTrackKm = reader[3].ToString();
                    p.PointOnTrackPk = Convert.ToDouble(reader[4]);
                    p.PointOnTrackM = Convert.ToDouble(reader[5]);
                    p.PointOnTrackCoordinate = Convert.ToDouble(reader[6]);

                    p.EgisDicPointOnTrackKindID = Convert.ToDouble(reader[7]);

                    p.GetPoinOntrackKindFromEgis();



                    if (p.EgisDicPointOnTrackKindID == 25)
                    {
                        Uksps l = new Uksps();
                        l.Start = p;
                        _egisRoute.UkspsList.Add(l);
                            _egisRoute.PointOnTracks.Add(p);
                    }
                    else if (p.EgisDicPointOnTrackKindID == 55)
                    {
                        TrafficSignal l = new TrafficSignal();
                        l.DicTrafficSignalKindID = 16;
                        l.Start = p;
                        _egisRoute.TrafficSignals.Add(l);
                        _egisRoute.PointOnTracks.Add(p);
                    }
                    else if (p.EgisDicPointOnTrackKindID == 24 || p.EgisDicPointOnTrackKindID == 49)
                    {
                        Ktsm l = new Ktsm();

                        l.Start = p;
                        _egisRoute.KtsmList.Add(l);
                        _egisRoute.PointOnTracks.Add(p);
                    }
                }


            }
            catch (Exception ex)
            {
                MessageBox.Show(ex.Message);
            }
            finally
            {
                _egisConnection.Close();

            }
        }

        public void LoadEgisTrts()
        {
            //загрузить трц - подвижные блок - участки

            string sql =

                "select T.TrackObjectID, S.SegmentID, " +
                "PP.PointOnTrackUsageDirection, PP.PointOnTrackKm, PP.PointOnTrackPk, PP.PointOnTrackM, PP.PointOnTrackCoordinate, " +
                "(TP1.TrackObjectPropertyValue)name " +
                "from TrackObject T " +
                "join PointOnTrack PP " +
                "on PP.TrackObjectID = T.TrackObjectID " +
                //"and PP.DicPointOnTrackKindID = 1 " +
                "join Segment S " +
                "on S.SegmentID = PP.SegmentID " +
                "join TrackObjectProperty TP1 on TP1.TrackObjectID = T.TrackObjectID and TP1.DicTrackObjectPropertyKindID = 2 " +
                //"left join TrackObjectProperty TP2 on TP2.TrackObjectID = T.TrackObjectID and TP2.DicTrackObjectPropertyKindID = 9 " +
                "where S.TrackID = " + EgisSelectedTrack.TrackID.ToString() + " " +
                "and T.DicTrackObjectKindID in (46) " +
                //"and PP.PointOnTrackUsageDirection = " + _usageDirectionToFind.ToString() + " " +
                //"and PP.PointOnTrackUsageDirection = 1 " +
                "order by cast(PP.PointOnTrackKm as int) * 10000 + PP.PointOnTrackPk * 100 + PP.PointOnTrackM, PP.PointOnTrackCoordinate desc";


            try
            {
                _egisConnection.Open();

                SqlCommand command = new SqlCommand(sql, _egisConnection);
                //command.Parameters.AddWithValue("@TrackID", EgisSelectedTrack.TrackID); // или передавайте значение как параметр
                //command.Parameters.AddWithValue("@UsageDirection", _usageDirectionToFind); // предполагается, что переменная существует

                SqlDataReader reader = command.ExecuteReader();

                while (reader.Read())
                {
                    TrafficLight l = new TrafficLight();

                    

                    l.DicTrafficLightKindID = 20; // Фиксированное значение 20

                    l.TrackObjectID = Convert.ToDouble(reader[0]);
                    l.Start = new PointOnTrack();
                    l.Start.SegmentID = Convert.ToDouble(reader[1]);
                    l.Start.PointOnTrackUsageDirection = Convert.ToDouble(reader[2]);
                    l.Start.EgisDicPointOnTrackKindID = 1;
                    l.Start.DicPointOnTrackKindID = 1;
                    l.Start.PointOnTrackKm = reader[3].ToString();
                    l.Start.PointOnTrackPk = Convert.ToDouble(reader[4]);
                    l.Start.PointOnTrackM = Convert.ToDouble(reader[5]);
                    l.Start.PointOnTrackCoordinate = Convert.ToDouble(reader[6]);
                    l.TrafficLightName = reader[7].ToString();


                    l.Start.GetPoinOntrackKindFromEgis();

                    // Добавляем
                    _egisRoute.TrafficLights.Add(l);
                    _egisRoute.PointOnTracks.Add(l.Start);
                    
                }
            }
            catch (Exception ex)
            {
                MessageBox.Show(ex.Message);
            }
            finally
            {
                _egisConnection.Close();
            }

        }

        public void LoadEgisTrafficLights()
        {


            string sql =

            "select T.TrackObjectID, S.SegmentID, " +
            "PP.PointOnTrackUsageDirection, PP.PointOnTrackKm, PP.PointOnTrackPk, PP.PointOnTrackM, PP.PointOnTrackCoordinate, " +
            "(TP1.TrackObjectPropertyValue)name, (TP2.TrackObjectPropertyValue)dictypeID " +
            "from TrackObject T " +
            "join PointOnTrack PP " +
            "on PP.TrackObjectID = T.TrackObjectID " +
            "and PP.DicPointOnTrackKindID = 1 " +
            "join Segment S " +
            "on S.SegmentID = PP.SegmentID " +
            "join TrackObjectProperty TP1 on TP1.TrackObjectID = T.TrackObjectID and TP1.DicTrackObjectPropertyKindID = 2 " +
            "left join TrackObjectProperty TP2 on TP2.TrackObjectID = T.TrackObjectID and TP2.DicTrackObjectPropertyKindID = 9 " +
            "where S.TrackID = " + EgisSelectedTrack.TrackID.ToString() + " " +
            "and T.DicTrackObjectKindID in (5) " +
            "and PP.PointOnTrackUsageDirection = " + _usageDirectionToFind.ToString() + " " +
            //"and PP.PointOnTrackUsageDirection = 1 " +
            "order by cast(PP.PointOnTrackKm as int) * 10000 + PP.PointOnTrackPk * 100 + PP.PointOnTrackM, PP.PointOnTrackCoordinate desc";



            try
            {

                _egisConnection.Open();

                SqlCommand command = new SqlCommand(sql, _egisConnection);
                SqlDataReader reader = command.ExecuteReader();



                while (reader.Read())
                {
                    TrafficLight l = new TrafficLight();

                    l.TrackObjectID = Convert.ToDouble(reader[0]);
                    l.Start = new PointOnTrack();
                    l.Start.SegmentID = Convert.ToDouble(reader[1]);
                    l.Start.PointOnTrackUsageDirection = Convert.ToDouble(reader[2]);
                    l.Start.EgisDicPointOnTrackKindID = 1;
                    l.Start.DicPointOnTrackKindID = 1;
                    l.Start.PointOnTrackKm = reader[3].ToString();
                    l.Start.PointOnTrackPk = Convert.ToDouble(reader[4]);
                    l.Start.PointOnTrackM = Convert.ToDouble(reader[5]);
                    l.Start.PointOnTrackCoordinate = Convert.ToDouble(reader[6]);
                    l.TrafficLightName = reader[7].ToString();
                    l.EgisTrafficLightKindValue = Convert.ToDouble(reader[8]);

                    l.Start.GetPoinOntrackKindFromEgis();
                    l.GetDicTrafficLightKindIDFromEgis();


                        // Убрать символы >> из названий светофоров
                    if (!string.IsNullOrEmpty(l.TrafficLightName))
                    {

                        l.TrafficLightName = l.TrafficLightName.Replace(">", "");
                    }


                    //для предвходных светофоров
                    if (l.DicTrafficLightKindID == 0 && ((l.TrafficLightName == "1") || (l.TrafficLightName == "2")))
                    {
                        l.DicTrafficLightKindID = 12;
                    }


                    if (l.DicTrafficLightKindID != 8)
                    {
                        _egisRoute.TrafficLights.Add(l);
                        _egisRoute.PointOnTracks.Add(l.Start);
                    }
                }


            }
            catch (Exception ex)
            {
                MessageBox.Show(ex.Message);
            }
            finally
            {
                _egisConnection.Close();

            }
        }

        public void LoadEgisCrossingPieces()
        {


            string sql =

            "select T.TrackObjectID, S.SegmentID, " +
            "PP.PointOnTrackUsageDirection, PP.PointOnTrackKm, PP.PointOnTrackPk, PP.PointOnTrackM, PP.PointOnTrackCoordinate, " +
            "(TP1.TrackObjectPropertyValue)name, (TP2.TrackObjectPropertyValue)StationID, (TP2.TrackObjectPropertyValue)station " +
            "from TrackObject T " +
            "join PointOnTrack PP " +
            "on PP.TrackObjectID = T.TrackObjectID " +
            "and PP.DicPointOnTrackKindID = 28 " +
            "join Segment S " +
            "on S.SegmentID = PP.SegmentID " +
            "join TrackObjectProperty TP1 on TP1.TrackObjectID = T.TrackObjectID and TP1.DicTrackObjectPropertyKindID = 2 " +
            "left join TrackObjectProperty TP2 on TP2.TrackObjectID = T.TrackObjectID and TP2.DicTrackObjectPropertyKindID = 3 " +
            "join TrackObject T2 " +
            "on T2.TrackObjectID = TP2.TrackObjectPropertyValue and T2.DicTrackObjectKindID = 1 " +
            "join TrackObjectProperty TP21 on TP21.TrackObjectID = T2.TrackObjectID and TP21.DicTrackObjectPropertyKindID = 2 " +
            "where S.TrackID = " + EgisSelectedTrack.TrackID.ToString() + " " +
            "and T.DicTrackObjectKindID in (4) " +
            //"and PP.PointOnTrackUsageDirection = " + UsageDirectionToFind.ToString() + " " +
            //"and PP.PointOnTrackUsageDirection = 1 " +
            "order by cast(PP.PointOnTrackKm as int) * 10000 + PP.PointOnTrackPk * 100 + PP.PointOnTrackM, PP.PointOnTrackCoordinate desc";



            try
            {

                _egisConnection.Open();

                SqlCommand command = new SqlCommand(sql, _egisConnection);
                SqlDataReader reader4 = command.ExecuteReader();



                while (reader4.Read())
                {
                    CrossingPiece crossingPiece = new CrossingPiece();

                    PointOnTrack p = new PointOnTrack();

                    crossingPiece.TrackObjectID = Convert.ToDouble(reader4[0]);
                    p.TrackObjectID = crossingPiece.TrackObjectID;
                    p.SegmentID = Convert.ToDouble(reader4[1]);
                    p.PointOnTrackUsageDirection = Convert.ToDouble(reader4[2]);
                    p.PointOnTrackKm = reader4[3].ToString();
                    p.PointOnTrackPk = Convert.ToDouble(reader4[4]);
                    p.PointOnTrackM = Convert.ToDouble(reader4[5]);
                    p.PointOnTrackCoordinate = Convert.ToDouble(reader4[6]);

                    p.EgisDicPointOnTrackKindID = 28;

                    if (p.PointOnTrackUsageDirection == 1)
                        p.DicPointOnTrackKindID = 28;
                    else
                        p.DicPointOnTrackKindID = 29;

                    crossingPiece.StationID = Convert.ToDouble(reader4[8]);
                    crossingPiece.CrossingPieceName = reader4[7].ToString();
                    crossingPiece.DicCrossingPieceKindID = 1;
                    crossingPiece.CrossingPieceFrogModel = 11;



                    _egisRoute.PointOnTracks.Add(p);


                    int index = _egisRoute.CrossingPieces.FindIndex(x => (x.TrackObjectID == p.TrackObjectID));

                    if (p.PointOnTrackCoordinate == 0)
                        crossingPiece.End = p;
                    else crossingPiece.Start = p;

                    if (index <= 0)
                    {
                        _egisRoute.CrossingPieces.Add(crossingPiece);
                    }
                    if (index >= 0)
                    {
                        if (p.PointOnTrackCoordinate == 0)
                            _egisRoute.CrossingPieces[index].End = p;
                        else _egisRoute.CrossingPieces[index].Start = p;

                    }


                }


            }
            catch (Exception ex)
            {
                MessageBox.Show(ex.Message);
            }
            finally
            {
                _egisConnection.Close();

            }
        }

        public void LoadEgisKmPoints()
        {


            string sql =

            "select " +
            "T.TrackObjectID, S.SegmentID, " +
            "T.DicTrackObjectKindID, PP.DicPointOnTrackKindID, " +
            "T.TrackObjectName, " +
            "PP.PointOnTrackUsageDirection, PP.PointOnTrackKm, PP.PointOnTrackPk, PP.PointOnTrackM, PP.PointOnTrackCoordinate " +
            "from TrackObject T " +
            "join PointOnTrack PP " +
            "on PP.TrackObjectID = T.TrackObjectID " +
            "and PP.DicPointOnTrackKindID = 0 " +
            //"and PP.DicPointOnTrackKindID in (0, 28) " +

            "join Segment S " +
            "on S.SegmentID = PP.SegmentID " +
            "where S.TrackID = " + EgisSelectedTrack.TrackID.ToString() + " " +
            "order by cast(PP.PointOnTrackKm as int) *10000 + PP.PointOnTrackPk * 100 + PP.PointOnTrackM, PP.PointOnTrackCoordinate desc";



            try
            {

                _egisConnection.Open();

                SqlCommand command = new SqlCommand(sql, _egisConnection);
                SqlDataReader reader = command.ExecuteReader();



                while (reader.Read())
                {
                    PointOnTrack p = new PointOnTrack();
                    p.EgisDicPointOnTrackKindID = Convert.ToDouble(reader[3]);
                    p.GetPoinOntrackKindFromEgis();
                    p.SegmentID = Convert.ToDouble(reader[1]);
                    p.PointOnTrackUsageDirection = Convert.ToDouble(reader[5]);
                    p.PointOnTrackKm = reader[6].ToString();
                    p.PointOnTrackPk = Convert.ToDouble(reader[7]);
                    p.PointOnTrackM = Convert.ToDouble(reader[8]);
                    p.PointOnTrackCoordinate = Convert.ToDouble(reader[9]);

                    _egisRoute.PointOnTracks.Add(p);
                }


            }
            catch (Exception ex)
            {
                MessageBox.Show(ex.Message);
            }
            finally
            {
                _egisConnection.Close();
            }

        }

        public void FillEgisKilometers(List<PointOnTrack> _PointOnTracks, List<Segment> _Segments)
        {
            foreach (PointOnTrack p in _PointOnTracks)
            {
                if ((p.EgisDicPointOnTrackKindID == 0))// || (p.RouteCoordinate == 0))
                {

                    Kilometer Kmtoadd = new Kilometer();
                    Kmtoadd.Start = p;
                    Kmtoadd.Km = p.PointOnTrackKm;
                    _egisRoute.Kilometers.Add(Kmtoadd);

                }

            }

            for (int i = 0; i < _egisRoute.Kilometers.Count; i++)
            {
                if (i < _egisRoute.Kilometers.Count - 1)
                    _egisRoute.Kilometers[i].End = _egisRoute.Kilometers[i + 1].Start;
                else if (i == _egisRoute.Kilometers.Count - 1)
                    _egisRoute.Kilometers[i].End = _egisRoute.PointOnTracks[_egisRoute.PointOnTracks.Count - 1];

                _egisRoute.Kilometers[i].Length = Math.Round((_egisRoute.Kilometers[i].End.RouteCoordinate - _egisRoute.Kilometers[i].Start.RouteCoordinate), 2);
            }

        }
        public void FindEndStations()
        {
            foreach (Station s in _egisRoute.Stations)
            {



                if (s.Start.DicPointOnTrackKindID == 0)
                {
                    int index1 = _egisRoute.CrossingPieces.FindIndex(x => x.StationID == s.EgisStationID);
                    if (index1 >= 0)
                    {
                        if (_egisRoute.CrossingPieces[index1].End.DicPointOnTrackKindID != 0)
                            s.Start = _egisRoute.CrossingPieces[index1].End;
                        if (_egisRoute.CrossingPieces[index1].Start.DicPointOnTrackKindID != 0)
                            s.Start = _egisRoute.CrossingPieces[index1].Start;
                    }
                }
                if (s.End.DicPointOnTrackKindID == 0)
                {
                    int index2 = _egisRoute.CrossingPieces.FindLastIndex(x => x.StationID == s.EgisStationID);
                    if (index2 >= 0)
                    {
                        if (_egisRoute.CrossingPieces[index2].End.DicPointOnTrackKindID != 0)
                            s.End = _egisRoute.CrossingPieces[index2].End;
                        if (_egisRoute.CrossingPieces[index2].Start.DicPointOnTrackKindID != 0)
                            s.End = _egisRoute.CrossingPieces[index2].Start;
                    }
                }


            }
        }
    }
}
