using Microsoft.UI.Xaml;
using Microsoft.UI.Xaml.Media;

namespace WOADeviceManager
{
    public sealed partial class MainWindow : Window
    {
        public static Window WindowInstance { get; private set; }

        public MainWindow()
        {
            InitializeComponent();
            WindowInstance = this;

            SystemBackdrop = new MicaBackdrop();

            _ = MainNavigation.Navigate(typeof(MainPage));
        }
    }
}
