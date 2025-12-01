using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.ComponentModel;
using System.Data.OleDb;
using System.Data.SqlClient;
using System.Linq;
using System.Runtime.CompilerServices;
using System.Text;
using System.Threading.Tasks;

namespace WpfApp04.ViewModels
{
    // Единый источник данных (самое правильное)
    // класс-модель для общих данных


    // Переменные заменены на использование _appData
    // DbRoute route1 = new DbRoute(); -> _appData.Route1
    // DbRoute toAddRoute = new DbRoute(); -> _appData.ToAddRoute
    // DbRoute egisRoute1 = new DbRoute(); -> _appData.EgisRoute1
    // public DbRoute routeToShowInDataGrids = new DbRoute(); -> _appData.RouteToShowInDataGrids
    // private List<PointOnTrack> pointOnTracksToShow = new List<PointOnTrack>(); -> _appData.PointOnTracksToShow
    // ElectonicMap map1 = new ElectonicMap(); -> _appData.Map1
    // RoutesElectronicMap routesElectronicMap = new RoutesElectronicMap(); -> _appData.RoutesElectronicMap
    // public DbRoute ekDbRoute = new DbRoute(); -> _appData.EkDbRoute
    // private List<Segment> segmentsSourseFromEgis = new List<Segment>(); -> _appData.SegmentsSourseFromEgis
    // List<Segment> segmentsToFillFromEgis = new List<Segment>(); -> _appData.SegmentsToFillFromEgis
    // private List<PointOnTrack> PointOnTracksToAdd = new List<PointOnTrack>(); -> _appData.PointOnTracksToAdd
    // static string egisconnectionString; -> _appData.EgisConnectionString
    // SqlConnection egisconnection; -> _appData.EgisConnection
    // List<Station> EgisSelectedStations= new List<Station>(); -> _appData.EgisSelectedStations
    // List<Track> EgisSelectedTracks= new List<Track>(); -> _appData.EgisSelectedTracks
    // List<PointOnTrack> EgisFoundPointObjects = new List<PointOnTrack>(); -> _appData.EgisFoundPointObjects
    // Track egisSelectedTrack = new Track(); -> _appData.EgisSelectedTrack
    // double UsageDirectionToFind = 1; -> _appData.UsageDirectionToFind
    // private double SpeedKindToFind = 0; -> _appData.SpeedKindToFind
    // public List<Kilometer> selectedKilometersToEdit = new List<Kilometer>(); -> _appData.SelectedKilometersToEdit
    // string fileName = ""; -> _appData.FileName
    // public static string ConnectString = ""; -> _appData.ConnectString
    // Масштаб и положение элементов -> соответствующие свойства в _appData
    // public double heighscale { get; set; } = 1; -> _appData.Heighscale
    // public double widtscale = 0.1; -> _appData.Widtscale
    // public double segmentsBottom = 0; -> _appData.SegmentsBottom
    // public double segmentsHeight = 20; -> _appData.SegmentsHeight
    // public double kilometersBottom = 20; -> _appData.KilometersBottom
    // public double kilometersHeight = 30; -> _appData.KilometersHeight
    // public double pkLineBottom = 50; -> _appData.PkLineBottom
    // public double pkLineHeight = 10; -> _appData.PkLineHeight
    // public double inclineControlBottom = 60; -> _appData.InclineControlBottom
    // public double inclineControlHeight = 240; -> _appData.InclineControlHeight
    // public double floorBottom = 100; -> _appData.FloorBottom
    // public double trafficSignalsBottom = 255; -> _appData.TrafficSignalsBottom
    // public double trackCircuitsBottom = 300; -> _appData.TrackCircuitsBottom
    // public double trackCircuitsHeight = 15; -> _appData.TrackCircuitsHeight
    // public double stationsBottom = 315; -> _appData.StationsBottom
    // public double stationsHeight = 65; -> _appData.StationsHeight
    // public double speedBottom = 400; -> _appData.SpeedBottom
    // public double maxSpeed = 300; -> _appData.MaxSpeed
    // public double kscale=1; -> _appData.Kscale
    // public double lscale=1; -> _appData.Lscale
    // public double maxElev=0; -> _appData.MaxElev
    // public double minElev=0; -> _appData.MinElev
    // public double lastX = 0; -> _appData.LastX
    // public double lastY = 0; -> _appData.LastY
    // public bool rowchanged = false; -> _appData.RowChanged
    // private bool EgisPtNormsGridLock = false; -> _appData.EgisPtNormsGridLock
    // private OleDbConnection myConnection; -> _appData.MyConnection
    // Компараторы -> соответствующие свойства в _appData

