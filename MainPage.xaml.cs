using SQLite;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using Windows.ApplicationModel.Core;
using Windows.ApplicationModel.DataTransfer;
using Windows.Storage;
using Windows.UI.Xaml;
using Windows.UI.Xaml.Controls;

namespace PassProtect
{
    public partial class MainPage : Page
    {
        //prepare application files
        public StorageFolder localFolder = ApplicationData.Current.LocalFolder;

        //prepare basic variables
        public static string userpass { get; set; }
        public bool newaccount = false;
        public int selectedID = 0;
        public int activeWelcome = 0; //change to bool?? this is part of new onboarding so will be checked in future
        public static SQLiteConnection dbconnection { get; set; }

        //prepare storage for the accounts
        internal List<DataAccess.AccountList> AccountData { get; set; }

        public MainPage()
        {
            //initialize the window
            this.InitializeComponent();

            //Hide the account viewer window so that the first thing the user will see after login is the "choose an account" message
            this.AccountDetailScroller.Visibility = Windows.UI.Xaml.Visibility.Collapsed;

            //stylize the window to match the colour scheme of the application
            ColorScheme_CheckForScheme();

            //inform of page load completion
            Loaded += Page_Loaded;
        }

        //function that activates upon page load completion
        private void Page_Loaded(object sender, RoutedEventArgs e)
        {
            LoginSequence();
        }

        //function to handle the login system
        private async void LoginSequence()
        {
            if (await localFolder.TryGetItemAsync("hash") != null) //if login file exists
            {
                //display the login dialog
                DisplayLoginDialog();
            }
            else //if login file does not exist
            {
                /*Frame.Navigate(typeof(OnboardingPage)); //navigate to the new onboarding page */
                CreatePassword(); //comment this line out if using the new onboarding page
            }
        }

        //function to run the login dialogue, begin creation of database, etc
        private async void DisplayLoginDialog()
        {
            bool notLoggedIn = true;
            //show the login dialog...
            PasswordPrompt signInDialog = new PasswordPrompt();
            while (notLoggedIn == true)
            {
                await signInDialog.ShowAsync();

                if (signInDialog.Result == PasswordPromptResult.SignInOK)
                {
                    //...sign in was successful
                    notLoggedIn = false;

                    //begin decryption process
                    //create the credential database if necessary, otherwise continue execution
                    dbconnection = DataAccess.OpenDB(userpass);
                    DataAccess.InitializeDatabase(dbconnection);

                    //first load of database accounts into account bar
                    RefreshAccounts();

                    //then load the stored settings of the password generator, if applicable
                    PassGenerator.loadSettings(generateLowercaseOption, generateCapitalsOption, generateNumbersOption, generateSymbolsOption, generateLengthSlider);

                    //hide the login rectangle to show the main ui
                    //note, the login rectangle is NOT a security feature, it is simply to make the login dialog look nice. The data behind remains encrypted and unloaded until the password is confirmed.
                    fadeLoginBackground.Begin(); //begin the fade animation
                    fadeLoginBackground.Completed += (s, e) => //when the fade animation is completed...
                    {
                        loginRectangle.Width = 0; //...set the width and height of the now invisible box to 0 pixels so that it is out of the way and user can interact with the ui
                        loginRectangle.Height = 0;
                    };

                    //begin the login breach check
                    CheckForBreaches();
                }
                else if (signInDialog.Result == PasswordPromptResult.SignInCancel)
                {
                    //...sign in was cancelled by the user, exiting application
                    CoreApplication.Exit();
                }
            }
        }

        //function to refresh the accounts from database
        private void RefreshAccounts()
        {
            //if account bar is not empty, clear it
            if (AccountData != null)
            {
                AccountData.Clear();
            }
            //request account data from database
            AccountData = DataAccess.GetAccountData(dbconnection);
            AccountData.RemoveAt(0);
            //display account data in the account bar
            accountList.ItemsSource = AccountData;
        }

