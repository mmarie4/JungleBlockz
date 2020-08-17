using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class StatsController : MonoBehaviour
{
    // UI
    public Slider expBar;
    public Text expText;
    public Text runesText;
    public Text levelText;
    public Button strengthBtn;
    public Button agilityBtn;
    public Button enduranceBtn;
    public Button dexterityBtn;
    public Button intelligenceBtn;
    public Text intelligenceText;
    public Text strengthText;
    public Text dexterityText;
    public Text agilityText;
    public Text enduranceText;
    public Text skillPoints;
    public Text computedDamage;
    public Text computedSpeed;
    public Text computedAtkSpeed;
    public Text computedCritRatio;
    public Text computedHP;

    private PlayerStats playerStats;

    // Start is called before the first frame update
    void Start()
    {
        playerStats = SaveHandler.Load();
        expBar.maxValue = (int)playerStats.GetRequiredExp();
        expBar.value = (int)playerStats.GetExperience();
        levelText.text = playerStats.GetLevel().ToString();
        expText.text = ((int)playerStats.GetExperience()).ToString() + " / " + ((int)playerStats.GetRequiredExp()).ToString();
        Refresh(playerStats);
    }

    public void Refresh(PlayerStats newStats)
    {
        playerStats = newStats;
        intelligenceText.text = playerStats.GetIntelligence().ToString();
        strengthText.text = playerStats.GetStrength().ToString();
        dexterityText.text = playerStats.GetDexterity().ToString();
        agilityText.text = playerStats.GetAgility().ToString();
        enduranceText.text = playerStats.GetEndurance().ToString();
        computedDamage.text = GameSettings.GetDamage(playerStats, playerStats.GetWeapon()).ToString() + "  damages";
        computedSpeed.text = "Speed  " + GameSettings.GetSpeed(playerStats).ToString();
        computedAtkSpeed.text = GameSettings.GetAtkPerSecond(playerStats, playerStats.GetWeapon()).ToString() + "  attacks / seconds";
        computedCritRatio.text = (GameSettings.GetCritRatio(playerStats) * 100).ToString() + " %  of critical hits";
        computedHP.text = GameSettings.GetMaxHp(playerStats).ToString() + "  Health Points";
        skillPoints.text = playerStats.GetSkillPoints().ToString();
        runesText.text = playerStats.GetRunes().ToString();
        if (playerStats.GetSkillPoints() <= 0)
        {
            strengthBtn.gameObject.SetActive(false);
            agilityBtn.gameObject.SetActive(false);
            enduranceBtn.gameObject.SetActive(false);
            dexterityBtn.gameObject.SetActive(false);
            intelligenceBtn.gameObject.SetActive(false);
        } else
        {
            strengthBtn.gameObject.SetActive(true);
            agilityBtn.gameObject.SetActive(true);
            enduranceBtn.gameObject.SetActive(true);
            dexterityBtn.gameObject.SetActive(true);
            intelligenceBtn.gameObject.SetActive(true);
        }
        SaveHandler.Save(playerStats);
    }

}
