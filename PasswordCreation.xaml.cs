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
    public enum PasswordChangeResult
    {
        PassChangeOK,
        PassChangeFail,
        PassChangeCancel,
        Nothing
    }

    public sealed partial class PasswordCreation : ContentDialog
    {
        public PasswordChangeResult Result { get; private set; }
        private string prevpass;
        private bool passchangeactive;
        public StorageFolder localFolder = ApplicationData.Current.LocalFolder;

        public PasswordCreation()
        {
            this.InitializeComponent();
            this.Opened += PasswordCreation_Opened;
            prevpass = MainPage.userpass;
            passchangeactive = MainPage.passchangeactive;
        }

        private async void ContentDialog_PrimaryButtonClick(ContentDialog sender, ContentDialogButtonClickEventArgs args)
        {
            //if both boxes are not filled or do not match
            if (string.IsNullOrEmpty(changePasswordBoxMain.Password) || string.IsNullOrEmpty(changePasswordBoxConfirm.Password) || (changePasswordBoxMain.Password != changePasswordBoxConfirm.Password))
            {
                //reject password change
                args.Cancel = true;
                this.noMatchText.Visibility = Windows.UI.Xaml.Visibility.Visible;
            }
            else
            {
                ContentDialogButtonClickDeferral deferral = args.GetDeferral();
                if (await AsyncPasswordStore())
                {
                    //if password store confirmed, send ok declaration
                    MainPage.userpass = changePasswordBoxMain.Password;
                    if (passchangeactive == true && MainPage.onboarding == false)
                    {
                        DataAccess.changeDBPassword(MainPage.dbconnection, changePasswordBoxMain.Password);
                    }
                    this.Result = PasswordChangeResult.PassChangeOK;
                }
                else
                {
                    //if password store failed, send fail declaration
                    this.Result = PasswordChangeResult.PassChangeFail;
                }
                deferral.Complete();
            }
        }

        private void ContentDialog_CloseButtonClick(ContentDialog sender, ContentDialogButtonClickEventArgs args)
        {
            this.Result = PasswordChangeResult.PassChangeCancel;
        }

        //set declaration to 'nothing' when dialog opens
        void PasswordCreation_Opened(ContentDialog sender, ContentDialogOpenedEventArgs args)
        {
            this.Result = PasswordChangeResult.Nothing;
        }

        private async Task<bool> AsyncPasswordStore()
        {
            try
            {  
                //generate a 64 bit salt
                byte[] salt;
                new RNGCryptoServiceProvider().GetBytes(salt = new byte[64]);
                //hash with salt over 10000 iterations
                var pbkdf2 = new Rfc2898DeriveBytes(changePasswordBoxMain.Password, salt, 10000);
                //convert hash and salt into single string
                byte[] hash = pbkdf2.GetBytes(20);
                byte[] hashBytes = new byte[84];
                Array.Copy(salt, 0, hashBytes, 0, 64);
                Array.Copy(hash, 0, hashBytes, 64, 20);
                string passwordHash = Convert.ToBase64String(hashBytes);

                //write the string to file
                StorageFile accHash = await localFolder.CreateFileAsync("hash", CreationCollisionOption.OpenIfExists);
                await FileIO.WriteTextAsync(accHash, passwordHash);

                //return true to state completed
                return true;
            }
            catch
            {
                //return false if errored
                return false;
            }
        }
    }
}
