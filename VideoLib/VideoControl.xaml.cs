using DirectShowLib;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Data;
using System.Windows.Documents;
using System.Windows.Input;
using System.Windows.Interop;
using System.Windows.Media;
using System.Windows.Media.Imaging;
using System.Windows.Navigation;
using System.Windows.Shapes;
using System.Data.OleDb;
using Microsoft.Win32;
using System.Data.SqlClient;
using System.Globalization;
using System.ComponentModel.Design;
using System.Data;
using WpfApp04;
using dao;


namespace VideoLib
{
    /// <summary>
    /// Interaction logic for VideoControl.xaml
    /// </summary>
    public partial class VideoControl : UserControl

    {
        string fileName1 = "";

        string filmFileName = "";
        double filmStartCoordinate;
        int filmId;
        List<FrameDistance> frameDistanceList = new List<FrameDistance>();

        string ConnectSrting0 = "Provider=Microsoft.Jet.OLEDB.4.0;Data Source=";
        public static string ConnectString1 = "";
        private OleDbConnection myConnection;

        public VideoCar _car;
        public CarInFrameInfo _carInFrameInfo;
        List<VideoCar> videoCars = new List<VideoCar>();

        private bool CarFrameInfoListBox_lock = false;
        private bool CrossingsListBox_lock = false;
        bool SliderLock = false;

        public List<ListBoxItem> ItemsNums= new List<ListBoxItem>(); // для поиска пок кадру в ItemsListBox

        public double frametoshow;
        public readonly List<VideoGraph> _graphs = new();

        CarInFrameInfoComparer _carInFrameInfoComparer = new CarInFrameInfoComparer();
        CarComparerById _carComparerById = new CarComparerById();
        private FrameDistanceComparerByTime _fdComparerByTime = new FrameDistanceComparerByTime();

        List<Crossing> _crossings = new List<Crossing>();

        public VideoControl()
        {
            // сгенерировать videocars

            for (int i = 0; i < 10; i++)
            {
                VideoCar videoCar = new VideoCar();
                videoCar.id = i+430;
              //  videoCars.Add(videoCar);

                for (int j = 0; j < 10; j++)
                {
                    CarInFrameInfo cfm = new CarInFrameInfo();
                    cfm.frameNumber = 555000 + i * 10000 + j * 176;
                    cfm.positionX = 0;
                    cfm.positionY = -8.5f+j*0.5f;
                    cfm.positionZ = 50 - j * 5;
                    cfm.rotationYaw = 0;

                   // videoCar.frameInfos.Add(cfm);
                }
            }


            //_car = videoCars[0];
            InitializeComponent();

            CarsListBox.ItemsSource = videoCars;
            

        }

        bool CheckIfCarTablesInBase()
        {

            bool result=false;
            string connectionstring = ConnectString1;
            string[] restrictionValues = new string[4] { null, null, null, "TABLE" };
            OleDbConnection oleDbCon = new OleDbConnection(connectionstring);
            List<string> tableNames = new List<string>();

            try
            {
                oleDbCon.Open();
                DataTable schemaInformation = oleDbCon.GetSchema("Tables", restrictionValues);

                foreach (DataRow row in schemaInformation.Rows)
                {
                    tableNames.Add(row.ItemArray[2].ToString());
                    if (row.ItemArray[2].ToString() == "CarOnCrossingVideo" || row.ItemArray[2].ToString() == "CarInFrameInfo")
                    {
                        //MessageBox.Show("В базе есть таблица "+row.ItemArray[2].ToString());
                        result = true;
                    }

                }
            }
            finally
            {
                oleDbCon.Close();
            }

            return result;
        }

        public void LoadCrossingsFromBase()
        {
            myConnection = new OleDbConnection(ConnectString1);
            myConnection.Open();

            string queryspeed =
                "SELECT Crossing.TrackObjectID, Crossing.DicCrossingKindID, P.PointOnTrackID, T.TrackObjectName, PF.FilmFrameTime " +
                "FROM ((Crossing " +
                "LEFT JOIN PointOnTrack AS [P] ON Crossing.TrackObjectID = P.TrackObjectID) " +
                "LEFT JOIN TrackObject AS [T] ON Crossing.TrackObjectID = T.TrackObjectID) " +
                "LEFT JOIN PointFrame AS [PF] ON PF.PointOnTrackID = P.PointOnTrackID ";

            OleDbCommand command6 = new OleDbCommand(queryspeed, myConnection);

            OleDbDataReader reader6 = command6.ExecuteReader();

            while (reader6.Read())
            {

                Crossing s = new Crossing();
                s.TrackObjectID = Convert.ToDouble(reader6[0]);
                s.DicCrossingKindID = Convert.ToDouble(reader6[1]);
                s.Start.TrackObjectID = Convert.ToDouble(reader6[0]);
                s.Start.PointOntrackID = Convert.ToDouble(reader6[2]);
                string sfft = reader6[4].ToString(); if (sfft != String.Empty) s.Start.FilmFrameTime = Convert.ToDouble(sfft);
                //s.Start.FilmFrameTime = Convert.ToDouble(reader6[3]);
                s.TrackObjectName = reader6[3].ToString();
                _crossings.Add(s);

            }

            reader6.Close();
            myConnection.Close();
        }

