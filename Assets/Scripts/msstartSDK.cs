using UnityEngine;
using System;
using System.Collections;
using System.Runtime.InteropServices;

public class msstartSDK : MonoBehaviour
{
    #region WebGL External Calls
    [DllImport("__Internal")]
    private static extern void pingAsync();
    
    [DllImport("__Internal")]
    private static extern void loadAdsAsync(bool isRewarded);
    
    [DllImport("__Internal")]
    private static extern void showAdsAsync(string instanceId);
    #endregion

    #region Constants
    private const float AD_LOAD_TIMEOUT = 5.0f;
    private const string LOG_PREFIX = "[MIG]";
    
    // Editor mock settings
    private const float MOCK_AD_LOAD_TIME = 1.0f;
    private const float MOCK_INTERSTITIAL_DURATION = 2.0f;
    private const float MOCK_REWARDED_DURATION = 3.0f;
    private const float MOCK_REWARDED_COMPLETION_RATE = 1.0f; // 100% chance of completion
    #endregion

    #region Ad State Variables
    private string interstitialInstance = "";
    private string rewardedInstance = "";
    
    private bool loadingInterstitial = false;
    private bool loadingRewarded = false;
    
    private bool adQueued = false;
    #endregion

    #region Game State Backup
    private float beforeAdVolume = 1.0f;
    private float beforeAdTimeScale = 1.0f;
    private bool beforeAdCalled = false;
    #endregion

    #region Public Properties
    public bool IsAdsAllowed { get; set; } = true;
    
    public bool InterstitialReady => !string.IsNullOrEmpty(interstitialInstance);
    public bool RewardedReady => !string.IsNullOrEmpty(rewardedInstance);
    public bool RvReady => RewardedReady;
    #endregion

    #region Events
    public static event Action OnAdStarted;
    public static event Action OnAdEnded;
    public static event Action<object> OnRewardPlayer;
    #endregion

    #region Unity Lifecycle
    public static msstartSDK Instance;

    void Awake()
    {
        if (Instance == null)
        {
            Instance = this;
            DontDestroyOnLoad(gameObject);
        }
        else if (Instance != null && Instance != this)
        {
            Destroy(this);
        }
    }

    void Start()
    {
        DontDestroyOnLoad(gameObject);
        Initialize();
    }

    void Update()
    {
        if (!IsAdsAllowed)
        {
            return;
        }

        // Auto-load ads when they're not ready
        if (!RewardedReady)
        {
            LoadRewarded();
        }
        
        if (!InterstitialReady)
        {
            LoadInterstitial();
        }
    }
    #endregion

    #region Initialization
    private void Initialize()
    {
        Debug.Log($"{LOG_PREFIX} Microsoft Ads SDK initialized");
    }
    #endregion

    #region Public API - Show Ads
    public void ShowInterstitial()
    {
        if (!CanShowAd(interstitialInstance, "Interstitial"))
        {
            return;
        }

        adQueued = true;
        AdStarted();
        StartCoroutine(ShowInterstitialCoroutine());
    }

    public void ShowRewarded(object reward = null)
    {
        if (!CanShowAd(rewardedInstance, "Rewarded"))
        {
            return;
        }

        adQueued = true;
        AdStarted();
        StartCoroutine(ShowRewardedCoroutine(reward));
    }
    #endregion

    #region Ad Validation
    private bool CanShowAd(string instanceId, string adType)
    {
        if (adQueued)
        {
            Debug.LogError($"{LOG_PREFIX} Ad already queued.");
            return false;
        }
        
        if (!IsAdsAllowed)
        {
            Debug.LogError($"{LOG_PREFIX} Ads aren't allowed.");
            return false;
        }
        
        if (string.IsNullOrEmpty(instanceId))
        {
            Debug.LogError($"{LOG_PREFIX} No {adType} instance available.");
            return false;
        }

        return true;
    }
    #endregion

