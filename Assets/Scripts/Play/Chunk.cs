using System.Collections.Generic;
using TMPro;
using UnityEngine;

public class Chunk : MonoBehaviour
{

    private Sprite[] blocksSprites;
    private Sprite[] stonesSprites;
    private int numberOfSprites;
    private int numberOfRows = 10;
    private int numberOfColumns = 100;
    private float blockWidth;
    private float blockHeight;
    private float[] rates = new float[] { 1.0f, 1.0f, 0.99f, 0.95f, 0.6f, 0.95f, 0.95f, 0.95f, 0.99f, 0.99f };
    private string[,] grid;
    private float firstRowY = -20.0f;

    // Runes
    private float runeSpawnFreq = 1.0f;
    private float timeSinceLastRune = 0.0f;
    private float runeSpawnRate = 0.2f;
    // Ammos
    private float ammoSpawnFreq = 1.0f;
    private float timeSinceLastAmmo = 0.0f;
    private float ammoSpawnRate = 0.5f;

    // Prefabs blocks
    public GameObject block;
    public GameObject stone;
    public GameObject[] actionBlocks;
    public float[] actionBlocksRates;
    public string[] actionBlocksNames;
    public GameObject healParticles;
    public GameObject freezeParticles;
    public GameObject shieldParticles;
    public GameObject rune;
    public GameObject ammoBag;

    private bool populated = false;

    private PlayerStats playerStats;

    // Start is called before the first frame update
    void Start()
    {
        blocksSprites = Resources.LoadAll<Sprite>("World/NormalBlock");
        stonesSprites = Resources.LoadAll<Sprite>("World/StoneBlock");
        numberOfSprites = blocksSprites.Length;
        grid = new string[numberOfRows, numberOfColumns];

        // Create grid with normal blocks
        //blockWidth = block.GetComponent<BoxCollider2D>().bounds.size[0];
        //blockHeight = block.GetComponent<BoxCollider2D>().bounds.size[1];
        blockWidth = 2.01f;
        blockHeight = 1.515f;

        playerStats = SaveHandler.Load();
        ammoBag.GetComponent<SpriteRenderer>().sprite = Resources.Load<Sprite>("Bullets/bag_" + playerStats.GetWeapon());
        // Create string grid that represents the map
        for (int row = 0; row < numberOfRows; row++)
        {
            for (int i = 0; i < numberOfColumns; i++)
            {
                float blockRate = rates[row];
                float rand = Random.Range(0.0f, 1.0f);
                bool blockUnder = row == 0 ? true : (i == 0 || i >= numberOfColumns - 1) ? false : (grid[row - 1, i].Equals("normal") && grid[row - 1, i - 1].Equals("normal") && grid[row - 1, i + 1].Equals("normal"));
                if (rand < blockRate && blockUnder)
                {
                    // generate normal block
                    grid[row, i] = "normal";
                }
                else
                {
                    grid[row, i] = "empty";
                }
            }
        }

        // Randomly change blocks in action blocks on top only
        for (int col = 0; col < numberOfColumns; col++)
        {
            bool skipCol = false; // We just need to transform the block on the top of each column
            for (int row = numberOfRows - 1; row >= 0; row--)
            {
                if (grid[row, col].Equals("normal") && !skipCol)
                {
                    grid[row, col] = RandomTransformBlock(col, row);
                    skipCol = true;
                }
            }

        }

        // *********** Instantiate GameObjects according to the grid *****************
        PopulateChunk();
    }