        private void OpenDbButton_Click(object sender, RoutedEventArgs e)
        {
            
            var openFileDialog = new OpenFileDialog { };
            var result = openFileDialog.ShowDialog();
            if (result != true) return;
            fileName1 = openFileDialog.FileName;

            ConnectString1 = ConnectSrting0 + fileName1 + ";";


            //
            _crossings.Clear();
            LoadCrossingsFromBase();

            


            myConnection = new OleDbConnection(ConnectString1);
            myConnection.Open();

            videoCars.Clear();
            frameDistanceList.Clear();
            _graphs.Clear();

            string queryfilm =
                "SELECT FilmID, FilmFileName, FilmStartCoordinate FROM Film";

            OleDbCommand command62 = new OleDbCommand(queryfilm, myConnection);
            OleDbDataReader reader62 = command62.ExecuteReader();
            while (reader62.Read())
            {
                string fscst = reader62[2].ToString();

                if (fscst != String.Empty) filmStartCoordinate = Convert.ToDouble(fscst);

                filmFileName = reader62[1].ToString();
                filmId = Convert.ToInt32(reader62[0]);

            }
            reader62.Close();



            string frtmsql =
                "SELECT FilmID, FrameTime, FrameDistance FROM FrameTime";
            OleDbCommand command63 = new OleDbCommand(frtmsql, myConnection);
            OleDbDataReader reader63 = command63.ExecuteReader();
            while (reader63.Read())
            {
                FrameDistance fd = new FrameDistance();
                fd.FilmID= Convert.ToInt32(reader63[0]);
                fd.FrameTime = Convert.ToDouble(reader63[1]);
                fd.Distance = Convert.ToDouble(reader63[2]);
                frameDistanceList.Add(fd);
            }
            reader63.Close();
            
            frameDistanceList.Sort(_fdComparerByTime);


            Init();

            
            
            if (CheckIfCarTablesInBase() == true)// проверить есть и таблицы car
            {
                try
                {
                    string qvcars = "SELECT CarOnCrossingVideo.CrossingID" +
                                    ", CarOnCrossingVideo.CarID" +
                                    ", CarOnCrossingVideo.CameraAngle" +
                                    " FROM CarOnCrossingVideo ";

                    OleDbCommand command61 = new OleDbCommand(qvcars, myConnection);
                    OleDbDataReader reader61 = command61.ExecuteReader();

                    while (reader61.Read())
                    {

                        VideoCar car = new VideoCar();

                        car.crossingId = Convert.ToInt32(reader61[0]);
                        car.id = Convert.ToInt32(reader61[1]);
                        car.cameraAngle= (float)Convert.ToDouble(reader61[2]);
                        videoCars.Add(car);


                    }

                    reader61.Close();


                    videoCars.Sort(_carComparerById);
                    CarsListBox.ItemsSource = videoCars;
                    CarsListBox.Items.Refresh();
                }
                catch (Exception ex)
                {
                    MessageBox.Show(ex.Message);
                }


                try
                {
                    foreach (VideoCar car in videoCars)
                    {
                        string qucfm =
                            "SELECT " +
                            "FilmID, " +
                            "CarID, " +
                            "FrameTime, " +
                            "PositionX, " +
                            "PositionY, " +
                            "PositionZ, " +
                            "RotationYaw, " +

                            "RotationPitch, "+
                            "RotationRoll, "+
                            //"CameraAngle, " +
                            "Visibility " +
                            "FROM CarInFrameInfo " +
                            "WHERE CarID = " + car.id.ToString("G", CultureInfo.InvariantCulture);

                        OleDbCommand command71 = new OleDbCommand(qucfm, myConnection);

                        OleDbDataReader reader71 = command71.ExecuteReader();
                        while (reader71.Read())
                        {
                            CarInFrameInfo cfmtoadd = new CarInFrameInfo();

                            cfmtoadd.videoNumber = Convert.ToInt32(reader71[0]);

                            cfmtoadd.frameNumber =
                                Convert.ToInt32(Convert.ToDouble(reader71[2]) * _graphs[0].framesInSecond);


                            cfmtoadd.positionX = (float)Convert.ToDouble(reader71[3]);
                            cfmtoadd.positionY = (float)Convert.ToDouble(reader71[4]);
                            cfmtoadd.positionZ = (float)Convert.ToDouble(reader71[5]);
                            cfmtoadd.rotationYaw = (float)Convert.ToDouble(reader71[6]);
                            cfmtoadd.rotationPitch = (float)Convert.ToDouble(reader71[7]);
                            cfmtoadd.rotationRoll = (float)Convert.ToDouble(reader71[8]);

                            //car.cameraAngle= cfmtoadd.cameraAngle = (float)Convert.ToDouble(reader71[9]);
                            //cfmtoadd.cameraAngle = 74.3f;
                            cfmtoadd.cameraAngle = car.cameraAngle;

                            cfmtoadd.visibility = (float)Convert.ToDouble(reader71[9]);

                            car.frameInfos.Add(cfmtoadd);
                        }

                        reader71.Close();


                    }
                }
                catch(Exception ex) 
                {
                    MessageBox.Show(ex.Message);
                }

                

                
            } 
            myConnection.Close();
            UpdateCrossingsListBox();


        }

        void UpdateCrossingsListBox()
        {
            CrossingsListBox_lock = true;
            CrossingsListbox.Items.Clear();



            //заполнить таблицу переездов
            foreach (var c in _crossings)
            {

                int index = videoCars.FindIndex(x => x.crossingId == c.TrackObjectID);

                string scarid="";
                if (index >= 0)
                {
                    videoCars[index].crossingId = (int)c.TrackObjectID;
                    scarid = videoCars[index].id.ToString();
                }

                ListBoxItem item = new ListBoxItem
                {
                    Tag = c.TrackObjectID,
                    Content = c.TrackObjectID + " " + scarid
                };
                CrossingsListbox.Items.Add(item);
            }
            CrossingsListbox.Items.Refresh();
            CrossingsListBox_lock = false;
        }

        private void VideoControl_OnLoaded(object sender, RoutedEventArgs e)
        {
           // Init();
        }

        public void Init()
        {

            var videoFiles = new[]
            {
                "F:\\video\\SPB2020\\Spb-MVishera.avi"
            };

            if (filmFileName != "" && filmFileName != null)
            {videoFiles[0] = filmFileName;}

            var handle = ((HwndSource)PresentationSource.FromVisual(VideoPanel)).Handle;
            foreach (var videoFile in videoFiles)
            {
                if (!File.Exists(videoFile)) continue;
                var graph = new VideoGraph(videoFile, handle);
                graph.SetSize(1280, 720);
                //graph.SetSize(960, 540);
                //graph.SetSize((int)ActualWidth, (int)ActualHeight);
                //graph.SetSize((int)VideoPanel.ActualWidth, (int)VideoPanel.ActualHeight);
                
                _graphs.Add(graph);
                Slider.Minimum = 0;
                Slider.Maximum = graph.duration*graph.framesInSecond;
                Slider.LargeChange = graph.framesInSecond;

                Slider2.Minimum = 15;
                Slider2.Maximum = 135;


            }

        }

