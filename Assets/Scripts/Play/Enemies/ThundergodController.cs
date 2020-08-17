using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using UnityEngine;
using UnityEngine.SceneManagement;
using UnityEngine.UI;

public class ThundergodController : Enemy
{

    #region variables

    private Sprite[] shooting0Sprites;
    private Sprite[] shooting1Sprites;
    private Sprite[] shooting2Sprites;
    private Sprite[] chargingSprites;
    private Sprite[] strikingSprites;
    private Sprite[] dyingSprites;

    public GameObject thunderPrefab;
    public GameObject thunderBoldPrefab;
    public GameObject thunderParticlesPrefab;
    public GameObject ammoBag;
    public GameObject healthBar;
    private Slider healthBarSlider;
    public Text healthText;
    public GameObject runesPack;

    public List<Vector2> tpLocations;
    public List<GameObject> blockzToRemove;
    public List<GameObject> blockzToTransform;
    public List<GameObject> actionBlockPrefabs;

    private int phase;
    private float timeSinceLastShot;
    private float timeBetweenShots = 0.5f;
    private float timeSinceLastAmmo;
    List<Vector2> ammoSpawnPos;
    private float strikeDuration = 2.0f;
    private float timeSinceStartStrike = 0f;
    private float chargeDuration = 1.0f;
    private float timeSinceStartCharge = 0.0f;
    private int numberOfShots = 0;
    private float timeSinceLastThunder = 0.0f;
    private float timeBetweenThunders = 1.0f;
    private Vector2 nextPosition;

    private GameObject playEvent;

    #endregion

    // Start is called before the first frame update
    void Start()
    {
        base.Start();
        InitStats("thundergod");
        PlayerStats playerStats = SaveHandler.Load();
        rigidbody2d = GetComponent<Rigidbody2D>();
        hitbox = GetComponent<BoxCollider2D>();
        shooting0Sprites = Resources.LoadAll<Sprite>("Thundergod/ShootingA");
        shooting1Sprites = Resources.LoadAll<Sprite>("Thundergod/ShootingB");
        shooting2Sprites = Resources.LoadAll<Sprite>("Thundergod/ShootingA");
        chargingSprites = Resources.LoadAll<Sprite>("Thundergod/Charging");
        strikingSprites = Resources.LoadAll<Sprite>("Thundergod/Striking");
        dyingSprites = Resources.LoadAll<Sprite>("EnemyDying");
        state = "waiting";
        spriteRenderer = gameObject.GetComponent<SpriteRenderer>();
        spriteMap = new Dictionary<string, Sprite[]>() { { "waiting", chargingSprites }, { "shooting0", shooting0Sprites }, { "shooting1", shooting1Sprites }, { "shooting2", shooting2Sprites }, { "charging", chargingSprites }, { "striking", strikingSprites }, { "dying", dyingSprites } };
        
        string weapon = playerStats.GetWeapon();
        burnDamage = GameSettings.GetItemDps(weapon) * (playerStats.GetStrength() * 0.1f + 1) / 4; // Divide by 4 since timeBetweenBurns = 0.25
        burningDuration = GameSettings.GetItemDotDuration(weapon);

        healthBarSlider = healthBar.GetComponent<Slider>();
        healthBarSlider.maxValue = maxHp;
        healthBarSlider.value = maxHp;
        healthBar.SetActive(false);
        phase = 1;
        timeSinceLastShot = 0f;
        timeSinceLastAmmo = 0f;
        ammoSpawnPos = new List<Vector2>() { new Vector2(135.6f, -6.13f), new Vector2(145.8f, -6.13f), new Vector2(155.8f, -4.5f) };
        ammoBag.GetComponent<SpriteRenderer>().sprite = Resources.Load<Sprite>("Bullets/bag_" + playerStats.GetWeapon());

        List<string> blockzEquipped = playerStats.GetBlocksEquipped();
        TransformBlock(blockzEquipped, 0);
        TransformBlock(blockzEquipped, 1);
        TransformBlock(blockzEquipped, 2);

        GameSettings.SetDifficulty(2);
        BossProgressions listProgressions = SaveHandler.LoadBossProgressions();
        BossProgression progress = listProgressions.List.FirstOrDefault(p => p.Id == "thundergod");
        if (progress != null && progress.RunesFound) runesPack.SetActive(false);

        playEvent = GameObject.Find("PlayEvent");
        AkSoundEngine.SetSwitch("Bossplaylist_switch", "Preboss", playEvent);
        AkSoundEngine.SetSwitch("BossNumber_switch", "Boss02", playEvent);
    }

