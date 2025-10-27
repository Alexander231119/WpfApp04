using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Globalization;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Data;
using System.Windows.Documents;
using System.Windows.Input;
using System.Windows.Media;
using System.Windows.Media.Imaging;
using System.Windows.Navigation;
using System.Windows.Shapes;

namespace WpfApp04.Controls
{
    /// <summary>
    /// Interaction logic for TrafficLightEditControl.xaml
    /// </summary>
    //public partial class TrafficLightEditControl : UserControl
    //{
    //    public TrafficLight _TrafficLight;// = new TrafficLight();
    //    public TrafficLightEditControl()
    //    {
    //        //this._TrafficLight= new TrafficLight();
    //        InitializeComponent();
    //        DataContext = new Combobox1ViewModel();


    //    }

    //    public void RefreshFromTrafficLight()
    //    {
    //        TliRestrictionDataGrid.ItemsSource = _TrafficLight?.TliRestrictions;
    //        TliRestrictionDataGrid.Items.Refresh();
    //    }

    //    private void AddTliRestrictionButton_Click(object sender, RoutedEventArgs e)
    //    {
    //        TliRestriction tr = new TliRestriction();
    //        tr.autoBlockCode = AutoBlockInternalControlCode.Dummy;
    //        tr.routeKind = ALSRouteKind.Dummy;
    //        tr.kind = TliRestrictionKind.Cargo;
    //        _TrafficLight.TliRestrictions.Add(tr);
    //        TliRestrictionDataGrid.Items.Refresh();
    //    }


    //}
    public partial class TrafficLightEditControl : UserControl
    {
        // Dependency Property для привязки TrafficLight
        public static readonly DependencyProperty TrafficLightProperty =
            DependencyProperty.Register(
                nameof(TrafficLight),
                typeof(TrafficLight),
                typeof(TrafficLightEditControl),
                new FrameworkPropertyMetadata(
                    null,
                    FrameworkPropertyMetadataOptions.AffectsRender,
                    OnTrafficLightChanged));

        public TrafficLight TrafficLight
        {
            get => (TrafficLight)GetValue(TrafficLightProperty);
            set => SetValue(TrafficLightProperty, value);
        }

        public TrafficLightEditControl()
        {
            InitializeComponent();
        }

        // Обработчик изменения TrafficLight
        private static void OnTrafficLightChanged(
            DependencyObject d,
            DependencyPropertyChangedEventArgs e)
        {
            var control = (TrafficLightEditControl)d;
            control.DataContext = new Combobox1ViewModel((TrafficLight)e.NewValue);
            control.RefreshFromTrafficLight();
        }

        public void RefreshFromTrafficLight()
        {
            TliRestrictionDataGrid.ItemsSource = TrafficLight?.TliRestrictions;
            TliRestrictionDataGrid.Items.Refresh();
        }

        private void AddTliRestrictionButton_Click(object sender, RoutedEventArgs e)
        {
            var tr = new TliRestriction
            {
                autoBlockCode = AutoBlockInternalControlCode.Dummy,
                routeKind = ALSRouteKind.Dummy,
                kind = TliRestrictionKind.Cargo
            };

            TrafficLight.TliRestrictions.Add(tr);
            TliRestrictionDataGrid.Items.Refresh();
        }
    }

    public class AutoBlockCodeProvider
    {
        public AutoBlockInternalControlCode Code { get; private set; }
        public string Description { get; private set; }
        public static IEnumerable<AutoBlockCodeProvider> GetValues()
        {

            yield return new AutoBlockCodeProvider
            {
                Code = AutoBlockInternalControlCode.Dummy,
                Description = "По умолчанию"
            };
            yield return new AutoBlockCodeProvider
            {
                Code = AutoBlockInternalControlCode.None,
                Description = "Нет кода"
            };
            yield return new AutoBlockCodeProvider
            {
                Code = AutoBlockInternalControlCode.YellowWithRed,
                Description = "КЖ"
            };
            yield return new AutoBlockCodeProvider
            {
                Code = AutoBlockInternalControlCode.Yellow,
                Description = "Ж"
            };
            yield return new AutoBlockCodeProvider
            {
                Code = AutoBlockInternalControlCode.Green,
                Description = "З"
            };

        }
    }


    //public class Combobox1ViewModel() : INotifyPropertyChanged
    //{
    //    TrafficLight _CboxtrafficLight;

    //    private double _selectedValue;
    //    public double SelectedValue
    //    {
    //        get => _selectedValue;
    //        set
    //        {

    //                _selectedValue = value;
    //              if(_CboxtrafficLight !=null)  _CboxtrafficLight.EgisABValue = _selectedValue;
    //        OnPropertyChanged(nameof(SelectedValue));

    //        }
    //    }


    //    // Или использовать объекты с полями Value и Display:
    //    public List<ComboItem> ComboItems { get; } = new List<ComboItem>
    //    {
    //        new ComboItem { Value = 243, Display = "паб" },
    //        new ComboItem { Value = 244, Display = "3-хзначная" },
    //        new ComboItem { Value = 245, Display = "4-значная" }
    //    };

    //    public event PropertyChangedEventHandler PropertyChanged;
    //    protected void OnPropertyChanged(string name) =>
    //        PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(name));
    //}
    
        public class Combobox1ViewModel : INotifyPropertyChanged
        {
            private readonly TrafficLight _trafficLight;
            private double _selectedValue;

            public Combobox1ViewModel(TrafficLight trafficLight)
            {
                if (trafficLight == null) return;
                _trafficLight = trafficLight ?? throw new ArgumentNullException(nameof(trafficLight));

                // Инициализация из модели
                _selectedValue = _trafficLight.EgisABValue;

                // Подписка на изменения модели
                _trafficLight.PropertyChanged += OnTrafficLightPropertyChanged;
            }

            public double SelectedValue
            {
                get => _selectedValue;
                set
                {
                    if (_selectedValue == value) return;

                    _selectedValue = value;
                    _trafficLight.EgisABValue = value; // Обновляем модель
                    OnPropertyChanged(nameof(SelectedValue));
                }
            }

            public List<ComboItem> ComboItems { get; } = new List<ComboItem>
            {
            new ComboItem { Value = 243, Display = "паб" },
            new ComboItem { Value = 244, Display = "3-хзначная" },
            new ComboItem { Value = 245, Display = "4-значная" }
            };

            // Обработчик изменений в модели
            private void OnTrafficLightPropertyChanged(object sender, PropertyChangedEventArgs e)
            {
                if (e.PropertyName == nameof(TrafficLight.EgisABValue))
                {
                    _selectedValue = _trafficLight.EgisABValue;
                    OnPropertyChanged(nameof(SelectedValue));
                }
            }

            public event PropertyChangedEventHandler PropertyChanged;
            protected void OnPropertyChanged(string name) =>
                PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(name));
        }

        
    

    public class ComboItem
    {
        public double Value { get; set; }
        public string Display { get; set; }
    }
}
