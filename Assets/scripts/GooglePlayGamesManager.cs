using UnityEngine;

#if UNITY_ANDROID
using GooglePlayGames;
using GooglePlayGames.BasicApi;
#endif

public class GooglePlayGamesManager : MonoBehaviour
{
    public static GooglePlayGamesManager instance;

    public bool IsSignedIn
    {
        get
        {
#if UNITY_ANDROID
            return PlayGamesPlatform.Instance.IsAuthenticated();
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
#if UNITY_ANDROID
        SignIn();
#endif
    }

    public void SignIn()
    {
#if UNITY_ANDROID
        PlayGamesPlatform.Instance.Authenticate(ProcessAuthentication);
#else
        Debug.Log("Google Play Games yalnýzca Android platformunda kullanýlmaktadýr.");
#endif
    }

#if UNITY_ANDROID
    private void ProcessAuthentication(SignInStatus status)
    {
        if (status == SignInStatus.Success)
        {
            Debug.Log("Google Play Games giriþ baþarýlý.");
        }
        else
        {
            Debug.LogWarning(
                "Google Play Games giriþ baþarýsýz. Durum: " + status
            );
        }
    }
#endif

    public void SubmitScore(long score)
    {
#if UNITY_ANDROID
        if (!IsSignedIn)
        {
            Debug.LogWarning(
                "Skor gönderilemedi: Google Play Games'e giriþ yapýlmamýþ."
            );

            return;
        }

        PlayGamesPlatform.Instance.ReportScore(
            score,
            GPGSIds.leaderboard_xroad_global_leaderboard,
            success =>
            {
                if (success)
                {
                    Debug.Log(
                        "Global skor baþarýyla gönderildi: " + score
                    );
                }
                else
                {
                    Debug.LogWarning(
                        "Global skor gönderilemedi."
                    );
                }
            }
        );
#else
        Debug.Log(
            "Google Play Games skor gönderme yalnýzca Android'de kullanýlmaktadýr."
        );
#endif
    }

    public void ShowLeaderboard()
    {
#if UNITY_ANDROID
        if (!IsSignedIn)
        {
            Debug.LogWarning(
                "Leaderboard açýlamadý: Google Play Games'e giriþ yapýlmamýþ."
            );

            return;
        }

        PlayGamesPlatform.Instance.ShowLeaderboardUI(
            GPGSIds.leaderboard_xroad_global_leaderboard
        );
#else
        Debug.Log(
            "Google Play Games leaderboard yalnýzca Android'de kullanýlmaktadýr."
        );
#endif
    }
}