        void MoveToPrevKeyFrame()
        {
            if (_car == null) return;
            CarInFrameInfo nextFrameInfo = _car.frameInfos.Find(x => x.frameNumber > frametoshow);

            if(nextFrameInfo!=null) MoveToFrame(nextFrameInfo.frameNumber);
            SliderLock = true;
            Slider.Value = frametoshow;
            SliderLock=false;
        }

        void MoveToNextKeyframe()
        {
            if (_car == null) return;
            CarInFrameInfo nextFrameInfo = _car.frameInfos.FindLast(x => x.frameNumber < frametoshow);

            if (nextFrameInfo != null) MoveToFrame(nextFrameInfo.frameNumber);
            SliderLock = true;
            Slider.Value = frametoshow;
            SliderLock = false;
        }
        
        void MoveToFrame(double _framenumber)
        {
            frametoshow = _framenumber;

            CalculateCarFrameInfo(_car, (int)frametoshow);

            if (_graphs.Count > 0)
            {
                _graphs[0]._compositor.carInFrame = _carInFrameInfo;
                _graphs[0].ShowFrame(frametoshow);
                UpdatecarframeInfoTextBlockText();
            }

            CarFrameInfoListBox_lock = true;
            if (_car != null && _car.frameInfos.Count > 0)
            {
                var item1 = ItemsNums.Find(x => x.Tag.ToString() == frametoshow.ToString());
                CarFrameInfoListBox.SelectedItem = item1;
                CarFrameInfoListBox.ScrollIntoView(item1);
            }
            CarFrameInfoListBox_lock = false;
        }


        private void Slider_OnValueChanged(object sender, RoutedPropertyChangedEventArgs<double> e)
        {
            //if (_carInFrameInfo == null) return;
            if (SliderLock == false)
            {
                MoveToFrame((int)Slider.Value);

                //frametoshow = (int)Slider.Value; ///_graphs[0].framesInSecond; 

                //CalculateCarFrameInfo(_car, (int)frametoshow);

                //if (_graphs.Count>0) {
                //    _graphs[0]._compositor.carInFrame = _carInFrameInfo;
                //    _graphs[0].ShowFrame(frametoshow);
                    
                //    UpdatecarframeInfoTextBlockText();
                //}
                
                //CarFrameInfoListBox_lock = true;
                //if (_car != null && _car.frameInfos.Count > 0)
                //{
                //    var item1 = ItemsNums.Find(x => x.Tag.ToString() == frametoshow.ToString());
                //    CarFrameInfoListBox.SelectedItem = item1;
                //    CarFrameInfoListBox.ScrollIntoView(item1);
                //}
                //CarFrameInfoListBox_lock = false;

            }
        }

        //обновить список с кадрами CarFrameInfoListBox
        void UpdateCarInfoLIst(VideoCar _videoCar)
        {
            CarFrameInfoListBox_lock = true;

            //int i2 = CarsListBox.SelectedIndex;
            //if (i2 >= 0) _car = videoCars[i2];

            CarFrameInfoListBox.Items.Clear();
            //if (i2 >= 0 && _car.frameInfos.Count > 0)
            if (_videoCar != null && _car.frameInfos.Count > 0)
            {
                ItemsNums.Clear();

                for (int i3 = _car.frameInfos[_car.frameInfos.Count - 1].frameNumber; i3 >= _car.frameInfos[0].frameNumber; i3--)
                {
                    int index = _car.frameInfos.FindIndex(x => x.frameNumber == i3);

                    bool keyframe = false;
                    Brush backgroundbrush = Brushes.White;

                    if (index >= 0)
                    {
                        keyframe = true;
                        backgroundbrush = Brushes.Aqua;
                    }

                    var item = new ListBoxItem
                    {
                        Background = backgroundbrush,
                        Content = i3,// + " "+ CalculateFrameDistance(i3),
                        Tag = i3
                    };
                    CarFrameInfoListBox.Items.Add(item);
                    ItemsNums.Add(item);
                }
            }

            CarFrameInfoListBox_lock = false;
        }

