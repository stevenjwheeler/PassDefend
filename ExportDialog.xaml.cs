using System;
using Windows.UI.Xaml;
using Windows.UI.Xaml.Controls;

namespace PassProtect
{
    public enum ExportDialogResult
    {
        ExportReady,
        ExportCancel,
        Nothing
    }

    public sealed partial class ExportDialog : ContentDialog
    {
        public ExportDialogResult Result { get; private set; }
        public Windows.Storage.StorageFolder folder;
        public PasswordPrompt passCheck = new PasswordPrompt();

        public ExportDialog()
        {
            this.InitializeComponent();
        }

        //set declaration to 'nothing' when dialog opens
        void PasswordCreation_Opened(ContentDialog sender, ContentDialogOpenedEventArgs args)
        {
            this.Result = ExportDialogResult.Nothing;
        }

        private async void ContentDialog_PrimaryButtonClick(ContentDialog sender, ContentDialogButtonClickEventArgs args)
        {
            bool passOK = false;
            //check if boxes are satisfactory
            if (passwordBox.Password.Length == 0)
            {
                this.errorMessage.Visibility = Windows.UI.Xaml.Visibility.Visible;
                errorMessage.Text = "Please enter your master password.";
                this.Result = ExportDialogResult.Nothing;
            }
            else if (passwordBox.Password.Length > 0)
            {
                if (await passCheck.AsyncPasswordCheck(passwordBox.Password))
                {
                    //if password check confirmed password, send ok declaration
                    this.errorMessage.Visibility = Windows.UI.Xaml.Visibility.Visible;
                    passOK = true;
                    this.Result = ExportDialogResult.Nothing;
                }
                else
                {
                    //if password check denied password, send fail declaration
                    this.errorMessage.Visibility = Windows.UI.Xaml.Visibility.Visible;
                    errorMessage.Text = "Master password incorrect.";
                    this.Result = ExportDialogResult.Nothing;
                }
            }
            if (folder == null)
            {
                this.errorMessage.Visibility = Windows.UI.Xaml.Visibility.Visible;
                errorMessage.Text = "Please select a folder.";
                this.Result = ExportDialogResult.Nothing;
            }
            if (passwordBox.Password.Length == 0 && folder == null)
            {
                this.errorMessage.Visibility = Windows.UI.Xaml.Visibility.Visible;
                errorMessage.Text = "Please select a folder and enter your master password.";
                this.Result = ExportDialogResult.Nothing;
            }


            if (passOK == true && folder != null)
            {
                ImportExportEngine.exportPath = folder.Path;
                this.Result = ExportDialogResult.ExportReady;
            }
        }

        private void ContentDialog_SecondaryButtonClick(ContentDialog sender, ContentDialogButtonClickEventArgs args)
        {
            this.Result = ExportDialogResult.ExportCancel;
        }

        private async void folderSelectButton_Click(object sender, RoutedEventArgs e)
        {
            //select where to save the export to
            var folderPicker = new Windows.Storage.Pickers.FolderPicker();
            folderPicker.SuggestedStartLocation = Windows.Storage.Pickers.PickerLocationId.Desktop;
            folderPicker.FileTypeFilter.Add("*");
            folder = await folderPicker.PickSingleFolderAsync();
            if (folder != null)
            {
                // Application now has read/write access to all contents in the picked folder
                // (including other sub-folder contents)
                Windows.Storage.AccessCache.StorageApplicationPermissions.FutureAccessList.AddOrReplace("ExportFolder", folder);
                folderSelectButton.Content = "Picked folder: " + folder.Path;
            }
            else
            {
                folderSelectButton.Content = "Cannot save to that folder, please select again.";
            }
        }
    }
}
