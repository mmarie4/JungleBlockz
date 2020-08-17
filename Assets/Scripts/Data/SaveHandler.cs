using System.IO;
using System.Runtime.Serialization.Formatters.Binary;
using UnityEngine;

public static class SaveHandler
{

    private static string filename = "/data-prod-1.0.dat";
    private static string bossFilename = "/boss-progressions-dev-2.dat";

    public static void Save(PlayerStats data)
    {
        string destination = Application.persistentDataPath + filename;
        FileStream file;

        if (File.Exists(destination)) file = File.OpenWrite(destination);
        else file = File.Create(destination);

        BinaryFormatter bf = new BinaryFormatter();
        bf.Serialize(file, data);
        file.Close();
    }

    public static void SaveBossProgressions(BossProgressions data)
    {
        string destination = Application.persistentDataPath + bossFilename;
        FileStream file;

        if (File.Exists(destination)) file = File.OpenWrite(destination);
        else file = File.Create(destination);

        BinaryFormatter bf = new BinaryFormatter();
        bf.Serialize(file, data);
        file.Close();
    }

    public static PlayerStats Load()
    {
        string destination = Application.persistentDataPath + filename;
        FileStream file;

        if (File.Exists(destination)) file = File.OpenRead(destination);
        else
        {
            return new PlayerStats();
        }

        BinaryFormatter bf = new BinaryFormatter();
        PlayerStats data = (PlayerStats)bf.Deserialize(file);
        file.Close();

        return data;
    }

    public static BossProgressions LoadBossProgressions()
    {
        string destination = Application.persistentDataPath + bossFilename;
        FileStream file;

        if (File.Exists(destination)) file = File.OpenRead(destination);
        else
        {
            return new BossProgressions();
        }

        BinaryFormatter bf = new BinaryFormatter();
        BossProgressions data = (BossProgressions)bf.Deserialize(file);
        file.Close();

        return data;
    }
}
