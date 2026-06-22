using TMPro;
using UnityEngine;
using UnityEngine.UI;

public class MouseSensitivitySliderUI : MonoBehaviour
{
    [Header("UI")]
    public Slider sensitivitySlider;
    public TMP_Text sensitivityValueText;

    [Header("Display")]
    public string prefix = "Mouse Sensitivity: ";
    public bool showAsPercent = false;

    private void Awake()
    {
        if (sensitivitySlider == null)
        {
            sensitivitySlider = GetComponent<Slider>();
        }
    }

    private void Start()
    {
        SetupSlider();
        RefreshUI();
    }

    private void SetupSlider()
    {
        if (sensitivitySlider == null)
        {
            Debug.Log("MouseSensitivitySliderUI needs a Slider.");
            return;
        }

        sensitivitySlider.minValue = MouseSensitivitySettings.MinSensitivity;
        sensitivitySlider.maxValue = MouseSensitivitySettings.MaxSensitivity;
        sensitivitySlider.wholeNumbers = false;

        sensitivitySlider.value = MouseSensitivitySettings.CurrentSensitivity;

        sensitivitySlider.onValueChanged.RemoveListener(OnSensitivityChanged);
        sensitivitySlider.onValueChanged.AddListener(OnSensitivityChanged);
    }

    private void OnSensitivityChanged(float newValue)
    {
        MouseSensitivitySettings.SetSensitivity(newValue);
        RefreshUI();
    }

    private void RefreshUI()
    {
        if (sensitivitySlider == null)
        {
            return;
        }

        if (sensitivityValueText == null)
        {
            return;
        }

        float value = sensitivitySlider.value;

        if (showAsPercent)
        {
            sensitivityValueText.text = prefix + Mathf.RoundToInt(value * 100f) + "%";
        }
        else
        {
            sensitivityValueText.text = prefix + value.ToString("F2");
        }
    }

    public void ResetToDefault()
    {
        MouseSensitivitySettings.ResetSensitivity();

        if (sensitivitySlider != null)
        {
            sensitivitySlider.value = MouseSensitivitySettings.CurrentSensitivity;
        }

        RefreshUI();
    }
}