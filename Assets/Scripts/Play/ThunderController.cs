using System.Runtime.InteropServices.WindowsRuntime;
using UnityEngine;

class ThunderController : MonoBehaviour
{
    private float timeSinceStart = 0.0f;
    private float loadingDuration = 0.01f;
    private float livingDuration = 0.3f;
    private bool active = false;

    public Sprite thunderSprite;
    void Start()
    {
        Debug.Log("thunder !");
    }

    void FixedUpdate()
    {
        timeSinceStart += Time.deltaTime;
        if(timeSinceStart >= loadingDuration)
        {
            active = true;
            GetComponent<SpriteRenderer>().sprite = thunderSprite;
        }
        if (timeSinceStart >= livingDuration) Destroy(gameObject);
    }

    public bool IsActive() => active;
}
