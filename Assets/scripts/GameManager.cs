using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.SceneManagement;
using TMPro;

public class GameManager : MonoBehaviour
{
    private static bool skipLogoOnRestart = false;
    private static bool howToPlayShownThisSession = false;

    // =========================================================
    // INTERSTITIAL AD FREQUENCY
    // =========================================================

    // Son reklamdan beri kaç tur oynandı?
    private static int roundsSinceLastAd = 0;

    // Mevcut reklam döngüsünün ilk iki turundan
    // herhangi birinde 30 veya üzeri skor yapıldı mı?
    private static bool reached30InCurrentAdCycle = false;

    // Reklam kararında kullanılacak skor sınırı.
    // Daha sonra Inspector kullanmadan buradan değiştirebilirsin.
    private const int adScoreThreshold = 30;

    private bool canRestartAfterGameOver = false;
    private bool canStartGame = false;

    [Header("Road Prefabs (0 = düz yol önerilir)")]
    public GameObject[] roadPrefabs;

    [Header("References")]
    public Transform player;

    [Header("Spawn Settings")]
    public int startRoadCount = 6;
    public float spawnDistance = 70f;
    public int maxRoadCount = 10;

    [Header("X Spawn Settings")]
    public GameObject xPrefab;

    [Tooltip("Yol genişliğinin yarısı. Örn: yol genişliği 5 ise buraya 2.5 yaz.")]
    public float roadHalfWidth = 1.35f;

    [Tooltip("X'ler yol prefabının 0.05 birim üstünde olacak.")]
    public float xSurfaceOffset = 0.05f;

    [Tooltip("Aynı yol parçasındaki X'ler arası minimum ileri mesafe")]
    public float minXDistance = 5f;

    [Tooltip("Aynı yol parçasındaki X'ler arası maksimum ileri mesafe")]
    public float maxXDistance = 12f;

    [Tooltip("Bir yol parçasında maksimum kaç X oluşabilir")]
    public int maxXPerRoad = 3;

    [Header("How To Play UI")]
    public GameObject howToPlayPanel;

    [Header("Global Leaderboard UI")]
    public GameObject glbPanel;

    [Header("Start UI")]
    public GameObject startPanel;
    public CanvasGroup startCanvasGroup;
    public bool isGameStarted = false;

    [Header("Logo UI")]
    public GameObject logoPanel;
    public CanvasGroup logoCanvasGroup;
    public float logoVisibleTime = 2f;
    public float fadeDuration = 1f;

    [Header("Game Over UI")]
    public GameObject gameOverPanel;
    public TextMeshProUGUI finalScoreText;
    public TextMeshProUGUI bestScoreText;
    public GameObject scoreTextObject;
    public bool isGameOver = false;

    private List<GameObject> activeRoads = new List<GameObject>();
    private Transform lastSpawnPoint;
    private int lastIndex = -1;


    void Awake()
    {
        // Başlangıçta Logo görünür olsun.
        if (logoPanel != null)
        {
            logoPanel.SetActive(true);
            logoPanel.transform.SetAsLastSibling();
        }

        // Diğer başlangıç panellerini kapat.
        if (startPanel != null)
        {
            startPanel.SetActive(false);
        }

        if (howToPlayPanel != null)
        {
            howToPlayPanel.SetActive(false);
        }

        if (glbPanel != null)
        {
            glbPanel.SetActive(false);
        }

        if (gameOverPanel != null)
        {
            gameOverPanel.SetActive(false);
        }

        SetCanvasGroup(logoCanvasGroup, 1f, false);
        SetCanvasGroup(startCanvasGroup, 0f, false);
    }


