/* 
 * 
 * Static class keeping data for one game
 * Used in Game Over scene, to compute and save new data
 * 
 */

using System;
using UnityEngine.Analytics;

public static class GameStats
{
    public static int kills;
    public static int runes;
    public static float survival;

    public static string type = "explore"; // either 'explore' or 'boss'
    public static string bossId = "";
    public static bool isWon = false; // For boss fights
    public static bool levelUp = false;

    public static void Reset() {
        runes = 0;
        kills = 0;
        survival = 0.0f;
        isWon = false;
        levelUp = false;
    }

    public static void AddKill()
    {
        kills++;
    }
    public static void AddRune()
    {
        runes++;
    }
    public static void SetSurvival(float s)
    {
        survival = s;
    }
}
