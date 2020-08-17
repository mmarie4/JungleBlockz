using System.Collections.Generic;
using UnityEngine;

class MapGenerator : MonoBehaviour
{

    public GameObject chunkPrefab;
    public GameObject ninja;

    private int currentChunkIndex; // Chunk the player is currently in

    /*
     * We keep chunks in order left to right
     * If player goes to left on first chunk, we add chunk at index 0 and empty index 3
     * If player goes right on last chunk, we add chunk at the end and empty chunk end - 3
     * We always have 3 populated chunks
     * - - - x x x P P P x - - -
     */
    private List<GameObject> chunks = new List<GameObject>();
    private float chunkWidth = 201f;
    private float chunkPosOffset = -50f;

    void Start()
    {
        // Instantiate three chunks
        for (int i = 0; i < 3; i++)
        {
            Debug.Log("Instantiate chunk " + i);
            GameObject chunk = Instantiate(chunkPrefab, new Vector3((i - 1) * chunkWidth + chunkPosOffset, 0, 0), transform.rotation, transform);
            chunks.Add(chunk);
        }
        currentChunkIndex = 1;
    }

    void FixedUpdate()
    {
        if (ninja.transform.position.x < chunks[currentChunkIndex].transform.position.x)
        {
            currentChunkIndex--;
            // If new current chunk is the first one, instantiate new chunk
            if (currentChunkIndex == 0)
            {
                GameObject chunk = Instantiate(
                    chunkPrefab,
                    new Vector3(chunks[currentChunkIndex].transform.position.x - chunkWidth, 0, 0),
                    transform.rotation,
                    transform);
                chunks.Insert(0, chunk);
                currentChunkIndex++;
            }
            else
            {
                Chunk chunk = chunks[currentChunkIndex - 1].GetComponent<Chunk>();
                chunk.PopulateChunk();
            }
            chunks[currentChunkIndex + 2].GetComponent<Chunk>().DestroyAllChildren();
        }
        else if (ninja.transform.position.x > chunks[currentChunkIndex].transform.position.x + chunkWidth)
        {
            currentChunkIndex++;
            // If new current chunk is the last one, instantiate new chunk
            if (currentChunkIndex == chunks.Count - 1)
            {
                GameObject chunk = Instantiate(
                    chunkPrefab,
                    new Vector3(chunks[currentChunkIndex].transform.position.x + chunkWidth, 0, 0),
                    transform.rotation,
                    transform);
                chunks.Add(chunk);
            }
            else
            {
                Chunk chunk = chunks[currentChunkIndex + 1].GetComponent<Chunk>();
                chunk.PopulateChunk();
            }
            chunks[currentChunkIndex - 2].GetComponent<Chunk>().DestroyAllChildren();
        }
    }
}