    public class AppDbRouteContextData : INotifyPropertyChanged
    {
        //строка для подключения к основной базе данных
        /// <summary>
        /// Строка подключения к основной базе данных маршрутов
        /// </summary>
        private string _connectString = "";
        public string ConnectString
        {
            get => _connectString;
            set
            {
                _connectString = value;
                OnPropertyChanged();
            }
        }

        //строка для подключения к БД для экспорта (скоростей)
        /// <summary>
        /// Строка подключения к целевой базе данных для экспорта данных (ограничений скорости)
        /// </summary>
        private string _connectString2 = "";
        public string ConnectString2
        {
            get => _connectString2;
            set
            {
                _connectString2 = value;
                OnPropertyChanged();
            }
        }
        //основа для строки подключения 
        /// <summary>
        /// Базовый шаблон строки подключения к Access базе данных
        /// Формат: "Provider=Microsoft.ACE.OLEDB.12.0;Data Source="
        /// </summary>
        private string _connectString1 = "Provider=Microsoft.ACE.OLEDB.12.0;Data Source=";
        public string ConnectString1
        {
            get => _connectString1;
            set
            {
                _connectString1 = value;
                OnPropertyChanged();
            }
        }

        private string _fileName = "";
        public string FileName
        {
            get => _fileName;
            set { _fileName = value; OnPropertyChanged(); }
        }
        
        //имя обьекта для поиска в егис
        private string _objectNameToFind = "";
        public string ObjectNameToFind
        {
            get => _objectNameToFind;
            set
            {
                _objectNameToFind = value;
                OnPropertyChanged();
            }
        }

        //имя станции для поиска в егис - текст введённый пользователем
        private string _stationNameToFind = "";
        public string StationNameToFind
        {
            get => _stationNameToFind;
            set
            {
                _stationNameToFind = value;
                OnPropertyChanged();
            }
        }
        

        // Основной маршрут
        private DbRoute _route1 = new DbRoute();
        public DbRoute Route1
        {
            get => _route1;
            set
            {
                _route1 = value;
                OnPropertyChanged();
            }
        }

        private DbRoute _toAddRoute = new DbRoute();
        public DbRoute ToAddRoute
        {
            get => _toAddRoute;
            set
            {
                _toAddRoute = value;
                OnPropertyChanged();
            }
        }

        private DbRoute _egisRoute1 = new DbRoute();
        public DbRoute EgisRoute1
        {
            get => _egisRoute1;
            set
            {
                _egisRoute1 = value;
                OnPropertyChanged();
            }
        }

        private DbRoute _routeToShowInDataGrids = new DbRoute();
        public DbRoute RouteToShowInDataGrids
        {
            get => _routeToShowInDataGrids;
            set
            {
                _routeToShowInDataGrids = value;
                OnPropertyChanged();
            }
        }

        // Списко трочек на пути для показа в таблице PointOnTrackEditGrid
        private List<PointOnTrack> _pointOnTracksToShow = new List<PointOnTrack>();
        public List<PointOnTrack> PointOnTracksToShow
        {
            get => _pointOnTracksToShow;
            set
            {
                _pointOnTracksToShow = value;
                OnPropertyChanged();
            }
        }

