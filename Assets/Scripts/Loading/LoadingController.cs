using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.SceneManagement;
using UnityEngine.UI;

public class LoadingController : MonoBehaviour
{

    public GameObject[] images;
    public Text text;

    private float timeSinceLastChange = 0.0f;
    private float timePerState = 0.2f;
    private int indexOfImagesDisplayed = 0;
    private bool mode = true; // add or remove img

    // Start is called before the first frame update
    void Start()
    {
        RemoveAll();
        string scene = "PlayScene";
        if (GameStats.type == "boss")
        {
            text.text = "Summoning you into the " + GameStats.bossId + " area...";
            scene = GameStats.bossId;
        }

        StartCoroutine(LoadYourAsyncScene(scene));
    }

    // Update is called once per frame
    void Update()
    {
        timeSinceLastChange += Time.deltaTime;
        if(timeSinceLastChange >= timePerState)
        {
            if(indexOfImagesDisplayed < images.Length) images[indexOfImagesDisplayed].SetActive(mode);

            if (indexOfImagesDisplayed == images.Length - 1) {
                mode = !mode;
                indexOfImagesDisplayed = 0;
            } else
            {
                indexOfImagesDisplayed++;
            }

            timeSinceLastChange = 0.0f;
        }
    }

    private void RemoveAll()
    {
        foreach (GameObject img in images)
        {
            img.SetActive(false);
        }
    }

    IEnumerator LoadYourAsyncScene(string scene)
    {
        // The Application loads the Scene in the background as the current Scene runs.
        // This is particularly good for creating loading screens.
        // You could also load the Scene by using sceneBuildIndex. In this case Scene2 has
        // a sceneBuildIndex of 1 as shown in Build Settings.

        AsyncOperation asyncLoad = SceneManager.LoadSceneAsync(scene);

        // Wait until the asynchronous scene fully loads
        while (!asyncLoad.isDone)
        {
            yield return null;
        }
    }
}