        //function to check all passwords against password breach databases
        private async void CheckForBreaches()
        {
            timeSinceBreachText.Text = "Checking for password breaches...";
            foreach (var account in AccountData)
            { //for each account in account data...
                bool passCheck = await BreachCheck.checkPassword(account.Password); //check the password against the API
                if (passCheck == true) //if the password is found in the breach list...
                {
                    ContentDialog deleteConfirmDialog = new ContentDialog
                    {
                        Title = "WARNING! Breach detected on your " + account.Name + " account.",
                        Content = "The password to your '" + account.Name + "' account appeared in an online breach as identified by HaveIBeenPwned. Please change the password on the service as soon as possible.",
                        PrimaryButtonText = "Okay"
                    };
                    ContentDialogResult result = await deleteConfirmDialog.ShowAsync();
                }
            }
            timeSinceBreachText.Text = "Last password breach check: " + DateTime.Now;
        }

        //function to complete the creation of the master password
        private async void CreatePassword()
        {
            bool dialogNotCompleted = true;
            //show the change password dialog...
            PasswordCreation createPasswordDialog = new PasswordCreation();
            while (dialogNotCompleted == true)
            {
                await createPasswordDialog.ShowAsync();

                if (createPasswordDialog.Result == PasswordCreationResult.PassCreateOK)
                {
                    dialogNotCompleted = false; //breaking loop because password change completed

                    DisplayLoginDialog(); //ask user for the password, for the first time
                }
                else if (createPasswordDialog.Result == PasswordCreationResult.PassCreateCancel)
                {
                    CoreApplication.Exit(); //exiting application because onboarding process has been cancelled
                }
            }
        }

        //function to complete the changing of the master password
        private async void ChangePassword()
        {
            bool dialogNotCompleted = true;
            //show the change password dialog...
            PasswordChange changePasswordDialog = new PasswordChange();
            while (dialogNotCompleted == true)
            {
                await changePasswordDialog.ShowAsync();

                if (changePasswordDialog.Result == PasswordChangeResult.PassChangeOK)
                {
                    dialogNotCompleted = false; //breaking loop because password change completed
                }
                else if (changePasswordDialog.Result == PasswordChangeResult.PassChangeCancel)
                {
                    dialogNotCompleted = false; //breaking loop because password change cancelled
                }
            }
        }

        //function to check for saved color scheme
        private async void ColorScheme_CheckForScheme()
        {
            if (await localFolder.TryGetItemAsync("colorScheme") != null) //if colorscheme file exists
            {
                //set scheme
                StorageFile colorschemefile = await localFolder.GetFileAsync("colorScheme");
                string fileContent = await FileIO.ReadTextAsync(colorschemefile);
                if (fileContent == "green")
                {
                    ColorSchemes.Green(AccountDetailWindow, AccountWindowSpacer, NoAccountWindow, OptionBar, StatusBar, SideBar, accountList, loginRectangle);
                }
                else if (fileContent == "red")
                {
                    ColorSchemes.Red(AccountDetailWindow, AccountWindowSpacer, NoAccountWindow, OptionBar, StatusBar, SideBar, accountList, loginRectangle);
                }
                else if (fileContent == "purple")
                {
                    ColorSchemes.Purple(AccountDetailWindow, AccountWindowSpacer, NoAccountWindow, OptionBar, StatusBar, SideBar, accountList, loginRectangle);
                }
                else if (fileContent == "black")
                {
                    ColorSchemes.Black(AccountDetailWindow, AccountWindowSpacer, NoAccountWindow, OptionBar, StatusBar, SideBar, accountList, loginRectangle);
                }
                else
                {
                    ColorSchemes.Green(AccountDetailWindow, AccountWindowSpacer, NoAccountWindow, OptionBar, StatusBar, SideBar, accountList, loginRectangle);
                }
            }
            else //if colorscheme file does not exist
            {
                ColorSchemes.Green(AccountDetailWindow, AccountWindowSpacer, NoAccountWindow, OptionBar, StatusBar, SideBar, accountList, loginRectangle);
            }
        }