        //выбранное ограничение скорости
        private SpeedRestriction _speedRestrictionToEdit;
        public SpeedRestriction SpeedRestrictionToEdit
        {
            get=> _speedRestrictionToEdit;
            set
            {
                _speedRestrictionToEdit=value;
                OnPropertyChanged();
            }
        }

        // Файл электронной карты
        private ElectonicMap _map1 = new ElectonicMap();
        public ElectonicMap Map1
        {
            get => _map1;
            set
            {
                _map1 = value;
                OnPropertyChanged();
            }
        }

        // Файл электронной карты сконвертированный в списки DbRoute
        private RoutesElectronicMap _routesElectronicMap = new RoutesElectronicMap();
        public RoutesElectronicMap RoutesElectronicMap
        {
            get => _routesElectronicMap;
            set
            {
                _routesElectronicMap = value;
                OnPropertyChanged();
            }
        }

        // Выбранная карта или pos в карте
        private DbRoute _ekDbRoute = new DbRoute();
        public DbRoute EkDbRoute
        {
            get => _ekDbRoute;
            set
            {
                _ekDbRoute = value;
                OnPropertyChanged();
            }
        }

        private List<Segment> _segmentsSourseFromEgis = new List<Segment>();
        public List<Segment> SegmentsSourseFromEgis
        {
            get => _segmentsSourseFromEgis;
            set
            {
                _segmentsSourseFromEgis = value;
                OnPropertyChanged();
            }
        }

        // Сегменты в целевом маршруте для выбора для экспорта обьектов например route1
        private List<Segment> _segmentsToFillFromEgis = new List<Segment>();
        public List<Segment> SegmentsToFillFromEgis
        {
            get => _segmentsToFillFromEgis;
            set
            {
                _segmentsToFillFromEgis = value;
                OnPropertyChanged();
            }
        }

        // Отдельный список точек на пути для добавления без создания новых обьектов
        private List<PointOnTrack> _pointOnTracksToAdd = new List<PointOnTrack>();
        public List<PointOnTrack> PointOnTracksToAdd
        {
            get => _pointOnTracksToAdd;
            set
            {
                _pointOnTracksToAdd = value;
                OnPropertyChanged();
            }
        }

        private string _egisConnectionString;
        public string EgisConnectionString
        {
            get => _egisConnectionString;
            set
            {
                _egisConnectionString = value;
                OnPropertyChanged();
            }
        }

        private SqlConnection _egisConnection;
        public SqlConnection EgisConnection
        {
            get => _egisConnection;
            set
            {
                _egisConnection = value;
                OnPropertyChanged();
            }
        }

        // Найденные станции
        private List<Station> _egisSelectedStations = new List<Station>();
        public List<Station> EgisSelectedStations
        {
            get => _egisSelectedStations;
            set
            {
                _egisSelectedStations = value;
                OnPropertyChanged();
            }
        }

        // Выбранная станция для поиска путей и обьектов
        private Station _egisSelectedStation=new Station();
        public Station EgisSelectedStation
        {
            get=> _egisSelectedStation;
            set
            {
                _egisSelectedStation = value;
                OnPropertyChanged();
            }
        }


        // Пути проходящие через станцию
        private List<Track> _egisSelectedTracks = new List<Track>();
        public List<Track> EgisSelectedTracks
        {
            get => _egisSelectedTracks;
            set
            {
                _egisSelectedTracks = value;
                OnPropertyChanged();
            }
        }

        // Для поиска по имени обьекта
        private List<PointOnTrack> _egisFoundPointObjects = new List<PointOnTrack>();
        public List<PointOnTrack> EgisFoundPointObjects
        {
            get => _egisFoundPointObjects;
            set
            {
                _egisFoundPointObjects = value;
                OnPropertyChanged();
            }
        }

        // выбранная точка на пути при поиске обьектов по названию обьекта
        private PointOnTrack _egisFoundPointOnTrack=new PointOnTrack();

