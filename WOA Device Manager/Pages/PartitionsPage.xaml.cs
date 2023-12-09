using Microsoft.UI.Xaml;
using Microsoft.UI.Xaml.Controls;
using Microsoft.UI.Xaml.Media.Animation;
using SAPTeam.AndroCtrl.Adb;
using SAPTeam.AndroCtrl.Adb.Receivers;
using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.IO;
using System.Text.RegularExpressions;
using System.Threading.Tasks;
using WOADeviceManager.Entities;
using WOADeviceManager.Helpers;
using WOADeviceManager.Managers;

namespace WOADeviceManager.Pages
{
    public sealed partial class PartitionsPage : Page
    {
        List<Partition> partitions = new List<Partition>();

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
            await ADBProcedures.PushParted();
            Debug.WriteLine(ADBManager.Shell.Interact("mv /sdcard/parted /sbin/parted"));
            await Task.Delay(200);
            Debug.WriteLine(ADBManager.Shell.Interact("chmod 755 /sbin/parted"));
            await Task.Delay(200);
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
            string[] lines = writer.ToString().Split(Environment.NewLine);
            int startingLine = -1;
            for (int i = 0; i < lines.Length; i++)
            {
                if (lines[i].Contains("Number") && lines[i].Contains("Start") && lines[i].Contains("End") && lines[i].Contains("Size") && lines[i].Contains("File system") && lines[i].Contains("Name") && lines[i].Contains("Flags"))
                {
                    startingLine = i;
                    break;
                }
            }
            for (int i = startingLine + 1; i < lines.Length; i++)
            {
                var parts = lines[i].Split("\t".ToCharArray());
                string pattern = @"^\s*(\d+)\s+(\S+)\s+(\S+)\s+(\S+)\s+(\S+)?\s*(\S+)?";

                Regex regex = new Regex(pattern);
                MatchCollection matches = regex.Matches(lines[i]);
                if (matches != null && matches.Count != 0 && matches[0].Groups[5] != null && !string.IsNullOrEmpty(matches[0].Groups[5].Value.Trim()))
                {
                    Debug.WriteLine("Partition found: " + matches[0].Groups[5].Value.Trim());
                    partitions.Add(new Partition()
                    {
                        Number = int.Parse(matches[0].Groups[1].Value.Trim()),
                        Start = matches[0].Groups[2].Value.Trim(),
                        End = matches[0].Groups[3].Value.Trim(),
                        Size = matches[0].Groups[4].Value.Trim(),
                        FileSystem = matches[0].Groups[5].Value.Trim(),
                        Name = matches[0].Groups[6].Value.Trim(),
                        Flags = matches[0].Groups[7].Value.Trim()
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
