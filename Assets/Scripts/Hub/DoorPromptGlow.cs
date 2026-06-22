using UnityEngine;

public class DoorPromptGlow : MonoBehaviour
{
    [Header("Blink Settings")]

    // How fast the glow blinks.
    // 闪烁速度
    public float blinkSpeed = 3f;

    // Minimum alpha / light strength.
    // 最暗的时候
    public float minIntensity = 0.2f;

    // Maximum alpha / light strength.
    // 最亮的时候
    public float maxIntensity = 1.5f;

    [Header("Object Animation")]

    // If true, this object rotates.
    // 如果为 true，物体会旋转
    public bool rotateObject = true;

    // Rotation speed.
    // 旋转速度
    public float rotateSpeed = 60f;

    // If true, this object floats up and down.
    // 如果为 true，物体会上下浮动
    public bool floatObject = true;

    // Floating height.
    // 上下浮动高度
    public float floatHeight = 0.15f;

    // Floating speed.
    // 上下浮动速度
    public float floatSpeed = 2f;

    [Header("References")]

    // Optional light component.
    // 可选的灯光组件
    public Light glowLight;

    // Optional renderer with emission material.
    // 可选的发光材质 Renderer
    public Renderer glowRenderer;

    // Emission color.
    // 发光颜色
    public Color glowColor = Color.cyan;

    private Vector3 startPosition;
    private Material glowMaterial;

    private void Start()
    {
        startPosition = transform.localPosition;

        if (glowRenderer != null)
        {
            glowMaterial = glowRenderer.material;
            glowMaterial.EnableKeyword("_EMISSION");
        }
    }

    private void Update()
    {
        UpdateBlink();
        UpdateRotation();
        UpdateFloating();
    }

    private void UpdateBlink()
    {
        float blinkValue = Mathf.Sin(Time.time * blinkSpeed) * 0.5f + 0.5f;
        float currentIntensity = Mathf.Lerp(minIntensity, maxIntensity, blinkValue);

        if (glowLight != null)
        {
            glowLight.intensity = currentIntensity;
        }

        if (glowMaterial != null)
        {
            Color emissionColor = glowColor * currentIntensity;
            glowMaterial.SetColor("_EmissionColor", emissionColor);
        }
    }

    private void UpdateRotation()
    {
        if (!rotateObject)
        {
            return;
        }

        transform.Rotate(Vector3.up, rotateSpeed * Time.deltaTime);
    }

    private void UpdateFloating()
    {
        if (!floatObject)
        {
            return;
        }

        float offsetY = Mathf.Sin(Time.time * floatSpeed) * floatHeight;

        transform.localPosition = new Vector3(
            startPosition.x,
            startPosition.y + offsetY,
            startPosition.z
        );
    }
}