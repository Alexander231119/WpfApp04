using System;
using System.Collections;
using System.Collections.Generic;
using System.Data.OleDb;
using System.Globalization;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Controls;
using System.IO;
using Microsoft.Win32;

namespace WpfApp04
{
    public static class DbRouteQuery
    {
        //отдельные запросы для бд

        public static void UpdateAutoBlockFrequency(string connectstring, double autoBlockFrequency)
        {
            //задать AutoBlockFrequency для всех сегментов базы
            string ConnectString = connectstring;
            OleDbConnection myConnection = new OleDbConnection(ConnectString);
            myConnection.Open();
            string query =
                "UPDATE Segment " +
                "SET AutoBlockFrequency = " + autoBlockFrequency.ToString(CultureInfo.InvariantCulture);

            OleDbCommand command4 = new OleDbCommand(query, myConnection);
            command4.ExecuteNonQuery();
            MessageBox.Show("ok");
            myConnection.Close();
        }

        public static void UpdateAutoBlockFrequency1(string connectstring, double autoBlockFrequency)
        {
            // Параметризованный запрос:
            // 
            // Более безопасен от SQL-инъекций

            string ConnectString = connectstring;
            OleDbConnection myConnection = new OleDbConnection(ConnectString);
            myConnection.Open();

            string query = "UPDATE Segment SET AutoBlockFrequency = ?";
            OleDbCommand command = new OleDbCommand(query, myConnection);
            command.Parameters.Add(new OleDbParameter("AutoBlockFrequency", autoBlockFrequency));

            command.ExecuteNonQuery();
            MessageBox.Show("ok");
            myConnection.Close();
        }

        public static void UpdateFrogModels(string connectstring)
        {
            string ConnectString = connectstring;
            OleDbConnection myConnection = new OleDbConnection(ConnectString);
            myConnection.Open();
            string query =
                "UPDATE CrossingPiece " +
                "SET CrossingPieceFrogModel = 11";

            OleDbCommand command4 = new OleDbCommand(query, myConnection);
            command4.ExecuteNonQuery();
            MessageBox.Show("ok");
            myConnection.Close();
        }

        
        public static void DeleteAllObjectsByKind(
            string connectstring,
            string trackObjectTable,
            double dicTrackObjectKindID,
            double dicPointOnTrackKindID)
        {
            //удалить все обьекты используя connectstring

            using (OleDbConnection _myConnection = new OleDbConnection(connectstring))
            {
                _myConnection.Open();

                // Удалить объекты из таблицы, соответствующей trackObjectTable
                string query1 = $"DELETE FROM {trackObjectTable}";
                OleDbCommand command1 = new OleDbCommand(query1, _myConnection);
                command1.ExecuteNonQuery();

                // Удалить trackObject
                string query2 = "DELETE FROM TrackObject WHERE TrackObject.DicTrackObjectKindID = @dicTrackObjectKindID";
                OleDbCommand command2 = new OleDbCommand(query2, _myConnection);
                command2.Parameters.AddWithValue("@dicTrackObjectKindID", dicTrackObjectKindID);
                command2.ExecuteNonQuery();

                // Удалить точки на пути
                string query3 = "DELETE FROM PointOnTrack WHERE PointOnTrack.DicPointOnTrackKindID = @dicPointOnTrackKindID";
                OleDbCommand command3 = new OleDbCommand(query3, _myConnection);
                command3.Parameters.AddWithValue("@dicPointOnTrackKindID", dicPointOnTrackKindID);
                command3.ExecuteNonQuery();

            }

        }

        public static void DeleteAllObjectsByKindConnected(
            OleDbConnection myConnection,
            string trackObjectTable,
            double dicTrackObjectKindID,
            double dicPointOnTrackKindID)
        {
                //удалить все обьекты используя OleDbConnection


                // Удалить объекты из таблицы, соответствующей trackObjectTable
                string query1 = $"DELETE FROM {trackObjectTable}";
                OleDbCommand command1 = new OleDbCommand(query1, myConnection);
                command1.ExecuteNonQuery();

                // Удалить trackObject
                string query2 = "DELETE FROM TrackObject WHERE TrackObject.DicTrackObjectKindID = @dicTrackObjectKindID";
                OleDbCommand command2 = new OleDbCommand(query2, myConnection);
                command2.Parameters.AddWithValue("@dicTrackObjectKindID", dicTrackObjectKindID);
                command2.ExecuteNonQuery();

                // Удалить точки на пути
                string query3 = "DELETE FROM PointOnTrack WHERE PointOnTrack.DicPointOnTrackKindID = @dicPointOnTrackKindID";
                OleDbCommand command3 = new OleDbCommand(query3, myConnection);
                command3.Parameters.AddWithValue("@dicPointOnTrackKindID", dicPointOnTrackKindID);
                command3.ExecuteNonQuery();
            
        }

