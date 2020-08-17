using UnityEngine;
using UnityEngine.UI;

public class BackgroundController : MonoBehaviour
{
    public Sprite easy;
    public Sprite medium;
    public Sprite hard;
    public Sprite extreme;

    // Start is called before the first frame update
    void Start()
    {
        Image img = gameObject.GetComponent<Image>();
        if (GameSettings.difficulty == 0) img.sprite = easy;
        else if (GameSettings.difficulty == 1) img.sprite = medium;
        else if (GameSettings.difficulty == 2) img.sprite = hard;
        else if (GameSettings.difficulty == 3) img.sprite = extreme;
    }
}