        private void UserControl_KeyDown(object sender, KeyEventArgs e)
        {

            float step = 0.05f;
            float stepMtlr = 800;

            var ctrl = (Keyboard.Modifiers & ModifierKeys.Control) != 0;
            var shift = (Keyboard.Modifiers & ModifierKeys.Shift) != 0;
            var alt = (Keyboard.Modifiers & ModifierKeys.Alt) != 0;
            switch (e.Key)
            {
                case Key.Delete when ctrl:
                {
                    _car.frameInfos.Remove(_carInFrameInfo);

                    var item = CarFrameInfoListBox.SelectedItem;
                    int index = CarFrameInfoListBox.Items.IndexOf(item);

                    UpdateCarInfoLIst(_car);

                    CarFrameInfoListBox.SelectedIndex = index;
                    
                    //CarFrameInfoListBox.SelectedItem = item;

                    break;
                }

                case Key.Enter when ctrl:
                {
                    // добавить ключевой кадр
                    CarInFrameInfo cfmtoadd = new CarInFrameInfo();
                    cfmtoadd.frameNumber = (int)frametoshow;

                    if (_carInFrameInfo != null)
                    {
                        cfmtoadd.positionZ = _carInFrameInfo.positionZ;
                        cfmtoadd.positionY = _carInFrameInfo.positionY;
                        
                    }
                    else if (_carInFrameInfo == null)
                    {
                        //если кадр больше последнего ключевого кадра
                            int index = _car.frameInfos.FindLastIndex(x => x.frameNumber < frametoshow);
                        if (index >= 0)
                        {
                            _carInFrameInfo = _car.frameInfos[index];

                            int index2 = index;
                            int index1 =_car.frameInfos.FindLastIndex(x => x.frameNumber < _car.frameInfos[index2].frameNumber);

                            if (index1 >= 0 && index2 >= 0 && index1 != index2)
                            {
                                float r21 = Math.Abs(
                                    (_car.frameInfos[index2].positionZ - _car.frameInfos[index1].positionZ) /
                                    ((float)CalculateFrameDistance(_car.frameInfos[index2].frameNumber) -
                                     (float)CalculateFrameDistance(_car.frameInfos[index1].frameNumber)));

                                cfmtoadd.positionZ = _car.frameInfos[index1].positionZ - r21 *
                                    Math.Abs((float)CalculateFrameDistance((int)frametoshow) -
                                             (float)CalculateFrameDistance(_car.frameInfos[index1].frameNumber));


                            }
                            else if (index2 >= 0 && index1 < 0) //если есть только один опорный кадр
                            {
                                cfmtoadd.positionZ = _car.frameInfos[index2].positionZ -
                                                     Math.Abs((float)CalculateFrameDistance((int)frametoshow) -
                                                              (float)CalculateFrameDistance(_car.frameInfos[index2].frameNumber));
                            }
                            cfmtoadd.positionY = _carInFrameInfo.positionY;
                            
                        }
                        else if (index < 0) // если кадр меньше первого ключевого кадра
                        {
                                
                            index = _car.frameInfos.FindIndex(x => x.frameNumber > frametoshow);
                            if (index>=0) {_carInFrameInfo = _car.frameInfos[index]; }

                            int index1 = _car.frameInfos.FindIndex(x => x.frameNumber > frametoshow);
                            int index2 = _car.frameInfos.FindIndex(x2 => x2.frameNumber > _car.frameInfos[index1].frameNumber);

                            if (index1 >= 0 && index2 >= 0 && index1 != index2)
                            {
                                float r21 =Math.Abs( (_car.frameInfos[index2].positionZ - _car.frameInfos[index1].positionZ) / 
                                           ((float)CalculateFrameDistance(_car.frameInfos[index2].frameNumber)-(float)CalculateFrameDistance(_car.frameInfos[index1].frameNumber)));

                                cfmtoadd.positionZ =
                                    r21 * ((float)CalculateFrameDistance(_car.frameInfos[index2].frameNumber) - (float)CalculateFrameDistance((int)frametoshow))
                                    + _car.frameInfos[index2].positionZ;

                                cfmtoadd.positionY =
                                   - Math.Abs((_car.frameInfos[index2].positionY - _car.frameInfos[index1].positionY) /
                                             ((float)CalculateFrameDistance(_car.frameInfos[index2].frameNumber) - (float)CalculateFrameDistance(_car.frameInfos[index1].frameNumber)))
                                    * ((float)CalculateFrameDistance(_car.frameInfos[index2].frameNumber) - (float)CalculateFrameDistance((int)frametoshow))
                                    + _car.frameInfos[index2].positionY;

                            }
                            else if (index1 >= 0 && index2 < 0)//если есть только один опорный кадр
                            {
                                cfmtoadd.positionZ = _car.frameInfos[index1].positionZ
                                                     + Math.Abs((float)CalculateFrameDistance(_car.frameInfos[index1].frameNumber) -
                                                                (float)CalculateFrameDistance((int)frametoshow));
                                cfmtoadd.positionY = _car.frameInfos[index1].positionY;
                                
                            }
                            
                        }
                        
                    }
                    //cfmtoadd.frameNumber = (int)frametoshow;
                    
                        if (_carInFrameInfo != null)
                        {
                            cfmtoadd.positionX = _carInFrameInfo.positionX;
                           // cfmtoadd.positionY = _carInFrameInfo.positionY;
                            //cfmtoadd.positionZ = _carInFrameInfo.positionZ;
                            cfmtoadd.rotationYaw = _carInFrameInfo.rotationYaw;
                            cfmtoadd.cameraAngle = _carInFrameInfo.cameraAngle;
                            cfmtoadd.visibility = _carInFrameInfo.visibility;

                            cfmtoadd.rotationPitch=_carInFrameInfo.rotationPitch;
                            cfmtoadd.rotationRoll= _carInFrameInfo.rotationRoll;
                        }
                        else
                        {
                            cfmtoadd.positionX = -0.15f;
                            cfmtoadd.positionY = -4.0f;
                            cfmtoadd.positionZ = 6;
                            cfmtoadd.rotationYaw = 0;
                            cfmtoadd.cameraAngle = 73.4f;
                            cfmtoadd.visibility = 1;
                        }

                        _car.frameInfos.Add(cfmtoadd);


                    _car.frameInfos.Sort(_carInFrameInfoComparer);
                    UpdateCarInfoLIst(_car);
                    //CarVideoControl._graphs[0].ShowFrame(CarVideoControl.frametoshow);
                    //MessageBox.Show("Key.Enter when shift");

                    MoveToFrame(cfmtoadd.frameNumber);

                    break;
                }
                case Key.W when shift:
                {
                    _carInFrameInfo.positionZ += (step + _carInFrameInfo.positionZ / stepMtlr);
                    _graphs[0].ShowFrame(frametoshow);
                    break;
                }
                case Key.S when shift:
                {
                        _carInFrameInfo.positionZ -= (step + _carInFrameInfo.positionZ / stepMtlr);
                        _graphs[0].ShowFrame(frametoshow);
                        break;
                }
                case Key.A when ctrl:
                {
                        _carInFrameInfo.positionX -= (step+ _carInFrameInfo.positionZ/stepMtlr);
                        _graphs[0].ShowFrame(frametoshow);
                        break;
                }
                case Key.D when ctrl:
                {
                        _carInFrameInfo.positionX += (step + _carInFrameInfo.positionZ / stepMtlr);
                        _graphs[0].ShowFrame(frametoshow);
                        break;
                }
                case Key.W when ctrl:
                {
                    _carInFrameInfo.positionY += step;
                    _graphs[0].ShowFrame(frametoshow);
                    break;
                }
                case Key.S when ctrl:
                {
                    _carInFrameInfo.positionY -= step;
                    _graphs[0].ShowFrame(frametoshow);
                    break;
                }
                case Key.D when shift: // rotationYaw
                {
                        _carInFrameInfo.rotationYaw -= step;
                        if (Math.Abs(_carInFrameInfo.rotationYaw) < step / 5) _carInFrameInfo.rotationYaw = 0;
                        _graphs[0].ShowFrame(frametoshow);
                        break;
                }
                case Key.A when shift:
                {
                        _carInFrameInfo.rotationYaw += step;
                        if (Math.Abs(_carInFrameInfo.rotationYaw) < step / 5) _carInFrameInfo.rotationYaw = 0;
                        _graphs[0].ShowFrame(frametoshow);
                        break;
                }
                case Key.Q when shift:
                {
                    _carInFrameInfo.rotationPitch += step;
                    if (Math.Abs(_carInFrameInfo.rotationPitch) < step / 5) _carInFrameInfo.rotationPitch = 0;
                        _graphs[0].ShowFrame(frametoshow);
                    break;
                }
                case Key.E when shift:
                {
                    _carInFrameInfo.rotationPitch -= step;
                    if (Math.Abs(_carInFrameInfo.rotationPitch) < step / 5) _carInFrameInfo.rotationPitch = 0;
                        _graphs[0].ShowFrame(frametoshow);
                    break;
                }
                case Key.Q when ctrl:
                {
                    _carInFrameInfo.rotationRoll += step/5;
                    _graphs[0].ShowFrame(frametoshow);
                    break;
                }
                case Key.E when ctrl:
                {
                    _carInFrameInfo.rotationRoll -= step/5;
                    _graphs[0].ShowFrame(frametoshow);
                    break;
                }
            }

            UpdatecarframeInfoTextBlockText();
        }

