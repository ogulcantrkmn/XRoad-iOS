using UnityEngine;
using TMPro;

public class NeonUIText : MonoBehaviour
{
    private TextMeshProUGUI text;
    private Material textMaterial;

    [Header("Color Animation")]
    public float colorSpeed = 0.25f;
    public float saturation = 1f;
    public float brightness = 1f;

    [Header("Neon Glow")]
    public float glowPower = 0.8f;
    public float glowOuter = 0.4f;
    public float outlineWidth = 0.2f;

    private float hue;

    void Start()
    {
        text = GetComponent<TextMeshProUGUI>();

        if (text == null)
        {
            Debug.LogError("NeonUIText scripti TextMeshProUGUI objesine eklenmeli.");
            enabled = false;
            return;
        }

        textMaterial = new Material(text.fontSharedMaterial);
        text.fontMaterial = textMaterial;

        hue = Random.Range(0f, 1f);

        textMaterial.EnableKeyword("GLOW_ON");
    }

    void Update()
    {
        hue += Time.unscaledDeltaTime * colorSpeed;

        if (hue > 1f)
            hue -= 1f;

        Color neonColor = Color.HSVToRGB(hue, saturation, brightness);

        text.color = neonColor;

        textMaterial.SetColor("_FaceColor", neonColor);
        textMaterial.SetColor("_OutlineColor", neonColor);
        textMaterial.SetFloat("_OutlineWidth", outlineWidth);

        textMaterial.SetColor("_GlowColor", neonColor);
        textMaterial.SetFloat("_GlowPower", glowPower);
        textMaterial.SetFloat("_GlowOuter", glowOuter);
    }
}