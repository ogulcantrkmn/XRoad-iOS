using UnityEngine;

public class XColor : MonoBehaviour
{
    public Color[] colors;

    private Renderer rend;

    public enum XType
    {
        Red,
        White,
        Black
    }

    public XType type;

    void Start()
    {
        rend = GetComponent<Renderer>();

        int index = Random.Range(0, colors.Length);
        Color selectedColor = colors[index];

        rend.material.color = selectedColor;

        if (selectedColor == Color.red)
            type = XType.Red;
        else if (selectedColor == Color.white)
            type = XType.White;
        else if (selectedColor == Color.black)
            type = XType.Black;
    }
}