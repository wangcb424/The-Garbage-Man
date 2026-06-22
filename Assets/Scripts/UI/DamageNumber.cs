using System.Collections;
using TMPro;
using UnityEngine;

public class DamageNumber : MonoBehaviour
{
    public float floatSpeed = 1.5f;
    public float fadeDuration = 0.8f;
    public float floatHeight = 1.5f;

    private TextMeshProUGUI text;
    private Color originalColor;

    private void Awake()
    {
        text = GetComponent<TextMeshProUGUI>();
        originalColor = text.color;
    }

    public static void Spawn(Vector3 worldPosition, int damage, GameObject prefab, Canvas canvas)
    {
        if (prefab == null || canvas == null)
        {
            return;
        }

        GameObject obj = Instantiate(prefab, canvas.transform);
        obj.transform.position = worldPosition + Vector3.up * 1.5f;

        DamageNumber number = obj.GetComponent<DamageNumber>();
        if (number != null)
        {
            number.Setup(damage);
        }
    }

    public void Setup(int damage)
    {
        text.text = damage.ToString();
        StartCoroutine(FloatAndFade());
    }

    private IEnumerator FloatAndFade()
    {
        float elapsed = 0f;
        Vector3 startPosition = transform.position;

        while (elapsed < fadeDuration)
        {
            elapsed += Time.deltaTime;
            float progress = elapsed / fadeDuration;

            transform.position = startPosition + Vector3.up * floatHeight * progress;

            Color color = originalColor;
            color.a = 1f - progress;
            text.color = color;

            yield return null;
        }

        Destroy(gameObject);
    }
}