        private void CarsListBox_SelectionChanged(object sender, SelectionChangedEventArgs e)
        {
            int i2 = CarsListBox.SelectedIndex;
            if (i2 >= 0) _car = videoCars[i2];
            UpdateCarInfoLIst(_car);

        }

        double CalculateFrameDistance(int framenumber)
        {
            double frameTime = Convert.ToDouble(framenumber) / (_graphs[0].framesInSecond);
            double frameDistance=0;

            FrameDistance pfd = frameDistanceList.FindLast(x => x.FrameTime <= frameTime);
            FrameDistance nfd = frameDistanceList.Find(x => x.FrameTime >= frameTime);

            if (pfd != null && nfd != null && pfd != nfd)
            {
                frameDistance =
                    ((frameTime - pfd.FrameTime) / (nfd.FrameTime - pfd.FrameTime)) * (nfd.Distance - pfd.Distance) + pfd.Distance;
            }
            else if (pfd == nfd)
            {
                frameDistance = pfd.Distance;
                
            }

            //return Math.Round(frameDistance,3);
            return frameDistance;
        }

        void CalculateCarFrameInfo(VideoCar car, int currentframenumber)
        {
            if (car == null) return;
            int index = _car.frameInfos.FindIndex(x => x.frameNumber == currentframenumber);
            int i1 = _car.frameInfos.FindLastIndex(x => x.frameNumber < currentframenumber);
            int i2 = _car.frameInfos.FindIndex(x => x.frameNumber > currentframenumber);

            


            if (index >= 0)
            {
                _carInFrameInfo = _car.frameInfos[index];
                frametoshow = _carInFrameInfo.frameNumber;
                
            }
            else if(i1>=0 &&i2>=0)
            {
                frametoshow = currentframenumber;

                CarInFrameInfo prevFrameInfo;
                CarInFrameInfo nextframeinfo;

                
                    prevFrameInfo = _car.frameInfos[i1];
                    nextframeinfo = _car.frameInfos[i2];
                

                _carInFrameInfo = new CarInFrameInfo();
                _carInFrameInfo.frameNumber = currentframenumber;

                float ratio = (float)(currentframenumber - prevFrameInfo.frameNumber) / (nextframeinfo.frameNumber - prevFrameInfo.frameNumber);

                _carInFrameInfo.positionY = 
                    (nextframeinfo.positionY - prevFrameInfo.positionY) 
                    * (currentframenumber - prevFrameInfo.frameNumber) 
                    / (nextframeinfo.frameNumber - prevFrameInfo.frameNumber) 
                    + prevFrameInfo.positionY;

                _carInFrameInfo.positionX = 
                    (nextframeinfo.positionX - prevFrameInfo.positionX) 
                    * (currentframenumber - prevFrameInfo.frameNumber) 
                    / (nextframeinfo.frameNumber - prevFrameInfo.frameNumber) 
                    + prevFrameInfo.positionX;

                _carInFrameInfo.positionZ = 
                    (nextframeinfo.positionZ - prevFrameInfo.positionZ) 
                    * (currentframenumber - prevFrameInfo.frameNumber) 
                    / (nextframeinfo.frameNumber - prevFrameInfo.frameNumber) 
                    + prevFrameInfo.positionZ;

                _carInFrameInfo.rotationYaw = 
                    (nextframeinfo.rotationYaw - prevFrameInfo.rotationYaw) 
                    * (currentframenumber - prevFrameInfo.frameNumber) 
                    / (nextframeinfo.frameNumber - prevFrameInfo.frameNumber) 
                    + prevFrameInfo.rotationYaw;

                _carInFrameInfo.rotationPitch =
                    (nextframeinfo.rotationPitch - prevFrameInfo.rotationPitch)
                    * (currentframenumber - prevFrameInfo.frameNumber)
                    / (nextframeinfo.frameNumber - prevFrameInfo.frameNumber)
                    + prevFrameInfo.rotationPitch;

                _carInFrameInfo.rotationRoll =
                    (nextframeinfo.rotationRoll - prevFrameInfo.rotationRoll)
                    * (currentframenumber - prevFrameInfo.frameNumber)
                    / (nextframeinfo.frameNumber - prevFrameInfo.frameNumber)
                    + prevFrameInfo.rotationRoll;

                _carInFrameInfo.cameraAngle = 
                    (nextframeinfo.cameraAngle - prevFrameInfo.cameraAngle) 
                    * (currentframenumber - prevFrameInfo.frameNumber) 
                    / (nextframeinfo.frameNumber - prevFrameInfo.frameNumber) 
                    + prevFrameInfo.cameraAngle;

                _carInFrameInfo.visibility = prevFrameInfo.visibility;

            }
            else
            {
                _carInFrameInfo = null;
            }

        }

