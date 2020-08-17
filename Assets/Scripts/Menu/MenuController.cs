using System;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;
using UnityEngine.Networking;
using UnityEngine.SceneManagement;
using UnityEngine.UI;

public class MenuController : MonoBehaviour
{

    #region variables
    public Slider expBar;
    public Text expText;
    public Text level;
    public Text runes;
    public GameObject SettingsPopup;
    public Slider sfxSlider;
    public Slider musicSlider;
    public GameObject UpdatePopup;
    PlayerStats playerStats;

    private RewardedAdController rewardedAdController;

    private bool canWatch;

    public GameObject watchBtn;
    public GameObject loadingTxt;
    public Text timer;
    public GameObject skillPointsNotifier;
    public Sprite checkedSprite;
    public GameObject notificationPopup;

    private BossProgressions progressions;
    #endregion

    // Start is called before the first frame update
    void Start()
    {
        AkSoundEngine.SetState("GameMod_states", "Menu_state");
        playerStats = SaveHandler.Load();
        expBar.maxValue = (int)playerStats.GetRequiredExp();
        expBar.value = (int)playerStats.GetExperience();
        expText.text = ((int)playerStats.GetExperience()).ToString() + " / " + ((int)playerStats.GetRequiredExp()).ToString();
        level.text = playerStats.GetLevel().ToString();
        runes.text = playerStats.GetRunes().ToString();
        SettingsPopup.SetActive(false);
        notificationPopup.SetActive(false);
        sfxSlider.value = PlayerPrefs.GetFloat("SFXVolume", 50);
        musicSlider.value = PlayerPrefs.GetFloat("MusicVolume", 100);
        AkSoundEngine.SetRTPCValue("SFXVolume", sfxSlider.value);
        AkSoundEngine.SetRTPCValue("MusicVolume", musicSlider.value);
        musicSlider.onValueChanged.AddListener(delegate { SetMusicValue(); });
        sfxSlider.onValueChanged.AddListener(delegate { SetSFXValue(); });

        if (playerStats.GetSkillPoints() <= 0) skillPointsNotifier.SetActive(false);

        if (GameSettings.platform == "phone")
        {
            rewardedAdController = GameObject.Find("RewardedAdController").GetComponent<RewardedAdController>();
            rewardedAdController.RewardEarned.AddListener(HandleRewardEarned);
            rewardedAdController.AdLoaded.AddListener(HandleAdLoaded);
            watchBtn.SetActive(rewardedAdController.loaded);
            loadingTxt.SetActive(!rewardedAdController.loaded);
            TimeSpan timeSinceLastSeen = DateTime.UtcNow.Subtract(playerStats.GetLastAdSeen());
            canWatch = playerStats.GetNumberOfAds() == 0 || timeSinceLastSeen.TotalSeconds > GameSettings.GetTimeBetweenRewards();
        }

        /* Map UI to progressions */
        /* BossProgressions include medium, hard, extreme levels because they have to be unlocked too so we can use the same collection */
        progressions = SaveHandler.LoadBossProgressions();
        List<string> levels = new List<string>() { "explore-medium", "explore-hard", "explore-extreme", "necromancer", "thundergod", "illusionist" };
        foreach (string level in levels)
        {
            /* Check if level is already unlocked */
            if (progressions.UnlockedBosses.Contains(level))
            {
                GameObject.Find(level + "Lock").SetActive(false);
                GameObject.Find(level + "LockMessage").SetActive(false);
            }

            /* Check if levels need to be unlocked */
            else
            {
                if (level == "explore-medium" && playerStats.GetLevel() >= 3)
                {
                    progressions.UnlockedBosses.Add("explore-medium");
                    SaveHandler.SaveBossProgressions(progressions);
                    GameObject.Find(level + "Lock").SetActive(false);
                    GameObject.Find(level + "LockMessage").SetActive(false);
                    ShowNotification("New Level Unlocked: Medium !", "New enemy : Kamikaz\nExperience and runes earned in this level are multiplied by 1.5 !");
                }
                if (level == "explore-hard" && playerStats.GetLevel() >= 8)
                {
                    progressions.UnlockedBosses.Add("explore-hard");
                    SaveHandler.SaveBossProgressions(progressions);
                    GameObject.Find(level + "Lock").SetActive(false);
                    GameObject.Find(level + "LockMessage").SetActive(false);
                    ShowNotification("New Level Unlocked: Hard !", "New enemy : Fatboy\nExperience and runes earned in this level are multiplied by 2 !");
                }
                if (level == "explore-extreme" && playerStats.GetLevel() >= 15)
                {
                    progressions.UnlockedBosses.Add("explore-extreme");
                    SaveHandler.SaveBossProgressions(progressions);
                    GameObject.Find(level + "Lock").SetActive(false);
                    GameObject.Find(level + "LockMessage").SetActive(false);
                    ShowNotification("New Level Unlocked: Extreme !", "New enemy : Bird\nExperience and runes earned in this level are multiplied by 2.5 !");
                }
                if (level == "necromancer")
                {
                    if (playerStats.GetLevel() >= 20)
                    {
                        progressions.UnlockedBosses.Add("necromancer");
                        SaveHandler.SaveBossProgressions(progressions);
                        GameObject.Find(level + "Lock").SetActive(false);
                        GameObject.Find(level + "LockMessage").SetActive(false);
                        ShowNotification("Boss Level Unlocked : Necromancer !", "Beat the boss to earn a special reward, and find the hidden runes in the level");
                    }
                    else
                    {
                        GameObject.Find(level + "Checked").SetActive(false);
                    }
                }
                if (level == "thundergod")
                {
                    if (playerStats.GetLevel() >= 30 && progressions.UnlockedBosses.Contains("necromancer"))
                    {
                        progressions.UnlockedBosses.Add("thundergod");
                        SaveHandler.SaveBossProgressions(progressions);
                        GameObject.Find(level + "Lock").SetActive(false);
                        GameObject.Find(level + "LockMessage").SetActive(false);
                        ShowNotification("Boss Level Unlocked : Thundergod !", "Beat the boss to earn a special reward, and find the hidden runes in the level");
                    }
                    else
                    {
                        GameObject.Find(level + "Checked").SetActive(false);
                    }
                }
                if (level == "illusionist")
                {
                    if (playerStats.GetLevel() >= 40 && progressions.UnlockedBosses.Contains("thundergod"))
                    {
                        progressions.UnlockedBosses.Add("illusionist");
                        SaveHandler.SaveBossProgressions(progressions);
                        GameObject.Find(level + "Lock").SetActive(false);
                        GameObject.Find(level + "LockMessage").SetActive(false);
                        ShowNotification("Boss Level Unlocked : Illusionist !", "Beat the boss to earn a special reward, and find the hidden runes in the level");
                    }
                    else
                    {
                        GameObject.Find(level + "Checked").SetActive(false);
                    }
                }
            }

            /* Handle checked icon and runes founds icon for bosses */
            if (level != "explore-medium" && level != "explore-hard" && level != "explore-extreme")
            {
                GameObject checkedImg = GameObject.Find(level + "Checked");
                BossProgression progression = progressions.List.FirstOrDefault(p => p.Id == level);
                if ((progression == null || !progression.IsPassed) && checkedImg != null) checkedImg.SetActive(false);
                GameObject runeImg = GameObject.Find(level + "RunesPackImg");
                if (progression != null && progression.RunesFound) runeImg.GetComponent<Image>().sprite = checkedSprite;
            }
        }

        /* Show update popup is version is not the latest - Application version has to be X.X to work */
        UpdatePopup.SetActive(false);
        if (!GameSettings.updatePopupSeen) StartCoroutine(GetVersion());

        /* Show notification if level up */
        if (GameStats.levelUp) ShowNotification("Level " + playerStats.GetLevel() + " Reached !", "Your earnt a Skill Point. Go to STATS to spend it and increase your character's skills");

        if (PlayerPrefs.GetInt("FirstLogin", 0) == 0)
        {
            ShowNotification("Welcome !", "- Fight in the EXPLORE MODE to get runes and experience\n- Equip weapons and blockz in the ITEMS menu\n- Gain experience to unlock more levels and get skill points\n- Unlock the BOSS FIGHTS to earn special rewards and challenge your skills");
            PlayerPrefs.SetInt("FirstLogin", 1);
        }

        /* HACK */
        /*
        playerStats.AddExp(100);
        SaveHandler.Save(playerStats);
        */
    }

