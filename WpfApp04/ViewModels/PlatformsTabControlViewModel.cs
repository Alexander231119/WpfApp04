using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.ComponentModel;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace WpfApp04.ViewModels
{
    public class PlatformsTabControlViewModel : INotifyPropertyChanged
    {
        // Событие, которое уведомляет UI (View) об изменениях в свойствах ViewModel
        public event PropertyChangedEventHandler PropertyChanged;
        // Приватное поле для хранения коллекции платформ
        private ObservableCollection<Platform> _platforms;

        /// <summary>
        /// Публичное свойство для доступа к коллекции платформ
        /// ObservableCollection автоматически уведомляет UI о добавлении/удалении элементов
        /// </summary>
        public ObservableCollection<Platform> Platforms
        {
            get => _platforms; // Возвращает текущую коллекцию платформ
            set
            {
                _platforms = value; // Устанавливает новое значение коллекции
                OnPropertyChanged(nameof(Platforms)); // Уведомляет UI об изменении свойства Platforms
            }
        }
        /// <summary>
        /// Метод для вызова события PropertyChanged
        /// Вызывается при изменении любого свойства ViewModel
        /// </summary>
        /// <param name="propertyName">Имя изменившегося свойства</param>
        protected virtual void OnPropertyChanged(string propertyName)
        {
            // Проверяем, есть ли подписчики на событие, и вызываем его если есть
            PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(propertyName));
        }
        /// <summary>
        /// Конструктор ViewModel - инициализирует начальное состояние
        /// </summary>
        public PlatformsTabControlViewModel()
        {
            // Создаем пустую коллекцию платформ при создании ViewModel
            Platforms = new ObservableCollection<Platform>();
        }
    }
}
