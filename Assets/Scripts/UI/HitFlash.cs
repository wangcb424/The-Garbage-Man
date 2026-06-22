using System.Collections;
using UnityEngine;

public class HitFlash : MonoBehaviour
{
    public Color flashColor = Color.white;
    public float flashDuration = 0.15f;
    public float flashIntensity = 2f;

    private Renderer[] renderers;
    private Color[][] originalColors;

    private void Awake()
    {
        renderers = GetComponentsInChildren<Renderer>();
        originalColors = new Color[renderers.Length][];

        for (int i = 0; i < renderers.Length; i++)
        {
            Material[] mats = renderers[i].materials;
            originalColors[i] = new Color[mats.Length];

            for (int j = 0; j < mats.Length; j++)
            {
                originalColors[i][j] = mats[j].GetColor("_BaseColor");
            }
        }
    }

    public void Flash()
    {
        StopAllCoroutines();
        StartCoroutine(FlashCoroutine());
    }

    private IEnumerator FlashCoroutine()
    {
        for (int i = 0; i < renderers.Length; i++)
        {
            Material[] mats = renderers[i].materials;

            for (int j = 0; j < mats.Length; j++)
            {
                mats[j].SetColor("_BaseColor", flashColor * flashIntensity);
            }

            renderers[i].materials = mats;
        }

        yield return new WaitForSeconds(flashDuration);

        for (int i = 0; i < renderers.Length; i++)
        {
            Material[] mats = renderers[i].materials;

            for (int j = 0; j < mats.Length; j++)
            {
                mats[j].SetColor("_BaseColor", originalColors[i][j]);
            }

            renderers[i].materials = mats;
        }
    }
}