using UnityEngine;

public class PlayerCollision : MonoBehaviour
{
    private PlayerMovement playerMovement;

    void Start()
    {
        playerMovement = GetComponent<PlayerMovement>();
    }

    void OnTriggerEnter(Collider other)
    {
        if (other.CompareTag("Collectible"))
        {
            XColor x = other.GetComponent<XColor>();

            if (x != null)
            {
                if (x.type == XColor.XType.Red)
                {
                    ScoreManager.instance.AddScore(3);

                    // Kalıcı hız artışı
                    playerMovement.IncreaseSpeed(0.2f);

                    // Geçici hız artışı
                    playerMovement.TemporarySpeedBoost(0.5f, 3f);
                }
                else if (x.type == XColor.XType.White)
                {
                    ScoreManager.instance.AddScore(-1);

                    // Kalıcı hız azalışı
                    playerMovement.DecreaseSpeed(0.1f);
                }
                else if (x.type == XColor.XType.Black)
                {
                    ScoreManager.instance.AddScore(6);

                    // Kalıcı hız artışı
                    playerMovement.IncreaseSpeed(0.2f);

                    // Sağ-sol ters kontrol
                    playerMovement.ReverseControls(0.75f);
                }
            }

            Destroy(other.gameObject);
        }
    }
}