        private void CarFrameInfoListBox_SelectionChanged(object sender, SelectionChangedEventArgs e)
        {
            if (CarFrameInfoListBox_lock != true)
            {

                var item = (ListBoxItem)CarFrameInfoListBox.SelectedItem;
                string framestr = item?.Tag.ToString();

                int frameint = Convert.ToInt32(framestr);

                CalculateCarFrameInfo(_car, frameint);
                
                _graphs[0]._compositor.carInFrame = _carInFrameInfo;
                if (_carInFrameInfo != null) _graphs[0]._compositor._carDrawer._angle = _carInFrameInfo.cameraAngle;
                _graphs[0].ShowFrame(frametoshow);

                SliderLock = true;
                Slider.Value = frametoshow;
                if (_carInFrameInfo != null)
                {
                    Slider2.Value = _carInFrameInfo.cameraAngle;

                    if(_carInFrameInfo.visibility==1) VisibilityCheckBox.IsChecked = true;
                    if (_carInFrameInfo.visibility == 0) VisibilityCheckBox.IsChecked = false;
                }
                SliderLock=false;


                UpdatecarframeInfoTextBlockText();

            }
        }

        void UpdatecarframeInfoTextBlockText()
        {
            if (_carInFrameInfo != null)
            {
                carframeInfoTextBlock.Text = "кадр: " + _carInFrameInfo.frameNumber.ToString() + "\n" +
                                             "M: " + Math.Round(CalculateFrameDistance(_carInFrameInfo.frameNumber),2) + "\n"+
                                             "X: " + _carInFrameInfo.positionX.ToString() + "\n" +
                                             "Y: " + _carInFrameInfo.positionY.ToString() + "\n" +
                                             "Z: " + _carInFrameInfo.positionZ.ToString() + "\n" +
                                             "Yaw: " + _carInFrameInfo.rotationYaw.ToString() + "\n" +
                                             "Pitch: " + _carInFrameInfo.rotationPitch.ToString() + "\n" +
                                             "Roll: " + _carInFrameInfo.rotationRoll.ToString() + "\n" +
                                             "angle: " + _carInFrameInfo.cameraAngle.ToString() + "\n" +
                                             "visibility: " + _carInFrameInfo.visibility.ToString()

                                             ;
            }
            else
            {
                carframeInfoTextBlock.Text = "кадр: " + frametoshow.ToString() + "\n" + Math.Round(CalculateFrameDistance((int)frametoshow), 2);
            }
        }

        private void CreateDataBaseParameters()
        {
            var dbEngine = new DBEngine();
            var database = dbEngine.OpenDatabase(fileName1);

            try
            {
                var recordset = database.OpenRecordset("DataBaseParameters");
                recordset.Close();
                database.Close();
                return;
            }
            catch (Exception)
            {
                //ignored
            }

            var tableDef = database.CreateTableDef("DataBaseParameters");

            var field = tableDef.CreateField("PropertyName", DataTypeEnum.dbText);
            field.Size = 255;
            field.Required = true;
            tableDef.Fields.Append(field);

            field = tableDef.CreateField("Value", DataTypeEnum.dbText);
            field.Required = true;
            field.Size = 255;
            tableDef.Fields.Append(field);

            var index = tableDef.CreateIndex("Index");
            index.Primary = true;
            index.Fields.Append(index.CreateField("PropertyName"));
            tableDef.Indexes.Append(index);

            database.TableDefs.Append(tableDef);
            database.TableDefs.Refresh();

            database.Close();
        }

        private void CreateCarInFrameInfoTable()
        {
            var dbEngine = new DBEngine();
            var database = dbEngine.OpenDatabase(fileName1);

            try
            {
                var recordset = database.OpenRecordset("CarInFrameInfo");
                recordset.Close();
                database.Close();
                return;
            }
            catch (Exception ex)
            {
                //MessageBox.Show(ex.Message);
            }

            var tableDef = database.CreateTableDef("CarInFrameInfo");

            dao.Field field = tableDef.CreateField("FilmID", DataTypeEnum.dbLong);
            field.Required = true;
            tableDef.Fields.Append(field);
            field = tableDef.CreateField("CarID", DataTypeEnum.dbLong);
            field.Required = true;
            tableDef.Fields.Append(field);
            field = tableDef.CreateField("FrameTime", DataTypeEnum.dbDouble);
            field.Required = true;
            tableDef.Fields.Append(field);
            field = tableDef.CreateField("PositionX", DataTypeEnum.dbSingle);
            field.Required = true;
            tableDef.Fields.Append(field);
            field = tableDef.CreateField("PositionY", DataTypeEnum.dbSingle);
            field.Required = true;
            tableDef.Fields.Append(field);
            field = tableDef.CreateField("PositionZ", DataTypeEnum.dbSingle);
            field.Required = true;
            tableDef.Fields.Append(field);
            field = tableDef.CreateField("RotationYaw", DataTypeEnum.dbSingle);
            field.Required = true;
            tableDef.Fields.Append(field);

            field = tableDef.CreateField("RotationPitch", DataTypeEnum.dbSingle);
            field.Required = true;
            tableDef.Fields.Append(field);
            field = tableDef.CreateField("RotationRoll", DataTypeEnum.dbSingle);
            field.Required = true;
            tableDef.Fields.Append(field);

            field = tableDef.CreateField("Visibility", DataTypeEnum.dbBoolean);
            field.Required = true;
            tableDef.Fields.Append(field);

            database.TableDefs.Append(tableDef);
            database.TableDefs.Refresh();

            database.Close();
        }