    void Start()
    {
        if (roadPrefabs == null || roadPrefabs.Length == 0)
        {
            Debug.LogError("Road prefab eklenmemiş!");
            return;
        }

        if (player == null)
        {
            Debug.LogError("Player atanmadı!");
            return;
        }

        if (xPrefab == null)
        {
            Debug.LogWarning("X Prefab atanmadı. X objeleri spawn olmayacak.");
        }

        // Oyun açılış ekranlarında fizik durmuş olmalı.
        Time.timeScale = 0f;

        // İlk yolları oluştur.
        SpawnStartRoads();

        if (gameOverPanel != null)
        {
            gameOverPanel.SetActive(false);
        }

        if (scoreTextObject != null)
        {
            scoreTextObject.SetActive(true);
        }

        isGameStarted = false;
        isGameOver = false;
        canStartGame = false;
        canRestartAfterGameOver = false;

        // Game Over sonrası yeniden yüklenen sahnede
        // Logo ve How To Play atlanır, GLB açılır.
        if (skipLogoOnRestart)
        {
            skipLogoOnRestart = false;
            ShowGLBPanel();
        }
        else
        {
            // Uygulamanın normal ilk açılışı.
            StartCoroutine(LogoSequence());
        }
    }


    void Update()
    {
        // Game Over durumundaysak yalnızca restart input'u dinle.
        if (isGameOver)
        {
            CheckRestartInput();
            return;
        }

        // Oyun henüz başlamadıysa yol üretme vb. çalışma.
        if (!isGameStarted)
        {
            // Sadece PLEASE TAP TO START ekranında
            // başlangıç input'una izin ver.
            if (canStartGame)
            {
                CheckStartInput();
            }

            return;
        }

        if (lastSpawnPoint == null)
        {
            return;
        }

        float dist = Vector3.Distance(
            player.position,
            lastSpawnPoint.position
        );

        if (dist < spawnDistance)
        {
            SpawnRoad();
        }
    }


    // =========================================================
    // ROAD SPAWN
    // =========================================================

    void SpawnStartRoads()
    {
        GameObject firstRoad = Instantiate(
            roadPrefabs[0],
            Vector3.zero,
            Quaternion.identity
        );

        activeRoads.Add(firstRoad);

        SpawnRandomXOnRoad(firstRoad);

        lastSpawnPoint = firstRoad.transform.Find("SpawnPoint");

        if (lastSpawnPoint == null)
        {
            Debug.LogError("Prefab içinde SpawnPoint yok!");
            return;
        }

        for (int i = 1; i < startRoadCount; i++)
        {
            SpawnRoad();
        }
    }


    void SpawnRoad()
    {
        if (lastSpawnPoint == null)
        {
            Debug.LogError("SpawnRoad çağrıldı fakat lastSpawnPoint bulunamadı!");
            return;
        }

        GameObject prefab = GetRandomRoad();

        if (prefab == null)
        {
            Debug.LogError("Spawn edilecek Road Prefab bulunamadı!");
            return;
        }

        GameObject newRoad = Instantiate(
            prefab,
            lastSpawnPoint.position,
            lastSpawnPoint.rotation
        );

        activeRoads.Add(newRoad);

        SpawnRandomXOnRoad(newRoad);

        lastSpawnPoint = newRoad.transform.Find("SpawnPoint");

        if (lastSpawnPoint == null)
        {
            Debug.LogError("Yeni prefab SpawnPoint yok!");
            return;
        }

        if (activeRoads.Count > maxRoadCount)
        {
            Destroy(activeRoads[0]);
            activeRoads.RemoveAt(0);
        }
    }


    GameObject GetRandomRoad()
    {
        if (roadPrefabs == null || roadPrefabs.Length == 0)
        {
            return null;
        }

        if (roadPrefabs.Length == 1)
        {
            return roadPrefabs[0];
        }

        int index = Random.Range(0, roadPrefabs.Length);

        int safety = 0;

        while (index == lastIndex && safety < 10)
        {
            index = Random.Range(0, roadPrefabs.Length);
            safety++;
        }

        lastIndex = index;

        return roadPrefabs[index];
    }


    // =========================================================
    // X SPAWN
    // =========================================================

