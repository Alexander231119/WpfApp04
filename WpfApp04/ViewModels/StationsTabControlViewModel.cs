using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.ComponentModel;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace WpfApp04.ViewModels
{
    public class StationsTabControlViewModel : INotifyPropertyChanged
    {
        // Событие, которое уведомляет UI (View) об изменениях в свойствах ViewModel
        public event PropertyChangedEventHandler PropertyChanged;
        // Приватное поле для хранения коллекции
        private ObservableCollection<Station> _stations;
        /// <summary>
        /// Публичное свойство для доступа к коллекции станций
        /// ObservableCollection автоматически уведомляет UI о добавлении/удалении элементов
        /// </summary>
        public ObservableCollection<Station> Stations
        {
            get => _stations;// Возвращает текущую коллекцию
            set
            {
                _stations = value;// Устанавливает новое значение коллекции
                OnPropertyChanged(nameof(Stations));// Уведомляет UI об изменении свойства Stations
            }
        }
        /// <summary>
        /// Метод для вызова события PropertyChanged
        /// Вызывается при изменении любого свойства ViewModel
        /// </summary>
        /// <param name="propertyName">Имя изменившегося свойства</param>
        protected virtual void OnPropertyChanged(string propertyName)
        {
            PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(propertyName));
        }

        public StationsTabControlViewModel()
        {
            Stations = new ObservableCollection<Station>();
        }
    }
}