    void FixedUpdate()
    {
        if (ninja.transform.position.x >= 130 && state.Equals("waiting")) EnterFight();
        if (hp <= 0)
        {
            GameStats.isWon = true;
            SceneManager.LoadScene("GameOver");
        }

        CheckBurning();

        timeSinceLastShot += Time.deltaTime;
        timeSinceLastAmmo += Time.deltaTime;
        if (phase == 1 && hp <= (maxHp * 0.5)) phase = 2;

        // Handle state
        if (state.Equals("striking"))
        {
            timeSinceStartStrike += Time.deltaTime;
            if (timeSinceStartStrike >= strikeDuration)
            {
                state = "shooting0";
            }
        };
        if (state.Contains("shooting") && timeSinceLastShot >= timeBetweenShots)
        {
            spriteRenderer.flipX = ninja.transform.position.x < gameObject.transform.position.x;
            /* Shoot 3 times then strike again */
            if (numberOfShots < 3) Shoot();
            else Charge();
        }
        if (state.Equals("charging"))
        {
            timeSinceStartCharge += Time.deltaTime;
            if (timeSinceStartCharge >= chargeDuration)
            {
                Strike();
            }
        }

        if (state.Equals("waiting"))
        {
            timeSinceLastThunder += Time.deltaTime;
            if (timeSinceLastThunder >= timeBetweenThunders)
            {
                Vector2 pos;
                int platform = Random.Range(0, 10);
                if (platform <= 3) pos = new Vector2(Random.Range(-9.0f, 16.0f), -1); // Hack to increase probability to be on first platform
                else if (platform == 4) pos = new Vector2(Random.Range(23f, 32f), 1);
                else if (platform == 6 || platform == 5) pos = new Vector2(Random.Range(37f, 57f), 1);
                else if (platform == 7) pos = new Vector2(Random.Range(65f, 77f), 4);
                else if (platform == 8) pos = new Vector2(Random.Range(82f, 92f), 5);
                else if (platform == 9) pos = new Vector2(Random.Range(98f, 106f), 3.5f);
                else pos = new Vector2(Random.Range(112f, 124f), 6f);

                GameObject _ = Instantiate(thunderPrefab, pos, Quaternion.Euler(new Vector3(0, 0, 1)));
                if (ninja.CheckDistance(pos) < 20) AkSoundEngine.PostEvent("thunder_event", gameObject);
                timeBetweenThunders = Random.Range(0.2f, 1.0f);
                timeSinceLastThunder = 0.0f;
            }
        }


        // spawn ammo bags
        if (timeSinceLastAmmo > 8.0f && !state.Equals("waiting"))
        {
            int randIndex = Random.Range(0, ammoSpawnPos.Count);
            _ = Instantiate(ammoBag, ammoSpawnPos[randIndex], Quaternion.Euler(new Vector3(0, 0, 1)));
            timeSinceLastAmmo = 0.0f;
        }

        healthBarSlider.value = hp;
        healthText.text = hp + " / " + maxHp;
    }

    private void Strike()
    {

        // Teleport
        transform.position = nextPosition;

        // Instantiate TP thunder
        StartCoroutine(ThunderStrike(new Vector2(transform.position.x - 2.5f, transform.position.y)));
        StartCoroutine(ThunderStrike(new Vector2(transform.position.x - 1.2f, transform.position.y)));
        StartCoroutine(ThunderStrike(new Vector2(transform.position.x, transform.position.y + 6)));
        StartCoroutine(ThunderStrike(new Vector2(transform.position.x + 1.2f, transform.position.y)));
        StartCoroutine(ThunderStrike(new Vector2(transform.position.x + 2.5f, transform.position.y)));
        
        state = "striking";
        timeSinceStartStrike = 0.0f;
    }
    private void Charge()
    {
        nextPosition = tpLocations[Random.Range(0, tpLocations.Count)];
        GameObject tpAnim = Instantiate(thunderParticlesPrefab, new Vector2(nextPosition.x + 1, nextPosition.y), Quaternion.Euler(new Vector3(0, 0, 1)));
        Destroy(tpAnim, chargeDuration);
        numberOfShots = 0;
        state = "charging";
        timeSinceStartCharge = 0.0f;
        AkSoundEngine.PostEvent("thundergodCharging_event", gameObject);
    }
    private void Shoot()
    {
        state = "shooting" + numberOfShots;
        timeSinceLastShot = 0.0f;
        // Instantiate thunderbolt
        if (spriteRenderer.flipX)
        {
            GameObject thunderbolt = Instantiate(thunderBoldPrefab, new Vector2(transform.position.x - 4, transform.position.y + 0.5f), Quaternion.Euler(new Vector3(0, 0, 1)));
            thunderbolt.GetComponent<SpriteRenderer>().flipX = true;
        } else
        {
            GameObject thunderbolt = Instantiate(thunderBoldPrefab, new Vector2(transform.position.x + 4, transform.position.y + 0.5f), Quaternion.Euler(new Vector3(0, 0, 1)));
        }
        numberOfShots++;
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
        state = "charging";
        blockzToRemove.ForEach(b => Destroy(b));
    }

    private IEnumerator<WaitForSeconds> ThunderStrike(Vector2 pos)
    {
        float randomDelay = Random.Range(0.0f, 1.0f);
        yield return new WaitForSeconds(randomDelay);
        Instantiate(thunderPrefab, pos, Quaternion.Euler(new Vector3(0, 0, 1)));
        AkSoundEngine.PostEvent("thunder_event", gameObject);
    }
}
