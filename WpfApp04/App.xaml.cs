using System;
using System.Collections.Generic;
using System.Configuration;
using System.Data;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows;

namespace WpfApp04
{
    /// <summary>
    /// Interaction logic for App.xaml
    /// </summary>
    public partial class App : Application
    {
        public App()
        {
            // Регистрируем провайдер кодировок для поддержки Windows-1251
            Encoding.RegisterProvider(CodePagesEncodingProvider.Instance);

            // Остальной код инициализации, если есть
        }
    }
}
