using UnityEngine;
using UnityEngine.SceneManagement;
using UnityEngine.UI;

public class PlayButtonHandler : MonoBehaviour
{

    public GameObject inGameMenu;
    public Slider sfxSlider;
    public Slider musicSlider;


    // Start is called before the first frame update
    void Start()
    {
        sfxSlider.value = PlayerPrefs.GetFloat("SFXVolume", 50);
        musicSlider.value = PlayerPrefs.GetFloat("MusicVolume", 100);
        musicSlider.onValueChanged.AddListener(delegate { SetMusicValue(); });
        sfxSlider.onValueChanged.AddListener(delegate { SetSfxValue(); });
        inGameMenu.SetActive(false);
    }

    private void SetMusicValue()
    {
        AkSoundEngine.SetRTPCValue("MusicVolume", musicSlider.value);
    }
    private void SetSfxValue()
    {
        AkSoundEngine.SetRTPCValue("SFXVolume", sfxSlider.value);
    }
    public void ToggleMenu()
    {
        inGameMenu.SetActive(!inGameMenu.activeSelf);
    }
    public void GoToMenu()
    {
        SceneManager.LoadScene("GameOver");
        AkSoundEngine.SetState("GameMod_states", "Nothing_state");
    }
    public void Save()
    {
        PlayerPrefs.SetFloat("SFXVolume", sfxSlider.value);
        PlayerPrefs.SetFloat("MusicVolume", musicSlider.value);
        inGameMenu.SetActive(false);
    }
}
