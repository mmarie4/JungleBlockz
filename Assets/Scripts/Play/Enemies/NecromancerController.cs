using System.Collections.Generic;
using System.Linq;
using UnityEngine;
using UnityEngine.SceneManagement;
using UnityEngine.UI;

public class NecromancerController : Enemy
{

    #region variables

    private Sprite[] shootingSprites;
    private Sprite[] loadingSprites;
    private Sprite[] loadedSprites;
    private Sprite[] dyingSprites;
    private Sprite[] waitingSprites;

    public GameObject necroBulletPrefab;
    public GameObject ammoBag;
    public Text healthText;
    public GameObject healthBar;
    private Slider healthBarSlider;
    public GameObject runesPack;

    public List<GameObject> blockzToRemove;
    public List<GameObject> blockzToTransform;
    public List<GameObject> actionBlockPrefabs;

    private string direction;
    private int phase;
    private float timeSinceLastShot;
    private float timeSinceLastAmmo;
    List<Vector2> ammoSpawnPos;
    private List<GameObject> bullets;

    private GameObject playEvent;

    #endregion

    // Start is called before the first frame update
    void Start()
    {
        base.Start();
        InitStats("necromancer");
        PlayerStats playerStats = SaveHandler.Load();
        rigidbody2d = GetComponent<Rigidbody2D>();
        hitbox = GetComponent<BoxCollider2D>();
        shootingSprites = Resources.LoadAll<Sprite>("Necromancer/Shooting");
        loadingSprites = Resources.LoadAll<Sprite>("Necromancer/Loading");
        loadedSprites = Resources.LoadAll<Sprite>("Necromancer/Loaded");
        waitingSprites = Resources.LoadAll<Sprite>("Necromancer/Shooting");
        dyingSprites = Resources.LoadAll<Sprite>("EnemyDying");
        state = "waiting";
        spriteRenderer = gameObject.GetComponent<SpriteRenderer>();
        spriteMap = new Dictionary<string, Sprite[]>() { { "waiting", waitingSprites }, { "shooting", shootingSprites }, { "loading", loadingSprites }, { "loaded", loadedSprites }, { "dying", dyingSprites } };

        string weapon = playerStats.GetWeapon();
        burnDamage = GameSettings.GetItemDps(weapon) * (playerStats.GetStrength() * 0.1f + 1) / 4; // Divide by 4 since timeBetweenBurns = 0.25
        burningDuration = GameSettings.GetItemDotDuration(weapon);

        healthBarSlider = healthBar.GetComponent<Slider>();
        healthBarSlider.maxValue = maxHp;
        healthBarSlider.value = maxHp;
        healthBar.SetActive(false);
        bullets = new List<GameObject>();
        direction = "up";
        phase = 1;
        timeSinceLastShot = 0f;
        timeSinceLastAmmo = 0f;
        ammoSpawnPos = new List<Vector2>() { new Vector2(125.8f, -8.6f), new Vector2(140, -8.6f) };
        ammoBag.GetComponent<SpriteRenderer>().sprite = Resources.Load<Sprite>("Bullets/bag_" + playerStats.GetWeapon());

        List<string> blockzEquipped = playerStats.GetBlocksEquipped();
        TransformBlock(blockzEquipped, 0);
        TransformBlock(blockzEquipped, 1);
        TransformBlock(blockzEquipped, 2);

        GameSettings.SetDifficulty(2);
        BossProgressions listProgressions = SaveHandler.LoadBossProgressions();
        BossProgression progress = listProgressions.List.FirstOrDefault(p => p.Id == "necromancer");
        if (progress != null && progress.RunesFound) runesPack.SetActive(false);

        playEvent = GameObject.Find("PlayEvent");
        AkSoundEngine.SetSwitch("Bossplaylist_switch", "Preboss", playEvent);
        AkSoundEngine.SetSwitch("BossNumber_switch", "Boss01", playEvent);
    }

    void FixedUpdate()
    {
        if (ninja.transform.position.x >= 120 && state.Equals("waiting")) EnterFight();
        if (hp <= 0)
        {
            GameStats.isWon = true;
            SceneManager.LoadScene("GameOver");
        }

        timeSinceLastShot += Time.deltaTime;
        timeSinceLastAmmo += Time.deltaTime;
        if (phase == 1 && hp <= (maxHp * 0.5)) phase = 2;

        // Handle state
        if (state.Equals("loaded")) Shoot();
        if (state.Equals("shooting") && timeSinceLastShot > 1.5) state = "loading";
        if (state.Equals("loading") && bullets.Count == 0)
        {
            bullets.Add(Instantiate(necroBulletPrefab, new Vector2(transform.position.x - 1, transform.position.y), Quaternion.Euler(new Vector3(0, 0, 1))));
            if (phase == 2)
            {
                bullets.Add(Instantiate(necroBulletPrefab, new Vector2(transform.position.x - 1, transform.position.y - 1), Quaternion.Euler(new Vector3(0, 0, 1))));
                bullets.Add(Instantiate(necroBulletPrefab, new Vector2(transform.position.x - 1, transform.position.y + 1), Quaternion.Euler(new Vector3(0, 0, 1))));
            }
        }


        // moveVertical
        if (transform.position.y < -9) direction = "up";
        if (transform.position.y > -4) direction = "down";
        if (direction.Equals("up")) moveVertical = phase == 1 ? 3f : 4f;
        else moveVertical = phase == 1 ? 0.05f : 0.01f;

        // spawn ammo bags
        if (timeSinceLastAmmo > 8.0f && !state.Equals("waiting"))
        {
            int randIndex = Random.Range(0, ammoSpawnPos.Count);
            _ = Instantiate(ammoBag, ammoSpawnPos[randIndex], Quaternion.Euler(new Vector3(0, 0, 1)));
            timeSinceLastAmmo = 0.0f;
        }

        Move();

        CheckBurning();

        healthBarSlider.value = hp;
        healthText.text = hp + " / " + maxHp;
    }

    // Collisions events
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

    private void Shoot()
    {
        state = "shooting";
        timeSinceLastShot = 0.0f;

        foreach (GameObject bullet in bullets)
        {
            if (bullet == null)
            {
                Debug.Log("bullet is null and there are " + bullets.Count + " bullets");
            } else
            {
                NecroBulletController controller = bullet.GetComponent<NecroBulletController>();
                controller.SetDamage((int)damagePerHit);
                controller.SetDirection(-1);
                controller.SetSpeed(10, 0);
            }
        }

        bullets.Clear();
    }

    private void TransformBlock(List<string> blockzEquipped, int i)
    {
        if (blockzEquipped.Count > i)
        {
            GameObject prefab = actionBlockPrefabs.FirstOrDefault(b => b.CompareTag(blockzEquipped[i]));
            Instantiate(prefab, blockzToTransform[i].transform.position, blockzToTransform[i].transform.rotation);
            Destroy(blockzToTransform[i]);
        }
    }

    public void EnterFight()
    {
        AkSoundEngine.SetSwitch("Bossplaylist_switch", "Boss", playEvent);
        healthBar.SetActive(true);
        state = "loading";
        blockzToRemove.ForEach(b => Destroy(b));
    }
}
