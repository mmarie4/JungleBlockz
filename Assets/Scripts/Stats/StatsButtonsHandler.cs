using UnityEngine;
using UnityEngine.SceneManagement;

public class StatsButtonsHandler : MonoBehaviour
{
    private PlayerStats playerStats;

    public StatsController controller;

    public void Start()
    {
        playerStats = SaveHandler.Load();
    }

    public void GoToMenu()
    {
        SceneManager.LoadScene("MenuScene");

    }

    public void AddStrength()
    {
        playerStats.AddStrength();
        controller.Refresh(playerStats);
    }

    public void AddAgility()
    {
        playerStats.AddAgility();
        controller.Refresh(playerStats);
    }

    public void AddEndurance()
    {
        playerStats.AddEndurance();
        controller.Refresh(playerStats);
    }

    public void AddDexterity()
    {
        playerStats.AddDexterity();
        controller.Refresh(playerStats);
    }

    public void AddIntelligence()
    {
        playerStats.AddIntelligence();
        controller.Refresh(playerStats);
    }

    public void ResetSkillPoints()
    {
        playerStats.Reset();
        controller.Refresh(playerStats);
    }
}
