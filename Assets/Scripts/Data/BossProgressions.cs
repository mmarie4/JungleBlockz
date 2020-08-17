using System;
using System.Collections.Generic;


[System.Serializable]
public class BossProgressions
{

    public List<BossProgression> List { get; set; } = new List<BossProgression>();
    public List<string> UnlockedBosses { get; set; } = new List<string>();

    public override string ToString()
    {
        string str = "";
        foreach (BossProgression progress in List)
        {
            str += progress + "\n";
        }
        return str;
    }
}