    #region Ad Loading
    private void LoadInterstitial()
    {
        if (loadingInterstitial)
        {
            return;
        }
        
        loadingInterstitial = true;
        StartCoroutine(LoadAdCoroutine(false));
    }

    private void LoadRewarded()
    {
        if (loadingRewarded)
        {
            return;
        }
        
        loadingRewarded = true;
        StartCoroutine(LoadAdCoroutine(true));
    }

    private IEnumerator LoadAdCoroutine(bool isRewarded)
    {
        string adType = isRewarded ? "rewarded" : "interstitial";
        Debug.Log($"{LOG_PREFIX} Loading {adType} ad...");
        
        #if UNITY_WEBGL && !UNITY_EDITOR
        loadAdsAsync(isRewarded);
        
        float timer = 0f;
        
        // Wait for callback from JavaScript or timeout
        while (timer < AD_LOAD_TIMEOUT)
        {
            timer += Time.unscaledDeltaTime;
            
            // Check if ad was loaded via callback
            string currentInstance = isRewarded ? rewardedInstance : interstitialInstance;
            if (!string.IsNullOrEmpty(currentInstance))
            {
                Debug.Log($"{LOG_PREFIX} {adType} loaded successfully: {currentInstance}");
                if (isRewarded)
                    loadingRewarded = false;
                else
                    loadingInterstitial = false;
                yield break;
            }
            
            yield return null;
        }
        
        // Timeout
        Debug.LogWarning($"{LOG_PREFIX} {adType} load timed out.");
        if (isRewarded)
        {
            rewardedInstance = "";
            loadingRewarded = false;
        }
        else
        {
            interstitialInstance = "";
            loadingInterstitial = false;
        }
        #else
        // Mock ad loading in Editor
        yield return new WaitForSecondsRealtime(MOCK_AD_LOAD_TIME);
        
        string mockInstanceId = $"{adType}_{UnityEngine.Random.Range(100, 999)}";
        
        if (isRewarded)
        {
            OnRewardedLoaded(mockInstanceId);
        }
        else
        {
            OnInterstitialLoaded(mockInstanceId);
        }
        
        Debug.Log($"{LOG_PREFIX} [Editor Mock] {adType} loaded successfully: {mockInstanceId}");
        #endif
    }
    #endregion

    #region Show Ad Coroutines
    private IEnumerator ShowInterstitialCoroutine()
    {
        #if UNITY_WEBGL && !UNITY_EDITOR
        showAdsAsync(interstitialInstance);
        yield return null;
        #else
        // Mock interstitial ad in Editor
        Debug.Log($"{LOG_PREFIX} [Editor Mock] Showing interstitial ad...");
        Debug.Log($"{LOG_PREFIX} [Editor Mock] Game paused - Ad playing for {MOCK_INTERSTITIAL_DURATION}s");
        
        yield return new WaitForSecondsRealtime(MOCK_INTERSTITIAL_DURATION);
        
        Debug.Log($"{LOG_PREFIX} [Editor Mock] Interstitial ad completed");
        OnInterstitialCompleted();
        #endif
    }

    private IEnumerator ShowRewardedCoroutine(object reward)
    {
        #if UNITY_WEBGL && !UNITY_EDITOR
        showAdsAsync(rewardedInstance);
        yield return null;
        #else
        // Mock rewarded ad in Editor
        Debug.Log($"{LOG_PREFIX} [Editor Mock] Showing rewarded ad...");
        Debug.Log($"{LOG_PREFIX} [Editor Mock] Game paused - Ad playing for {MOCK_REWARDED_DURATION}s");
        
        yield return new WaitForSecondsRealtime(MOCK_REWARDED_DURATION);
        
        // Simulate random completion or skip
        bool adCompleted = UnityEngine.Random.Range(0f, 1f) < MOCK_REWARDED_COMPLETION_RATE;
        
        if (adCompleted)
        {
            Debug.Log($"{LOG_PREFIX} [Editor Mock] Rewarded ad completed - User watched full ad");
            OnRewardedCompleted(true, reward);
        }
        else
        {
            Debug.Log($"{LOG_PREFIX} [Editor Mock] Rewarded ad skipped - User closed early");
            OnRewardedCompleted(false, reward);
        }
        #endif
    }
    #endregion

