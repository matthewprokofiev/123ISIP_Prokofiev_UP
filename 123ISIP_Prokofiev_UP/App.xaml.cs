using System.Windows;
using System.Windows.Threading;
using _123ISIP_Prokofiev_UP.Services;

namespace _123ISIP_Prokofiev_UP
{
    public partial class App : Application
    {
        public App()
        {
            DispatcherUnhandledException += App_DispatcherUnhandledException;
        }

        private void App_DispatcherUnhandledException(object sender, DispatcherUnhandledExceptionEventArgs e)
        {
            Log.Write("UNHANDLED: " + e.Exception);
            MessageBox.Show("Произошла ошибка: " + e.Exception.Message, "Ошибка",
                MessageBoxButton.OK, MessageBoxImage.Error);
            e.Handled = true;
        }
    }
}
