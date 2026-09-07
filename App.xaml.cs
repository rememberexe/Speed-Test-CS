using System;
using System.Windows;

namespace SpeedTestCS
{
    public partial class App : Application
    {
        public static void ChangeTheme(bool isDark)
        {
            var appResources = Current.Resources.MergedDictionaries;
            appResources[0].Source = new Uri(isDark ? "Dictionaries/Theme.Dark.xaml" : "Dictionaries/Theme.Light.xaml", UriKind.Relative);
        }

        public static void ChangeLanguage(bool isEnglish)
        {
            var appResources = Current.Resources.MergedDictionaries;
            appResources[1].Source = new Uri(isEnglish ? "Dictionaries/StringResources.en.xaml" : "Dictionaries/StringResources.tr.xaml", UriKind.Relative);
        }
    }
}