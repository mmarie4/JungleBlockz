using System.Collections.Generic;
using UnityEngine;
using UnityEngine.SceneManagement;
using UnityEngine.UI;

public class Ninja : MonoBehaviour
{

    [SerializeField] private LayerMask layerMask;

    #region sprites
    private Sprite[] fallingSprites;
    private Sprite[] shootingSprites;
    private Sprite[] jumpingSprites;
    private Sprite[] runningSprites;
    private Sprite[] idleSprites;
    private Sprite[] doubleJumpSprites;
    private Dictionary<string, Sprite[]> spriteMap;
    private int spriteIndex = 0;
    #endregion

    #region prefabs
    public GameObject shuriken;
    public GameObject damageText;
    public GameObject healText;
    public Canvas canvas;
    public GameObject shurikenTrail;
    public GameObject speedTrailPrefab;
    public GameObject fireballTrail;
    public GameObject fireballTrailLeft;
    #endregion

    #region private variables
    private PlayerStats playerStats;
    private string weapon;
    private float jumpForce;
    private float ninjaSpeed;
    private int hp;
    private int maxHp;
    private Rigidbody2D rigidbody2d;
    private BoxCollider2D hitbox;
    private SpriteRenderer spriteRenderer;
    private string ninjaState = "spawning";
    private float moveHorizontal;
    private float timeSinceFrameChanged = 0;
    private float timeSinceLastShot = 0.0f;
    private float survivalTime = 0;
    private bool wantsToJump = false;
    private bool doubleJumpUsed = false;
    private int mommiesAround = 0;
    private bool forceIntenseMusic = false;
    private int nbOfAmmo;
    private int maxAmmo;

    private float timeSinceLastHit = 0.0f;
    private float timeBetweenHits;
    private float timeSinceLastHeal = 0.0f;
    private float timeBetweenHeals;

    // Special blockz effects
    private bool isFreezed;
    private bool isShielded;
    private bool isSpeededUp;
    private float timeSinceLastSpeedUp;
    private GameObject speedTrail;

    private GameObject playEvent;
    #endregion

    #region var from editor
    // UI
    public Slider healthBar;
    public Text runesText;
    public Canvas floatingTextCanvas;
    public Text ammoNumber;
    public Text timer;
    #endregion