    #region Game State Management
    private void AdStarted()
    {
        // Backup current game state
        beforeAdVolume = AudioListener.volume;
        beforeAdTimeScale = Time.timeScale;
        
        // Pause game during ad
        AudioListener.volume = 0f;
        Time.timeScale = 0f;
        
        beforeAdCalled = true;
        OnAdStarted?.Invoke();
        
        Debug.Log($"{LOG_PREFIX} Ad started - Game paused");
    }

    private void AdEnded()
    {
        adQueued = false;
        
        if (beforeAdCalled)
        {
            // Restore game state
            AudioListener.volume = beforeAdVolume;
            Time.timeScale = beforeAdTimeScale;
            beforeAdCalled = false;
        }
        
        OnAdEnded?.Invoke();
        Debug.Log($"{LOG_PREFIX} Ad ended - Game resumed");
    }
    #endregion

    #region JavaScript Callbacks (Called from WebGL)
    // Callback invoked from JavaScript when an interstitial ad is successfully loaded
    // Do not call manually - this is invoked automatically by the WebGL bridge
    public void OnInterstitialLoaded(string instanceId)
    {
        interstitialInstance = instanceId;
        loadingInterstitial = false;
        Debug.Log($"{LOG_PREFIX} Interstitial loaded: {instanceId}");
    }

    // Callback invoked from JavaScript when a rewarded ad is successfully loaded
    // Do not call manually - this is invoked automatically by the WebGL bridge
    public void OnRewardedLoaded(string instanceId)
    {
        rewardedInstance = instanceId;
        loadingRewarded = false;
        Debug.Log($"{LOG_PREFIX} Rewarded loaded: {instanceId}");
    }

    // Callback invoked from JavaScript when an interstitial ad finishes playing
    // Do not call manually - this is invoked automatically by the WebGL bridge
    public void OnInterstitialCompleted()
    {
        AdEnded();
        Debug.Log($"{LOG_PREFIX} Interstitial completed");
        interstitialInstance = "";
    }
    
    // Overload to handle string parameter from JavaScript (not used but needed for compatibility)
    public void OnInterstitialCompleted(string unused)
    {
        OnInterstitialCompleted();
    }

    // Callback invoked from JavaScript when a rewarded ad finishes or is skipped
    // Do not call manually - this is invoked automatically by the WebGL bridge
    // JavaScript sends "true" or "false" as string
    public void OnRewardedCompleted(string shouldRewardStr)
    {
        bool shouldReward = shouldRewardStr == "true";
        
        AdEnded();
        
        if (shouldReward)
        {
            Debug.Log($"{LOG_PREFIX} Rewarded completed - Player rewarded");
            OnRewardPlayer?.Invoke(null);
        }
        else
        {
            Debug.Log($"{LOG_PREFIX} Rewarded skipped");
        }
        
        rewardedInstance = "";
    }
    
    // Overload for direct bool calls (used by editor mock)
    public void OnRewardedCompleted(bool shouldReward, object reward = null)
    {
        AdEnded();
        
        if (shouldReward)
        {
            Debug.Log($"{LOG_PREFIX} Rewarded completed - Player rewarded");
            OnRewardPlayer?.Invoke(reward);
        }
        else
        {
            Debug.Log($"{LOG_PREFIX} Rewarded skipped");
        }
        
        rewardedInstance = "";
    }

    // Callback invoked from JavaScript when an ad fails to load or play
    // Do not call manually - this is invoked automatically by the WebGL bridge
    public void OnAdError(string error)
    {
        AdEnded();
        Debug.LogError($"{LOG_PREFIX} Ad error: {error}");
        interstitialInstance = "";
        rewardedInstance = "";
        loadingInterstitial = false;
        loadingRewarded = false;
    }
    #endregion
}
