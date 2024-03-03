using Microsoft.UI.Xaml;
using Microsoft.UI.Xaml.Media;
using WOADeviceManager.Pages;

namespace WOADeviceManager
{
    public sealed partial class MainWindow : Window
    {
        public MainWindow()
        {
            InitializeComponent();

            SystemBackdrop = new MicaBackdrop();

            _ = MainNavigation.Navigate(typeof(MainPage));
        }
    }
}
