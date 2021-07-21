using System;
using System.IO;
using Windows.Storage;

namespace PassProtect
{
    class ImportExportEngine
    {
        public static string exportPath { get; set; }

        public static async void ExportDB(string key)
        {
            bool dialogNotCompleted = true;
            ExportDialog exportDialog = new ExportDialog();
            while (dialogNotCompleted == true)
            {
                await exportDialog.ShowAsync();
                if (exportDialog.Result == ExportDialogResult.ExportReady)
                {
                    dialogNotCompleted = false; //breaking loop because export ready
                    //prepare the file path
                    string dbPath = Path.Combine(ApplicationData.Current.LocalFolder.Path, "core");
                    string fileExport = Path.Combine(exportPath, "PassProtectExport"); //+ DateTime.Now.ToString("yyyy-MM-dd HH-mm-ss") + ".PASSPROTECT");
                    //close the database for copying
                    DataAccess.CloseDB(MainPage.dbconnection);
                    //copy the database
                    //HERE
                    //reopen the database afterwards
                    MainPage.dbconnection = DataAccess.OpenDB(key);
                }
                else if (exportDialog.Result == ExportDialogResult.ExportCancel)
                {
                    dialogNotCompleted = false;
                }
            }
        }
    }
}
