using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class SpawnController : MonoBehaviour
{

    private float spawnDelay;

    private Rigidbody2D rigidbody2d;
    private Vector2 offset;

    public GameObject mommy;
    public GameObject kamikaz;
    public GameObject bird;
    public GameObject fatboy;

    private float timeSinceLastSpawn;

    // Start is called before the first frame update
    void Start()
    {
        spawnDelay = GameSettings.GetSpawnDelay();
        rigidbody2d = gameObject.GetComponent<Rigidbody2D>();
        offset = new Vector2(0, 2);
        timeSinceLastSpawn = Random.Range(0.0f, spawnDelay);
    }

    // Update is called once per frame
    void FixedUpdate()
    {
        timeSinceLastSpawn += Time.deltaTime;
        if (timeSinceLastSpawn > spawnDelay)
        {
            int rand = Random.Range(0, (int)(GameSettings.difficulty) + 1) ;
            if (rand == 0) Instantiate(mommy, rigidbody2d.position + offset, transform.rotation, transform);
            if (rand == 1) Instantiate(kamikaz, rigidbody2d.position + offset, transform.rotation, transform);
            if (rand == 2) Instantiate(fatboy, rigidbody2d.position + offset, transform.rotation, transform);
            if (rand == 3) Instantiate(bird, rigidbody2d.position + offset, transform.rotation, transform);
            timeSinceLastSpawn = 0.0f;
        }
    }
}
