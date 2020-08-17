using System;
using System.Collections.Generic;

[System.Serializable]
public class PlayerStats
{

    private bool isLoaded = false;
    // Initial data - Overwritten if there is a save file to load
    private int level;
    private int maxHp;
    private float experience;
    private float speed;
    private float jumpForce;
    private float requiredExpToNextLevel;
    private int skillPoints;
    private int runes;
    private float critRatio;
    private int strength;
    private int dexterity;
    private int agility;
    private int endurance;
    private int intelligence;
    private string weapon;
    private List<string> itemsUnlocked;
    private List<string> blocksUnlocked;
    private List<string> blocksEquipped;
    private int maxNumberOfBlocks;
    private DateTime lastAdSeen;
    private int adsSeen;

    // Initial constructor - Used only if there is no save file
    public PlayerStats()
    {
        // Global Stats
        level = 1;
        maxHp = 100;
        experience = 0.0f;
        skillPoints = 0;
        runes = 0;
        strength = 0;
        dexterity = 0;
        agility = 0;
        endurance = 0;
        intelligence = 0;
        speed = 8.0f;
        jumpForce = 20;
        requiredExpToNextLevel = 50.0f;
        critRatio = 0.01f;

        // Items
        weapon = "shuriken";
        itemsUnlocked = new List<string>() { "shuriken" };
        blocksUnlocked = new List<string>() { "DamageBlock", "MommySpawn", "JumpBlock", "HealBlock" };
        blocksEquipped = new List<string>() { "DamageBlock", "JumpBlock", "HealBlock" };
        maxNumberOfBlocks = 3;

        // Ad
        adsSeen = 0;
        lastAdSeen = DateTime.UtcNow;
    }

    // Getters
    public bool IsLoaded() { return isLoaded; }
    public int GetLevel() { return level; }
    public int GetMaxHp() { return maxHp + endurance * 5; }
    public float GetExperience() { return experience; }
    public float GetRequiredExp() { return requiredExpToNextLevel; }
    public float GetSpeed() { return speed; }
    public float GetCritRatio() { return critRatio; }
    public float GetJumpForce() { return jumpForce; }
    public int GetSkillPoints() { return skillPoints; }
    public int GetRunes() { return runes; }
    public float GetStrength() { return strength; }
    public float GetEndurance() { return endurance; }
    public float GetAgility() { return agility; }
    public float GetDexterity() { return dexterity; }
    public float GetIntelligence() { return intelligence; }
    public int GetAmmoNumber() { return GameSettings.GetItemAmmo(weapon); }
    public string GetWeapon() { return weapon; }
    public List<string> GetItemsUnlocked() { return itemsUnlocked; }
    public List<string> GetBlocksUnlocked() { return blocksUnlocked; }
    public List<string> GetBlocksEquipped() { return blocksEquipped; }
    public DateTime GetLastAdSeen() { return lastAdSeen; }
    public int GetNumberOfAds() { return adsSeen; }
    public int GetMaxNumberOfBlocks() { return maxNumberOfBlocks; }

    // Add XP
    public void AddExp(float xp)
    {
        experience += xp;
        while (experience >= requiredExpToNextLevel)
        {
            level++;
            skillPoints++;
            experience -= requiredExpToNextLevel;
            requiredExpToNextLevel += requiredExpToNextLevel * 0.1f;
        }
    }

    // Add Runes
    public void AddRunes(int additionalRunes) { runes += additionalRunes; }

    // Items
    public void SpendRunes(int price) { if (runes >= price) runes -= price; }
    public void AddToItems(string item) { itemsUnlocked.Add(item); }
    public void Equip(string newWeapon) { weapon = newWeapon; }
    public void UnlockBlock(string block) { blocksUnlocked.Add(block); }
    public void EquipBlock(string block) { if (blocksEquipped.Count < maxNumberOfBlocks) blocksEquipped.Add(block); }
    public void UnequipBlock(string block) { blocksEquipped = blocksEquipped.FindAll(b => b != block); }
    public void IncreaseNumberOfBlocks() { maxNumberOfBlocks++; }


    // Add player stats using skill points
    public void AddStrength()
    {
        if (skillPoints > 0)
        {
            strength++;
            skillPoints--;
        }
    }
    public void AddEndurance()
    {
        if (skillPoints > 0)
        {
            endurance++;
            skillPoints--;
        }
    }
    public void AddAgility()
    {
        if (skillPoints > 0)
        {
            agility++;
            skillPoints--;
        }
    }
    public void AddDexterity()
    {
        if (skillPoints > 0)
        {
            dexterity++;
            skillPoints--;
        }
    }
    public void AddIntelligence()
    {
        if (skillPoints > 0)
        {
            intelligence++;
            skillPoints--;
        }
    }

    public void Reset()
    {
        if (runes >= 500)
        {
            skillPoints = intelligence + dexterity + agility + endurance + strength;
            intelligence = 0;
            dexterity = 0;
            agility = 0;
            strength = 0;
            endurance = 0;
            runes -= 500;
        }
        else
        {
            throw new System.Exception("Not enough runes.");
        }
    }

    // Set date time
    public void SeeAd()
    {
        lastAdSeen = DateTime.UtcNow;
        adsSeen++;
        runes += GameSettings.GetAdReward(); ;
    }

    // To string
    public override string ToString() => "Player lvl" + level + " - XP: " + experience + " / " + requiredExpToNextLevel + " (" + runes + " runes | " + skillPoints + "SP)";


}