    // Start is called before the first frame update
    void Start()
    {
        AkSoundEngine.SetState("GameMod_states", GameStats.type.Equals("explore") ? "Explore_state" : "Boss_state");
        playerStats = SaveHandler.Load();
        weapon = playerStats.GetWeapon();
        jumpForce = playerStats.GetJumpForce();
        maxHp = GameSettings.GetMaxHp(playerStats);
        ninjaSpeed = GameSettings.GetSpeed(playerStats);
        hp = maxHp;
        maxAmmo = playerStats.GetAmmoNumber();
        nbOfAmmo = playerStats.GetAmmoNumber();
        ammoNumber.text = nbOfAmmo + " / " + maxAmmo;
        timeBetweenHits = 1 / GameSettings.GetBlocksDps();
        timeBetweenHeals = 1 / GameSettings.GetBlocksHps();
        healthBar.maxValue = maxHp;
        healthBar.value = maxHp;

        rigidbody2d = GetComponent<Rigidbody2D>();
        hitbox = GetComponent<BoxCollider2D>();

        spriteRenderer = gameObject.GetComponent<SpriteRenderer>();
        runningSprites = Resources.LoadAll<Sprite>("Ninja/Running");
        jumpingSprites = Resources.LoadAll<Sprite>("Ninja/Jumping");
        fallingSprites = Resources.LoadAll<Sprite>("Ninja/Falling");
        shootingSprites = Resources.LoadAll<Sprite>("Ninja/Shooting");
        doubleJumpSprites = Resources.LoadAll<Sprite>("Ninja/DoubleJump");
        idleSprites = Resources.LoadAll<Sprite>("Ninja/Idle");
        spriteMap = new Dictionary<string, Sprite[]>() { { "jumping", jumpingSprites }, { "idle", idleSprites }, { "running", runningSprites }, { "falling", fallingSprites }, { "spawning", fallingSprites }, { "shooting", shootingSprites }, { "doubleJump", doubleJumpSprites } };

        shuriken.GetComponent<SpriteRenderer>().sprite = Resources.Load<Sprite>("Bullets/" + playerStats.GetWeapon());
        GameStats.Reset();

        isFreezed = false;
        isShielded = false;
        isSpeededUp = false;

        playEvent = GameObject.Find("PlayEvent");
        AkSoundEngine.SetSwitch("CoolorStrress_switch", "Cool", playEvent);
    }
    void FixedUpdate()
    {
        survivalTime += Time.deltaTime;
        // Attach camera after spawn and landing
        if (ninjaState.Equals("spawning") && isGrounded()) attachCamera();

        // Land after jump
        if (isGrounded() && (ninjaState.Equals("jumping") || ninjaState.Equals("falling")))
        {
            ninjaState = "idle";
            doubleJumpUsed = false;
        }
        // Jump
        if (wantsToJump)
        {
            wantsToJump = false;
            if (ninjaState.Equals("falling") && !doubleJumpUsed)
            {
                rigidbody2d.AddForce(new Vector2(0, jumpForce * 1.5f), ForceMode2D.Impulse);
                doubleJumpUsed = true;
                ninjaState = "doubleJump";
                AkSoundEngine.PostEvent("doubleJump_event", gameObject);
            }
            if (isGrounded())
            {
                rigidbody2d.AddForce(new Vector2(0, jumpForce), ForceMode2D.Impulse);
                ninjaState = "jumping";
                AkSoundEngine.PostEvent("jump_event", gameObject);
            }
        }

        healthBar.value = hp;

        // On PC
        if (GameSettings.platform.Equals("PC"))
        {
            moveHorizontal = Input.GetAxis("Horizontal");
            if (Input.GetKeyDown(KeyCode.Space)) wantsToJump = true;
            if (Input.GetKeyDown(KeyCode.S)) Shoot();
        }

        if (ninjaState.Equals("shooting"))
        {
            timeSinceLastShot += Time.deltaTime;
            if (timeSinceLastShot > 0.1f) ninjaState = "idle";
        }

        HandleBlock();
        Move();
        SetSprite();

        GameStats.SetSurvival(survivalTime);
        if(timer != null) timer.text = string.Format("{0:0} : {1:00}", Mathf.Floor(GameStats.survival / 60), GameStats.survival % 60);
    }