    // Update is called once per frame
    void Update()
    {
        TimeSpan timeSinceLastSeen = DateTime.UtcNow.Subtract(playerStats.GetLastAdSeen());
        if (playerStats.GetNumberOfAds() == 0 || timeSinceLastSeen.TotalSeconds > GameSettings.GetTimeBetweenRewards())
        {
            if (!canWatch)
            {
                canWatch = true;
                timer.text = "";
            }
        }
        else
        {
            TimeSpan timeBeforeAvailable = TimeSpan.FromSeconds(GameSettings.GetTimeBetweenRewards() - (int)(timeSinceLastSeen.TotalSeconds));
            timer.text = string.Format("Available in {0:D2}:{1:D2}:{2:D2}", timeBeforeAvailable.Hours, timeBeforeAvailable.Minutes, timeBeforeAvailable.Seconds);
        }
    }

    // HTTP GET
    private IEnumerator<UnityWebRequestAsyncOperation> GetVersion()
    {
        UnityWebRequest www = UnityWebRequest.Get("http://51.38.68.118:10100/version");
        yield return www.SendWebRequest();

        if (www.isNetworkError || www.isHttpError)
        {
            Debug.Log(www.error);
        }
        else
        {
            double fetchedVersion = double.Parse(www.downloadHandler.text);
            UpdatePopup.SetActive(double.Parse(Application.version) < fetchedVersion);
            GameSettings.updatePopupSeen = true;
        }
    }

