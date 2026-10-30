using System;
using UnityEngine;
using UnityEngine.Advertisements;

public class AdManager : MonoBehaviour, IUnityAdsInitializationListener, IUnityAdsLoadListener, IUnityAdsShowListener
{
    public static AdManager instance;

    private Action onInterstitialClosed;

    [Header("Unity Ads Game IDs")]
    [SerializeField] private string androidGameId = "YOUR_ANDROID_GAME_ID";
    [SerializeField] private string iosGameId = "YOUR_IOS_GAME_ID";

    [Header("Ad Unit IDs")]
    [SerializeField] private string androidInterstitialAdUnitId = "Interstitial_Android";
    [SerializeField] private string iosInterstitialAdUnitId = "Interstitial_iOS";

    [Header("Settings")]
    [SerializeField] private bool testMode = true;

    private string gameId;
    private string interstitialAdUnitId;
    private bool interstitialLoaded = false;

    void Awake()
    {
        if (instance != null && instance != this)
        {
            Destroy(gameObject);
            return;
        }

        instance = this;
        DontDestroyOnLoad(gameObject);

#if UNITY_IOS
        gameId = iosGameId;
        interstitialAdUnitId = iosInterstitialAdUnitId;
#elif UNITY_ANDROID
        gameId = androidGameId;
        interstitialAdUnitId = androidInterstitialAdUnitId;
#else
        gameId = androidGameId;
        interstitialAdUnitId = androidInterstitialAdUnitId;
#endif

        InitializeAds();
    }

    public void InitializeAds()
    {
        if (Advertisement.isInitialized)
        {
            LoadInterstitial();
            return;
        }

        Advertisement.Initialize(gameId, testMode, this);
    }

    public void LoadInterstitial()
    {
        interstitialLoaded = false;
        Advertisement.Load(interstitialAdUnitId, this);
    }

    public void ShowInterstitial(Action onClosed)
    {
        onInterstitialClosed = onClosed;

        if (!Advertisement.isInitialized || !interstitialLoaded)
        {
            Debug.LogWarning("Reklam hazýr deðil, direkt devam ediliyor.");
            onInterstitialClosed?.Invoke();
            onInterstitialClosed = null;
            LoadInterstitial();
            return;
        }

        Advertisement.Show(interstitialAdUnitId, this);
    }

    public void OnInitializationComplete()
    {
        Debug.Log("Unity Ads initialization complete.");
        LoadInterstitial();
    }

    public void OnInitializationFailed(UnityAdsInitializationError error, string message)
    {
        Debug.LogError("Unity Ads initialization failed: " + error + " - " + message);
    }

    public void OnUnityAdsAdLoaded(string adUnitId)
    {
        if (adUnitId == interstitialAdUnitId)
        {
            interstitialLoaded = true;
            Debug.Log("Interstitial reklam yüklendi: " + adUnitId);
        }
    }

    public void OnUnityAdsFailedToLoad(string adUnitId, UnityAdsLoadError error, string message)
    {
        if (adUnitId == interstitialAdUnitId)
        {
            interstitialLoaded = false;
        }

        Debug.LogError("Reklam yüklenemedi: " + adUnitId + " - " + error + " - " + message);
    }

    public void OnUnityAdsShowStart(string adUnitId)
    {
        Debug.Log("Reklam baþladý: " + adUnitId);
    }

    public void OnUnityAdsShowClick(string adUnitId)
    {
        Debug.Log("Reklama týklandý: " + adUnitId);
    }

    public void OnUnityAdsShowComplete(string adUnitId, UnityAdsShowCompletionState showCompletionState)
    {
        Debug.Log("Reklam tamamlandý: " + adUnitId + " - " + showCompletionState);

        if (adUnitId == interstitialAdUnitId)
        {
            interstitialLoaded = false;

            onInterstitialClosed?.Invoke();
            onInterstitialClosed = null;

            LoadInterstitial();
        }
    }

    public void OnUnityAdsShowFailure(string adUnitId, UnityAdsShowError error, string message)
    {
        onInterstitialClosed?.Invoke();
        onInterstitialClosed = null;

        Debug.LogError("Reklam gösterilemedi: " + adUnitId + " - " + error + " - " + message);

        if (adUnitId == interstitialAdUnitId)
        {
            interstitialLoaded = false;
            LoadInterstitial();
        }
    }
}