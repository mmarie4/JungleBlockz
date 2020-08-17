using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using GoogleMobileAds.Api;
using System;
using UnityEngine.Events;

public class RewardedAdController : MonoBehaviour
{
    public RewardedAd rewardedAd;

    public UnityEvent RewardEarned;
    public UnityEvent AdLoaded;

    public bool loaded;

    // Start is called before the first frame update
    void Start()
    {
        MobileAds.Initialize(initStatus => { });

        RewardEarned = new UnityEvent();
        AdLoaded = new UnityEvent();
        loaded = false;
        
        CreateAndLoadRewardedAd();

        DontDestroyOnLoad(gameObject);
    }

    // Rewarded ad handlers
    public void CreateAndLoadRewardedAd()
    {

        // Test ads
#if UNITY_ANDROID
        string adUnitId = "ca-app-pub-7374363857491670/9314583470";
#elif UNITY_IPHONE
            string adUnitId = "ca-app-pub-7374363857491670/6298829860";
#else
            string adUnitId = "unexpected_platform";
#endif


        // Test id
        if (GameSettings.env.Equals("development")) adUnitId = "ca-app-pub-3940256099942544/5224354917";

        this.rewardedAd = new RewardedAd(adUnitId);
        this.rewardedAd.OnUserEarnedReward += HandleUserEarnedReward;
        this.rewardedAd.OnAdClosed += this.HandleRewardedAdClosed;
        this.rewardedAd.OnAdLoaded += this.HandleRewardedAdLoaded;
        // Create an empty ad request.
        AdRequest request = new AdRequest.Builder().Build();
        // Load the rewarded ad with the request.
        this.rewardedAd.LoadAd(request);
    }
    public void HandleUserEarnedReward(object sender, Reward args)
    {
        RewardEarned.Invoke();
        CreateAndLoadRewardedAd();

    }
    public void HandleRewardedAdClosed(object sender, EventArgs args)
    {
        CreateAndLoadRewardedAd();
    }
    public void HandleRewardedAdLoaded(object sender, EventArgs args)
    {
        loaded = true;
        AdLoaded.Invoke();
    }
}