        public PointOnTrack EgisFoundPointOnTrack
        {
            get=> _egisFoundPointOnTrack;
            set
            {
                _egisFoundPointOnTrack = value;
                OnPropertyChanged();
            }
        }

        // Выбранный путь
        private Track _egisSelectedTrack = new Track();
        public Track EgisSelectedTrack
        {
            get => _egisSelectedTrack;
            set
            {
                _egisSelectedTrack = value;
                OnPropertyChanged();
            }
        }

        // Направление для поиска обьектов возрастание или убывание
        private double _usageDirectionToFind = 1;
        public double UsageDirectionToFind
        {
            get => _usageDirectionToFind;
            set
            {
                _usageDirectionToFind = value;
                OnPropertyChanged();
            }
        }

        // Вид движения для поиска скоростей и проб тормозов
        private double _speedKindToFind = 0;
        public double SpeedKindToFind
        {
            get => _speedKindToFind;
            set
            {
                _speedKindToFind = value;
                OnPropertyChanged();
            }
        }

        // Километры которые выбрал пользователь
        private List<Kilometer> _selectedKilometersToEdit = new List<Kilometer>();
        public List<Kilometer> SelectedKilometersToEdit
        {
            get => _selectedKilometersToEdit;
            set
            {
                _selectedKilometersToEdit = value;
                OnPropertyChanged();
            }
        }



        // Масштаб и положение элементов
        private double _heighscale = 1;
        public double Heighscale
        {
            get => _heighscale;
            set
            {
                _heighscale = value;
                OnPropertyChanged();
            }
        }

        private double _widtscale = 0.1;
        public double Widtscale
        {
            get => _widtscale;
            set
            {
                _widtscale = value;
                OnPropertyChanged();
            }
        }

        private double _segmentsBottom = 0;
        public double SegmentsBottom
        {
            get => _segmentsBottom;
            set
            {
                _segmentsBottom = value;
                OnPropertyChanged();
            }
        }

        private double _segmentsHeight = 20;
        public double SegmentsHeight
        {
            get => _segmentsHeight;
            set
            {
                _segmentsHeight = value;
                OnPropertyChanged();
            }
        }

        private double _kilometersBottom = 20;
        public double KilometersBottom
        {
            get => _kilometersBottom;
            set
            {
                _kilometersBottom = value;
                OnPropertyChanged();
            }
        }

        private double _kilometersHeight = 30;
        public double KilometersHeight
        {
            get => _kilometersHeight;
            set
            {
                _kilometersHeight = value;
                OnPropertyChanged();
            }
        }

        private double _pkLineBottom = 50;
        public double PkLineBottom
        {
            get => _pkLineBottom;
            set
            {
                _pkLineBottom = value;
                OnPropertyChanged();
            }
        }

        private double _pkLineHeight = 10;
        public double PkLineHeight
        {
            get => _pkLineHeight;
            set
            {
                _pkLineHeight = value;
                OnPropertyChanged();
            }
        }

        private double _inclineControlBottom = 60;
        public double InclineControlBottom
        {
            get => _inclineControlBottom;
            set
            {
                _inclineControlBottom = value;
                OnPropertyChanged();
            }
        }

        // Не используется
        private double _inclineControlHeight = 240;
        public double InclineControlHeight
        {
            get => _inclineControlHeight;
            set
            {
                _inclineControlHeight = value;
                OnPropertyChanged();
            }
        }

        private double _floorBottom = 100;
        public double FloorBottom
        {
            get => _floorBottom;
            set
            {
                _floorBottom = value;
                OnPropertyChanged();
            }
        }

        private double _trafficSignalsBottom = 255;
        public double TrafficSignalsBottom
        {
            get => _trafficSignalsBottom;
            set
            {
                _trafficSignalsBottom = value;
                OnPropertyChanged();
            }
        }

        private double _trackCircuitsBottom = 300;
        public double TrackCircuitsBottom
        {
            get => _trackCircuitsBottom;
            set
            {
                _trackCircuitsBottom = value;
                OnPropertyChanged();
            }
        }