        private async void resetProgram()
        {
            //The resetProgram function deletes all user data and returns the program back to the freshly installed state
            if (await localFolder.TryGetItemAsync("hash") != null) //if login file exists
            {
                StorageFile deleteTarget = await localFolder.GetFileAsync("hash");
                await deleteTarget.DeleteAsync(); //delete the hash
            }
            if (await localFolder.TryGetItemAsync("core") != null) //if core exists
            {
                DataAccess.CloseDB(dbconnection); //close core
                StorageFile deleteTarget = await localFolder.GetFileAsync("core");
                await deleteTarget.DeleteAsync(); //delete the core
            }
            if (await localFolder.TryGetItemAsync("colorScheme") != null) //if color personalisation exists
            {
                StorageFile deleteTarget = await localFolder.GetFileAsync("colorScheme");
                await deleteTarget.DeleteAsync(); //delete the color
            }
            if (await localFolder.TryGetItemAsync("genSettings") != null) //if password generation settings exist
            {
                StorageFile deleteTarget = await localFolder.GetFileAsync("genSettings");
                await deleteTarget.DeleteAsync(); //delete the settings
            }


            ContentDialog deleteCompleteDialog = new ContentDialog
            {
                Title = "Reset complete",
                Content = "PassProtect has been reset and will now restart.",
                PrimaryButtonText = "Okay"
            };
            ContentDialogResult result = await deleteCompleteDialog.ShowAsync();

            if (result == ContentDialogResult.Primary)
            {
                await CoreApplication.RequestRestartAsync(""); //reboot the application
            }
        }

        /* --- EVERYTHING PAST THIS LINE IS EVENT FUNCTIONS --- */

        //function to take searchbox content and filter out the account list
        private void SearchBox_TextChanged(object sender, TextChangedEventArgs e)
        {
            var cont = from s in AccountData where s.Name.Contains(SearchBox.Text, StringComparison.CurrentCultureIgnoreCase) select s;
            accountList.ItemsSource = cont;
        }

        //add account button
        private void addAccountButton_Click(object sender, Windows.UI.Xaml.RoutedEventArgs e)
        {
            //Deselect account list item if listed
            accountList.SelectedItem = null;
            newaccount = true;
            deleteEntryButton.IsEnabled = false;

            //clear out the account panel information if filled
            accountNameTextBox.Text = "";
            usernameTextBox.Text = "";
            emailTextBox.Text = "";
            passwordTextBox.Text = "";
            notesTextBox.Text = "";

            //bring up the account panel if not already up
            this.AccountDetailScroller.Visibility = Windows.UI.Xaml.Visibility.Visible;
        }

        //account selected
        private void accountList_SelectionChanged(object sender, SelectionChangedEventArgs e)
        {
            newaccount = false;
            saveButton.IsEnabled = false;
            revertButton.IsEnabled = false;
            deleteEntryButton.IsEnabled = true;
            if (accountList.SelectedIndex != -1)
            {
                //get the ID of the requested content
                object info = accountList.SelectedValue;
                Type type = info.GetType();
                IList<PropertyInfo> props = new List<PropertyInfo>(type.GetProperties());
                foreach (PropertyInfo prop in props)
                {
                    object propValue = prop.GetValue(info, null);

                    if (prop.Name == "ID")
                    {
                        selectedID = Int32.Parse((string)propValue);
                    }
                }

                //Fill the data
                foreach (var item in AccountData)
                {
                    if (Int32.Parse(item.ID) == selectedID)
                    {
                        accountNameHeader.Text = "Your " + item.Name + " account";
                        accountNameTextBox.Text = item.Name;
                        usernameTextBox.Text = item.Username;
                        emailTextBox.Text = item.Email;
                        passwordTextBox.Text = item.Password;
                        notesTextBox.Text = item.Notes;
                    }
                }
            }

            //bring up the account panel if not already up
            this.AccountDetailScroller.Visibility = Windows.UI.Xaml.Visibility.Visible;
        }

        //function to copy username to clipboard upon button press
        private void copyUsernameButton_Click(object sender, Windows.UI.Xaml.RoutedEventArgs e)
        {
            var dataPackage = new DataPackage();
            dataPackage.SetText(usernameTextBox.Text);
            Clipboard.SetContent(dataPackage);
        }

        //function to copy username to clipboard upon button press
        private void copyEmailButton_Click(object sender, Windows.UI.Xaml.RoutedEventArgs e)
        {
            var dataPackage = new DataPackage();
            dataPackage.SetText(emailTextBox.Text);
            Clipboard.SetContent(dataPackage);
        }

        //function to copy username to clipboard upon button press
        private void copyPasswordButton_Click(object sender, Windows.UI.Xaml.RoutedEventArgs e)
        {
            var dataPackage = new DataPackage();
            dataPackage.SetText(passwordTextBox.Text);
            Clipboard.SetContent(dataPackage);
        }