    void SpawnRandomXOnRoad(GameObject road)
    {
        if (xPrefab == null)
        {
            return;
        }

        if (road == null)
        {
            return;
        }

        Transform roadSpawnPoint = road.transform.Find("SpawnPoint");

        if (roadSpawnPoint == null)
        {
            Debug.LogError(
                "Bu road prefab içinde SpawnPoint yok: " + road.name
            );

            return;
        }

        float calculatedRoadLength = Vector3.Distance(
            road.transform.position,
            roadSpawnPoint.position
        );

        int spawnedCount = 0;

        float currentDistance = Random.Range(
            minXDistance,
            maxXDistance
        );

        while (
            currentDistance < calculatedRoadLength &&
            spawnedCount < maxXPerRoad
        )
        {
            float randomSide = Random.Range(
                -roadHalfWidth,
                roadHalfWidth
            );

            Vector3 localPos = new Vector3(
                randomSide,
                10f,
                currentDistance
            );

            Vector3 rayStart = road.transform.TransformPoint(localPos);

            if (
                Physics.Raycast(
                    rayStart,
                    Vector3.down,
                    out RaycastHit hit,
                    30f
                )
            )
            {
                Vector3 spawnPos =
                    hit.point + hit.normal * xSurfaceOffset;

                Quaternion spawnRot =
                    Quaternion.LookRotation(
                        road.transform.forward,
                        hit.normal
                    );

                Instantiate(
                    xPrefab,
                    spawnPos,
                    spawnRot,
                    road.transform
                );
            }

            spawnedCount++;

            currentDistance += Random.Range(
                minXDistance,
                maxXDistance
            );
        }
    }


    // =========================================================
    // GAME START
    // =========================================================

    void CheckStartInput()
    {
        bool keyboardStart =
            Input.GetKeyDown(KeyCode.LeftArrow) ||
            Input.GetKeyDown(KeyCode.RightArrow) ||
            Input.GetKeyDown(KeyCode.A) ||
            Input.GetKeyDown(KeyCode.D);

        bool touchOrMouseStart =
            Input.GetMouseButtonDown(0);

        if (keyboardStart || touchOrMouseStart)
        {
            StartGame();
        }
    }


    void StartGame()
    {
        // Logo / How To Play / GLB gibi ekranlarda
        // yanlışlıkla çağrılırsa oyunu başlatma.
        if (!canStartGame)
        {
            return;
        }

        isGameStarted = true;

        // Aynı input'un tekrar başlangıç input'u sayılmasını engelle.
        canStartGame = false;

        if (startPanel != null)
        {
            startPanel.SetActive(false);
        }

        // Fizik ve oyun artık çalışabilir.
        Time.timeScale = 1f;
    }


    bool ShouldShowInterstitialAd(int currentScore)
    {
        // Yeni bir tur tamamlandı.
        roundsSinceLastAd++;

        // Bu reklam döngüsünde 30 veya üzeri skor
        // görülüp görülmediğini kaydet.
        if (currentScore >= adScoreThreshold)
        {
            reached30InCurrentAdCycle = true;
        }

        Debug.Log(
            "AD CHECK | Tur: " +
            roundsSinceLastAd +
            " | Skor: " +
            currentScore +
            " | Bu döngüde 30+: " +
            reached30InCurrentAdCycle
        );

        // ---------------------------------------------------------
        // 1. TUR
        // ---------------------------------------------------------

        // İlk turda hiçbir durumda reklam yok.
        if (roundsSinceLastAd < 2)
        {
            Debug.Log(
                "AD CHECK | 1. tur tamamlandı. Reklam gösterilmeyecek."
            );

            return false;
        }

        // ---------------------------------------------------------
        // 2. TUR
        // ---------------------------------------------------------

        if (roundsSinceLastAd == 2)
        {
            // İlk iki turun herhangi birinde
            // 30 veya üzeri skor varsa reklam göster.
            if (reached30InCurrentAdCycle)
            {
                Debug.Log(
                    "AD CHECK | İlk iki turdan en az birinde 30+ skor var. Reklam gösterilecek."
                );

                ResetAdCycle();

                return true;
            }

            // İlk iki turun ikisi de 30'un altındaysa
            // oyuncuya üçüncü turu reklamsız ver.
            Debug.Log(
                "AD CHECK | İlk iki turun ikisi de 30 altında. Reklam 3. turun sonuna ertelendi."
            );

            return false;
        }

        // ---------------------------------------------------------
        // 3. TUR
        // ---------------------------------------------------------

        // Buraya geldiysek ilk iki turda 30'a ulaşılamamıştı.
        // Üçüncü turun skoru ne olursa olsun reklam göster.
        if (roundsSinceLastAd >= 3)
        {
            Debug.Log(
                "AD CHECK | 3. tur tamamlandı. Reklam gösterilecek."
            );

            ResetAdCycle();

            return true;
        }

        return false;
    }

