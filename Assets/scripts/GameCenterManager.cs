using UnityEngine;

#if UNITY_IOS
using UnityEngine.SocialPlatforms;
#endif

public class GameCenterManager : MonoBehaviour
{
    public static GameCenterManager instance;

    private const string leaderboardID =
        "com.jxjr.xroad.leaderboard.global";

    public bool IsAuthenticated
    {
        get
        {
#if UNITY_IOS && !UNITY_EDITOR
            return Social.localUser.authenticated;
#else
            return false;
#endif
        }
    }

    private void Awake()
    {
        if (instance == null)
        {
            instance = this;
            DontDestroyOnLoad(gameObject);
        }
        else
        {
            Destroy(gameObject);
            return;
        }
    }

    private void Start()
    {
#if UNITY_IOS && !UNITY_EDITOR
        Authenticate();
#endif
    }

    public void Authenticate()
    {
#if UNITY_IOS && !UNITY_EDITOR

        Social.localUser.Authenticate(
            (success, errorMessage) =>
            {
                if (success)
                {
                    Debug.Log(
                        "Game Center giriþ baþarýlý. Kullanýcý: " +
                        Social.localUser.userName
                    );
                }
                else
                {
                    Debug.LogWarning(
                        "Game Center giriþ baþarýsýz: " +
                        errorMessage
                    );
                }
            }
        );

#else

        Debug.Log(
            "Game Center authentication yalnýzca gerçek iOS cihazýnda çalýþýr."
        );

#endif
    }

    public void SubmitScore(long score)
    {
#if UNITY_IOS && !UNITY_EDITOR

        if (!Social.localUser.authenticated)
        {
            Debug.LogWarning(
                "Game Center skor gönderilemedi: kullanýcý giriþ yapmamýþ."
            );

            return;
        }

        Social.ReportScore(
            score,
            leaderboardID,
            success =>
            {
                if (success)
                {
                    Debug.Log(
                        "Game Center skor gönderildi: " +
                        score
                    );
                }
                else
                {
                    Debug.LogWarning(
                        "Game Center skor gönderilemedi."
                    );
                }
            }
        );

#else

        Debug.Log(
            "Game Center skor gönderimi yalnýzca iOS cihazýnda çalýþýr."
        );

#endif
    }

    public void ShowLeaderboard()
    {
#if UNITY_IOS && !UNITY_EDITOR

        if (!Social.localUser.authenticated)
        {
            Debug.LogWarning(
                "Game Center leaderboard açýlamadý: kullanýcý giriþ yapmamýþ."
            );

            Authenticate();
            return;
        }

        Social.ShowLeaderboardUI();

#else

        Debug.Log(
            "Game Center leaderboard yalnýzca gerçek iOS cihazýnda açýlýr."
        );

#endif
    }
}