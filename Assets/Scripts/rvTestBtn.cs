using UnityEngine;
using System;
public class rvTestBtn : MonoBehaviour
{
    public void OnClick()
    {
        msstartSDK.Instance.ShowRewarded(new Action(() =>
        {
            Debug.Log("Rewarded ad completed - grant reward to player");
        }));
    }
}
