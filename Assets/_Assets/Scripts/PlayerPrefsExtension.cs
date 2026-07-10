using UnityEngine;
using System.Globalization;

public static class PlayerPrefsExtension
{
    public static void SetBool(string key, bool state)
    {
        // Converts the boolean to 1 if true, 0 if false
        PlayerPrefs.SetInt(key, state ? 1 : 0);
    }

    public static bool GetBool(string key)
    {
        // Returns true if the saved integer is 1
        return PlayerPrefs.GetInt(key) == 1;
    }

    // Optional: an overload to specify a default value
    public static bool GetBool(string key, bool defaultValue)
    {
        if (PlayerPrefs.HasKey(key))
        {
            return GetBool(key);
        }
        else
        {
            SetBool(key, defaultValue);
        }
        return defaultValue;
    }

    public static void SetDouble(string key, double value)
    {
        PlayerPrefs.SetString(key, value.ToString(CultureInfo.InvariantCulture));
    }

    public static double GetDouble(string key, double defaultValue = 0d)
    {
        if (!PlayerPrefs.HasKey(key)) return defaultValue;

        string value = PlayerPrefs.GetString(key);

        if (double.TryParse(value, NumberStyles.Any, CultureInfo.InvariantCulture, out double result))
        {
            return result;
        }

        return defaultValue;
    }
}