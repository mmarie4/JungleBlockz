using UnityEngine;
using UnityEngine.SceneManagement;
using UnityEngine.UI;

public class ItemsButtonHandler : MonoBehaviour
{

    private PlayerStats playerStats;
    public Button[] weaponButtons;
    public Button[] blockButtons;

    // Info
    public Slider expBar;
    public Text expText;
    public Text level;
    public Text runes;
    public Text maxNumberOfBlockz;

    // Start
    void Start()
    {
        playerStats = SaveHandler.Load();
        SetAllStatuses();
        expBar.maxValue = (int)playerStats.GetRequiredExp();
        expBar.value = (int)playerStats.GetExperience();
        expText.text = ((int)playerStats.GetExperience()).ToString() + " / " + ((int)playerStats.GetRequiredExp()).ToString();
        level.text = playerStats.GetLevel().ToString();
        runes.text = playerStats.GetRunes().ToString();
        maxNumberOfBlockz.text = playerStats.GetMaxNumberOfBlocks().ToString();
    }

    // Functions
    private void SetWeaponButtonStatus(Button button)
    {
        if (playerStats.GetItemsUnlocked().Contains(button.tag))
        {
            if (button.CompareTag(playerStats.GetWeapon()))
            {
                button.GetComponentInChildren<Text>().text = "EQUIPPED";
                button.GetComponent<Image>().color = new Color(0.1f, 0.15f, 0.54f, 1.0f);
                try { GameObject.Find(button.tag + "RuneImg").SetActive(false); } catch { }
                return;
            }
            button.GetComponentInChildren<Text>().text = "EQUIP";
            button.GetComponent<Image>().color = new Color(0.19f, 0.45f, 0.15f, 1.0f);
            try { GameObject.Find(button.tag + "RuneImg").SetActive(false); } catch { }
        }
        else
        {
            GameObject.Find(button.tag + "RuneImg").SetActive(true);
            button.GetComponentInChildren<Text>().text = GameSettings.GetItemPrice(button.tag).ToString();
            if (playerStats.GetRunes() >= GameSettings.GetItemPrice(button.tag))
            {
                button.GetComponent<Image>().color = new Color(0.57f, 0.06f, 0.56f, 1.0f);

            }
            else
            {
                button.GetComponent<Image>().color = new Color(0.48f, 0.48f, 0.48f, 1.0f);
            }
        }
    }
    private void SetBlockButtonStatus(Button button)
    {
        if (playerStats.GetBlocksUnlocked().Contains(button.tag))
        {
            if (playerStats.GetBlocksEquipped().Contains(button.tag))
            {
                button.GetComponentInChildren<Text>().text = "EQUIPPED";
                button.GetComponent<Image>().color = new Color(0.1f, 0.15f, 0.54f, 1.0f);
                try { GameObject.Find(button.tag + "RuneImg").SetActive(false); } catch { }
                return;
            }
            button.GetComponentInChildren<Text>().text = "EQUIP";
            button.GetComponent<Image>().color = new Color(0.19f, 0.45f, 0.15f, 1.0f);
            try { GameObject.Find(button.tag + "RuneImg").SetActive(false); } catch { }
        }
        else
        {
            try { GameObject.Find(button.tag + "RuneImg").SetActive(true); } catch { }
            button.GetComponentInChildren<Text>().text = GameSettings.GetItemPrice(button.tag).ToString();
            if (playerStats.GetRunes() >= GameSettings.GetItemPrice(button.tag))
            {
                button.GetComponent<Image>().color = new Color(0.57f, 0.06f, 0.56f, 1.0f);
            }
            else
            {
                button.GetComponent<Image>().color = new Color(0.48f, 0.48f, 0.48f, 1.0f);
            }
        }
    }
    private void SetAllStatuses()
    {
        for (int i = 0; i < weaponButtons.Length; i++)
        {
            SetWeaponButtonStatus(weaponButtons[i]);
        }
        for (int i = 0; i < blockButtons.Length; i++)
        {
            SetBlockButtonStatus(blockButtons[i]);
        }
    }


    // Click handlers
    public void OnClickWeapon(string item)
    {
        if (!playerStats.GetWeapon().Equals(item) && playerStats.GetItemsUnlocked().Contains(item))
        {
            playerStats.Equip(item);
            SaveHandler.Save(playerStats);
        }
        if (!playerStats.GetWeapon().Equals(item) && !playerStats.GetItemsUnlocked().Contains(item))
        {
            if (playerStats.GetRunes() >= GameSettings.GetItemPrice(item))
            {
                Debug.Log("item : " + item);
                playerStats.SpendRunes(GameSettings.GetItemPrice(item));
                playerStats.AddToItems(item);
                playerStats.Equip(item);
                SaveHandler.Save(playerStats);
                runes.text = playerStats.GetRunes().ToString();
            }
            else
            {
                // TODO: show popup error message
                return;
            }
        }
        SetAllStatuses();
    }
    public void OnClickBlock(string item)
    {
        if (!playerStats.GetBlocksEquipped().Contains(item) && playerStats.GetBlocksUnlocked().Contains(item))
        {
            playerStats.EquipBlock(item);
            SaveHandler.Save(playerStats);
        }
        else if (playerStats.GetBlocksEquipped().Contains(item))
        {
            playerStats.UnequipBlock(item);
            SaveHandler.Save(playerStats);
        }
        else if (!playerStats.GetBlocksUnlocked().Contains(item) && GameSettings.GetItemPrice(item) <= playerStats.GetRunes())
        {
            playerStats.SpendRunes(GameSettings.GetItemPrice(item));
            playerStats.UnlockBlock(item);
            SaveHandler.Save(playerStats);
            runes.text = playerStats.GetRunes().ToString();
        }
        SetAllStatuses();
    }

    public void OnClickIncreaseMaxNumber()
    {
        if (playerStats.GetRunes() >= 1000)
        {
            playerStats.IncreaseNumberOfBlocks();
            playerStats.SpendRunes(1000);
            maxNumberOfBlockz.text = playerStats.GetMaxNumberOfBlocks().ToString();
            runes.text = playerStats.GetRunes().ToString();
            SaveHandler.Save(playerStats);
        }
    }

    public void GoToMenu()
    {
        SceneManager.LoadScene("MenuScene");
    }

}
