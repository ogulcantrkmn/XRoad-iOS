using UnityEngine;
using System.Collections;

public class PlayerGroundCheck : MonoBehaviour
{
    [Header("Ground Check")]

    [Tooltip("Topun altından yol aramak için kullanılacak ekstra mesafe.")]
    public float extraGroundDistance = 0.30f;

    [Tooltip("Yol bulunamazsa Game Over olmadan önce beklenecek süre.")]
    public float fallDelay = 1f;

    [Tooltip("Merkez dışındaki ek raylerin merkeze uzaklık oranı.")]
    [Range(0f, 1f)]
    public float sideRayOffset = 0.45f;


    private SphereCollider playerCollider;
    private GameManager gameManager;
    private LayerMask groundLayer;

    private bool isFalling = false;
    private Coroutine fallCoroutine;


    void Awake()
    {
        // Player objesindeki SphereCollider'ı al.
        playerCollider = GetComponent<SphereCollider>();

        if (playerCollider == null)
        {
            Debug.LogError(
                "PlayerGroundCheck: Player üzerinde SphereCollider bulunamadı!"
            );
        }

        // Ground layer'ını otomatik al.
        groundLayer = LayerMask.GetMask("Ground");

        if (groundLayer.value == 0)
        {
            Debug.LogError(
                "PlayerGroundCheck: Ground layer bulunamadı!"
            );
        }
    }


    void Start()
    {
        gameManager = FindAnyObjectByType<GameManager>();

        if (gameManager == null)
        {
            Debug.LogError(
                "PlayerGroundCheck: GameManager bulunamadı!"
            );
        }

        CancelFallCheck();
    }


    void Update()
    {
        if (gameManager == null)
            return;

        if (playerCollider == null)
            return;

        // Logo, How To Play, GLB ve Start ekranlarında
        // zemin kontrolü çalışmasın.
        if (!gameManager.isGameStarted)
        {
            CancelFallCheck();
            return;
        }

        if (gameManager.isGameOver)
        {
            CancelFallCheck();
            return;
        }

        CheckGround();
    }


    void CheckGround()
    {
        bool grounded = IsGrounded();

        if (grounded)
        {
            // Daha önce düşme sayacı başladıysa iptal et.
            if (isFalling)
            {
                CancelFallCheck();
            }

            return;
        }

        // Yol bulunamadı.
        if (!isFalling)
        {
            isFalling = true;

            fallCoroutine = StartCoroutine(
                FallDelayCoroutine()
            );
        }
    }


    bool IsGrounded()
    {
        Bounds bounds = playerCollider.bounds;

        // Topun dünya koordinatlarındaki merkezi.
        Vector3 center = bounds.center;

        // Topun merkezinden tabanına kadar olan mesafe.
        float radius = bounds.extents.y;

        // Merkezden aşağı:
        // topun yarıçapı + küçük ekstra mesafe.
        float rayLength = radius + extraGroundDistance;

        // Yan raylerin X/Z yönündeki uzaklığı.
        float offset = radius * sideRayOffset;

        // 1 - TAM MERKEZ
        if (GroundRay(
            center,
            rayLength
        ))
        {
            return true;
        }

        // 2 - SAĞ
        if (GroundRay(
            center + transform.right * offset,
            rayLength
        ))
        {
            return true;
        }

        // 3 - SOL
        if (GroundRay(
            center - transform.right * offset,
            rayLength
        ))
        {
            return true;
        }

        // 4 - ÖN
        if (GroundRay(
            center + transform.forward * offset,
            rayLength
        ))
        {
            return true;
        }

        // 5 - ARKA
        if (GroundRay(
            center - transform.forward * offset,
            rayLength
        ))
        {
            return true;
        }

        return false;
    }


    bool GroundRay(
        Vector3 origin,
        float distance
    )
    {
        bool hitGround = Physics.Raycast(
            origin,
            Vector3.down,
            out RaycastHit hit,
            distance,
            groundLayer,
            QueryTriggerInteraction.Ignore
        );

        Debug.DrawRay(
            origin,
            Vector3.down * distance,
            hitGround ? Color.green : Color.red
        );

        return hitGround;
    }


    IEnumerator FallDelayCoroutine()
    {
        yield return new WaitForSeconds(
            fallDelay
        );

        // Coroutine artık tamamlandı.
        fallCoroutine = null;

        if (gameManager == null)
        {
            isFalling = false;
            yield break;
        }

        if (!gameManager.isGameStarted)
        {
            isFalling = false;
            yield break;
        }

        if (gameManager.isGameOver)
        {
            isFalling = false;
            yield break;
        }

        // 1 saniyenin sonunda zemini tekrar kontrol et.
        bool stillNotGrounded = !IsGrounded();

        if (!stillNotGrounded)
        {
            // Yol tekrar bulundu.
            isFalling = false;
            yield break;
        }

        // 1 saniye boyunca gerçekten yol bulunamadı.
        isFalling = false;

        gameManager.GameOver();
    }


    void CancelFallCheck()
    {
        if (fallCoroutine != null)
        {
            StopCoroutine(fallCoroutine);
            fallCoroutine = null;
        }

        isFalling = false;
    }


    void OnDisable()
    {
        CancelFallCheck();
    }
}