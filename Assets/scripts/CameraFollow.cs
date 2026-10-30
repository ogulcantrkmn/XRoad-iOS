using UnityEngine;

public class CameraFollow : MonoBehaviour
{
    public Transform target;

    [Header("Offset")]
    public Vector3 offset = new Vector3(0f, 6f, -10f);

    [Header("Smooth")]
    public float smoothSpeed = 5f;

    void LateUpdate()
    {
        if (target == null) return;

        // 🎯 SABİT OFFSET (DÜNYAYA GÖRE)
        Vector3 desiredPosition = target.position + offset;

        transform.position = Vector3.Lerp(
            transform.position,
            desiredPosition,
            smoothSpeed * Time.deltaTime
        );

        // 🎯 HER ZAMAN TOPA BAK
        transform.LookAt(target);
    }
}