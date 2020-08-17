using System;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;
using UnityEngine.SceneManagement;
using UnityEngine.UI;

public class IllusionistController : Enemy
{

    #region variables

    private Sprite[] chargingSprites;
    private Sprite[] strikingSprites;
    private Sprite[] dyingSprites;

    public GameObject ammoBag;
    public GameObject healthBar;
    private Slider healthBarSlider;
    public Text healthText;
    public GameObject runesPack;

    public List<GameObject> blockzToRemove;
    public List<GameObject> actionBlockPrefabs;
    public GameObject normalBlockPrefab;
    public int numberOfRotatingBlock;

    private int phase;
    private float timeSinceLastAmmo;

    public float strikeDuration = 1.0f;
    private float timeSinceStartStrike = 0.0f;
    public float chargeDuration = 3.0f;
    private float timeSinceStartCharge= 0.0f;
    public GameObject edotensei;

    // Moving blockz
    private float blockSpeed = 3.0f;
    public List<Rigidbody2D> firstMovingBlockz;
    public List<int> firstMovingBlockzDirections;
    public List<Rigidbody2D> secondMovingBlockz;
    public List<int> secondMovingBlockzDirections;
    public RotatingBlockz rotatingBlockz;

    private GameObject playEvent;

    #endregion

    // Start is called before the first frame update
    void Start()
    {
        base.Start();
        InitStats("illusionist");
        PlayerStats playerStats = SaveHandler.Load();
        rigidbody2d = GetComponent<Rigidbody2D>();
        hitbox = GetComponent<BoxCollider2D>();
        chargingSprites = Resources.LoadAll<Sprite>("Illusionist/Charging");
        strikingSprites = Resources.LoadAll<Sprite>("Illusionist/Striking");
        dyingSprites = Resources.LoadAll<Sprite>("EnemyDying");
        state = "waiting";
        spriteRenderer = gameObject.GetComponent<SpriteRenderer>();
        spriteMap = new Dictionary<string, Sprite[]>() { { "waiting", chargingSprites }, { "charging", chargingSprites }, { "striking", strikingSprites }, { "dying", dyingSprites } };

        string weapon = playerStats.GetWeapon();
        burnDamage = GameSettings.GetItemDps(weapon) * (playerStats.GetStrength() * 0.1f + 1) / 4; // Divide by 4 since timeBetweenBurns = 0.25
        burningDuration = GameSettings.GetItemDotDuration(weapon);

        healthBarSlider = healthBar.GetComponent<Slider>();
        healthBarSlider.maxValue = maxHp;
        healthBarSlider.value = maxHp;
        healthBar.SetActive(false);
        phase = 1;
        timeSinceLastAmmo = 0f;
        ammoBag.GetComponent<SpriteRenderer>().sprite = Resources.Load<Sprite>("Bullets/bag_" + playerStats.GetWeapon());

        List<string> blockzEquipped = playerStats.GetBlocksEquipped();
        for(int i = 0; i < numberOfRotatingBlock; i++)
        {
            GameObject prefab = blockzEquipped.Count > i && i % 2 == 0 ? actionBlockPrefabs.FirstOrDefault(b => b.CompareTag(blockzEquipped[i])) : normalBlockPrefab;
            var block = Instantiate(prefab, transform.position, transform.rotation);
            block.name = "BossMovingBlock" + i;
            block.transform.SetParent(rotatingBlockz.transform);
        }
        

        GameSettings.SetDifficulty(2);
        BossProgressions listProgressions = SaveHandler.LoadBossProgressions();
        BossProgression progress = listProgressions.List.FirstOrDefault(p => p.Id == "illusionist");
        if (progress != null && progress.RunesFound) runesPack.SetActive(false);

        playEvent = GameObject.Find("PlayEvent");
        AkSoundEngine.SetSwitch("Bossplaylist_switch", "Preboss", playEvent);
        AkSoundEngine.SetSwitch("BossNumber_switch", "Boss01", playEvent);
    }

    void FixedUpdate()
    {
        if (state.Equals("waiting") && rotatingBlockz.entered) EnterFight();
        if (hp <= 0)
        {
            GameStats.isWon = true;
            SceneManager.LoadScene("GameOver");
        }

        CheckBurning();

        timeSinceLastAmmo += Time.deltaTime;
        if (phase == 1 && hp <= (maxHp * 0.5)) phase = 2;

        // Handle state
        if (state.Equals("charging"))
        {
            // Move blocks around illusionist in circle - Handled in rotatingBlockz
            timeSinceStartCharge += Time.deltaTime;
            if (timeSinceStartCharge >= chargeDuration)
            {
                // Strike
                Instantiate(edotensei, transform.position + new Vector3(0, 1, 0), transform.rotation);
                state = "striking";
                timeSinceStartStrike = 0.0f;
            }
        };
        if (state.Equals("striking"))
        {
            timeSinceStartStrike += Time.deltaTime;
            if (timeSinceStartStrike >= strikeDuration)
            {
                state = "charging";
                timeSinceStartCharge = 0.0f;
            }
        };

        if (state.Equals("waiting"))
        {
            // Move blocks in the mini-level
            int i = 0;
            foreach(Rigidbody2D block in firstMovingBlockz)
            {
                if (block.transform.position.x >= 25) firstMovingBlockzDirections[i] = -1;
                else if(block.transform.position.x <= 10) firstMovingBlockzDirections[i] = 1;
                block.transform.position += new Vector3(firstMovingBlockzDirections[i] * blockSpeed * Time.deltaTime, 0, 0);
                i++;
            }
            i = 0;
            foreach (Rigidbody2D block in secondMovingBlockz)
            {
                if (block.transform.position.y >= 2) secondMovingBlockzDirections[i] = -1;
                else if (block.transform.position.y <= -9) secondMovingBlockzDirections[i] = 1;
                block.transform.position += new Vector3(0, secondMovingBlockzDirections[i] * blockSpeed * Time.deltaTime, 0);
                i++;
            }
        }


        // spawn ammo bags
        if (timeSinceLastAmmo > 12.0f && !state.Equals("waiting"))
        {
            int randIndex = UnityEngine.Random.Range(0, rotatingBlockz.transform.childCount);
            var bag = Instantiate(ammoBag, rotatingBlockz.transform.GetChild(randIndex).transform.position + new Vector3(0, 2, 0), Quaternion.Euler(new Vector3(0, 0, 1)));
            bag.transform.SetParent(rotatingBlockz.transform.GetChild(randIndex).transform);
            timeSinceLastAmmo = 0.0f;
        }

        healthBarSlider.value = hp;
        healthText.text = hp + " / " + maxHp;
    }


    public void EnterFight()
    {
        AkSoundEngine.SetSwitch("Bossplaylist_switch", "Boss", playEvent);
        healthBar.SetActive(true);
        state = "charging";
        blockzToRemove.ForEach(b => Destroy(b));
    }

    private void OnCollisionStay2D(Collision2D collision)
    {
        if (collision.gameObject.name.Equals("Ninja"))
        {
            timeSinceLastHit += Time.deltaTime;
            if (timeSinceLastHit > timeBetweenHits)
            {
                ninja.TakeDamage((int)(damagePerHit));
                timeSinceLastHit = 0.0f;
            }
        }
    }
}
