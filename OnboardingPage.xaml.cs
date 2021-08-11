using System;
using Windows.ApplicationModel.Core;
using Windows.UI.Xaml;
using Windows.UI.Xaml.Controls;

// The Blank Page item template is documented at https://go.microsoft.com/fwlink/?LinkId=234238

namespace PassDefend
{
    public sealed partial class OnboardingPage : Page
    {
        public OnboardingPage()
        {
            this.InitializeComponent();
            versionText.Text = "Version " + MainPage.GetAppVersion();
        }

        private async void getStartedButton_Click(object sender, RoutedEventArgs e)
        {
            bool dialogNotCompleted = true;
            //show the change password dialog...
            PasswordCreation createPasswordDialog = new PasswordCreation();
            while (dialogNotCompleted == true)
            {
                welcomeText.Visibility = Visibility.Collapsed;
                versionText.Visibility = Visibility.Collapsed;
                programDescriptionText.Visibility = Visibility.Collapsed;
                getStartedButton.Visibility = Visibility.Collapsed;
                await createPasswordDialog.ShowAsync();

                if (createPasswordDialog.Result == PasswordCreationResult.PassCreateOK)
                {
                    dialogNotCompleted = false; //breaking loop because password change completed
                    MainPage.welcomeActive = false;
                    Frame.GoBack();
                }
                else if (createPasswordDialog.Result == PasswordCreationResult.PassCreateCancel)
                {
                    CoreApplication.Exit(); //exiting application because onboarding process has been cancelled
                }
            }
        }
    }
}