    void ResetAdCycle()
    {
        roundsSinceLastAd = 0;
        reached30InCurrentAdCycle = false;

        Debug.Log(
            "AD CHECK | Reklam döngüsü sıfırlandı."
        );
    }

    // =========================================================
    // GAME OVER
    // =========================================================

    public void GameOver()
    {
        if (isGameOver)
        {
            return;
        }

        isGameOver = true;
        isGameStarted = false;
        canStartGame = false;
        canRestartAfterGameOver = false;

        Time.timeScale = 0f;

        if (scoreTextObject != null)
        {
            scoreTextObject.SetActive(false);
        }

        // Bu turun skorunu al.
        int currentScore = 0;

        if (ScoreManager.instance != null)
        {
            currentScore = ScoreManager.instance.score;
        }
        else
        {
            Debug.LogWarning(
                "ScoreManager bulunamadı. Reklam kararı için skor 0 kabul edildi."
            );
        }

        // Yeni reklam kuralımıza göre
        // bu turun sonunda reklam gösterilip
        // gösterilmeyeceğine karar ver.
        bool shouldShowAd =
            ShouldShowInterstitialAd(currentScore);

        if (
            shouldShowAd &&
            AdManager.instance != null
        )
        {
            Debug.Log(
                "AD CHECK | Interstitial reklam çağrılıyor."
            );

            AdManager.instance.ShowInterstitial(
                ShowGameOverPanel
            );
        }
        else
        {
            if (!shouldShowAd)
            {
                Debug.Log(
                    "AD CHECK | Bu turun sonunda reklam yok."
                );
            }
            else
            {
                Debug.LogWarning(
                    "AD CHECK | Reklam gösterilmesi gerekiyordu fakat AdManager bulunamadı."
                );
            }

            ShowGameOverPanel();
        }
    }


    void ShowGameOverPanel()
    {
        if (gameOverPanel != null)
        {
            gameOverPanel.SetActive(true);
            gameOverPanel.transform.SetAsLastSibling();
        }

        int currentScore = 0;

        if (ScoreManager.instance != null)
        {
            currentScore = ScoreManager.instance.score;
        }
        else
        {
            Debug.LogWarning(
                "ScoreManager bulunamadı. Current Score 0 kabul edildi."
            );
        }

        // Yerel Best Score
        int bestScore = PlayerPrefs.GetInt(
            "BestScore",
            0
        );

        if (currentScore > bestScore)
        {
            bestScore = currentScore;

            PlayerPrefs.SetInt(
                "BestScore",
                bestScore
            );

            PlayerPrefs.Save();
        }

        // Global Leaderboard'a bu turun skorunu gönder.
        // Google giriş yapılamamışsa GooglePlayGamesManager
        // kendi içinde skoru göndermeden çıkacaktır.
        if (GooglePlayGamesManager.instance != null)
        {
            #if UNITY_ANDROID

            if (GooglePlayGamesManager.instance != null)
            {
                GooglePlayGamesManager.instance.SubmitScore(currentScore);
            }

            #elif UNITY_IOS

            if (GameCenterManager.instance != null)
            {
                GameCenterManager.instance.SubmitScore(currentScore);
            }

            #endif
            }
            else
        {
            Debug.LogWarning(
                "GooglePlayGamesManager bulunamadı. Global skor gönderilmedi."
            );
        }

        if (finalScoreText != null)
        {
            finalScoreText.text =
                "Game Over\nScore: " + currentScore;
        }

        if (bestScoreText != null)
        {
            bestScoreText.text =
                "Best Score: " + bestScore;
        }

        // Artık ekrana dokunarak restart yapılabilir.
        canRestartAfterGameOver = true;
    }