        private void SaveCarsButton_Click(object sender, RoutedEventArgs e)
        {
            // удалить таблицы

            try
            {
                OleDbConnection myConnection1 = new OleDbConnection(ConnectString1);
                myConnection1.Open();
                string sql721 = "DROP TABLE CarOnCrossingVideo ";
                OleDbCommand command721 = new OleDbCommand(sql721, myConnection1);
                command721.ExecuteNonQuery();
                

                string sql722 = "DROP TABLE CarInFrameInfo ";
                OleDbCommand command722 = new OleDbCommand(sql722, myConnection1);
                command722.ExecuteNonQuery();

                myConnection1.Close();
            }
            catch (Exception ex)
            {
                //MessageBox.Show(ex.Message);
            }

            // создать таблицу DataBaseParameters
            CreateDataBaseParameters();
            //try
            //{
            //    string query72 = "CREATE TABLE DataBaseParameters([PropertyName] char, [Value] char(50));";
            //    OleDbCommand command72 = new OleDbCommand(query72, myConnection);
            //    command72.ExecuteNonQuery();
            //}
            //catch (Exception ex)
            //{
            //    MessageBox.Show(ex.Message);
            //}

            // создать таблицу CarInFrameInfos
            CreateCarInFrameInfoTable();

            //try
            //{
            //    string query72 = "CREATE TABLE CarInFrameInfo( " +
            //                     "FilmID int NOT NULL , " +
            //                     "CarID int NOT NULL , " +
            //                     "FrameTime float , " +
            //                     "PositionX float , " +
            //                     "PositionY float , " +
            //                     "PositionZ float , " +
            //                     "RotationYaw float , " +
            //                     //"CameraAngle float , " +
            //                     "Visibility bit " +
            //                     ");";
            //    OleDbCommand command72 = new OleDbCommand(query72, myConnection);
            //    command72.ExecuteNonQuery();
            //}
            //catch (Exception ex)
            //{
            //    MessageBox.Show(ex.Message);
            //}
            myConnection = new OleDbConnection(ConnectString1);
            myConnection.Open();
            

            // создать таблицу CarOnCrossingVideo
            try
            {
                string query72 = "CREATE TABLE CarOnCrossingVideo(CrossingID int NOT NULL, CarID int NOT NULL PRIMARY KEY, CameraAngle float );";
                OleDbCommand command72 = new OleDbCommand(query72, myConnection);
                command72.ExecuteNonQuery();
            }
            catch (Exception ex)
            {
                //MessageBox.Show(ex.Message);
            }


            // удалить запись DataBaseParameters VideoCarOnCrossing
            try
            {
                string query78 = "DELETE " +
                                 "FROM DataBaseParameters " +
                                 "WHERE PropertyName = 'VideoCarOnCrossing'";

                OleDbCommand command78 = new OleDbCommand(query78, myConnection);
                command78.ExecuteNonQuery();
            }
            catch (Exception ex)
            {
                //MessageBox.Show(ex.Message);
            }


            //удалить CarOnCrossingVideo & CarInFrameInfo из бд
            string query7 = "DELETE " +
                            "FROM CarOnCrossingVideo ";
            
            OleDbCommand command7 = new OleDbCommand(query7, myConnection);
            command7.ExecuteNonQuery();

            try
            {
                string query8 = "DELETE " +
                                "FROM CarInFrameInfo ";

                OleDbCommand command8 = new OleDbCommand(query8, myConnection);
                command8.ExecuteNonQuery();
            }
            catch (Exception ex)
            {
                MessageBox.Show(ex.Message);
            }

            // внести DataBaseParameters
            try
            {
                string v1 = "VideoCarOnCrossing";
                string v2 = "1";

                string vcis = "INSERT INTO DataBaseParameters ( PropertyName, [Value] ) VALUES ( 'VideoCarOnCrossing', 1 )";
                OleDbCommand commandvcis = new OleDbCommand(vcis, myConnection);
                commandvcis.ExecuteNonQuery();
            }
            catch (Exception ex)
            {
                MessageBox.Show(ex.Message);
            }

            // внести CarOnCrossingVideo & CarInFrameInfo в бд
            foreach (VideoCar car in videoCars)
            {
                try
                {
                    string vcis = "INSERT INTO CarOnCrossingVideo ( CrossingID, CarID, CameraAngle ) " +
                                  "VALUES (" + car.crossingId + ", " + car.id + ", " + car.cameraAngle.ToString("G", CultureInfo.InvariantCulture) + " )";
                    OleDbCommand commandvcis = new OleDbCommand(vcis, myConnection);
                    commandvcis.ExecuteNonQuery();
                }
                catch (Exception ex)
                {
                    MessageBox.Show(ex.Message);
                }

                try
                {
                    foreach (CarInFrameInfo cfm in car.frameInfos)
                    {
                        InsertCarInFrameInfo(
                            car.id,
                            cfm,
                            myConnection
                        );
                    }
                }
                catch (Exception ex)
                {
                    MessageBox.Show(ex.Message);
                }
            }
            myConnection.Close();
            MessageBox.Show("ok");
        }

        void InsertCarInFrameInfo(int CarId, CarInFrameInfo _carInFrameInfo, OleDbConnection _myConnection)
        {

            double frametime = (Math.Round(Convert.ToDouble(_carInFrameInfo.frameNumber),2) / _graphs[0].framesInSecond);
            
            string query41 = "INSERT INTO CarInFrameInfo ( " +
                             "FilmID, " +
                             "CarID, " +
                             "FrameTime, " +
                             "PositionX, " +
                             "PositionY, " +
                             "PositionZ, " +
                             "RotationYaw, " +

                             "RotationPitch, " +
                             "RotationRoll, " +
                              //"CameraAngle, " +
                              "Visibility " +
                             ") " +
                            "VALUES (" 
                                   + filmId
                                   //+ _carInFrameInfo.videoNumber
                            + ", " + CarId
                            + ", " + frametime.ToString("G", CultureInfo.InvariantCulture)
                            + ", " + _carInFrameInfo.positionX.ToString("G", CultureInfo.InvariantCulture)
                            + ", " + _carInFrameInfo.positionY.ToString("G", CultureInfo.InvariantCulture)
                            + ", " + _carInFrameInfo.positionZ.ToString("G", CultureInfo.InvariantCulture)
                            + ", " + _carInFrameInfo.rotationYaw.ToString("G", CultureInfo.InvariantCulture)
                             
                            + ", " + _carInFrameInfo.rotationPitch.ToString("G", CultureInfo.InvariantCulture)
                            + ", " + _carInFrameInfo.rotationRoll.ToString("G", CultureInfo.InvariantCulture)

                            // + ", " + _carInFrameInfo.cameraAngle.ToString("G", CultureInfo.InvariantCulture)
                            + ", " + _carInFrameInfo.visibility
                            + " )";

            OleDbCommand command41 = new OleDbCommand(query41, _myConnection);
            command41.ExecuteNonQuery();
        }

