using System;
using System.Collections.Generic;
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
    /// Interaction logic for ImportOptionsControl.xaml
    /// </summary>
    public partial class ImportOptionsControl : UserControl
    {
        // Событие для уведомления об изменении фильтров
        public event Action FilterChanged;


        public ImportOptionsControl()
        {
            InitializeComponent();

            // Подписка на изменения всех чекбоксов
            foreach (var checkBox in GetCheckBoxes())
            {
                checkBox.Checked += (s, e) => FilterChanged?.Invoke();
                checkBox.Unchecked += (s, e) => FilterChanged?.Invoke();
            }
        }

        // Метод для фильтрации точек на пути
        public IEnumerable<PointOnTrack> FilterPoints(IEnumerable<PointOnTrack> points)
        {
            // Если ни один чекбокс не выбран - показываем все точки
            if (!GetCheckBoxes().Any(cb => cb.IsChecked == true))
                return points;

            var filteredPoints = new List<PointOnTrack>();

            // Соответствие между чекбоксами и типами точек
            if (ImportTrafficLights)
                filteredPoints.AddRange(points.Where(p => p.DicPointOnTrackKindID == 1)); // Светофоры

            if (ImportPlatforms)
                filteredPoints.AddRange(points.Where(p => p.DicPointOnTrackKindID == 9)); // Платформы

            if (ImportUksps)
                filteredPoints.AddRange(points.Where(p => p.DicPointOnTrackKindID == 25)); // УКСПС

            // Добавьте остальные соответствия по аналогии...
            if (ImportKtsm)
                filteredPoints.AddRange(points.Where(p => p.DicPointOnTrackKindID == 24)); // КТСМ

            if (ImportSigns)
                filteredPoints.AddRange(points.Where(p => p.DicPointOnTrackKindID == 37)); // Знаки

            if (ImportCrossings)
                filteredPoints.AddRange(points.Where(p => p.DicPointOnTrackKindID == 23)); // Переезды

            if (ImportNeutralSections)
                filteredPoints.AddRange(points.Where(p => p.DicPointOnTrackKindID == 35)); // Нейтральные вставки

            if (ImportInclines)
                filteredPoints.AddRange(points.Where(p => p.DicPointOnTrackKindID == 32)); // Уклоны

            if (ImportSpeed)
                filteredPoints.AddRange(points.Where(p => p.DicPointOnTrackKindID == 2)); // Ограничения скорости

            if (ImportStationBorders)
                filteredPoints.AddRange(points.Where(p => p.DicPointOnTrackKindID == 8)); // Границы станций

            if (ImportCurrentChange)
                filteredPoints.AddRange(points.Where(p => p.DicPointOnTrackKindID == 40)); // токораздел


            return filteredPoints.Distinct(); // На случай если точка попадает под несколько критериев
        }

        // Вспомогательный метод для получения всех чекбоксов
        private IEnumerable<CheckBox> GetCheckBoxes()
        {
            return new[]
            {
                ImportTrafficLightsCheckBox,
                ImportPlatformsCheckBox,
                ImportUkspsCheckBox,
                ImportKtsmCheckBox,
                ImportSignsCheckBox,
                ImportCrossingsCheckBox,
                ImportNeutralSectionsCheckBox,
                ImportInclinesCheckBox,
                ImportSpeedCheckBox,
                ImportStationBordersCheckBox,
                ImportCurrentChangeCheckBox
            };
        }

        // Свойства для доступа к состоянию чекбоксов
        public bool ImportTrafficLights => ImportTrafficLightsCheckBox.IsChecked ?? false;
        public bool ImportPlatforms => ImportPlatformsCheckBox.IsChecked ?? false;
        public bool ImportUksps => ImportUkspsCheckBox.IsChecked ?? false;
        public bool ImportKtsm => ImportKtsmCheckBox.IsChecked ?? false;
        public bool ImportSigns => ImportSignsCheckBox.IsChecked ?? false;
        public bool ImportCrossings => ImportCrossingsCheckBox.IsChecked ?? false;
        public bool ImportNeutralSections => ImportNeutralSectionsCheckBox.IsChecked ?? false;
        public bool ImportInclines => ImportInclinesCheckBox.IsChecked ?? false;
        public bool ImportSpeed => ImportSpeedCheckBox.IsChecked ?? false;
        public bool ImportStationBorders => ImportStationBordersCheckBox.IsChecked ?? false;
        public bool ImportCurrentChange => ImportCurrentChangeCheckBox.IsChecked?? false;

        // Метод для применения состояний к RouteExportCheckBoxList
        public void ApplyToCheckBoxList(RouteExportCheckBoxList checkBoxList)
        {
            checkBoxList._ImportTrafficLightsCheckBox = ImportTrafficLights;
            checkBoxList._ImportPlatformsCheckBox = ImportPlatforms;
            checkBoxList._ImportUkspsCheckBox = ImportUksps;
            checkBoxList._ImportKtsmCheckBox = ImportKtsm;
            checkBoxList._ImportSignsCheckBox = ImportSigns;
            checkBoxList._ImportCrossingsCheckBox = ImportCrossings;
            checkBoxList._ImportNeutralSectionsCheckBox = ImportNeutralSections;
            checkBoxList._ImportInclinesCheckBox = ImportInclines;
            checkBoxList._ImportSpeedCheckBox = ImportSpeed;
            checkBoxList._ImportStationBordersCheckBox = ImportStationBorders;
            checkBoxList._ImportCurrentKindChangeCheckBox = ImportCurrentChange;
        }

        
    }
}
