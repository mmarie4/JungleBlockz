[System.Serializable]
public class BossProgression
{
    public string Id { get; set; }
    public bool IsPassed { get; set; }
    public bool RunesFound { get; set; }

    public override string ToString()
    {
        return "Boss " + Id + " (isPassed: " + IsPassed + " | RunesFound: " + RunesFound + ")";
    }
}
