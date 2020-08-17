using UnityEngine.UI;
using UnityEngine;
using System.Linq;
using GoogleMobileAds.Api;
using UnityEngine.Analytics;

public class GameOverController : MonoBehaviour
{
    public Text survival;
    public Text survivalLabel;
    public Text kills;
    public Text killsLabel;
    public Text expText;
    public Text runes;
    public Text multiplier;
    public Text multiplierLabel;
    public Text title;
    public GameObject notificationPopup;

    private BannerWrapper bannerWrapper;

    void Start()
    {
        notificationPopup.SetActive(false);
        try
        {
            GameObject myGameObject = GameObject.Find("BannerWrapper");
            bannerWrapper = myGameObject.GetComponent<BannerWrapper>();
            bannerWrapper.bannerView.Show();
        }
        catch { }

        PlayerStats playerStats = SaveHandler.Load();

        int levelBeforeComputingStats = playerStats.GetLevel();

        /* boss fights results */
        if (GameStats.type.Equals("boss"))
        {
            multiplier.text = "";
            multiplierLabel.text = "";
            kills.text = "";
            killsLabel.text = "";
            survival.text = "";
            survivalLabel.text = "";


            BossProgressions listProgressions = SaveHandler.LoadBossProgressions();
            BossProgression progress = listProgressions.List.FirstOrDefault(p => p.Id == GameStats.bossId);

            if (GameStats.isWon)
            {
                title.text = "YOU WON !";
                float experience = GameSettings.GetBossXPReward(GameStats.bossId);
                int rewardRunes = GameSettings.GetBossRunesReward(GameStats.bossId);
                if (progress == null || !progress.IsPassed)
                {
                    expText.text = experience.ToString();
                    playerStats.AddExp(experience);
                    runes.text = rewardRunes + " + " + GameStats.runes.ToString();
                    playerStats.AddRunes(GameStats.runes + rewardRunes);
                }
            }
            else if (GameStats.runes > 0)
            {
                runes.text = GameStats.runes.ToString();
                playerStats.AddRunes(GameStats.runes);
            }

            if (progress == null)
            {
                listProgressions.List.Add(new BossProgression() { Id = GameStats.bossId, IsPassed = GameStats.isWon, RunesFound = GameStats.runes > 0 });
            }
            else
            {
                progress.IsPassed = progress.IsPassed || GameStats.isWon;
                progress.RunesFound = progress.RunesFound || GameStats.runes > 0;
            }

            SaveHandler.SaveBossProgressions(listProgressions);

        }
        else
        /* explore mode result */
        {
            survival.text = string.Format("{0:0} min {1:00} sec.", Mathf.Floor(GameStats.survival / 60), GameStats.survival % 60);
            kills.text = GameStats.kills.ToString();
            multiplier.text = GameSettings.GetMultiplicator().ToString();
            runes.text = ((int)(GameStats.runes * GameSettings.GetMultiplicator())).ToString();
            float experience = (GameStats.kills + GameStats.survival / 10) * GameSettings.GetMultiplicator();
            expText.text = ((int)experience).ToString();
            playerStats.AddExp(experience);
            playerStats.AddRunes((int)(GameStats.runes * GameSettings.GetMultiplicator()));
        }

        if (playerStats.GetLevel() > levelBeforeComputingStats) GameStats.levelUp = true;

        SaveHandler.Save(playerStats);
        if (GameStats.levelUp) ShowNotification("Level " + playerStats.GetLevel() + " Reached !", "Your earnt a Skill Point. Go to STATS to spend it and increase your character's skills");

    }

    private void ShowNotification(string headline, string text)
    {
        notificationPopup.GetComponentsInChildren<Text>()[0].text = headline;
        notificationPopup.GetComponentsInChildren<Text>()[1].text = text;
        notificationPopup.SetActive(true);
        GameStats.levelUp = false;
    }

    public void CloseNotifications()
    {
        notificationPopup.SetActive(false);
    }

    void OnDestroy()
    {
        bannerWrapper.bannerView.Hide();
        bannerWrapper.bannerView.Destroy();
        bannerWrapper.CreateAndLoadBanner();
    }
}
