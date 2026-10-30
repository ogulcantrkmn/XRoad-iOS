using UnityEngine;

public class RoadColorController : MonoBehaviour
{
    public static Color CurrentColor;

    [Header("Color Settings")]
    public float colorSpeed = 0.9f;
    public float saturation = 0.9f;
    public float brightness = 1f;

    private float hue;

    void Update()
    {
        // Sürekli renk akışı
        hue += Time.deltaTime * colorSpeed;

        if (hue > 1f)
            hue -= 1f;

        CurrentColor = Color.HSVToRGB(hue, saturation, brightness);
    }
}