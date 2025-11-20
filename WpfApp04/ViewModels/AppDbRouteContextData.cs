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
    class AppDbRouteContextData : INotifyPropertyChanged
    {
        // Строки подключения и файлы
        private string _connectString;
        public string ConnectString
        {
            get => _connectString;
            set
            {
                _connectString = value;
                OnPropertyChanged();
            }
        }

        private string _fileName = "";
        public string FileName
        {
            get => _fileName;
            set
            {
                _fileName = value;
                OnPropertyChanged();
            }
        }

        public string ConnectSrting1 { get; set; } = "Provider=Microsoft.ACE.OLEDB.12.0;Data Source=";

        // Маршруты
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

        // Коллекции
        private ObservableCollection<PointOnTrack> _pointOnTracksToShow = new ObservableCollection<PointOnTrack>();
        public ObservableCollection<PointOnTrack> PointOnTracksToShow
        {
            get => _pointOnTracksToShow;
            set
            {
                _pointOnTracksToShow = value;
                OnPropertyChanged();
            }
        }

        private ObservableCollection<Segment> _segmentsSourseFromEgis = new ObservableCollection<Segment>();
        public ObservableCollection<Segment> SegmentsSourseFromEgis
        {
            get => _segmentsSourseFromEgis;
            set
            {
                _segmentsSourseFromEgis = value;
                OnPropertyChanged();
            }
        }

        private ObservableCollection<Segment> _segmentsToFillFromEgis = new ObservableCollection<Segment>();
        public ObservableCollection<Segment> SegmentsToFillFromEgis
        {
            get => _segmentsToFillFromEgis;
            set
            {
                _segmentsToFillFromEgis = value;
                OnPropertyChanged();
            }
        }

        private ObservableCollection<PointOnTrack> _pointOnTracksToAdd = new ObservableCollection<PointOnTrack>();
        public ObservableCollection<PointOnTrack> PointOnTracksToAdd
        {
            get => _pointOnTracksToAdd;
            set
            {
                _pointOnTracksToAdd = value;
                OnPropertyChanged();
            }
        }

        private ObservableCollection<Station> _egisSelectedStations = new ObservableCollection<Station>();
        public ObservableCollection<Station> EgisSelectedStations
        {
            get => _egisSelectedStations;
            set
            {
                _egisSelectedStations = value;
                OnPropertyChanged();
            }
        }

        private ObservableCollection<Track> _egisSelectedTracks = new ObservableCollection<Track>();
        public ObservableCollection<Track> EgisSelectedTracks
        {
            get => _egisSelectedTracks;
            set
            {
                _egisSelectedTracks = value;
                OnPropertyChanged();
            }
        }

        private ObservableCollection<PointOnTrack> _egisFoundPointObjects = new ObservableCollection<PointOnTrack>();
        public ObservableCollection<PointOnTrack> EgisFoundPointObjects
        {
            get => _egisFoundPointObjects;
            set
            {
                _egisFoundPointObjects = value;
                OnPropertyChanged();
            }
        }

        private ObservableCollection<Kilometer> _selectedKilometersToEdit = new ObservableCollection<Kilometer>();
        public ObservableCollection<Kilometer> SelectedKilometersToEdit
        {
            get => _selectedKilometersToEdit;
            set
            {
                _selectedKilometersToEdit = value;
                OnPropertyChanged();
            }
        }

        // Электронные карты
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

        // Выбранные объекты
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

        // Параметры поиска и направления
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

        // Настройки отрисовки и масштабирования
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

        // Масштабирование уклонов
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

        // Состояние UI
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

        // Соединения с базами данных (не обязательно делать свойствами с уведомлением)
        public string EgisConnectionString { get; set; }
        public SqlConnection EgisConnection { get; set; }
        public OleDbConnection MyConnection { get; set; }

        // Компараторы (можно оставить как поля)
        public PointOnTrackComparer Pcr { get; } = new PointOnTrackComparer();
        public SpeedComparerToshow Scts { get; } = new SpeedComparerToshow();
        public InclineComparer Inclc { get; } = new InclineComparer();
        public StationComparer StationsByRoute { get; } = new StationComparer();

        public event PropertyChangedEventHandler PropertyChanged;

        protected virtual void OnPropertyChanged([CallerMemberName] string propertyName = null)
        {
            PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(propertyName));
        }
    }
}
