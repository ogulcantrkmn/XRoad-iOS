using UnityEngine;

public class CameraOrbit : MonoBehaviour
{
    public Transform target; // top
    public float distance = 10f;
    public float height = 5f;
    public float followSpeed = 5f;
    public float rotateSpeed = 120f;

    private float currentYRotation = 0f;

    void LateUpdate()
    {
        if (target == null) return;

        // 🎯 INPUT SADECE BURADA OKUNUR
        float input = Input.GetAxisRaw("Horizontal");

        // 🎯 SADECE BASILIRKEN DÖN
        if (input != 0)
        {
            currentYRotation += input * rotateSpeed * Time.deltaTime;
        }

        // 🎯 POZİSYON (TOPU TAKİP ET)
        Vector3 targetPosition = target.position;

        Quaternion rotation = Quaternion.Euler(0f, currentYRotation, 0f);

        Vector3 offset = rotation * new Vector3(0f, height, -distance);

        Vector3 desiredPosition = targetPosition + offset;

        transform.position = Vector3.Lerp(
            transform.position,
            desiredPosition,
            followSpeed * Time.deltaTime
        );

        // 🎯 HER ZAMAN TOPA BAK
        transform.LookAt(target);
    }
}