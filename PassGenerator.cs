using System;
using System.Linq;
using Windows.Storage;
using System.Security.Cryptography;
using Windows.UI.Xaml.Controls;

namespace PassDefend
{
    class PassGenerator
    {
        //function to prepare to generate passwords
        public static void regenerateGeneratedPass(Button regenerateGeneratedButton, Button storeGeneratedButton, CheckBox generateLowercaseOption, CheckBox generateCapitalsOption, CheckBox generateNumbersOption, CheckBox generateSymbolsOption, Slider generateLengthSlider, TextBox generateResultBox)
        {
            //set valid characters to 0
            string validCharacters = "";

            //list available characters
            const string lowercases = "abcdefghijklmnopqrstuvwxyz";
            const string uppercases = "ABCDEFGHIJKLMNOPQRSTUVWXYZ";
            const string numbers = "0123456789";
            const string symbols = "@!#%$*&^?";

            //determine the valid characters from the selected options
            if ((bool)generateLowercaseOption.IsChecked == true)
            {
                validCharacters += lowercases;
            }

            if ((bool)generateCapitalsOption.IsChecked == true)
            {
                validCharacters += uppercases;
            }

            if ((bool)generateNumbersOption.IsChecked == true)
            {
                validCharacters += numbers;
            }

            if ((bool)generateSymbolsOption.IsChecked == true)
            {
                validCharacters += symbols;
            }

            //if no options are selected...
            if ((bool)generateSymbolsOption.IsChecked == false & (bool)generateNumbersOption.IsChecked == false & (bool)generateCapitalsOption.IsChecked == false & (bool)generateLowercaseOption.IsChecked == false)
            {
                //cancel the regeneration and lock the buttons
                generateResultBox.Text = "Select some options";
                regenerateGeneratedButton.IsEnabled = false;
                storeGeneratedButton.IsEnabled = false;
                return;
            }
            else //else if options are selected...
            {
                //unlock the buttons
                regenerateGeneratedButton.IsEnabled = true;
                storeGeneratedButton.IsEnabled = true;
            }

            //generate the string
            string generatedPassword = generatePassword(validCharacters, generateLengthSlider);

            //check if the string contains a symbol if symbols are required, and regenerate if not
            if ((bool)generateSymbolsOption.IsChecked == true)
            {
                while (!generatedPassword.Any(c => symbols.Contains(c)))
                {
                    generatedPassword = generatePassword(validCharacters, generateLengthSlider);
                }
            }

            //check if the string contains a number if numbers are required, and regenerate if not
            if ((bool)generateNumbersOption.IsChecked == true)
            {
                while (!generatedPassword.Any(c => numbers.Contains(c)))
                {
                    generatedPassword = generatePassword(validCharacters, generateLengthSlider);
                }
            }

            //check if the string contains a capital if capitals are required, and regenerate if not
            if ((bool)generateCapitalsOption.IsChecked == true)
            {
                while (!generatedPassword.Any(c => uppercases.Contains(c)))
                {
                    generatedPassword = generatePassword(validCharacters, generateLengthSlider);
                }
            }

            //check if the string contains a lowercase if lowercases are required, and regenerate if not
            if ((bool)generateLowercaseOption.IsChecked == true)
            {
                while (!generatedPassword.Any(c => lowercases.Contains(c)))
                {
                    generatedPassword = generatePassword(validCharacters, generateLengthSlider);
                }
            }

            //send the generated string to the results box
            generateResultBox.Text = generatedPassword;
        }

        //function to generate a password using the prep work from the previous function
        private static string generatePassword(string validCharacters, Slider generateLengthSlider)
        {
            string generatedPass = "";
            using (RNGCryptoServiceProvider provider = new RNGCryptoServiceProvider())
            {
                while (generatedPass.Length != generateLengthSlider.Value)
                {
                    {
                        byte[] oneByte = new byte[1];
                        provider.GetBytes(oneByte);
                        char character = (char)oneByte[0];
                        if (validCharacters.Contains(character))
                        {
                            generatedPass += character;
                        }
                    }
                }
            }
            return generatedPass;
        }

        public static async void saveSettings(CheckBox generateLowercaseOption, CheckBox generateCapitalsOption, CheckBox generateNumbersOption, CheckBox generateSymbolsOption, Slider generateLengthSlider)
        {
            //prep storage
            StorageFolder localFolder = ApplicationData.Current.LocalFolder;
            StorageFile genSettings = await localFolder.CreateFileAsync("genSettings", CreationCollisionOption.OpenIfExists);

            //prepare to write the settings line to file for future use
            /*string format: each digit either 1 or 0, 1 being on, 0 being off, except for length. Example:
             * 1 0 1 0 16
             * ^ ^ ^ ^ ^
             * | | | | |
             * | | | | length is 16
             * | | | symbols are off
             * | | numbers are on
             * | lowercase is off
             * uppercase is on
            */
            string settings = "";
            //this allows the string to be written to file, then read back when the program is reopened to keep the users choices.

            if ((bool)generateLowercaseOption.IsChecked == true)
            {
                settings += "1 ";
            }
            else
            {
                settings += "0 ";
            }

            if ((bool)generateCapitalsOption.IsChecked == true)
            {
                settings += "1 ";
            }
            else
            {
                settings += "0 ";
            }

            if ((bool)generateNumbersOption.IsChecked == true)
            {
                settings += "1 ";
            }
            else
            {
                settings += "0 ";
            }

            if ((bool)generateSymbolsOption.IsChecked == true)
            {
                settings += "1 ";
            }
            else
            {
                settings += "0 ";
            }

            settings += generateLengthSlider.Value;
            
            //save the generation settings for future sessions
            await FileIO.WriteTextAsync(genSettings, settings);
        }

        public static async void loadSettings(CheckBox generateLowercaseOption, CheckBox generateCapitalsOption, CheckBox generateNumbersOption, CheckBox generateSymbolsOption, Slider generateLengthSlider)
        {
            //prep storage
            StorageFolder localFolder = ApplicationData.Current.LocalFolder;

            if (await localFolder.TryGetItemAsync("genSettings") != null) //if generator settings exist
            {
                //read file
                StorageFile settingsfile = await localFolder.GetFileAsync("genSettings");
                string fileContent = await FileIO.ReadTextAsync(settingsfile);

                //read each number and act accordingly
                //order: lowercase, capitals, numbers, symbols, length
                string[] settingentries = fileContent.Split(' ');
                if (settingentries[0] == "0")
                {
                    generateLowercaseOption.IsChecked = false;
                }
                if (settingentries[1] == "0")
                {
                    generateCapitalsOption.IsChecked = false;
                }
                if (settingentries[2] == "0")
                {
                    generateNumbersOption.IsChecked = false;
                }
                if (settingentries[3] == "0")
                {
                    generateSymbolsOption.IsChecked = false;
                }
                generateLengthSlider.Value = Double.Parse(settingentries[4]);
            }
        }
    }
}