    // =========================================================
    // RESTART
    // =========================================================

    void CheckRestartInput()
    {
        if (!canRestartAfterGameOver)
        {
            return;
        }

        bool keyboardRestart =
            Input.GetKeyDown(KeyCode.LeftArrow) ||
            Input.GetKeyDown(KeyCode.RightArrow) ||
            Input.GetKeyDown(KeyCode.A) ||
            Input.GetKeyDown(KeyCode.D);

        bool touchOrMouseRestart =
            Input.GetMouseButtonDown(0);

        if (keyboardRestart || touchOrMouseRestart)
        {
            RestartGame();
        }
    }


    void RestartGame()
    {
        // Yeni sahne açıldığında Logo ve How To Play yerine
        // doğrudan GLB gösterilecek.
        skipLogoOnRestart = true;

        // Scene yüklenmeden önce timescale'i normale döndür.
        Time.timeScale = 1f;

        SceneManager.LoadScene(
            SceneManager.GetActiveScene().buildIndex
        );
    }


    // =========================================================
    // LOGO
    // =========================================================

    IEnumerator LogoSequence()
    {
        canStartGame = false;
        isGameStarted = false;

        if (logoPanel != null)
        {
            logoPanel.SetActive(true);
            logoPanel.transform.SetAsLastSibling();
        }

        if (howToPlayPanel != null)
        {
            howToPlayPanel.SetActive(false);
        }

        if (glbPanel != null)
        {
            glbPanel.SetActive(false);
        }

        if (startPanel != null)
        {
            startPanel.SetActive(false);
        }

        SetCanvasGroup(
            logoCanvasGroup,
            1f,
            false
        );

        SetCanvasGroup(
            startCanvasGroup,
            0f,
            false
        );

        Time.timeScale = 0f;

        // Logo ekranda beklesin.
        yield return new WaitForSecondsRealtime(
            logoVisibleTime
        );

        // Logo fade-out.
        yield return StartCoroutine(
            FadeCanvasGroup(
                logoCanvasGroup,
                1f,
                0f,
                fadeDuration
            )
        );

        if (logoPanel != null)
        {
            logoPanel.SetActive(false);
        }

        // Uygulamanın bu oturumunda How To Play
        // henüz gösterilmediyse göster.
        if (!howToPlayShownThisSession)
        {
            ShowHowToPlayPanel();
        }
        else
        {
            // How To Play daha önce görülmüşse
            // Logo'dan sonra GLB'ye geç.
            ShowGLBPanel();
        }
    }


    IEnumerator FadeCanvasGroup(
        CanvasGroup canvasGroup,
        float startAlpha,
        float endAlpha,
        float duration
    )
    {
        if (canvasGroup == null)
        {
            yield break;
        }

        float timer = 0f;

        canvasGroup.alpha = startAlpha;

        while (timer < duration)
        {
            timer += Time.unscaledDeltaTime;

            float t = timer / duration;

            canvasGroup.alpha =
                Mathf.Lerp(
                    startAlpha,
                    endAlpha,
                    t
                );

            yield return null;
        }

        canvasGroup.alpha = endAlpha;
    }