        private void DeleteCarButton_Click(object sender, RoutedEventArgs e)
        {

            videoCars.Remove(_car);
            UpdateCrossingsListBox();
            CarFrameInfoListBox.Items.Clear();

            //foreach (var item in CarsListBox.SelectedItems)
            //{
            //    VideoCar c = (VideoCar)item;
            //    videoCars.Remove(c);
            //}
            CarsListBox.Items.Refresh();
        }

        private void CrossingsListbox_SelectionChanged(object sender, SelectionChangedEventArgs e)
        {
            if (CrossingsListBox_lock == false )
            {
                var item = (ListBoxItem)CrossingsListbox.SelectedItem;

                //string framestr = item?.Tag.ToString();
                double crossingid = Convert.ToInt32(item?.Tag);

                int carindex = videoCars.FindIndex(y => y.crossingId == crossingid);
                int crossingindex = _crossings.FindIndex(x => x.TrackObjectID == crossingid);

                if (carindex >= 0)
                {

                    _car = videoCars[carindex];
                    UpdateCarInfoLIst(_car);

                }
                if (carindex < 0)
                {
                    CarFrameInfoListBox_lock = true;
                    CarFrameInfoListBox.Items.Clear();
                    CarFrameInfoListBox_lock = false;
                }


                
                if (_graphs.Count > 0)
                {
                    SliderLock = true;
                    frametoshow = _crossings[crossingindex].Start.FilmFrameTime * _graphs[0].framesInSecond;
                    Slider.Value = frametoshow;



                    //показать машину на переезде при его выборе
                    int frameint = Convert.ToInt32(frametoshow);

                    CalculateCarFrameInfo(_car, frameint);

                    _graphs[0]._compositor.carInFrame = _carInFrameInfo;
                    if (_carInFrameInfo != null) _graphs[0]._compositor._carDrawer._angle = _carInFrameInfo.cameraAngle;
                    if (_carInFrameInfo != null) Slider2.Value = _carInFrameInfo.cameraAngle;

                    _graphs[0].ShowFrame(frametoshow);
                    SliderLock = false;

                    UpdatecarframeInfoTextBlockText();
                }
            }
        }

        private void AddCarButton_Click(object sender, RoutedEventArgs e)
        {
            if (CrossingsListbox.SelectedItem != null)
            {
                var item = (ListBoxItem)CrossingsListbox.SelectedItem;
                double crossingid = Convert.ToInt32(item?.Tag);
                
                int carindex = videoCars.FindIndex(y => y.crossingId == crossingid);
                int crossingindex = _crossings.FindIndex(x => x.TrackObjectID == crossingid);

                
                if (carindex < 0)
                {
                    VideoCar carToAdd = new VideoCar();

                    if (videoCars.Count > 0)
                    {
                        carToAdd.id = videoCars[videoCars.Count - 1].id + 1;
                    }
                    else
                    {
                        carToAdd.id = 1;
                    }

                    carToAdd.crossingId = (int)crossingid;

                    CarInFrameInfo cfmtoadd = new CarInFrameInfo();
                    cfmtoadd.frameNumber = (int)frametoshow;
                    cfmtoadd.positionX = -0.15f;
                    cfmtoadd.positionY = -4.0f;
                    cfmtoadd.positionZ = 6;
                    cfmtoadd.rotationYaw = 0;
                    cfmtoadd.cameraAngle = 73.4f;

                    //carToAdd.frameInfos.Add(cfmtoadd);
                    videoCars.Add(carToAdd);
                }
            }
            UpdateCrossingsListBox();
        }

        private void Slider2_ValueChanged(object sender, RoutedPropertyChangedEventArgs<double> e)
        {
            if (SliderLock == false && _carInFrameInfo!=null)
            {
                _graphs[0]._compositor._carDrawer._angle = Slider2.Value;
                frametoshow = (int)Slider.Value; //_graphs[0].framesInSecond;
                

                CalculateCarFrameInfo(_car, (int)frametoshow);


                //для данного кадра
                //_carInFrameInfo.cameraAngle = (float)Slider2.Value;

                // для всех ключевых кадров этой машины
                foreach (CarInFrameInfo V in _car.frameInfos)
                {
                    V.cameraAngle = (float)Slider2.Value;
                }
                // как свойство машины
                _car.cameraAngle= (float)Slider2.Value;


                if (_graphs.Count > 0)
                {
                    _graphs[0]._compositor.carInFrame = _carInFrameInfo;
                    _graphs[0].ShowFrame(frametoshow);

                    UpdatecarframeInfoTextBlockText();
                    //carframeInfoTextBlock.Text = "кадр: " + frametoshow.ToString() 
                    //                                      + "\n" + Math.Round(CalculateFrameDistance((int)frametoshow), 2)
                    //                                      + "\n угол: " + Slider2.Value
                    //                                      ;
                }
            }
        }

        

        private void VisibilityCheckBox_Unchecked(object sender, RoutedEventArgs e)
        {
            _carInFrameInfo.visibility = 0;
            UpdatecarframeInfoTextBlockText();
        }

        private void VisibilityCheckBox_Checked(object sender, RoutedEventArgs e)
        {
            _carInFrameInfo.visibility = 1;
            UpdatecarframeInfoTextBlockText();
        }

        private void PrevKeyFrameButton_Click(object sender, RoutedEventArgs e)
        {
            MoveToPrevKeyFrame();
        }

        private void NextKeyFrameButton_Click(object sender, RoutedEventArgs e)
        {
            MoveToNextKeyframe();
        }
    }
}