    // Buttons handlers
    public void ToggleMenu() => SettingsPopup.SetActive(!SettingsPopup.activeSelf);
    public void Save()
    {
        PlayerPrefs.SetFloat("SFXVolume", sfxSlider.value);
        PlayerPrefs.SetFloat("MusicVolume", musicSlider.value);
        SettingsPopup.SetActive(false);
    }
    public void Quit()
    {
        Application.Quit();
    }
    public void SetDifficulty(int d)
    {
        string level = d == 0 ? "explore-easy" : d == 1 ? "explore-medium" : d == 2 ? "explore-hard" : "explore-extreme";
        if (level.Equals("explore-easy") || progressions.UnlockedBosses.Contains(level))
        {
            GameSettings.SetDifficulty(d);
            GameStats.type = "explore";
            GameStats.bossId = "";
            SceneManager.LoadScene("LoadingScene");
            AkSoundEngine.SetState("GameMod_states", "Nothing_state");
        }
    }
    public void GoToStats() { SceneManager.LoadScene("StatsScene"); }
    public void GoToItems() { SceneManager.LoadScene("ItemsScene"); }
    public void GoToBoss(string bossId)
    {
        if (progressions.UnlockedBosses.Contains(bossId))
        {
            GameStats.type = "boss";
            GameStats.bossId = bossId;
            SceneManager.LoadScene("LoadingScene");
            AkSoundEngine.SetState("GameMod_states", "Nothing_state");
        }
    }
    public void CloseUpdatePopup()
    {
        UpdatePopup.SetActive(!UpdatePopup.activeSelf);
        GameSettings.updatePopupSeen = true;
    }
    public void LinkToStore()
    {
        if (GameSettings.platform.Equals("android")) Application.OpenURL("market://details?id=com.TheMaskProduction.JungleBlockz");
        if (GameSettings.platform.Equals("iphone")) Application.OpenURL("itms-apps://apps.apple.com/app/id1527852358");
#if UNITY_ANDROID
        Application.OpenURL("market://details?id=com.TheMaskProduction.JungleBlockz");
#elif UNITY_IPHONE
        Application.OpenURL("itms-apps://itunes.apple.com/us/developer/xxx"); // TODO: change to apple store link
#else
        // do nothing
#endif
    }
    public void SetMusicValue()
    {
        AkSoundEngine.SetRTPCValue("MusicVolume", musicSlider.value);
    }
    public void SetSFXValue()
    {
        AkSoundEngine.SetRTPCValue("SFXVolume", sfxSlider.value);
    }

    private void ShowNotification(string headline, string text)
    {
        notificationPopup.GetComponentsInChildren<Text>()[0].text = headline;
        notificationPopup.GetComponentsInChildren<Text>()[1].text = text;
        notificationPopup.SetActive(true);
    }
    public void CloseNotification()
    {
        notificationPopup.SetActive(false);
    }

    /* Ad handlers */
    public void OpenAd()
    {
        if (!canWatch) return;
        if (rewardedAdController.rewardedAd.IsLoaded())
        {
            rewardedAdController.rewardedAd.Show();
        }
    }
    public void HandleRewardEarned()
    {
        playerStats.SeeAd();
        SaveHandler.Save(playerStats);
        canWatch = false;
        runes.text = playerStats.GetRunes().ToString();
        ShowNotification("20 runes earnt !", "Go to ITEMS to spend your runes and unlock new weapons and blockz");

    }
    public void HandleAdLoaded()
    {
        watchBtn.SetActive(true);
        loadingTxt.SetActive(false);
    }


}