    // Public Functions
    public void SetMoveHorizontal(float input)
    {
        moveHorizontal = input;
    }
    public void Jump()
    {
        wantsToJump = true;
    }
    public void SetState(string newState)
    {
        ninjaState = newState;
    }
    public void TakeDamage(int dmg)
    {
        if (isShielded)
        {
            float rand = Random.Range(0.0f, 1.0f);
            if (rand > 0.3f) return;
        }
        if (hp + dmg >= 0)
        {
            ShowFloatingText(damageText, dmg.ToString());
            hp -= dmg;
        }
        if (hp <= 0) Die();
    }
    public void Heal(int heal)
    {
        if (hp < maxHp)
        {
            hp += heal;
            ShowFloatingText(healText, heal.ToString());
        }
    }
    public void Shoot()
    {
        if (nbOfAmmo <= 0)
        {
            return;
        }
        ninjaState = "shooting";
        timeSinceLastShot = 0.0f;
        float size = GetComponent<BoxCollider2D>().size.x;
        AkSoundEngine.PostEvent(weapon + "_event", gameObject);
        int numberOfInstances = GameSettings.GetItemNumberOfInstances(weapon);
        for (int i = 0; i < numberOfInstances; i++)
        {
            GameObject clone = Instantiate(shuriken, new Vector2(transform.position.x, transform.position.y), Quaternion.Euler(new Vector3(0, 0, 1)));
            clone.GetComponent<BulletController>().SetDirection(spriteRenderer.flipX ? -1 : 1);
            clone.GetComponent<BulletController>().SetDamage(GameSettings.GetDamage(playerStats, weapon));
            clone.GetComponent<BulletController>().SetCritRatio(GameSettings.GetCritRatio(playerStats));
            clone.GetComponent<BulletController>().SetSpeed(
                GameSettings.GetItemMovementSpeed(weapon) * Random.Range(0.5f, 1.0f),
                GameSettings.GetItemVerticalSpeed(weapon) * Random.Range(-1.0f, 1.0f)
                ) ;
            clone.GetComponent<BulletController>().SetType(playerStats.GetWeapon());
            clone.GetComponent<BulletController>().SetMaxNumberOfEnemies(GameSettings.GetItemPiercingCapacity(playerStats.GetWeapon()));
            clone.GetComponent<SpriteRenderer>().flipX = spriteRenderer.flipX;
            GameObject trail;
            if (weapon.Equals("fireball"))
            {
                if (spriteRenderer.flipX) trail = Instantiate(fireballTrailLeft, new Vector2(transform.position.x, transform.position.y), Quaternion.Euler(new Vector3(0, 0, 1)));
                else trail = Instantiate(fireballTrail, new Vector2(transform.position.x, transform.position.y), Quaternion.Euler(new Vector3(0, 0, 1)));
                trail.transform.SetParent(clone.transform);
            }
            else
            {
                trail = Instantiate(shurikenTrail, new Vector2(transform.position.x, transform.position.y), Quaternion.Euler(new Vector3(0, 0, 1)));
                trail.transform.SetParent(clone.transform);
            }
        }
        nbOfAmmo--;
        ammoNumber.text = nbOfAmmo + " / " + maxAmmo;
    }
    public void Die()
    {
        GameStats.SetSurvival(survivalTime);
        SceneManager.LoadScene("GameOver");
        AkSoundEngine.SetState("GameMod_states", "Nothing_state");
    }
    public float CheckDistance(Vector3 pos)
    {
        return Vector3.Distance(transform.position, pos);
    }
    public float CheckDistance(Vector2 pos)
    {
        return Vector2.Distance(transform.position, pos);
    }
    public void AddMommyAround()
    {
        mommiesAround++;
        if (mommiesAround >= 8) AkSoundEngine.SetSwitch("CoolorStrress_switch", "Stress", playEvent);
    }
    public void RemoveMommyAround()
    {
        mommiesAround--;
        if (mommiesAround < 8 && !forceIntenseMusic) AkSoundEngine.SetSwitch("CoolorStrress_switch", "Cool", playEvent);
    }
    public void ForceIntenseMusic()
    {
        AkSoundEngine.SetSwitch("CoolorStrress_switch", "Stress", playEvent);
    }
    public PlayerStats GetPlayerStats() => playerStats;

    /************* Private Functions ********************/
    // Move
    private void Move()
    {
        // Move on x
        float xSpeed = ninjaState == "jumping" ? moveHorizontal * ninjaSpeed * 0.8f : moveHorizontal * ninjaSpeed;
        if (isFreezed) xSpeed = xSpeed / 4;
        if (isSpeededUp) xSpeed = xSpeed * 2;
        if (xSpeed != 0 && ninjaState != "jumping" && ninjaState != "doubleJump" && ninjaState != "shooting") ninjaState = "running";
        if (xSpeed == 0 && ninjaState != "jumping" && ninjaState != "doubleJump" && ninjaState != "shooting") ninjaState = "idle";
        if (rigidbody2d.velocity.y < 0 && ninjaState != "shooting") ninjaState = "falling";
        if (rigidbody2d.velocity.y > 0.1 && ninjaState != "shooting" && ninjaState != "doubleJump") ninjaState = "jumping";
        rigidbody2d.velocity = new Vector2(xSpeed, rigidbody2d.velocity.y);

        // Check if falling too low
        if (rigidbody2d.position.y < -30) Die();
    }
    private void ShowFloatingText(GameObject text, string value)
    {
        GameObject clone = Instantiate(text, new Vector2(transform.position.x + Random.Range(-1.0f, 1.0f), transform.position.y + Random.Range(0.5f, 1.5f)), transform.rotation);
        clone.transform.SetParent(floatingTextCanvas.transform, false);
        clone.GetComponent<Text>().text = value;
        Destroy(clone, 0.50f);
    }

