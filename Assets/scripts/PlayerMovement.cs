using UnityEngine;
using System.Collections;

public class PlayerMovement : MonoBehaviour
{
    [Header("Movement")]
    public float forwardSpeed = 10f;
    public float turnSpeed = 120f;

    private Rigidbody rb;

    private Coroutine speedCoroutine;
    private bool isControlReversed = false;
    private Coroutine reverseCoroutine;

    void Start()
    {
        rb = GetComponent<Rigidbody>();
    }

    void FixedUpdate()
    {
        MoveForward();
        RotatePlayer();
    }

    void MoveForward()
    {
        rb.MovePosition(rb.position + transform.forward * forwardSpeed * Time.fixedDeltaTime);
    }

    void RotatePlayer()
    {
        float input = GetTurnInput();

        if (isControlReversed)
        {
            input *= -1f;
        }

        if (Mathf.Abs(input) > 0.01f)
        {
            Quaternion turn = Quaternion.Euler(0f, input * turnSpeed * Time.fixedDeltaTime, 0f);
            rb.MoveRotation(rb.rotation * turn);
        }

        rb.angularVelocity = Vector3.zero;
    }

    float GetTurnInput()
    {
        // PC klavye kontrolü
        float keyboardInput = Input.GetAxisRaw("Horizontal");

        if (Mathf.Abs(keyboardInput) > 0.01f)
        {
            return keyboardInput;
        }

        // Mobil / mouse kontrolü
        if (Input.GetMouseButton(0))
        {
            Vector3 touchPosition = Input.mousePosition;

            if (touchPosition.x < Screen.width / 2f)
            {
                return -1f; // ekranın solu
            }
            else
            {
                return 1f; // ekranın sağı
            }
        }

        return 0f;
    }

    public void IncreaseSpeed(float amount)
    {
        forwardSpeed += amount;
    }

    public void DecreaseSpeed(float amount)
    {
        forwardSpeed -= amount;

        if (forwardSpeed < 1f)
        {
            forwardSpeed = 1f;
        }
    }

    public void TemporarySpeedBoost(float amount, float duration)
    {
        if (speedCoroutine != null)
        {
            StopCoroutine(speedCoroutine);
        }

        speedCoroutine = StartCoroutine(SpeedBoostCoroutine(amount, duration));
    }

    IEnumerator SpeedBoostCoroutine(float amount, float duration)
    {
        forwardSpeed += amount;

        yield return new WaitForSeconds(duration);

        forwardSpeed -= amount;
        speedCoroutine = null;
    }

    public void ReverseControls(float duration)
    {
        if (reverseCoroutine != null)
        {
            StopCoroutine(reverseCoroutine);
        }

        reverseCoroutine = StartCoroutine(ReverseControlsCoroutine(duration));
    }

    IEnumerator ReverseControlsCoroutine(float duration)
    {
        isControlReversed = true;

        yield return new WaitForSeconds(duration);

        isControlReversed = false;
        reverseCoroutine = null;
    }
}