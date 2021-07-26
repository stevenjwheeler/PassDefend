using System;
using Windows.ApplicationModel.Core;
using Windows.UI;
using Windows.UI.ViewManagement;
using Windows.UI.Xaml.Controls;
using Windows.UI.Xaml.Media;
using Windows.UI.Xaml.Shapes;

namespace PassProtect
{
    class ColorSchemes
    {
        public static SolidColorBrush GetSolidColorBrush(string hex)
        {
            hex = hex.Replace("#", string.Empty);
            byte a = (byte)(Convert.ToUInt32(hex.Substring(0, 2), 16));
            byte r = (byte)(Convert.ToUInt32(hex.Substring(2, 2), 16));
            byte g = (byte)(Convert.ToUInt32(hex.Substring(4, 2), 16));
            byte b = (byte)(Convert.ToUInt32(hex.Substring(6, 2), 16));
            SolidColorBrush myBrush = new SolidColorBrush(Windows.UI.Color.FromArgb(a, r, g, b));
            return myBrush;
        }

        public static void ModifyTitleBar(string colorValue, CoreApplicationViewTitleBar coreTitleBar)
        {
            //set colour for title bar
            var color = GetSolidColorBrush(colorValue).Color;
            //customise the title bar
            coreTitleBar.ExtendViewIntoTitleBar = true;

            //customise the exit, minimize and maximize buttons
            ApplicationViewTitleBar titleBar = ApplicationView.GetForCurrentView().TitleBar;
            titleBar.BackgroundColor = color;
            titleBar.ButtonBackgroundColor = color;
            titleBar.ForegroundColor = Colors.White;
            titleBar.ButtonForegroundColor = Colors.White;
            titleBar.ButtonInactiveBackgroundColor = color;
            titleBar.ButtonInactiveForegroundColor = Colors.White;
        }

        public static void Green(Grid AccountDetailWindow, Grid AccountWindowSpacer, Grid NoAccountWindow, Grid OptionBar, Grid StatusBar, Grid SideBar, ListView accountList, Rectangle loginRectangle)
        {
            AccountDetailWindow.Background = GetSolidColorBrush("FF165D43");
            AccountWindowSpacer.Background = GetSolidColorBrush("FF165D43");
            NoAccountWindow.Background = GetSolidColorBrush("FF165D43");
            OptionBar.Background = GetSolidColorBrush("FF19664A");
            StatusBar.Background = GetSolidColorBrush("FF1E7957");
            SideBar.Background = GetSolidColorBrush("FF26956C");
            accountList.Background = GetSolidColorBrush("FF739E8E");
            loginRectangle.Fill = GetSolidColorBrush("FF165D43");

            CoreApplicationViewTitleBar coreTitleBar = CoreApplication.GetCurrentView().TitleBar;
            ModifyTitleBar("FF165D43", coreTitleBar);
        }

        public static void Red(Grid AccountDetailWindow, Grid AccountWindowSpacer, Grid NoAccountWindow, Grid OptionBar, Grid StatusBar, Grid SideBar, ListView accountList, Rectangle loginRectangle)
        {
            AccountDetailWindow.Background = GetSolidColorBrush("FF5D1616");
            AccountWindowSpacer.Background = GetSolidColorBrush("FF5D1616");
            NoAccountWindow.Background = GetSolidColorBrush("FF5D1616");
            OptionBar.Background = GetSolidColorBrush("FF802020");
            StatusBar.Background = GetSolidColorBrush("FF952626");
            SideBar.Background = GetSolidColorBrush("FF952626");
            accountList.Background = GetSolidColorBrush("FF9E7373");
            loginRectangle.Fill = GetSolidColorBrush("FF5D1616");
            CoreApplicationViewTitleBar coreTitleBar = CoreApplication.GetCurrentView().TitleBar;
            ModifyTitleBar("FF5D1616", coreTitleBar);
        }

        public static void Purple(Grid AccountDetailWindow, Grid AccountWindowSpacer, Grid NoAccountWindow, Grid OptionBar, Grid StatusBar, Grid SideBar, ListView accountList, Rectangle loginRectangle)
        {
            AccountDetailWindow.Background = GetSolidColorBrush("FF40165D");
            AccountWindowSpacer.Background = GetSolidColorBrush("FF40165D");
            NoAccountWindow.Background = GetSolidColorBrush("FF40165D");
            OptionBar.Background = GetSolidColorBrush("FF4C2080");
            StatusBar.Background = GetSolidColorBrush("FF6D2695");
            SideBar.Background = GetSolidColorBrush("FF6D2695");
            accountList.Background = GetSolidColorBrush("FF81739E");
            loginRectangle.Fill = GetSolidColorBrush("FF40165D");
            CoreApplicationViewTitleBar coreTitleBar = CoreApplication.GetCurrentView().TitleBar;
            ModifyTitleBar("FF40165D", coreTitleBar);
        }

        public static void Black(Grid AccountDetailWindow, Grid AccountWindowSpacer, Grid NoAccountWindow, Grid OptionBar, Grid StatusBar, Grid SideBar, ListView accountList, Rectangle loginRectangle)
        {
            AccountDetailWindow.Background = GetSolidColorBrush("FF1B1B1B");
            AccountWindowSpacer.Background = GetSolidColorBrush("FF1B1B1B");
            NoAccountWindow.Background = GetSolidColorBrush("FF1B1B1B");
            OptionBar.Background = GetSolidColorBrush("FF171717");
            StatusBar.Background = GetSolidColorBrush("FF0F0F0F");
            SideBar.Background = GetSolidColorBrush("FF0F0F0F");
            accountList.Background = GetSolidColorBrush("FF2E2E2E");
            loginRectangle.Fill = GetSolidColorBrush("FF1B1B1B");
            CoreApplicationViewTitleBar coreTitleBar = CoreApplication.GetCurrentView().TitleBar;
            ModifyTitleBar("FF1B1B1B", coreTitleBar);
        }
    }
}
