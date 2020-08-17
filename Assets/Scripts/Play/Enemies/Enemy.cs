using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class Enemy : MonoBehaviour
{

    /*
     * 
     * PARENT CLASS FOR ALL ENEMY CONTROLLERS
     * 
     */

    private string type;

    [SerializeField] protected LayerMask blocksLayerMask;
    [SerializeField] protected LayerMask ninjaLayerMask;
    protected float speed;
    protected int hp;
    protected int maxHp;
    protected Rigidbody2D rigidbody2d;
    protected BoxCollider2D hitbox;
    protected SpriteRenderer spriteRenderer;
    protected Dictionary<string, Sprite[]> spriteMap;
    protected int spriteIndex = 0;
    protected string state;
    protected float moveHorizontal = 0.0f;
    protected float moveVertical = 0.0f;
    protected float timeSinceFrameChanged = 0;
    protected float timePerFrame = 0.1f;
    protected int dyingFrame = 0;
    protected bool isAround = false;

    protected float timeSinceLastHit = 0.0f;
    protected float timeBetweenHits;
    protected float damagePerHit;

    protected float timeSinceLastBlockHit = 0.0f;
    protected float timeBetweenBlockHits;
    protected float timeSinceLastBlockHeal = 0.0f;
    protected float timeBetweenBlockHeals;

    protected Ninja ninja;

    protected Canvas floatingTextCanvas;
    protected GameObject damageText;
    protected GameObject healingText;
    protected bool isFreezed;
    protected bool isShielded;
    protected bool isSpeededUp;
    protected float timeSinceLastSpeedUp;
    public GameObject speedTrailPrefab;
    protected GameObject speedTrail;

    protected bool isTouched; // Only get XP if enemy is touched by player before dying
    protected bool isBurning = false;
    protected float timeSinceStartBurning = 0.0f;
    protected float burningDuration;
    protected float timeSinceLastBurn = 0.0f;
    protected float timeBetweenBurns = 0.25f;
    protected float burnDamage;
    protected bool needInitBurnDamage = false;

    protected void Start()
    {
        ninja = GameObject.Find("Ninja").GetComponent<Ninja>();

        timeBetweenBlockHits = 1 / GameSettings.GetBlocksDps();
        timeBetweenBlockHeals = 1 / GameSettings.GetBlocksHps();

        floatingTextCanvas = GameObject.Find("FloatingTextCanvas").GetComponent<Canvas>();
        damageText = Resources.Load<GameObject>("prefab/DamageText");
        healingText = Resources.Load<GameObject>("prefab/HealingText");

        isTouched = false;
        isFreezed = false;
        isShielded = false;
    }

    /* For bosses, we don't want to do this in Start, because Ninja may not have loaded the playerStats */
    /* So this function is only called in normal enemies */
    protected void InitBurningDamages()
    {
        string weapon = ninja.GetPlayerStats().GetWeapon();
        burnDamage = GameSettings.GetItemDps(weapon) * (ninja.GetPlayerStats().GetStrength() * 0.1f + 1) / 4; // Divide by 4 since timeBetweenBurns = 0.25
        burningDuration = GameSettings.GetItemDotDuration(weapon);
    }



    // Update is called once per frame
    protected void Update()
    {
        SetSprite();
    }

    protected void InitStats(string pType)
    {
        type = pType;
        maxHp = GameSettings.GetEnemyHp(type);
        damagePerHit = GameSettings.GetEnemyDamagePerHit(type);
        timeBetweenHits = damagePerHit / GameSettings.GetEnemyDps(type);
        hp = maxHp;
        speed = GameSettings.GetEnemySpeed(type);
    }

    public void TakeDamage(int dmg, bool isCrit)
    {
        if (!isTouched && hp <= 5) return;
        if (isShielded)
        {
            float rand = Random.Range(0.0f, 1.0f);
            if (rand > 0.3f) return;
        }
        hp -= dmg;
        ShowFloatingText(damageText, dmg.ToString(), isCrit);
        if (hp <= 0)
        {
            state = "dying";
            Destroy(rigidbody2d);
            gameObject.layer = 13;
        }
    }
    public void SetTouched() => isTouched = true;
    public void SetBurning()
    {
        isBurning = true;
        timeSinceStartBurning = 0.0f;
    }
    public void Heal(int heal)
    {
        if (hp < maxHp)
        {
            ShowFloatingText(healingText, heal.ToString(), false);
            hp += heal;
        }
    }
    protected void CheckBurning()
    {
        if (isBurning && timeSinceStartBurning <= burningDuration)
        {
            timeSinceStartBurning += Time.deltaTime;
            timeSinceLastBurn += Time.deltaTime;
            if (timeSinceLastBurn > timeBetweenBurns)
            {
                timeSinceLastBurn = 0.0f;
                TakeDamage((int)burnDamage, false);
            }
            if (timeSinceStartBurning >= burningDuration) isBurning = false;
        }
    }

    protected void SetSprite()
    {
        /* TODO: for states that should finish after all sprites are shown, remove them from here and handle it in children */
        /* No states should be hardcoded here */
        if (spriteIndex >= spriteMap[state].Length) spriteIndex = 0;
        spriteRenderer.sprite = spriteMap[state][spriteIndex];
        timeSinceFrameChanged += Time.deltaTime;
        if (timeSinceFrameChanged > timePerFrame)
        {
            timeSinceFrameChanged = 0;
            // Die
            if (spriteIndex >= spriteMap[state].Length - 1 && state.Equals("dying"))
            {
                ninja.RemoveMommyAround();
                Destroy(gameObject);
                if (isTouched) GameStats.AddKill();
            }
            // Loading attack - Necromancer
            if (spriteIndex >= spriteMap[state].Length - 1 && state.Equals("loading"))
            {
                state = "loaded";
            }
            spriteIndex = spriteIndex >= spriteMap[state].Length - 1 ? 0 : spriteIndex + 1;
        }
        // Flip if needed
        if (moveHorizontal != 0) spriteRenderer.flipX = moveHorizontal < 0;
    }

    // Check which block is under and trigger corresponding action
    protected void HandleBlockUnder()
    {
        RaycastHit2D raycastHit2D = Physics2D.BoxCast(hitbox.bounds.center, hitbox.bounds.size, 0f, Vector2.down, 0.2f, blocksLayerMask);
        timeSinceLastSpeedUp += Time.deltaTime;
        if (raycastHit2D.collider != null)
        {
            if (raycastHit2D.collider.gameObject.tag.Equals("HealBlock"))
            {
                timeSinceLastBlockHeal += Time.deltaTime;
                if (timeSinceLastBlockHeal > timeBetweenBlockHeals)
                {
                    Heal(1);
                    timeSinceLastBlockHeal = 0.0f;
                }
            }

            if (raycastHit2D.collider.gameObject.tag.Equals("DamageBlock"))
            {
                timeSinceLastBlockHit += Time.deltaTime;
                if (timeSinceLastBlockHit > timeBetweenHits)
                {
                    TakeDamage(1, false);
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
        }
    }

    // Private methods
    protected virtual void Move()
    {
        float xSpeed = moveHorizontal * speed;
        float ySpeed = moveVertical != 0 ? moveVertical * speed : rigidbody2d.velocity.y;
        if (isFreezed) xSpeed = xSpeed / 4;
        if (isSpeededUp) xSpeed = xSpeed * 2;
        rigidbody2d.velocity = new Vector2(xSpeed, ySpeed);
    }

    protected void ShowFloatingText(GameObject text, string value, bool isCrit)
    {
        GameObject clone = Instantiate(text, new Vector2(transform.position.x + Random.Range(-1.0f, 1.0f), transform.position.y + Random.Range(0.5f, 1.5f)), transform.rotation);
        clone.transform.SetParent(floatingTextCanvas.transform, false);
        if (isCrit) clone.transform.localScale = clone.transform.localScale * 1.5f;
        clone.GetComponent<Text>().text = value;
        Destroy(clone, 0.50f);
    }

    protected virtual bool isGrounded()
    {
        RaycastHit2D raycastHit2D = Physics2D.BoxCast(hitbox.bounds.center, hitbox.bounds.size, 0f, Vector2.down, 0.1f, blocksLayerMask);
        RaycastHit2D ninjaRaycastHit2D = Physics2D.BoxCast(hitbox.bounds.center, hitbox.bounds.size, 0f, Vector2.down, 0.1f, ninjaLayerMask);
        return raycastHit2D.collider != null || ninjaRaycastHit2D.collider != null;
    }

    protected void CheckDistanceWithPlayer(Transform transform)
    {
        // Check distance with player, to decide intensity of music
        if (ninja.CheckDistance(transform.position) < 10.0f)
        {
            if (!isAround)
            {
                isAround = true;
                ninja.AddMommyAround();
            }
        }
        else
        {
            if (isAround)
            {
                isAround = false;
                ninja.RemoveMommyAround();
            }
        }
    }

}
