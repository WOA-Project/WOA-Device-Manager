using Microsoft.UI.Xaml;
using Microsoft.UI.Xaml.Controls;
using System.Collections.Generic;
using System.Diagnostics;
using System.Threading.Tasks;
using WOADeviceManager.Entities;
using WOADeviceManager.Helpers;
using WOADeviceManager.Managers;

namespace WOADeviceManager.Pages
{
    public sealed partial class PartitionsPage : Page
    {
        private readonly List<Partition> partitions = new();

        public PartitionsPage()
        {
            InitializeComponent();

            if (DeviceManager.Device.State == Device.DeviceStateEnum.TWRP)
            {
                MainPage.ToggleLoadingScreen(true);
                _ = RetrieveInitialState();
            }
        }

        private async void RebootToTWRPButton_Click(object sender, RoutedEventArgs e)
        {
            MainPage.ToggleLoadingScreen(true);
            await RetrieveInitialState();
        }

        // TODO: REWRITE THIS MESS OF SPAGHETTI
        private async Task RetrieveInitialState()
        {
            await DeviceRebootHelper.RebootToTWRPAndWait();
            _ = await ADBProcedures.PushParted();
            Debug.WriteLine(ADBManager.Shell.Interact("mv /sdcard/parted /sbin/parted"));
            await Task.Delay(200);
            Debug.WriteLine(ADBManager.Shell.Interact("chmod 755 /sbin/parted"));
            await Task.Delay(200);
            string output = ADBManager.Shell.Interact("parted -m /dev/block/sda print");
            string[] lines = output.Split(';');
            int startingLine = 2;
            for (int i = startingLine; i < lines.Length; i++)
            {
                string[] parts = lines[i].Split(':');
                if (parts != null && parts.Length == 7)
                {
                    Debug.WriteLine("Partition found: " + parts[5]);
                    partitions.Add(new Partition()
                    {
                        Number = int.Parse(parts[0]),
                        Start = parts[1],
                        End = parts[2],
                        Size = parts[3],
                        FileSystem = parts[4],
                        Name = parts[5],
                        Flags = parts[6]
                    });
                }
            }
            PartitionsListView.ItemsSource = null;
            PartitionsListView.ItemsSource = partitions;
            TWRPModeNeededOverlay.Visibility = Visibility.Collapsed;
            MainPage.ToggleLoadingScreen(false);
        }
    }
}

/*
 Interactive shell example reference:
    ADBManager.Shell.SendCommand("parted /dev/block/sda");
    TextWriter writer = new StringWriter();
    while (ADBManager.Shell.Connected && !writer.ToString().Contains("(parted)"))
    {
        ADBManager.Shell.ReadAvailable(false, writer);
        writer.Flush();
        await Task.Delay(100);
    }
    Debug.WriteLine(writer.ToString());
    ADBManager.Shell.SendCommand("print");
    writer = new StringWriter();
    while (ADBManager.Shell.Connected && !writer.ToString().Contains("(parted)"))
    {
        ADBManager.Shell.ReadAvailable(false, writer);
        writer.Flush();
        await Task.Delay(100);
    }
    Debug.WriteLine(writer.ToString());
*/