        //save the account button
        private void saveButton_Click(object sender, Windows.UI.Xaml.RoutedEventArgs e)
        {
            if (newaccount == true)
            {
                //add new account entry to database
                DataAccess.AddData(dbconnection, accountNameTextBox.Text, emailTextBox.Text, usernameTextBox.Text, passwordTextBox.Text, notesTextBox.Text);
                RefreshAccounts();
                saveButton.IsEnabled = false;
                revertButton.IsEnabled = false;
                accountList.SelectedIndex = accountList.Items.Count - 1;
            }
            else
            {
                //update the account entry in the database
                DataAccess.UpdateData(dbconnection, selectedID, accountNameTextBox.Text, emailTextBox.Text, usernameTextBox.Text, passwordTextBox.Text, notesTextBox.Text);
                RefreshAccounts();
                saveButton.IsEnabled = false;
                revertButton.IsEnabled = false;
            }
        }

        //change title of account window as textbox changes
        private void accountNameTextbox_TextChanged(object sender, TextChangedEventArgs e)
        {
            //if textbox is changed...
            if (accountNameTextBox.Text.Length == 0) //...and textbox is empty...
            {
                //...set text to default
                accountNameHeader.Text = "New account";
            }
            else //...or textbox is not empty...
            {
                //...set text to the textbox content
                accountNameHeader.Text = "Your " + accountNameTextBox.Text + " account";
            }

            if (accountNameTextBox.Text.Length > 0 && (usernameTextBox.Text.Length > 0 || emailTextBox.Text.Length > 0) && passwordTextBox.Text.Length > 0)
            {
                saveButton.IsEnabled = true;
                if (newaccount == false)
                {
                    revertButton.IsEnabled = true;
                }
            }
            else
            {
                saveButton.IsEnabled = false;
                revertButton.IsEnabled = false;
            }
        }

        //function to change the header text with the account name
        private void usernameTextBox_TextChanged(object sender, TextChangedEventArgs e)
        {
            if (accountNameTextBox.Text.Length > 0 && (usernameTextBox.Text.Length > 0 || emailTextBox.Text.Length > 0) && passwordTextBox.Text.Length > 0)
            {
                saveButton.IsEnabled = true;
                if (newaccount == false)
                {
                    revertButton.IsEnabled = true;
                }
            }
            else
            {
                saveButton.IsEnabled = false;
                revertButton.IsEnabled = false;
            }
        }

        //handling the allowance of the save button being used
        private void emailTextBox_TextChanged(object sender, TextChangedEventArgs e)
        {
            if (accountNameTextBox.Text.Length > 0 && (usernameTextBox.Text.Length > 0 || emailTextBox.Text.Length > 0) && passwordTextBox.Text.Length > 0)
            {
                saveButton.IsEnabled = true;
                if (newaccount == false)
                {
                    revertButton.IsEnabled = true;
                }
            }
            else
            {
                saveButton.IsEnabled = false;
                revertButton.IsEnabled = false;
            }
        }

        //handling the allowance of the save button being used
        private void notesTextBox_TextChanged(object sender, TextChangedEventArgs e)
        {
            if (accountNameTextBox.Text.Length > 0 && (usernameTextBox.Text.Length > 0 || emailTextBox.Text.Length > 0) && passwordTextBox.Text.Length > 0)
            {
                saveButton.IsEnabled = true;
                if (newaccount == false)
                {
                    revertButton.IsEnabled = true;
                }
            }
            else
            {
                saveButton.IsEnabled = false;
                revertButton.IsEnabled = false;
            }
        }

        //handling the allowance of the save button being used
        private void passwordTextBox_TextChanged(object sender, TextChangedEventArgs e)
        {
            if (accountNameTextBox.Text.Length > 0 && (usernameTextBox.Text.Length > 0 || emailTextBox.Text.Length > 0) && passwordTextBox.Text.Length > 0)
            {
                saveButton.IsEnabled = true;
                if (newaccount == false)
                {
                    revertButton.IsEnabled = true;
                }
            }
            else
            {
                saveButton.IsEnabled = false;
                revertButton.IsEnabled = false;
            }
        }

