using UnityEngine;

public static class GameSettingsStore
{
    private const string ResolutionWidthKey = "settings.resolution.width";
    private const string ResolutionHeightKey = "settings.resolution.height";
    private const string FullScreenKey = "settings.fullscreen";
    private const string BrightnessKey = "settings.brightness";

    public static bool TryGetResolution(out int width, out int height)
    {
        width = PlayerPrefs.GetInt(ResolutionWidthKey, -1);
        height = PlayerPrefs.GetInt(ResolutionHeightKey, -1);
        return width > 0 && height > 0;
    }

    public static void SaveResolution(int width, int height)
    {
        PlayerPrefs.SetInt(ResolutionWidthKey, width);
        PlayerPrefs.SetInt(ResolutionHeightKey, height);
    }

    public static bool TryGetFullScreen(out bool fullScreen)
    {
        if (!PlayerPrefs.HasKey(FullScreenKey))
        {
            fullScreen = Screen.fullScreen;
            return false;
        }

        fullScreen = PlayerPrefs.GetInt(FullScreenKey, Screen.fullScreen ? 1 : 0) == 1;
        return true;
    }

    public static void SaveFullScreen(bool fullScreen)
    {
        PlayerPrefs.SetInt(FullScreenKey, fullScreen ? 1 : 0);
    }

    public static float GetBrightness(float defaultValue)
    {
        return PlayerPrefs.GetFloat(BrightnessKey, defaultValue);
    }

    public static void SaveBrightness(float value)
    {
        PlayerPrefs.SetFloat(BrightnessKey, value);
    }

    public static void SaveAll()
    {
        PlayerPrefs.Save();
    }
}

