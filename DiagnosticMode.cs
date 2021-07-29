using System;
using System.Linq;
using Windows.Networking.Connectivity;
using Windows.Storage;
using Windows.System.Profile;
using Windows.UI.Xaml.Controls;
using System.IO;

namespace PassProtect
{
    class DiagnosticMode
    {
        //byte converter
        static double convertBytesToMegabytes(long bytes)
        {
            return (bytes / 1024f) / 1024f;
        }

        //diagnostic mode displays information about the program and the system the program is running on for development and support purposes.
        public static async void openDiagnostics()
        {
            StorageFolder localFolder = ApplicationData.Current.LocalFolder;

            string hashPresent = "";
            string corePresent = "";
            string savedGenSettingsPresent = "";
            string activeColorScheme = "";

            var hostNames = NetworkInformation.GetHostNames();
            var localName = hostNames.FirstOrDefault(name => name.DisplayName.Contains(".local"));
            var computerName = localName.DisplayName.Replace(".local", "");

            string deviceFamilyVersion = AnalyticsInfo.VersionInfo.DeviceFamilyVersion;
            ulong version = ulong.Parse(deviceFamilyVersion);
            ulong major = (version & 0xFFFF000000000000L) >> 48;
            ulong minor = (version & 0x0000FFFF00000000L) >> 32;
            ulong build = (version & 0x00000000FFFF0000L) >> 16;
            ulong revision = (version & 0x000000000000FFFFL);
            var osVersion = $"{major}.{minor}.{build}.{revision}";

            if (await localFolder.TryGetItemAsync("hash") != null)
            {
                hashPresent = "Yes";
            }
            else
            {
                hashPresent = "No";
            }

            if (await localFolder.TryGetItemAsync("core") != null)
            {
                corePresent = "Yes";
            }
            else
            {
                corePresent = "No";
            }

            if (await localFolder.TryGetItemAsync("genSettings") != null)
            {
                savedGenSettingsPresent = "Stored";
            }
            else
            {
                savedGenSettingsPresent = "Not stored";
            }

            if (await localFolder.TryGetItemAsync("colorScheme") != null)
            {
                StorageFile colorschemefile = await localFolder.GetFileAsync("colorScheme");
                string fileContent = await FileIO.ReadTextAsync(colorschemefile);
                if (fileContent == "green")
                {
                    activeColorScheme = "Yes (Green)";
                }
                else if (fileContent == "red")
                {
                    activeColorScheme = "Yes (Red)";
                }
                else if (fileContent == "purple")
                {
                    activeColorScheme = "Yes (Purple)";
                }
                else if (fileContent == "black")
                {
                    activeColorScheme = "Yes (Black)";
                }
            }
            else
            {
                activeColorScheme = "Default";
            }

            string dbpath = Path.Combine(ApplicationData.Current.LocalFolder.Path, "core");
            long bytes = new System.IO.FileInfo(dbpath).Length;
            double dbfilesize = convertBytesToMegabytes(bytes);

            ContentDialog diagnosticsDialog = new ContentDialog
            {
                Title = "PassProtect v" + MainPage.GetAppVersion() + " diagnostics",
                Content = "Diagnostics generated: " + DateTime.Now + "\r\nUsing: DiagnosticEngine1 on PassProtect v" + MainPage.GetAppVersion() + "\r\n\r\nMachine name: " + computerName + "\r\nWindows version: " + osVersion + "\r\n\r\nHash present: " + hashPresent + "\r\nCore present: " + corePresent + "\r\nGeneration settings: " + savedGenSettingsPresent + "\r\nActive color scheme: " + activeColorScheme + "\r\n\r\nCurrent database size: " + dbfilesize.ToString("0.0000") + " MB (" + bytes + " bytes)",
                PrimaryButtonText = "Okay",
            };
            ContentDialogResult result = await diagnosticsDialog.ShowAsync();
        }
    }
}

