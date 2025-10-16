mergeInto(LibraryManager.library, {
  // Main API - automatically uses real SDK if available, otherwise uses mock
  loadAdsAsync: function (isRewarded) {
    // Check if real Microsoft Start SDK is available
    var isMSStartAvailable = typeof window.$msstart !== 'undefined' && window.$msstart !== null;
    
    if (isMSStartAvailable) {
      console.log(`[MIG] Loading ${isRewarded ? 'rewarded' : 'interstitial'} ad from real SDK...`);
      
      return window.$msstart.loadAdsAsync(isRewarded)
        .then(adInstance => {
          const gameObjectName = "msstartSDK";
          const methodName = isRewarded ? "OnRewardedLoaded" : "OnInterstitialLoaded";
          
          if (adInstance && adInstance.instanceId) {
            console.log(`[MIG] ${isRewarded ? 'Rewarded' : 'Interstitial'} loaded: ${adInstance.instanceId}`);
            
            // Call Unity callback
            if (typeof unityInstance !== 'undefined') {
              unityInstance.SendMessage(gameObjectName, methodName, adInstance.instanceId);
            } else if (typeof gameInstance !== 'undefined') {
              gameInstance.SendMessage(gameObjectName, methodName, adInstance.instanceId);
            } else {
              console.warn("[MIG] Unity instance not found for callback");
            }
          } else {
            console.warn(`[MIG] ${isRewarded ? 'Rewarded' : 'Interstitial'} ad instance or instanceId is missing`);
          }
          
          return adInstance;
        })
        .catch(error => {
          console.error(`[MIG] Load ${isRewarded ? 'rewarded' : 'interstitial'} error:`, error);
          
          // Call Unity error callback
          const gameObjectName = "msstartSDK";
          const errorMessage = error.message || "Failed to load ad";
          
          if (typeof unityInstance !== 'undefined') {
            unityInstance.SendMessage(gameObjectName, "OnAdError", errorMessage);
          } else if (typeof gameInstance !== 'undefined') {
            gameInstance.SendMessage(gameObjectName, "OnAdError", errorMessage);
          } else {
            console.warn("[MIG] Unity instance not found for callback");
          }

          throw error;
        });
    } else {
      console.warn("[MIG] Real SDK not available, using mock");
      
      // Mock implementation
      return new Promise((resolve) => {
        setTimeout(() => {
          var mockInstanceId = isRewarded ? "rewarded_mock_" + Math.floor(Math.random() * 1000) : "interstitial_mock_" + Math.floor(Math.random() * 1000);
          var adInstance = {
            instanceId: mockInstanceId,
            result: true
          };
          
          const gameObjectName = "msstartSDK";
          const methodName = isRewarded ? "OnRewardedLoaded" : "OnInterstitialLoaded";
          
          // Call Unity callback
          if (typeof unityInstance !== 'undefined') {
            unityInstance.SendMessage(gameObjectName, methodName, adInstance.instanceId);
          } else if (typeof gameInstance !== 'undefined') {
            gameInstance.SendMessage(gameObjectName, methodName, adInstance.instanceId);
          }
          
          resolve(adInstance);
        }, 1000);
      });
    }
  },

  showAdsAsync: function (instanceID_string) {
    const instanceId = UTF8ToString(instanceID_string);
    
    // Check if real Microsoft Start SDK is available
    var isMSStartAvailable = typeof window.$msstart !== 'undefined' && window.$msstart !== null;
    
    if (isMSStartAvailable) {
      return window.$msstart.showAdsAsync(instanceId)
        .then(adInstance => {
          if (adInstance && adInstance.showAdsCompletedAsync) {
            adInstance.showAdsCompletedAsync
              .then(() => {
                const gameObjectName = "msstartSDK";
                const isRewarded = instanceId.includes("rewarded");
                const methodName = isRewarded ? "OnRewardedCompleted" : "OnInterstitialCompleted";
                
                // Call Unity callback
                if (typeof unityInstance !== 'undefined') {
                  if (isRewarded) {
                    unityInstance.SendMessage(gameObjectName, methodName, "true");
                  } else {
                    unityInstance.SendMessage(gameObjectName, methodName, "");
                  }
                } else if (typeof gameInstance !== 'undefined') {
                  if (isRewarded) {
                    gameInstance.SendMessage(gameObjectName, methodName, "true");
                  } else {
                    gameInstance.SendMessage(gameObjectName, methodName, "");
                  }
                }
              })
              .catch(ex => {
                const gameObjectName = "msstartSDK";
                const isRewarded = instanceId.includes("rewarded");
                
                if (isRewarded) {
                  // Rewarded ad was skipped
                  if (typeof unityInstance !== 'undefined') {
                    unityInstance.SendMessage(gameObjectName, "OnRewardedCompleted", "false");
                  } else if (typeof gameInstance !== 'undefined') {
                    gameInstance.SendMessage(gameObjectName, "OnRewardedCompleted", "false");
                  }
                } else {
                  // Interstitial error
                  if (typeof unityInstance !== 'undefined') {
                    unityInstance.SendMessage(gameObjectName, "OnAdError", "Ad skipped or failed");
                  } else if (typeof gameInstance !== 'undefined') {
                    gameInstance.SendMessage(gameObjectName, "OnAdError", "Ad skipped or failed");
                  }
                }
              });
          } else {
            console.warn("[MIG] Ad instance has no showAdsCompletedAsync");
          }
          
          return adInstance;
        })
        .catch(error => {
          console.error("[MIG] Show ad error:", error);
          
          const gameObjectName = "msstartSDK";
          const errorMessage = error.message || "Failed to show ad";
          
          if (typeof unityInstance !== 'undefined') {
            unityInstance.SendMessage(gameObjectName, "OnAdError", errorMessage);
          } else if (typeof gameInstance !== 'undefined') {
            gameInstance.SendMessage(gameObjectName, "OnAdError", errorMessage);
          }
          
          throw error;
        });
    } else {
      console.warn("[MIG] Real SDK not available, using mock");
      
      // Mock implementation
      return new Promise((resolve) => {
        setTimeout(() => {
          const adWasCompleted = Math.random() < 0.8; // 80% completion rate
          const adInstance = {};

          if (adWasCompleted) {
            adInstance.showAdsCompletedAsync = new Promise((resolve) => {
              setTimeout(() => {
                const gameObjectName = "msstartSDK";
                const isRewarded = instanceId.includes("rewarded");
                const methodName = isRewarded ? "OnRewardedCompleted" : "OnInterstitialCompleted";
                
                if (typeof unityInstance !== 'undefined') {
                  if (isRewarded) {
                    unityInstance.SendMessage(gameObjectName, methodName, "true");
                  } else {
                    unityInstance.SendMessage(gameObjectName, methodName, "");
                  }
                } else if (typeof gameInstance !== 'undefined') {
                  if (isRewarded) {
                    gameInstance.SendMessage(gameObjectName, methodName, "true");
                  } else {
                    gameInstance.SendMessage(gameObjectName, methodName, "");
                  }
                }
                
                resolve("completed");
              }, 1000);
            });
          } else {
            adInstance.showAdsCompletedAsync = new Promise((_, reject) => {
              setTimeout(() => {
                const gameObjectName = "msstartSDK";
                const isRewarded = instanceId.includes("rewarded");
                
                if (isRewarded) {
                  if (typeof unityInstance !== 'undefined') {
                    unityInstance.SendMessage(gameObjectName, "OnRewardedCompleted", "false");
                  } else if (typeof gameInstance !== 'undefined') {
                    gameInstance.SendMessage(gameObjectName, "OnRewardedCompleted", "false");
                  }
                }
                
                reject("skipped");
              }, 1000);
            });
          }

          resolve(adInstance);
        }, 800);
      });
    }
  },

  pingAsync: function () {
    return new Promise((resolve) => {
      console.log("[MIG] pingAsync called");
      setTimeout(() => resolve({ result: "pong" }), 500);
    });
  },
});
