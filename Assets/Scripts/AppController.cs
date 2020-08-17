using UnityEngine;
using UnityEngine.SceneManagement;

public class AppController : MonoBehaviour
{
    // Start is called before the first frame update
    void Start()
    {
        SceneManager.LoadScene("MenuScene");
    }
}
