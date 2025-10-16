using UnityEngine;
using UnityEngine.UI;
using System;

public class TestBtn : MonoBehaviour
{
    private enum BtnTypes { Interstitial, Rewarded };
    [SerializeField] private BtnTypes btnType;
    private Button button;

    void Start()
    {
        button = gameObject.GetComponent<Button>();
    }
    void Update()
    {
        if (msstartSDK.Instance == null) return;
        if (btnType == BtnTypes.Interstitial)
            button.interactable = msstartSDK.Instance.InterstitialReady;
        else if (btnType == BtnTypes.Rewarded)
        {
            button.interactable = msstartSDK.Instance.RewardedReady;
            Image img = gameObject.GetComponent<Image>();
                img.color = Color.Lerp(img.color, Color.white, Time.deltaTime * 5f);
        }
    }


    public void RequestReward()
    {
        msstartSDK.Instance.ShowRewarded(new Action(() =>
        {
            Debug.Log("Rewarded ad completed - grant reward to player");
            gameObject.GetComponent<Image>().color = Color.green;
        }));
    }
}
