/* 
 * 
 * Static class keeping variables to set up one game
 * Set the difficulty when launching a game, and returns differents parameters depending on the difficulty
 * 
 */

using UnityEngine.Monetization;

public static class GameSettings
{
    private static string development = "development";
    private static string production = "production";
    private static string PC = "PC";
    private static string phone = "phone";

    // PLATFORM - TO HANDLE CONTROLS - Either 'phone', or 'PC'
    public static string platform = phone;

    // ENVIRONMENT - TO HANDLE ADS - Either 'development' or 'production'
    public static string env = production;

    // UPDATE POPUP
    public static bool updatePopupSeen = false;

    // TIME BETWEEN REWARDED ADS
    public static int GetTimeBetweenRewards() => 7200;

    // DIFFICULTY
    public static int difficulty = 0;
    public static void SetDifficulty(int d) => difficulty = d;

    // SPECIAL BLOCKZ EFFECTS
    public static float GetBlocksDps() => 12.0f;
    public static float GetBlocksHps() => 9.0f;
    public static float GetDurationSpeedEffect() => 0.4f;

    // SPAWN DELAY
    public static float GetSpawnDelay()
    {
        if (difficulty == 0) return 10.0f;
        if (difficulty == 1) return 8.0f;
        if (difficulty == 2) return 7f;
        if (difficulty == 3) return 6.0f;
        else return 5.0f;
    }
    // PLAYER STATS
    public static int GetDamage(PlayerStats playerStats, string weapon) { return (int)(GetItemDamage(weapon) * (playerStats.GetStrength () * 0.1f + 1)); }
    public static float GetAtkPerSecond(PlayerStats playerStats, string weapon) { return GetItemAtkSpeed(weapon) + playerStats.GetDexterity() * 0.1f; }
    public static float GetSpeed(PlayerStats playerStats) { return playerStats.GetSpeed() + playerStats.GetAgility() * 0.25f; }
    public static float GetCritRatio(PlayerStats playerStats) { return playerStats.GetCritRatio() + playerStats.GetIntelligence() * 0.005f; }
    public static int GetMaxHp(PlayerStats playerStats) { return (int)(playerStats.GetMaxHp() + playerStats.GetEndurance() * 5); }
    // ENEMIES STATS
    public static int GetEnemyHp(string type)
    {
        if (type == "mommy") return 80;
        if (type == "kamikaz") return 40;
        if (type == "bird") return 20;
        if (type == "fatboy") return 60;
        if (type == "necromancer") return 1500;
        if (type == "thundergod") return 2000;
        if (type == "illusionist") return 3000;
        return 0;
    }
    public static float GetEnemyDps(string type)
    {
        if (type == "mommy") return 18.0f;
        if (type == "kamikaz") return 50.0f;
        if (type == "bird") return 30.0f;
        if (type == "fatboy") return 1000.0f;
        if (type == "necromancer") return 1000.0f;
        if (type == "thundergod") return 1000.0f;
        if (type == "illusionist") return 1000.0f;
        return 0.0f;
    }
    public static float GetEnemySpeed(string type)
    {
        if (type == "mommy") return 1.6f;
        if (type == "kamikaz") return 3.0f;
        if (type == "bird") return 3.0f;
        if (type == "fatboy") return 2.0f;
        if (type == "necromancer") return 1.0f;
        if (type == "thundergod") return 0.0f;
        if (type == "illusionist") return 0.0f;
        return 0.0f;
    }
    public static float GetEnemyDamagePerHit(string type)
    {
        if (type == "mommy") return 5f;
        if (type == "kamikaz") return 1.0f;
        if (type == "bird") return 3.0f;
        if (type == "fatboy") return 10f;
        if (type == "necromancer") return 50f;
        if (type == "thundergod") return 50f;
        if (type == "illusionist") return 50f;
        return 0.0f;
    }
    public static int GetThunderDmg() => 30;

    public static float GetMultiplicator()
    {
        if (difficulty == 0) return 1.0f;
        if (difficulty == 1) return 1.5f;
        if (difficulty == 2) return 2.0f;
        if (difficulty == 3) return 2.5f;
        else return 0.0f;
    }

    public static float GetBossXPReward(string type)
    {
        if (type == "necromancer") return 100f;
        if (type == "thundergod") return 300f;
        if (type == "illusionist") return 500f;
        return 0f;
    }

    public static int GetBossRunesReward(string type)
    {
        if (type == "necromancer") return 100;
        if (type == "thundergod") return 200;
        if (type == "illusionist") return 300;
        return 0;
    }

    public static float GetRuneLifetime() => 10.0f;
    public static float GetAmmoBagLifetime() => 6.0f;

    // =============  ITEMS  ====================
    public static int GetItemPiercingCapacity(string item)
    {
        if (item == "shuriken") return 1;
        if (item == "kunai") return 3;
        if (item == "fireball") return 5;
        if (item == "spikes") return 1;
        return 0;
    }
    public static int GetItemDamage(string item)
    {
        if (item == "shuriken") return 10;
        if (item == "kunai") return 15;
        if (item == "fireball") return 20;
        if (item == "spikes") return 2;
        return 0;
    }
    public static float GetItemDps(string item)
    {
        if (item == "fireball") return 10f;
        return 1.00f;
    }
    public static float GetItemDotDuration(string item)
    {
        if (item == "fireball") return 5f;
        return 0.0f;
    }
    public static int GetItemAmmo(string item)
    {
        if (item == "shuriken") return 30;
        if (item == "kunai") return 8;
        if (item == "fireball") return 5;
        if (item == "spikes") return 12;
        return 0;
    }
    public static float GetItemAtkSpeed(string item)
    {
        if (item == "shuriken") return 5.0f;
        if (item == "kunai") return 3.0f;
        if (item == "fireball") return 1.0f;
        if (item == "spikes") return 2.0f;
        return 0;
    }
    public static int GetItemMovementSpeed(string item)
    {
        if (item == "shuriken") return 22;
        if (item == "kunai") return 15;
        if (item == "fireball") return 10;
        if (item == "spikes") return 15;
        return 0;
    }
    public static int GetItemVerticalSpeed(string item)
    {
        if (item == "spikes") return 5;
        return 0;
    }
    public static int GetItemPrice(string item)
    {
        if (item == "shuriken") return 0;
        if (item == "kunai") return 100;
        if (item == "fireball") return 300;
        if (item == "spikes") return 200;
        if (item == "FreezeBlock") return 100;
        if (item == "ShieldBlock") return 300;
        if (item == "SpeedBlock") return 200;
        return 0;
    }
    public static int GetItemNumberOfInstances(string item)
    {
        if (item == "spikes") return 15;
        return 1;
    }

    public static int GetAdReward() => 20;
}

