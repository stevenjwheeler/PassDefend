using System;
using Windows.Storage;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.IO;
using Windows.UI.Xaml;
using Windows.UI.Xaml.Controls;

namespace PassProtect
{
    class ImportExportEngine
    {
        public static string exportPath { get; set; }

        public static async void ExportDB()
        {
            bool dialogNotCompleted = true;
            ExportDialog exportDialog = new ExportDialog();
            while (dialogNotCompleted == true)
            {
                await exportDialog.ShowAsync();
                if (exportDialog.Result == ExportDialogResult.ExportReady)
                {
                    dialogNotCompleted = false; //breaking loop because export ready
                    //complete the export
                    string dbPath = Path.Combine(ApplicationData.Current.LocalFolder.Path, "core");
                    string fileExport = Path.Combine(exportPath, "PassProtectExport"); //+ DateTime.Now.ToString("yyyy-MM-dd HH-mm-ss") + ".PASSPROTECT");
                    

                   File.Copy(dbPath, fileExport, true);
                }
                else if (exportDialog.Result == ExportDialogResult.ExportCancel)
                {
                    dialogNotCompleted = false;
                }
            }
        }
    }
}
