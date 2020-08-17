using GoogleMobileAds.Api;
using UnityEngine;
using System;

public class BannerWrapper : MonoBehaviour
{
    public BannerView bannerView;

    public void Start()
    {
        CreateAndLoadBanner();
        DontDestroyOnLoad(gameObject);
    }

    public void CreateAndLoadBanner()
    {
#if UNITY_ANDROID
        // Real ad - For Android release only
        string adUnitId = "ca-app-pub-7374363857491670/6919196877";
#elif UNITY_IPHONE
        string adUnitId = "ca-app-pub-7374363857491670/4751720570";
#else
        string adUnitId = "unexpected_platform";
#endif

        // Test id
        if (GameSettings.env.Equals("development")) adUnitId = "ca-app-pub-3940256099942544/6300978111";

        // Create a 320x50 banner at the bottom of the screen.
        this.bannerView = new BannerView(adUnitId, AdSize.Banner, AdPosition.Bottom);
        this.bannerView.OnAdLoaded += this.HandleOnAdLoaded;
        // Create an empty ad request.
        AdRequest request = new AdRequest.Builder().Build();
        // Load the banner with the request.
        this.bannerView.LoadAd(request);
    }

    public void HandleOnAdLoaded(object sender, EventArgs args)
    {
        this.bannerView.Hide();
    }
}