        //handling the reverting of details when clicked
        private void revertButton_Click(object sender, Windows.UI.Xaml.RoutedEventArgs e)
        {
            foreach (var item in AccountData)
            {
                if (Int32.Parse(item.ID) == selectedID)
                {
                    accountNameHeader.Text = "Your " + item.Name + " account";
                    accountNameTextBox.Text = item.Name;
                    usernameTextBox.Text = item.Username;
                    emailTextBox.Text = item.Email;
                    passwordTextBox.Text = item.Password;
                    notesTextBox.Text = item.Notes;
                }
            }
            saveButton.IsEnabled = false;
            revertButton.IsEnabled = false;
        }

        //handling of deleting entry when clicked
        private async void deleteEntryButton_Click(object sender, Windows.UI.Xaml.RoutedEventArgs e)
        {
            ContentDialog deleteConfirmDialog = new ContentDialog
            {
                Title = "Are you sure?",
                Content = "You are about to delete this account. \r\nThis cannot be undone!",
                PrimaryButtonText = "Delete",
                SecondaryButtonText = "Cancel"
            };
            ContentDialogResult result = await deleteConfirmDialog.ShowAsync();

            if (result == ContentDialogResult.Primary)
            {
                DataAccess.DeleteData(dbconnection, selectedID);
                RefreshAccounts();
                accountList.SelectedIndex = -1;
                this.AccountDetailScroller.Visibility = Windows.UI.Xaml.Visibility.Collapsed;
                deleteEntryButton.IsEnabled = false;
            }
        }

        //handling of refresh breach check button
        private void refreshBreachCheckButton_Click(object sender, Windows.UI.Xaml.RoutedEventArgs e)
        {
            CheckForBreaches();
        }

        private void MenuFlyoutItem_Click(object sender, Windows.UI.Xaml.RoutedEventArgs e)
        {
            //change master password
            ChangePassword();
        }

        //the following functions change the color scheme when selected in the menu
        private async void MenuFlyoutItem_Click_1(object sender, Windows.UI.Xaml.RoutedEventArgs e)
        {
            //change colour green
            ColorSchemes.Green(AccountDetailWindow, AccountWindowSpacer, NoAccountWindow, OptionBar, StatusBar, SideBar, accountList, loginRectangle);
            StorageFile colorschemefile = await localFolder.CreateFileAsync("colorScheme", CreationCollisionOption.OpenIfExists);
            await FileIO.WriteTextAsync(colorschemefile, "green");
        }

        private async void MenuFlyoutItem_Click_2(object sender, Windows.UI.Xaml.RoutedEventArgs e)
        {
            //change colour red
            ColorSchemes.Red(AccountDetailWindow, AccountWindowSpacer, NoAccountWindow, OptionBar, StatusBar, SideBar, accountList, loginRectangle);
            StorageFile colorschemefile = await localFolder.CreateFileAsync("colorScheme", CreationCollisionOption.OpenIfExists);
            await FileIO.WriteTextAsync(colorschemefile, "red");
        }

        private async void MenuFlyoutItem_Click_3(object sender, Windows.UI.Xaml.RoutedEventArgs e)
        {
            //change colour purple
            ColorSchemes.Purple(AccountDetailWindow, AccountWindowSpacer, NoAccountWindow, OptionBar, StatusBar, SideBar, accountList, loginRectangle);
            StorageFile colorschemefile = await localFolder.CreateFileAsync("colorScheme", CreationCollisionOption.OpenIfExists);
            await FileIO.WriteTextAsync(colorschemefile, "purple");
        }

        private async void MenuFlyoutItem_Click_4(object sender, Windows.UI.Xaml.RoutedEventArgs e)
        {
            //change colour black
            ColorSchemes.Black(AccountDetailWindow, AccountWindowSpacer, NoAccountWindow, OptionBar, StatusBar, SideBar, accountList, loginRectangle);
            StorageFile colorschemefile = await localFolder.CreateFileAsync("colorScheme", CreationCollisionOption.OpenIfExists);
            await FileIO.WriteTextAsync(colorschemefile, "black");
        }
        //end of color scheme changes

        private void MenuFlyoutItem_Click_5(object sender, Windows.UI.Xaml.RoutedEventArgs e)
        {
            //Export database button
            ImportExportEngine.ExportDB(userpass);
        }

