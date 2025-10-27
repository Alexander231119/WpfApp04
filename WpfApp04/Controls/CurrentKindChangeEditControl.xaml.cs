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
    /// Interaction logic for CurrentKindChangeEditControl.xaml
    /// </summary>
    public partial class CurrentKindChangeEditControl : UserControl
    {
        // Dependency Property для привязки CurrentKindChange
        public static readonly DependencyProperty CurrentKindChangeProperty =
            DependencyProperty.Register(
                nameof(CurrentKindChange),
                typeof(CurrentKindChange),
                typeof(CurrentKindChangeEditControl),
                new FrameworkPropertyMetadata(
                    null,
                    FrameworkPropertyMetadataOptions.AffectsRender,
                    OnCurrentKindChangeChanged));

        public CurrentKindChange CurrentKindChange
        {
            get => (CurrentKindChange)GetValue(CurrentKindChangeProperty);
            set => SetValue(CurrentKindChangeProperty, value);
        }

        public CurrentKindChangeEditControl()
        {
            InitializeComponent();
            InitializeComboBoxes();
        }

        private void InitializeComboBoxes()
        {
            // Заполняем комбобоксы значениями из словаря
            foreach (var item in CurrentKindChange.DicCurrentKindNames)
            {
                DicCurrentKindNamesStartComboBox.Items.Add(new KeyValuePair<double, string>(item.Key, item.Value));
                DicCurrentKindNamesEndComboBox.Items.Add(new KeyValuePair<double, string>(item.Key, item.Value));
            }

            // Настраиваем отображение
            DicCurrentKindNamesStartComboBox.DisplayMemberPath = "Value";
            DicCurrentKindNamesStartComboBox.SelectedValuePath = "Key";

            DicCurrentKindNamesEndComboBox.DisplayMemberPath = "Value";
            DicCurrentKindNamesEndComboBox.SelectedValuePath = "Key";
        }

        // Обработчик изменения CurrentKindChange
        private static void OnCurrentKindChangeChanged(
            DependencyObject d,
            DependencyPropertyChangedEventArgs e)
        {
            var control = (CurrentKindChangeEditControl)d;
            control.RefreshFromCurrentKindChange();
        }

        public void RefreshFromCurrentKindChange()
        {
            if (CurrentKindChange != null)
            {
                // Устанавливаем выбранные значения в комбобоксы
                DicCurrentKindNamesStartComboBox.SelectedValue = CurrentKindChange.DicCurrentKindIDLeft;
                DicCurrentKindNamesEndComboBox.SelectedValue = CurrentKindChange.DicCurrentKindIDRight;
            }
            else
            {
                DicCurrentKindNamesStartComboBox.SelectedIndex = -1;
                DicCurrentKindNamesEndComboBox.SelectedIndex = -1;
            }
        }

        private void DicCurrentKindNamesStartComboBox_SelectionChanged(object sender, SelectionChangedEventArgs e)
        {
            if (CurrentKindChange != null && DicCurrentKindNamesStartComboBox.SelectedValue != null)
            {
                CurrentKindChange.DicCurrentKindIDLeft = (double)DicCurrentKindNamesStartComboBox.SelectedValue;
            }
        }

        private void DicCurrentKindNamesEndComboBox_SelectionChanged(object sender, SelectionChangedEventArgs e)
        {
            if (CurrentKindChange != null && DicCurrentKindNamesEndComboBox.SelectedValue != null)
            {
                CurrentKindChange.DicCurrentKindIDRight = (double)DicCurrentKindNamesEndComboBox.SelectedValue;
            }
        }
    }
}
