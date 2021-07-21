using System;
using System.Linq;
using System.Security.Cryptography;
using Windows.UI.Xaml.Controls;

namespace PassProtect
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
                validCharacters = validCharacters + lowercases;
            }

            if ((bool)generateCapitalsOption.IsChecked == true)
            {
                validCharacters = validCharacters + uppercases;
            }

            if ((bool)generateNumbersOption.IsChecked == true)
            {
                validCharacters = validCharacters + numbers;
            }

            if ((bool)generateSymbolsOption.IsChecked == true)
            {
                validCharacters = validCharacters + symbols;
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
    }
}
