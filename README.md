# Microsoft Start SDK for Unity

A complete Unity integration for Microsoft Start Ads, supporting both Interstitial and Rewarded Video ads with seamless WebGL deployment.

## 🚀 Features

- ✅ **Interstitial Ads** - Full-screen ads between game sessions
- ✅ **Rewarded Video Ads** - Reward players for watching ads
- ✅ **Editor Testing** - Mock ad system for testing without WebGL builds
- ✅ **Automatic Fallback** - Works with or without Microsoft Start SDK
- ✅ **Game State Management** - Auto-pauses game and mutes audio during ads
- ✅ **Callback System** - Flexible Action-based callbacks for ad events
- ✅ **Singleton Pattern** - Easy access from anywhere in your project

---

## 📦 Installation

### 1. Copy Files to Your Project

Copy these files from this SDK into your Unity project:

```
YourProject/
├── Assets/
│   ├── Plugins/
│   │   └── MSSTART.jslib                     # JavaScript bridge
│   ├── Scripts/
│   │   ├── msstartSDK.cs                     # Main SDK
│   │   └── TestBtn.cs (optional)             # Example test button
│   └── WebGLTemplates/
│       └── MSSTART/                          # (Copy entire folder)
│           └── (Template files)              # Microsoft Start template
```

### 2. Configure WebGL Template

1. In **Build Settings** → **Player Settings** → **Resolution and Presentation**
2. Set **WebGL Template** to `MSSTART`
3. Ensure your Microsoft Start SDK initialization is in the template

### 3. Configure Build Settings

1. Go to **File** → **Build Settings**
2. Switch platform to **WebGL**
3. Click **Player Settings**
4. Under **Publishing Settings**, ensure compression is enabled

---

## 🎮 Basic Usage

### Loading Ads

```csharp
using UnityEngine;

public class GameManager : MonoBehaviour
{
    void Start()
    {
        // Load interstitial ad
        msstartSDK.Instance.LoadInterstitial();
        
        // Load rewarded video ad
        msstartSDK.Instance.LoadRewarded();
    }
}
```

### Showing Interstitial Ads

```csharp
public void OnLevelComplete()
{
    if (msstartSDK.Instance.InterstitialReady)
    {
        msstartSDK.Instance.ShowInterstitial();
    }
}
```

### Showing Rewarded Video Ads

```csharp
public void OnWatchAdButton()
{
    if (msstartSDK.Instance.RewardedReady)
    {
        // With callback
        msstartSDK.Instance.ShowRewarded(new Action(() =>
        {
            Debug.Log("Player earned reward!");
            GivePlayerCoins(100);
        }));
    }
}

void GivePlayerCoins(int amount)
{
    // Your reward logic here
}
```

### Checking Ad Availability

```csharp
void Update()
{
    // Enable/disable UI buttons based on ad readiness
    rewardButton.interactable = msstartSDK.Instance.RewardedReady;
    interstitialButton.interactable = msstartSDK.Instance.InterstitialReady;
}
```

---

## 🧪 Testing in Unity Editor

The SDK includes a mock ad system for testing without WebGL builds:

1. **Editor Mode**: Automatically uses mock ads with realistic delays
2. **Mock Completion Rate**: 80% chance to complete (simulates user skip behavior)
3. **Realistic Timing**: 
   - Load delay: 500ms
   - Show delay: 2000ms (simulates ad duration)

### Test Button Example

Attach `TestBtn.cs` to a UI Button to test ads:

```csharp
public class TestBtn : MonoBehaviour
{
    public ButtonType buttonType; // Set in Inspector
    
    // Button automatically enables/disables based on ad readiness
    // Changes color to green when reward is earned
}
```

---

## 🔧 Configuration

### Adjusting Timeout Settings

Edit `msstartSDK.cs` to change load timeout:

```csharp
private const float AD_LOAD_TIMEOUT = 5f; // Seconds before timeout
```

### Customizing Game State Behavior

The SDK automatically:
- Pauses game time (`Time.timeScale = 0`)
- Mutes all audio (`AudioListener.pause = true`)

To customize this behavior, modify the `AdStarted()` and `AdEnded()` methods in `msstartSDK.cs`.

---

## 🌐 Deployment

### Building for WebGL

1. **Build the project**: **File** → **Build Settings** → **Build**
2. Upload the **entire build folder** to your web server

---

## 📋 API Reference

### Properties

| Property | Type | Description |
|----------|------|-------------|
| `Instance` | `msstartSDK` | Singleton instance |
| `InterstitialReady` | `bool` | True if interstitial ad is loaded |
| `RewardedReady` | `bool` | True if rewarded ad is loaded |

### Methods

| Method | Parameters | Description |
|--------|------------|-------------|
| `LoadInterstitial()` | None | Loads an interstitial ad |
| `LoadRewarded()` | None | Loads a rewarded video ad |
| `ShowInterstitial()` | None | Shows loaded interstitial ad |
| `ShowRewarded()` | `object reward` (optional) | Shows rewarded ad with optional callback |

### Events

| Event | Description |
|-------|-------------|
| `OnRewardPlayer` | Fired when player completes rewarded ad |
| `OnAdStarted` | Fired when any ad starts playing |
| `OnAdEnded` | Fired when any ad ends |

### Callback Pattern

Pass an `Action` to `ShowRewarded()` for per-ad callbacks:

```csharp
msstartSDK.Instance.ShowRewarded(new Action(() => {
    // This runs only if player completes the ad
    Debug.Log("Reward granted!");
}));
```

