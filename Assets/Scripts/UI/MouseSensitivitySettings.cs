using UnityEngine;

public static class MouseSensitivitySettings
{
    private const string MouseSensitivityKey = "MouseSensitivity";

    public const float DefaultSensitivity = 1f;
    public const float MinSensitivity = 0.2f;
    public const float MaxSensitivity = 3f;

    public static float CurrentSensitivity
    {
        get
        {
            return PlayerPrefs.GetFloat(MouseSensitivityKey, DefaultSensitivity);
        }
    }

    public static void SetSensitivity(float newSensitivity)
    {
        float clampedSensitivity = Mathf.Clamp(
            newSensitivity,
            MinSensitivity,
            MaxSensitivity
        );

        PlayerPrefs.SetFloat(MouseSensitivityKey, clampedSensitivity);
        PlayerPrefs.Save();
    }

    public static void ResetSensitivity()
    {
        SetSensitivity(DefaultSensitivity);
    }
}