    // Update is called once per frame
    void Update()
    {
        // Spawn runes
        if(populated)
        {
            timeSinceLastRune += Time.deltaTime;
            if (timeSinceLastRune > runeSpawnFreq)
            {
                timeSinceLastRune = 0.0f;
                int col = Random.Range(0, numberOfColumns);
                for (int row = numberOfRows - 1; row > 0; row--)
                {
                    if (grid[row, col] != "empty")
                    {
                        float rand = Random.Range(0.0f, 1.0f);
                        if (rand < runeSpawnRate)
                        {
                            GameObject clone = Instantiate(
                                rune,
                                transform.position + new Vector3(blockWidth * (col - numberOfColumns / 2), firstRowY + (blockHeight * row) + 1.5f, 0),
                                transform.rotation,
                                transform);
                            Destroy(clone, GameSettings.GetRuneLifetime());
                        }
                        return;
                    }
                }
            }
            // Spawn ammos
            timeSinceLastAmmo += Time.deltaTime;
            if (timeSinceLastAmmo > ammoSpawnFreq && playerStats.GetAmmoNumber() != 0)
            {
                timeSinceLastAmmo = 0.0f;
                int col = Random.Range(0, numberOfColumns);
                for (int row = numberOfRows - 1; row > 0; row--)
                {
                    if (grid[row, col] != "empty")
                    {
                        float rand = Random.Range(0.0f, 1.0f);
                        if (rand < ammoSpawnRate)
                        {
                            GameObject clone = Instantiate(
                                ammoBag,
                                transform.position + new Vector3(blockWidth * (col - numberOfColumns / 2), firstRowY + (blockHeight * row) + 1.5f, 0),
                                transform.rotation,
                                transform);
                            Destroy(clone, GameSettings.GetAmmoBagLifetime());
                        }
                        return;
                    }
                }
            }
        }
    }

    // Public methods
    public string[,] GetGrid()
    {
        return grid;
    }

    public bool IsPopulated() => populated;

    public void DestroyAllChildren()
    {
        foreach (Transform child in transform)
        {
            Destroy(child.gameObject);
        }
        populated = false;
    }

    public void PopulateChunk()
    {
        for (int row = 0; row < numberOfRows; row++)
        {
            for (int col = 0; col < numberOfColumns; col++)
            {
                // generate block
                string type = grid[row, col];
                if (type.Equals("normal"))
                {
                    GameObject clone = Instantiate(
                        block,
                        transform.position + new Vector3(blockWidth * (col - numberOfColumns / 2), firstRowY + (blockHeight * row), 0),
                        transform.rotation,
                        transform);
                    clone.GetComponent<SpriteRenderer>().sprite = blocksSprites[Random.Range(0, numberOfSprites)];
                }
                else if (!type.Equals("empty"))
                {
                    GameObject clone = Instantiate(
                        GetActionBlockFromType(type),
                        transform.position + new Vector3(blockWidth * (col - numberOfColumns / 2), firstRowY + (blockHeight * row), 0),
                        transform.rotation,
                        transform);
                    if (type.Equals("HealBlock")) Instantiate(healParticles, clone.transform.position, clone.transform.rotation, clone.transform);
                    if (type.Equals("FreezeBlock")) Instantiate(freezeParticles, clone.transform.position, clone.transform.rotation, clone.transform);
                    if (type.Equals("ShieldBlock")) Instantiate(shieldParticles, clone.transform.position, clone.transform.rotation, clone.transform);
                }
            }
        }

        // Add stone sprites under
        for (int col = 0; col < numberOfColumns; col++)
        {
            for (int i = 0; i < 4; i++)
            {
                GameObject clone = Instantiate(
                    stone,
                    transform.position + new Vector3(blockWidth * (col - numberOfColumns / 2), firstRowY - (blockHeight * (i + 1)), 0),
                    transform.rotation,
                    transform);
                clone.GetComponent<SpriteRenderer>().sprite = stonesSprites[Random.Range(0, 2)];
            }
        }
        populated = true;
    }

    // Private methods
    private string RandomTransformBlock(int col, int row)
    {
        float rand = Random.Range(0.0f, 1.0f);
        int index = -1;

        for (int i = 0; i < actionBlocks.Length; i++)
        {
            if (rand < actionBlocksRates[i])
            {
                index = i;
                break;
            }
        }

        if (index == -1) return "normal";
        if (actionBlocksNames[index].Equals("MommySpawn")) return actionBlocksNames[index];
        return playerStats.GetBlocksEquipped().Contains(actionBlocksNames[index]) ? actionBlocksNames[index] : "normal";
    }

    private GameObject GetActionBlockFromType(string type)
    {
        int index = System.Array.IndexOf(actionBlocksNames, type);
        return actionBlocks[index];
    }

}