        public static void DeleteNoFrameObjects(
            string connectstring, 
            string trackObjectTable,
            double dicTrackObjectKindID,
            double dicPointOnTrackKindID)
        {
            // удалить непроставленные обьекты

            OleDbConnection _myConnection;
            string ConnectString = connectstring;
            _myConnection = new OleDbConnection(ConnectString);
            _myConnection.Open();


            //удалить точки на пути
            string query1 = "Delete FROM PointOnTrack WHERE (((PointOnTrack.PointOnTrackID ) NOT In (Select PointOnTrackID FROM PointFrame))) AND PointOnTrack.DicPointOnTrackKindID = @dicPointOnTrackKindID";
            OleDbCommand command1 = new OleDbCommand(query1, _myConnection);
            command1.Parameters.AddWithValue("@dicPointOnTrackKindID", dicPointOnTrackKindID);
            command1.ExecuteNonQuery();

            //удалить trackObject
            string query2 = "Delete FROM TrackObject WHERE (((TrackObject.TrackObjectID ) NOT In (Select TrackObjectID FROM PointOnTrack))) AND TrackObject.DicTrackObjectKindID = @dicTrackObjectKindID";
            OleDbCommand command2 = new OleDbCommand(query2, _myConnection);
            command2.Parameters.AddWithValue("@dicTrackObjectKindID", dicTrackObjectKindID);
            command2.ExecuteNonQuery();
            //удалить 
            if (trackObjectTable != "")
            {
                string query = $"Delete FROM {trackObjectTable} WHERE ((({trackObjectTable}.TrackObjectID) NOT In (Select TrackObjectID FROM TrackObject)))";
                OleDbCommand command = new OleDbCommand(query, _myConnection);
                command.ExecuteNonQuery();
            }
            

            _myConnection.Close();


            // удалить непроставленные знаки С

            //OleDbConnection _myConnection;
            //string ConnectString = connectstring;
            //_myConnection = new OleDbConnection(ConnectString);
            //_myConnection.Open();


            ////удалить точки на пути
            //string query1 = "Delete FROM PointOnTrack WHERE (((PointOnTrack.PointOnTrackID ) NOT In (Select PointOnTrackID FROM PointFrame))) AND PointOnTrack.DicPointOnTrackKindID = 37";
            //OleDbCommand command1 = new OleDbCommand(query1, _myConnection);
            //command1.ExecuteNonQuery();

            ////удалить trackObject
            //string query2 = "Delete FROM TrackObject WHERE (((TrackObject.TrackObjectID ) NOT In (Select TrackObjectID FROM PointOnTrack))) AND TrackObject.DicTrackObjectKindID = 21";
            //OleDbCommand command2 = new OleDbCommand(query2, _myConnection);
            //command2.ExecuteNonQuery();
            ////удалить сигнальные знаки TrafficSignal
            //string query = "Delete FROM TrafficSignal WHERE (((TrafficSignal.TrackObjectID) NOT In (Select TrackObjectID FROM TrackObject)))";
            //OleDbCommand command = new OleDbCommand(query, _myConnection);
            //command.ExecuteNonQuery();

            //_myConnection.Close();


        }

        public static void DeleteNoPointSigns(string connectstring)
        {
            // удалить знаки С которые не удалились полностью чере редактор
            string ConnectString = connectstring;

            OleDbConnection _myConnection;

            _myConnection = new OleDbConnection(ConnectString);
            _myConnection.Open();
            string query = "Delete FROM TrafficSignal WHERE (((TrafficSignal.TrackObjectID) NOT In (Select TrackObjectID FROM TrackObject)))";
            OleDbCommand command = new OleDbCommand(query, _myConnection);
            command.ExecuteNonQuery();
            _myConnection.Close();
        }

        public static void ImportInitialStationsToDb(string connectstring, List<Station> stations)
        {
            string ConnectString = connectstring;
            OleDbConnection _myConnection;
            _myConnection = new OleDbConnection(ConnectString);
            _myConnection.Open();
            double TrackObjectID = DbRouteDataExporter.SelectMaxTrackObjectFromBaseConnection(_myConnection);

            foreach (var item in stations)
            {
                TrackObjectID += 1;
                Station s = (Station)item;
                string TrackObjectName = "Станция " + s.StationName;
                string query = "INSERT INTO TrackObject ( TrackObjectID, DicTrackObjectKindID, TrackObjectName) " +
                               "VALUES (" + TrackObjectID + ", 1, '" + TrackObjectName + "' )";
                string query2 = "INSERT INTO Station ( TrackObjectID, StationName) " +
                                "VALUES (" + TrackObjectID + ", '" + s.StationName + "' )";
                OleDbCommand command = new OleDbCommand(query, _myConnection);
                OleDbCommand command2 = new OleDbCommand(query2, _myConnection);
                command.ExecuteNonQuery();
                command2.ExecuteNonQuery();

            }
            _myConnection.Close();
        }