    // =========================================================
    // HOW TO PLAY
    // =========================================================

    void ShowHowToPlayPanel()
    {
        canStartGame = false;
        isGameStarted = false;

        if (logoPanel != null)
        {
            logoPanel.SetActive(false);
        }

        if (startPanel != null)
        {
            startPanel.SetActive(false);
        }

        if (glbPanel != null)
        {
            glbPanel.SetActive(false);
        }

        if (howToPlayPanel != null)
        {
            howToPlayPanel.SetActive(true);
            howToPlayPanel.transform.SetAsLastSibling();
        }

        Time.timeScale = 0f;
    }


    public void OnGotItPressed()
    {
        Debug.Log(
            "GOT IT butonuna basildi."
        );

        // How To Play bu uygulama oturumunda bir daha
        // gösterilmesin.
        howToPlayShownThisSession = true;

        if (howToPlayPanel != null)
        {
            howToPlayPanel.SetActive(false);
        }

        // GOT IT sonrası doğrudan GLB.
        ShowGLBPanel();
    }


    // =========================================================
    // GLOBAL LEADERBOARD
    // =========================================================

    void ShowGLBPanel()
    {
        // GLB açıkken dokunma oyunu başlatmamalı.
        canStartGame = false;
        isGameStarted = false;

        if (logoPanel != null)
        {
            logoPanel.SetActive(false);
        }

        if (howToPlayPanel != null)
        {
            howToPlayPanel.SetActive(false);
        }

        if (startPanel != null)
        {
            startPanel.SetActive(false);
        }

        if (gameOverPanel != null)
        {
            gameOverPanel.SetActive(false);
        }

        if (glbPanel != null)
        {
            glbPanel.SetActive(true);
            glbPanel.transform.SetAsLastSibling();
        }

        Time.timeScale = 0f;
    }


    public void OnViewGlobalLeaderboardPressed()
    {
#if UNITY_ANDROID

    if (GooglePlayGamesManager.instance != null)
    {
        GooglePlayGamesManager.instance.ShowLeaderboard();
    }

#elif UNITY_IOS

        if (GameCenterManager.instance != null)
        {
            GameCenterManager.instance.ShowLeaderboard();
        }

#endif
    }


    public void OnGLBContinuePressed()
    {
        Debug.Log(
            "GLB CONTINUE butonuna basildi."
        );

        if (glbPanel != null)
        {
            glbPanel.SetActive(false);
        }

        ShowStartPanelDirectly();
    }


    // =========================================================
    // PLEASE TAP TO START
    // =========================================================

    void ShowStartPanelDirectly()
    {
        if (logoPanel != null)
        {
            logoPanel.SetActive(false);
        }

        if (howToPlayPanel != null)
        {
            howToPlayPanel.SetActive(false);
        }

        if (glbPanel != null)
        {
            glbPanel.SetActive(false);
        }

        if (startPanel != null)
        {
            startPanel.SetActive(true);
            startPanel.transform.SetAsLastSibling();
        }

        SetCanvasGroup(
            logoCanvasGroup,
            0f,
            false
        );

        SetCanvasGroup(
            startCanvasGroup,
            1f,
            true
        );

        // PLEASE TAP TO START aşamasındayız.
        isGameStarted = false;

        // Artık ekrandaki dokunma oyunu başlatabilir.
        canStartGame = true;

        // Oyuncu gerçekten dokunana kadar fizik durmuş.
        Time.timeScale = 0f;
    }


    // =========================================================
    // UI HELPER
    // =========================================================

    void SetCanvasGroup(
        CanvasGroup canvasGroup,
        float alpha,
        bool interactable
    )
    {
        if (canvasGroup == null)
        {
            return;
        }

        canvasGroup.alpha = alpha;
        canvasGroup.interactable = interactable;
        canvasGroup.blocksRaycasts = interactable;
    }
}