---

## 🐛 Troubleshooting

### Ads Not Loading in WebGL Build

1. Check browser console for errors
2. Verify Microsoft Start SDK is loaded in your WebGL template
3. Ensure your domain is authorized in Microsoft Start dashboard
4. Test with mock ads first (see Editor testing)

### GameObject Name Mismatch Error

The SDK GameObject **must** be named `"msstartSDK"` (case-sensitive). This happens automatically via `Awake()`:

```csharp
gameObject.name = "msstartSDK"; // Required for JS bridge
```

Don't rename the GameObject manually.

### Callbacks Not Executing

Ensure you're passing an `Action`, not just any object:

```csharp
// ✅ Correct
msstartSDK.Instance.ShowRewarded(new Action(() => {
    // Your code
}));

// ❌ Wrong
msstartSDK.Instance.ShowRewarded("some string");
```

### Ads Playing But Game Not Pausing

Verify `Time.timeScale` and `AudioListener.pause` restore correctly:
- Check `AdStarted()` and `AdEnded()` methods
- Ensure no other scripts override these settings

---

## 📝 Example: Complete Reward Shop

```csharp
using UnityEngine;
using UnityEngine.UI;

public class RewardShop : MonoBehaviour
{
    [Header("UI References")]
    public Button watchAdButton;
    public Text coinText;
    
    private int coins = 0;
    
    void Start()
    {
        UpdateUI();
        msstartSDK.Instance.LoadRewarded(); // Preload ad
    }
    
    void Update()
    {
        // Enable button only when ad is ready
        watchAdButton.interactable = msstartSDK.Instance.RewardedReady;
    }
    
    public void OnWatchAdClicked()
    {
        msstartSDK.Instance.ShowRewarded(new Action(() =>
        {
            // Grant reward
            coins += 100;
            UpdateUI();
            
            // Reload next ad
            msstartSDK.Instance.LoadRewarded();
            
            Debug.Log("Player earned 100 coins!");
        }));
    }
    
    void UpdateUI()
    {
        coinText.text = $"Coins: {coins}";
    }
}
```

---

## 🏗️ Architecture

### Component Overview

```
┌─────────────────────────────────────────┐
│           Unity C# Layer                │
│  ┌───────────────────────────────────┐  │
│  │     msstartSDK.cs (Singleton)     │  │
│  │  - Ad Loading/Showing             │  │
│  │  - State Management               │  │
│  │  - Callback Execution             │  │
│  └───────────────────────────────────┘  │
└──────────────┬──────────────────────────┘
               │ DllImport
┌──────────────▼──────────────────────────┐
│      JavaScript Bridge Layer            │
│  ┌───────────────────────────────────┐  │
│  │      MSSTART.jslib                │  │
│  │  - SDK Detection                  │  │
│  │  - Mock Fallback                  │  │
│  │  - SendMessage to Unity           │  │
│  └───────────────────────────────────┘  │
└──────────────┬──────────────────────────┘
               │ window.$msstart
┌──────────────▼──────────────────────────┐
│      Microsoft Start SDK (External)     │
│  - Ad Serving                           │
│  - Analytics                            │
│  - Monetization                         │
└─────────────────────────────────────────┘
```

### Data Flow

1. **Unity** calls `LoadInterstitial()` → `DllImport` → **JavaScript**
2. **JavaScript** checks for `window.$msstart` SDK
3. If available: Real SDK loads ad
4. If unavailable: Mock system simulates ad
5. **JavaScript** calls `SendMessage("msstartSDK", "OnInterstitialLoaded", instanceId)`
6. **Unity** receives callback, stores `instanceId`, sets `InterstitialReady = true`
7. User calls `ShowInterstitial()` → **JavaScript** shows ad
8. Ad completes → **JavaScript** sends completion callback → **Unity** executes stored `Action`

---

## 🔐 Best Practices

### 1. Preload Ads Early
```csharp
// Load ads at game start or level load
void Start()
{
    msstartSDK.Instance.LoadInterstitial();
    msstartSDK.Instance.LoadRewarded();
}
```

### 2. Check Ad Readiness
```csharp
// Always check before showing
if (msstartSDK.Instance.RewardedReady)
{
    msstartSDK.Instance.ShowRewarded(callback);
}
```

### 3. Reload After Showing
```csharp
// Ads are single-use, reload for next time
msstartSDK.Instance.ShowRewarded(new Action(() =>
{
    GiveReward();
    msstartSDK.Instance.LoadRewarded(); // Preload next ad
}));
```

### 4. Handle Timeouts
```csharp
// SDK has built-in 5-second timeout
// Provide user feedback if ad fails to load
if (!msstartSDK.Instance.RewardedReady)
{
    ShowMessage("No ads available, try again later");
}
```

### 5. Test in Editor First
```csharp
// Use mock system to verify game logic
// Then test in WebGL build with real SDK
#if UNITY_EDITOR
    Debug.Log("Using mock ads in Editor");
#endif

```

## 🆘 Support

For issues or questions:
1. Check the **Troubleshooting** section
2. Review browser console logs in WebGL builds
3. Verify Microsoft Start SDK is properly initialized
4. Refer to the [MSSTART SDK DOCUMENTATION](https://msstart-games-sdk-doc.azurewebsites.net/docs/sdk-apis/#rewarded-ad)


**You're ready to monetize your Unity WebGL game! 🚀**