        private async void MenuFlyoutItem_Click_6(object sender, RoutedEventArgs e)
        {
            //Reset button
            ContentDialog deleteConfirmDialog = new ContentDialog
            {
                Title = "Are you sure you want to reset?",
                Content = "You are about to completely reset PassProtect. \r\nThis will delete all information and restore PassProtect to being freshly installed. \r\n\nTHIS CANNOT BE REVERSED, so it is highly recommended to export the database before resetting. \r\n\nYou will be asked for your password to complete the reset.",
                PrimaryButtonText = "Reset",
                SecondaryButtonText = "Cancel"
            };
            ContentDialogResult result = await deleteConfirmDialog.ShowAsync();

            if (result == ContentDialogResult.Primary)
            {
                //check for user password before running reset
                bool notLoggedIn = true;
                PasswordPrompt signInDialog = new PasswordPrompt();
                while (notLoggedIn == true)
                {
                    await signInDialog.ShowAsync();

                    if (signInDialog.Result == PasswordPromptResult.SignInOK)
                    {
                        //...sign in was successful
                        notLoggedIn = false;

                        //reset the program
                        resetProgram();
                    }
                    else if (signInDialog.Result == PasswordPromptResult.SignInCancel)
                    {
                        //...sign in was cancelled by the user, do not reset the program
                        notLoggedIn = false;
                    }
                }
            }
        }

        //store the generated password when button is clicked
        private void storeGeneratedButton_Click(object sender, RoutedEventArgs e)
        {
            passwordTextBox.Text = generateResultBox.Text;
            PassGenerator.saveSettings(generateLowercaseOption, generateCapitalsOption, generateNumbersOption, generateSymbolsOption, generateLengthSlider);
            passwordGeneratorFlyout.Hide();
        }

        //call the function to regenerate the generated password when the regenerate button is pressed
        private void regenerateGeneratedButton_Click(object sender, RoutedEventArgs e)
        {
            PassGenerator.regenerateGeneratedPass(regenerateGeneratedButton, storeGeneratedButton, generateLowercaseOption, generateCapitalsOption, generateNumbersOption, generateSymbolsOption, generateLengthSlider, generateResultBox);
        }

        //call the function to generate a password when the generator form is opened
        private void passwordGeneratorFlyout_Opened(object sender, object e)
        {
            PassGenerator.regenerateGeneratedPass(regenerateGeneratedButton, storeGeneratedButton, generateLowercaseOption, generateCapitalsOption, generateNumbersOption, generateSymbolsOption, generateLengthSlider, generateResultBox);
        }

        //regenerate password when option changed
        private void generateCapitalsOption_Click(object sender, RoutedEventArgs e)
        {
            PassGenerator.regenerateGeneratedPass(regenerateGeneratedButton, storeGeneratedButton, generateLowercaseOption, generateCapitalsOption, generateNumbersOption, generateSymbolsOption, generateLengthSlider, generateResultBox);
        }

        //regenerate password when option changed
        private void generateLowercaseOption_Click(object sender, RoutedEventArgs e)
        {
            PassGenerator.regenerateGeneratedPass(regenerateGeneratedButton, storeGeneratedButton, generateLowercaseOption, generateCapitalsOption, generateNumbersOption, generateSymbolsOption, generateLengthSlider, generateResultBox);
        }

        //regenerate password when option changed
        private void generateNumbersOption_Click(object sender, RoutedEventArgs e)
        {
            PassGenerator.regenerateGeneratedPass(regenerateGeneratedButton, storeGeneratedButton, generateLowercaseOption, generateCapitalsOption, generateNumbersOption, generateSymbolsOption, generateLengthSlider, generateResultBox);
        }

        //regenerate password when option changed
        private void generateSymbolsOption_Click(object sender, RoutedEventArgs e)
        {
            PassGenerator.regenerateGeneratedPass(regenerateGeneratedButton, storeGeneratedButton, generateLowercaseOption, generateCapitalsOption, generateNumbersOption, generateSymbolsOption, generateLengthSlider, generateResultBox);
        }

        //regenerate password when option changed
        private void generateLengthSlider_ValueChanged(object sender, Windows.UI.Xaml.Controls.Primitives.RangeBaseValueChangedEventArgs e)
        {
            if (passwordGeneratorFlyout.IsOpen)
            {
                PassGenerator.regenerateGeneratedPass(regenerateGeneratedButton, storeGeneratedButton, generateLowercaseOption, generateCapitalsOption, generateNumbersOption, generateSymbolsOption, generateLengthSlider, generateResultBox);
            }
        }
    }
}
