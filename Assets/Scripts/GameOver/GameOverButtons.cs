using UnityEngine.SceneManagement;
using UnityEngine;

public class GameOverButtons : MonoBehaviour
{
    // Buttons
    public void PlayAgain()
    {
        SceneManager.LoadScene("LoadingScene");
    }

    public void Menu()
    {
        SceneManager.LoadScene("MenuScene");
    }
}
