using TMPro;
using UnityEngine;
using UnityEngine.UI;

public class SettingsPanel : MonoBehaviour
{
    [Header("Brightness Settings")]

    // Brightness slider.
    // 亮度滑条
    public Slider brightnessSlider;

    // Brightness value text.
    // 亮度数值文字
    public TMP_Text brightnessValueText;

    // Full screen black overlay used to darken the game.
    // 用来调暗画面的黑色遮罩
    public Image brightnessOverlayImage;

    [Header("Music Settings")]

    // Music volume slider.
    // 音乐音量滑条
    public Slider musicVolumeSlider;

    // Music volume value text.
    // 音量数值文字
    public TMP_Text musicVolumeValueText;

    // Background music AudioSource.
    // 背景音乐 AudioSource
    public AudioSource musicAudioSource;

    [Header("Default Values")]

    // 1 means normal brightness.
    // 1 表示正常亮度
    public float defaultBrightness = 1f;

    // Default music volume.
    // 默认音乐音量
    public float defaultMusicVolume = 0.8f;

    private const string BrightnessKey = "GameBrightness";
    private const string MusicVolumeKey = "MusicVolume";

    private void Start()
    {
        SetupSliders();
        LoadSettings();
    }

    private void SetupSliders()
    {
        if (brightnessSlider != null)
        {
            brightnessSlider.minValue = 0.3f;
            brightnessSlider.maxValue = 1f;
            brightnessSlider.wholeNumbers = false;

            brightnessSlider.onValueChanged.RemoveListener(SetBrightness);
            brightnessSlider.onValueChanged.AddListener(SetBrightness);
        }

        if (musicVolumeSlider != null)
        {
            musicVolumeSlider.minValue = 0f;
            musicVolumeSlider.maxValue = 1f;
            musicVolumeSlider.wholeNumbers = false;

            musicVolumeSlider.onValueChanged.RemoveListener(SetMusicVolume);
            musicVolumeSlider.onValueChanged.AddListener(SetMusicVolume);
        }
    }

    public void LoadSettings()
    {
        float brightness = PlayerPrefs.GetFloat(BrightnessKey, defaultBrightness);
        float musicVolume = PlayerPrefs.GetFloat(MusicVolumeKey, defaultMusicVolume);

        if (brightnessSlider != null)
        {
            brightnessSlider.value = brightness;
        }

        if (musicVolumeSlider != null)
        {
            musicVolumeSlider.value = musicVolume;
        }

        ApplyBrightness(brightness);
        ApplyMusicVolume(musicVolume);
    }

    public void SetBrightness(float value)
    {
        ApplyBrightness(value);

        PlayerPrefs.SetFloat(BrightnessKey, value);
        PlayerPrefs.Save();
    }

    public void SetMusicVolume(float value)
    {
        ApplyMusicVolume(value);

        PlayerPrefs.SetFloat(MusicVolumeKey, value);
        PlayerPrefs.Save();
    }

    private void ApplyBrightness(float value)
    {
        // value = 1 means no dark overlay.
        // value = 0.3 means darker screen.
        float overlayAlpha = 1f - value;

        if (brightnessOverlayImage != null)
        {
            Color color = brightnessOverlayImage.color;
            color.a = overlayAlpha;
            brightnessOverlayImage.color = color;
        }

        if (brightnessValueText != null)
        {
            brightnessValueText.text = Mathf.RoundToInt(value * 100f) + "%";
        }
    }

    private void ApplyMusicVolume(float value)
    {
        if (musicAudioSource != null)
        {
            musicAudioSource.volume = value;
        }
        else
        {
            // If no music AudioSource is assigned, control all audio.
            // 如果没拖背景音乐，就控制全局音量
            AudioListener.volume = value;
        }

        if (musicVolumeValueText != null)
        {
            musicVolumeValueText.text = Mathf.RoundToInt(value * 100f) + "%";
        }
    }

    public void ResetSettings()
    {
        if (brightnessSlider != null)
        {
            brightnessSlider.value = defaultBrightness;
        }

        if (musicVolumeSlider != null)
        {
            musicVolumeSlider.value = defaultMusicVolume;
        }

        SetBrightness(defaultBrightness);
        SetMusicVolume(defaultMusicVolume);
    }
}