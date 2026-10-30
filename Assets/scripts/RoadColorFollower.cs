using UnityEngine;

public class RoadColorFollower : MonoBehaviour
{
    private Renderer rend;

    void Start()
    {
        rend = GetComponentInChildren<Renderer>();
    }

    void Update()
    {
        Color c = RoadColorController.CurrentColor;

        rend.material.color = c;
        rend.material.SetColor("_EmissionColor", c * 2f);
    }
}