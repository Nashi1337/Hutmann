using System.Collections.Generic;
using TMPro;
using UnityEngine;
using UnityEngine.Rendering;
using UnityEngine.Rendering.Universal;
using UnityEngine.UI;

public class GraphicsSettingsController : MonoBehaviour
{
    [Header("UI")]
    [SerializeField] private TMP_Dropdown resolutionDropdown;
    [SerializeField] private Toggle fullscreenToggle;
    [SerializeField] private Slider brightnessSlider;
    [SerializeField] private TMP_Text brightnessValueLabel;

    [Header("Brightness")]
    [SerializeField] private Volume globalVolume;
    [SerializeField] private Image brightnessOverlay;
    [SerializeField] private float exposureMin = -2f;
    [SerializeField] private float exposureMax = 2f;

    private readonly List<Resolution> resolutionOptions = new List<Resolution>();
    private ColorAdjustments colorAdjustments;

    private void Start()
    {
        BuildResolutionDropdown();
        InitializeFullScreenToggle();
        InitializeBrightness();
    }

    private void BuildResolutionDropdown()
    {
        if (resolutionDropdown == null)
            return;

        resolutionDropdown.ClearOptions();
        resolutionOptions.Clear();

        Resolution[] allResolutions = Screen.resolutions;
        var labels = new List<string>();
        var seen = new HashSet<string>();

        for (int i = 0; i < allResolutions.Length; i++)
        {
            var resolution = allResolutions[i];
            string key = resolution.width + "x" + resolution.height;
            if (!seen.Add(key))
                continue;

            resolutionOptions.Add(resolution);
            labels.Add(key);
        }

        if (resolutionOptions.Count == 0)
        {
            var fallback = Screen.currentResolution;
            resolutionOptions.Add(fallback);
            labels.Add(fallback.width + "x" + fallback.height);
        }

        resolutionDropdown.AddOptions(labels);

        int selectedIndex = GetCurrentResolutionIndex();
        if (GameSettingsStore.TryGetResolution(out int savedWidth, out int savedHeight))
        {
            int savedIndex = FindResolutionIndex(savedWidth, savedHeight);
            if (savedIndex >= 0)
            {
                selectedIndex = savedIndex;
                ApplyResolution(savedIndex);
            }
        }

        resolutionDropdown.SetValueWithoutNotify(selectedIndex);
        resolutionDropdown.onValueChanged.AddListener(OnResolutionDropdownChanged);
    }

    private int GetCurrentResolutionIndex()
    {
        int currentWidth = Screen.width;
        int currentHeight = Screen.height;
        return Mathf.Max(FindResolutionIndex(currentWidth, currentHeight), 0);
    }

    private int FindResolutionIndex(int width, int height)
    {
        for (int i = 0; i < resolutionOptions.Count; i++)
        {
            if (resolutionOptions[i].width == width && resolutionOptions[i].height == height)
                return i;
        }

        return -1;
    }

    private void InitializeFullScreenToggle()
    {
        if (fullscreenToggle == null)
            return;

        bool value = Screen.fullScreen;
        if (GameSettingsStore.TryGetFullScreen(out bool savedValue))
        {
            value = savedValue;
            SetFullScreen(savedValue);
        }

        fullscreenToggle.SetIsOnWithoutNotify(value);
        fullscreenToggle.onValueChanged.AddListener(OnFullScreenChanged);
    }

    private void InitializeBrightness()
    {
        if (brightnessSlider == null)
            return;

        TryCacheColorAdjustments();

        float defaultBrightness = 0.5f;
        float value = GameSettingsStore.GetBrightness(defaultBrightness);
        brightnessSlider.SetValueWithoutNotify(value);
        ApplyBrightness(value);

        brightnessSlider.onValueChanged.AddListener(OnBrightnessChanged);
    }

    private void OnDestroy()
    {
        if (resolutionDropdown != null)
            resolutionDropdown.onValueChanged.RemoveListener(OnResolutionDropdownChanged);
        if (fullscreenToggle != null)
            fullscreenToggle.onValueChanged.RemoveListener(OnFullScreenChanged);
        if (brightnessSlider != null)
            brightnessSlider.onValueChanged.RemoveListener(OnBrightnessChanged);
    }

    private void OnResolutionDropdownChanged(int selectedIndex)
    {
        ApplyResolution(selectedIndex);
        GameSettingsStore.SaveAll();
    }

    private void ApplyResolution(int selectedIndex)
    {
        if (selectedIndex < 0 || selectedIndex >= resolutionOptions.Count)
            return;

        Resolution resolution = resolutionOptions[selectedIndex];
        Screen.SetResolution(resolution.width, resolution.height, Screen.fullScreenMode);
        GameSettingsStore.SaveResolution(resolution.width, resolution.height);
    }

    private void OnFullScreenChanged(bool isFullScreen)
    {
        SetFullScreen(isFullScreen);
        GameSettingsStore.SaveFullScreen(isFullScreen);
        GameSettingsStore.SaveAll();
    }

    private void SetFullScreen(bool isFullScreen)
    {
        Screen.fullScreenMode = isFullScreen ? FullScreenMode.FullScreenWindow : FullScreenMode.Windowed;
    }

    private void OnBrightnessChanged(float value)
    {
        ApplyBrightness(value);
        GameSettingsStore.SaveBrightness(value);
        GameSettingsStore.SaveAll();
    }

    private void ApplyBrightness(float sliderValue)
    {
        if (colorAdjustments != null)
        {
            float exposure = Mathf.Lerp(exposureMin, exposureMax, sliderValue);
            colorAdjustments.postExposure.Override(exposure);
        }

        if (brightnessOverlay != null)
        {
            Color overlayColor = brightnessOverlay.color;
            overlayColor.a = Mathf.Lerp(0.55f, 0f, sliderValue);
            brightnessOverlay.color = overlayColor;
        }

        if (brightnessValueLabel != null)
            brightnessValueLabel.text = Mathf.RoundToInt(sliderValue * 100f) + "%";
    }

    private void TryCacheColorAdjustments()
    {
        if (globalVolume == null || globalVolume.profile == null)
            return;

        globalVolume.profile.TryGet(out colorAdjustments);
    }
}