    // Set sprite depending on ninjaState
    private void SetSprite()
    {
        float timePerFrame = ninjaState.Equals("idle") ? 2.0f : ninjaState.Equals("doubleJump") ? 0.05f : 0.1f;
        if (spriteIndex >= spriteMap[ninjaState].Length)
        {
            spriteIndex = 0; // Reset spriteIndex to loop on the animation
        }
        spriteRenderer.sprite = spriteMap[ninjaState][spriteIndex];
        timeSinceFrameChanged += Time.deltaTime;
        if (timeSinceFrameChanged > timePerFrame)
        {
            timeSinceFrameChanged = 0;
            spriteIndex++;
        }
        // Flip if needed
        if (moveHorizontal != 0) spriteRenderer.flipX = moveHorizontal < 0;
    }

    // Check with a raycast if it is touching a platform
    private bool isGrounded()
    {
        RaycastHit2D raycastHit2D = Physics2D.BoxCast(hitbox.bounds.center, hitbox.bounds.size, 0f, Vector2.down, 0.2f, layerMask);
        return raycastHit2D.collider != null;
    }

    // Check which block is under and trigger corresponding action
    private void HandleBlock()
    {
        RaycastHit2D raycastHit2D = Physics2D.BoxCast(hitbox.bounds.center, hitbox.bounds.size, 0f, Vector2.down, 0.2f, layerMask);
        timeSinceLastSpeedUp += Time.deltaTime;
        if (raycastHit2D.collider != null)
        {
            if (raycastHit2D.collider.gameObject.tag.Equals("HealBlock"))
            {
                timeSinceLastHeal += Time.deltaTime;
                if (timeSinceLastHeal > timeBetweenHeals)
                {
                    Heal(1);
                    timeSinceLastHeal = 0.0f;
                }
            }
            if (raycastHit2D.collider.gameObject.tag.Equals("DamageBlock"))
            {
                timeSinceLastHit += Time.deltaTime;
                if (timeSinceLastHit > timeBetweenHits)
                {
                    TakeDamage(1);
                    timeSinceLastHit = 0.0f;
                }
            }
            if (raycastHit2D.collider.gameObject.tag.Equals("FreezeBlock"))
            {
                isFreezed = true;
            }
            else if (isFreezed)
            {
                isFreezed = false;
            }
            if (raycastHit2D.collider.gameObject.tag.Equals("ShieldBlock"))
            {
                isShielded = true;
            }
            else if (isShielded)
            {
                isShielded = false;
            }
            if (raycastHit2D.collider.gameObject.tag.Equals("SpeedBlock"))
            {
                isSpeededUp = true;
                timeSinceLastSpeedUp = 0.0f;
                if (speedTrail == null)
                {
                    speedTrail = Instantiate(speedTrailPrefab, transform.position, transform.rotation);
                    speedTrail.transform.SetParent(transform);
                }
            }
            else if (isSpeededUp && timeSinceLastSpeedUp >= GameSettings.GetDurationSpeedEffect())
            {
                isSpeededUp = false;
                Destroy(speedTrail);
                speedTrail = null;
            }
            if (raycastHit2D.collider.gameObject.tag.Equals("MovingBlock") || raycastHit2D.collider.gameObject.name.Contains("MovingBlock"))
            {
                transform.parent = raycastHit2D.collider.gameObject.transform;
            } else
            {
                transform.parent = null;
            }
            if (raycastHit2D.collider.gameObject.tag.Equals("JumpBlock"))
            {
                // TODO : SFX BOUNCE
            }
        }
        else if(transform.parent != null)
        {
            transform.parent = null;
        }
    }

    // Fix camera on player after spawn
    private void attachCamera()
    {
        ninjaState = "idle";
        GameObject.Find("Main Camera").GetComponent<CameraController>().AttachToPlayer();
    }

    // Collisions events
    private void OnCollisionEnter2D(Collision2D collision)
    {
        if (collision.gameObject.CompareTag("Rune"))
        {
            GameStats.AddRune();
            runesText.text = GameStats.runes.ToString();
            Destroy(collision.gameObject);
            AkSoundEngine.PostEvent("collectable_event", gameObject);
        }
        if (collision.gameObject.CompareTag("Ammo"))
        {
            nbOfAmmo = maxAmmo;
            ammoNumber.text = maxAmmo + " / " + maxAmmo;
            Destroy(collision.gameObject);
            AkSoundEngine.PostEvent("collectable_event", gameObject);
        }
        if (collision.gameObject.CompareTag("Thunder"))
        {
            TakeDamage(GameSettings.GetThunderDmg());
        }
    }

}
