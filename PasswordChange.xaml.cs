using System;
using System.Security.Cryptography;
using System.Threading.Tasks;
using Windows.Storage;
using Windows.UI.Xaml.Controls;

namespace PassProtect
{
    public enum PasswordChangeResult
    {
        PassChangeOK,
        PassChangeFail,
        PassChangeCancel,
        Nothing
    }

    public sealed partial class PasswordChange : ContentDialog
    {
        public PasswordChangeResult Result { get; private set; }
        private string prevpass;
        public StorageFolder localFolder = ApplicationData.Current.LocalFolder;

        public PasswordChange()
        {
            this.InitializeComponent();
            this.Opened += PasswordChange_Opened;
            prevpass = MainPage.userpass;
        }

        private async void ContentDialog_PrimaryButtonClick(ContentDialog sender, ContentDialogButtonClickEventArgs args)
        {
            //if both boxes are not filled, do not match, or old pass doesnt match
            if (changePasswordBoxOld.Password != prevpass)
            {
                //reject password change due to incorrect old pass
                this.noMatchText.Visibility = Windows.UI.Xaml.Visibility.Collapsed;
                this.noOldMatchText.Visibility = Windows.UI.Xaml.Visibility.Visible;
                args.Cancel = true;
            }
            else if (string.IsNullOrEmpty(changePasswordBoxMain.Password) || string.IsNullOrEmpty(changePasswordBoxConfirm.Password) || (changePasswordBoxMain.Password != changePasswordBoxConfirm.Password))
            {
                //reject password change due to new pass mismatch
                this.noOldMatchText.Visibility = Windows.UI.Xaml.Visibility.Collapsed;
                this.noMatchText.Visibility = Windows.UI.Xaml.Visibility.Visible;
                args.Cancel = true;
            }
            else
            {
                ContentDialogButtonClickDeferral deferral = args.GetDeferral();
                if (await AsyncPasswordStore())
                {
                    //store password
                    DataAccess.changeDBPassword(MainPage.dbconnection, changePasswordBoxMain.Password);
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
        void PasswordChange_Opened(ContentDialog sender, ContentDialogOpenedEventArgs args)
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
