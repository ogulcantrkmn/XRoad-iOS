using UnityEngine;
using TMPro;

public class ScoreManager : MonoBehaviour
{
    public static ScoreManager instance;

    public int score = 0;
    public TextMeshProUGUI scoreText;

    // EKLEDİK
    public PlayerMovement player;

    void Awake()
    {
        instance = this;
    }

    void Start()
    {
        UpdateScore();
    }

   

    // EKLEDİK
    public void AddScore(int amount)
    {
        score += amount;
        UpdateScore();

        // 🎯 HIZ ARTIŞI
        if (player != null)
        {
            player.IncreaseSpeed(0.1f);
        }
    }

    void UpdateScore()
    {
        scoreText.text = "Score: " + score;
    }
}