        private double _trackCircuitsHeight = 15;
        public double TrackCircuitsHeight
        {
            get => _trackCircuitsHeight;
            set
            {
                _trackCircuitsHeight = value;
                OnPropertyChanged();
            }
        }

        private double _stationsBottom = 315;
        public double StationsBottom
        {
            get => _stationsBottom;
            set
            {
                _stationsBottom = value;
                OnPropertyChanged();
            }
        }

        private double _stationsHeight = 65;
        public double StationsHeight
        {
            get => _stationsHeight;
            set
            {
                _stationsHeight = value;
                OnPropertyChanged();
            }
        }

        // Высота отрисовки ограничений скорости
        private double _speedBottom = 400;
        public double SpeedBottom
        {
            get => _speedBottom;
            set
            {
                _speedBottom = value;
                OnPropertyChanged();
            }
        }

        private double _maxSpeed = 300;
        public double MaxSpeed
        {
            get => _maxSpeed;
            set
            {
                _maxSpeed = value;
                OnPropertyChanged();
            }
        }

        // Для масштабирования уклонов
        private double _kscale = 1;
        public double Kscale
        {
            get => _kscale;
            set
            {
                _kscale = value;
                OnPropertyChanged();
            }
        }

        private double _lscale = 1;
        public double Lscale
        {
            get => _lscale;
            set
            {
                _lscale = value;
                OnPropertyChanged();
            }
        }

        private double _maxElev = 0;
        public double MaxElev
        {
            get => _maxElev;
            set
            {
                _maxElev = value;
                OnPropertyChanged();
            }
        }

        private double _minElev = 0;
        public double MinElev
        {
            get => _minElev;
            set
            {
                _minElev = value;
                OnPropertyChanged();
            }
        }

        // положение курсора на экране

        private double _lastX = 0;
        public double LastX
        {
            get => _lastX;
            set
            {
                _lastX = value;
                OnPropertyChanged();
            }
        }

        private double _lastY = 0;
        public double LastY
        {
            get => _lastY;
            set
            {
                _lastY = value;
                OnPropertyChanged();
            }
        }

        private bool _rowChanged = false;
        public bool RowChanged
        {
            get => _rowChanged;
            set
            {
                _rowChanged = value;
                OnPropertyChanged();
            }
        }

        private bool _egisPtNormsGridLock = false;
        public bool EgisPtNormsGridLock
        {
            get => _egisPtNormsGridLock;
            set
            {
                _egisPtNormsGridLock = value;
                OnPropertyChanged();
            }
        }

        private OleDbConnection _myConnection;
        public OleDbConnection MyConnection
        {
            get => _myConnection;
            set
            {
                _myConnection = value;
                OnPropertyChanged();
            }
        }

        // Компараторы
        private PointOnTrackComparer _pcr = new PointOnTrackComparer();
        public PointOnTrackComparer Pcr
        {
            get => _pcr;
            set
            {
                _pcr = value;
                OnPropertyChanged();
            }
        }

        private SpeedComparerToshow _scts = new SpeedComparerToshow();
        public SpeedComparerToshow Scts
        {
            get => _scts;
            set
            {
                _scts = value;
                OnPropertyChanged();
            }
        }

        private InclineComparer _inclc = new InclineComparer();
        public InclineComparer Inclc
        {
            get => _inclc;
            set
            {
                _inclc = value;
                OnPropertyChanged();
            }
        }

        private StationComparer _stationsByRoute = new StationComparer();
        public StationComparer StationsByRoute
        {
            get => _stationsByRoute;
            set
            {
                _stationsByRoute = value;
                OnPropertyChanged();
            }
        }

        public event PropertyChangedEventHandler PropertyChanged;
        protected virtual void OnPropertyChanged([CallerMemberName] string propertyName = null)
        {
            PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(propertyName));
        }
    }
}
