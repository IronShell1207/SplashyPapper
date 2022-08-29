using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Windows.Storage;
using SplashyPapper.Constants;

namespace SplashyPapper.Helpers;

public sealed class LocalSettingsHelper
{
    private static ApplicationDataContainer _localSettings = Windows.Storage.ApplicationData.Current.LocalSettings;

    public static void SaveValue(string key, object value)
    {
        _localSettings.Values[key] = value;
    }

    public static object TryGetValue(string key)
    {
        return _localSettings.Values[key];
    }

    public static bool IsCancelTipShowed =
        _localSettings.Values[SettingsName.CANCEL_TIP_SHOWED] != null && (bool)TryGetValue(SettingsName.CANCEL_TIP_SHOWED);
}