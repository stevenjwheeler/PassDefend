using System;
using Windows.UI.Xaml.Controls;
using System.Threading.Tasks;
using System.Security.Cryptography;
using Windows.Storage;
using Windows.UI.Xaml;
using Windows.System;
using Windows.UI.Core;

namespace PassProtect
{
    public enum PasswordPromptResult
    {
        SignInOK,
        SignInFail,
        SignInCancel,
        Nothing
    }

    public sealed partial class PasswordPrompt : ContentDialog
    {
        public PasswordPromptResult Result { get; private set; }
        public StorageFolder localFolder = ApplicationData.Current.LocalFolder;

        public PasswordPrompt()
        {
            //initialize the password prompt
            this.InitializeComponent();
            this.Opened += PasswordPrompt_Opened; 
            masterPasswordBox.GotFocus += new RoutedEventHandler(passwordBox_focused);
        }

        private void passwordBox_focused(object sender, RoutedEventArgs e)
        {
            if (Window.Current.CoreWindow.GetKeyState(VirtualKey.CapitalLock) == CoreVirtualKeyStates.Down)
            {
                capsWarningText.Visibility = Visibility.Visible;
            }
            else
            {
                capsWarningText.Visibility = Visibility.Collapsed;
            }
        }

        //upon submission...
        private async void ContentDialog_PrimaryButtonClick(ContentDialog sender, ContentDialogButtonClickEventArgs args)
        {
            //...check if box is empty and ignore the click if so
            if (string.IsNullOrEmpty(masterPasswordBox.Password))
            {
                args.Cancel = true;
            }

            //...send the request through to the password check phase, then respond depending on the response
            ContentDialogButtonClickDeferral deferral = args.GetDeferral();
            if (await AsyncPasswordCheck(masterPasswordBox.Password))
            {
                //if password check confirmed password, send ok declaration
                this.Result = PasswordPromptResult.SignInOK;
                MainPage.userpass = masterPasswordBox.Password;
            }
            else
            {
                //if password check denied password, send fail declaration
                this.Result = PasswordPromptResult.SignInFail;
            }
            deferral.Complete();
        }

        //declare cancellation if cancel button or esc is pressed
        private void ContentDialog_CloseButtonClick(ContentDialog sender, ContentDialogButtonClickEventArgs args)
        {
            this.Result = PasswordPromptResult.SignInCancel;
        }

        //set declaration to 'nothing' when dialog opens
        void PasswordPrompt_Opened(ContentDialog sender, ContentDialogOpenedEventArgs args)
        {
            this.Result = PasswordPromptResult.Nothing;
        }

        //check the password and respond with true if correct or false if incorrect
        public async Task<bool> AsyncPasswordCheck(string password)
        {
            try
            {
                //read in hash info from storage
                StorageFile accHash = await localFolder.GetFileAsync("hash");
                string fileContent = await FileIO.ReadTextAsync(accHash);
                byte[] hashBytes = Convert.FromBase64String(fileContent);

                //extact the salt from the string
                byte[] salt = new byte[64];
                Array.Copy(hashBytes, 0, salt, 0, 64);

                //hash the inputted password with the salt
                var pbkdf2 = new Rfc2898DeriveBytes(password, salt, 10000);
                byte[] hash = pbkdf2.GetBytes(20);

                int approved = 1;
                for (int i = 0; i < 20; i++)
                {
                    if (hashBytes[i+64] != hash[i]) {
                        approved = 0;
                    }
                }
                if (approved == 1)
                {
                    return true;
                }
                else
                {
                    return false;
                }
            }
            catch
            {
                return false;
            }
        }
    }
}