        public static void KmLengthSetPerform(string connectstring, Kilometer k, DbRoute route)
        {

            // изменение длины километра в базе данных
            string ConnectString = connectstring;

            double d1 = k.End.PointOnTrackCoordinate - k.Start.PointOnTrackCoordinate;
            double d2 = Math.Abs(d1);
            //double d2 = Math.Abs(k.End.RouteCoordinate - k.Start.RouteCoordinate);

            double ddistance = Math.Round(k.Length - d2, 2);

            int pindex;

            if (d1 < 0) // убывающий километраж
            {
                pindex = route.PointOnTracks.IndexOf(k.Start);
            }
            else
            {
                pindex = route.PointOnTracks.IndexOf(k.End);
            }

            //MessageBox.Show(
            //    "новая длина: " + k.Length.ToString() + "\n" +
            //    "км: " + k.Km + "\n" +
            //    "старая длина: " + d2.ToString() + "\n" +
            //    "ddistance: " + ddistance.ToString() + "\n" +
            //    route.PointOnTracks[pindex].PointOnTrackKm + "-" +
            //    route.PointOnTracks[pindex].PointOnTrackPk.ToString() + "-" +
            //    route.PointOnTracks[pindex].PointOnTrackM.ToString()

            //    );

            int sindex = route.Segments.FindIndex(x => (x.SegmentID == route.PointOnTracks[pindex].SegmentID));

            OleDbConnection myConnection = new OleDbConnection(ConnectString);
            myConnection.Open();


            string ddistanceSign;
            if (ddistance >= 0)
            {
                ddistanceSign = "+";
            }
            else
            {
                ddistanceSign = "-";
            }

            string query =
                "UPDATE PointOnTrack " +
                "SET PointOnTrackCoordinate = PointOnTrackCoordinate " + ddistanceSign + " " + Math.Abs(ddistance).ToString("G", CultureInfo.InvariantCulture) + " " +
                "WHERE PointOnTrack.SegmentID = " + route.Segments[sindex].SegmentID.ToString("G", CultureInfo.InvariantCulture) + " " +
                "AND PointOnTrack.PointOnTrackCoordinate >= " + route.PointOnTracks[pindex].PointOnTrackCoordinate.ToString("G", CultureInfo.InvariantCulture) + " ";


            OleDbCommand command4 = new OleDbCommand(query, myConnection);
            OleDbDataReader reader = command4.ExecuteReader();

            string query2 =
                "UPDATE Segment " +
                "SET SegmentLength = SegmentLength " + ddistanceSign + " " + Math.Abs(ddistance).ToString("G", CultureInfo.InvariantCulture) + " " +
                "WHERE Segment.SegmentID = " + route.Segments[sindex].SegmentID.ToString("G", CultureInfo.InvariantCulture) + " ";

            OleDbCommand command2 = new OleDbCommand(query2, myConnection);
            OleDbDataReader reader2 = command2.ExecuteReader();
            myConnection.Close();
        }

        public static void SaveInclinesToCsvFile(IEnumerable items, string defaultFileName = "inclines.csv")
        {
            var saveFileDialog = new SaveFileDialog
            {
                Filter = "CSV file (*.csv)|*.csv|All Files (*.*)|*.*",
                FileName = defaultFileName
            };

            if (saveFileDialog.ShowDialog() != true)
                return;

            string filePath = saveFileDialog.FileName;

            using (var writer = new StreamWriter(filePath))
            {
                foreach (var item in items)
                {
                    if (item is Incline incline)
                    {
                        writer.WriteLine(
                            $"{incline.StartPointOnTrackKm};" +
                            $"{incline.StartPointOnTrackPk};" +
                            $"{incline.StartPointOnTrackM};;;;" +
                            $"{incline.Value}");
                    }
                }
            }

            MessageBox.Show("Экспорт завершен успешно", "Экспорт данных",
                MessageBoxButton.OK, MessageBoxImage.Information);
        }

        public static void InsertTrafficLightsToDb(string connectionstring, IEnumerable items)
        {


            string ConnectString = connectionstring;
            OleDbConnection _myConnection;
            _myConnection = new OleDbConnection(ConnectString);
            _myConnection.Open();
            // удалить все TliRestriction
            string query7 = "DELETE " +
                            "FROM TrafficLightSpeedRestriction ";

            //удалить все светофоры
            string query71 = "DELETE FROM TrackObject " +
                             "WHERE DicTrackObjectKindID = 5 ";

            //удалить точки светофоров 
            string query72 = "DELETE FROM PointOnTrack " +
                             "WHERE DicPointOnTrackKindID = 1 ";
            OleDbCommand command7 = new OleDbCommand(query7, _myConnection);
            OleDbCommand command71 = new OleDbCommand(query71, _myConnection);
            OleDbCommand command72 = new OleDbCommand(query72, _myConnection);

            command7.ExecuteNonQuery();

            foreach (var item in items)
            {

                if (item is TrafficLight)
                {
                    TrafficLight t = (TrafficLight)item;
                    // так как мы добавляем TliRestrictions  только в те светофоры которые уже есть в базе

                    //DbRouteDataExporter.InsertTrafficlight(t, t.TrackObjectID, _myConnection);

                    foreach (TliRestriction tli in t.TliRestrictions)
                    {
                        DbRouteDataExporter.InsertTliRestriction(tli, t.TrackObjectID, _myConnection);
                    }
                }
                
            }
            _myConnection.Close();
            MessageBox.Show("ok");
        }

    }
}
