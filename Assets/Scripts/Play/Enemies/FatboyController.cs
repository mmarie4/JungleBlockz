using System.Collections.Generic;
using UnityEngine;

public class FatboyController : Enemy
{
    private Sprite[] groundedSprites;
    private Sprite[] fallingSprites;
    private Sprite[] jumpingSprites;
    private Sprite[] dyingSprites;

    void Start()
    {
        base.Start();
        InitBurningDamages();
        InitStats("fatboy");
        rigidbody2d = GetComponent<Rigidbody2D>();
        // rigidbody2d.constraints = RigidbodyConstraints2D.FreezePositionX | RigidbodyConstraints2D.FreezePositionY | RigidbodyConstraints2D.FreezeRotation;
        hitbox = GetComponent<BoxCollider2D>();

        state = "grounded";

        groundedSprites = Resources.LoadAll<Sprite>("Fatboy/Grounded");
        fallingSprites = Resources.LoadAll<Sprite>("Fatboy/Falling");
        jumpingSprites = Resources.LoadAll<Sprite>("Fatboy/Jumping");
        dyingSprites = Resources.LoadAll<Sprite>("EnemyDying");
        spriteMap = new Dictionary<string, Sprite[]>() { { "falling", fallingSprites }, { "grounded", groundedSprites }, { "jumping", jumpingSprites }, { "dying", dyingSprites } };
        spriteRenderer = gameObject.GetComponent<SpriteRenderer>();

        // throw in the air when spawn
        rigidbody2d.AddForce(new Vector2(0, 100f), ForceMode2D.Impulse);
    }

    void FixedUpdate()
    {

        if (!state.Equals("dying"))
        {
            HandleBlockUnder();

            if (rigidbody2d.velocity.y < 0.1) state = "falling";
            if (rigidbody2d.velocity.y > 0.1) state = "jumping";
            if (isGrounded()) state = "grounded";

            if (state.Equals("jumping")) moveHorizontal = ninja.transform.position.x > gameObject.transform.position.x ? 1 : -1;
            if (state.Equals("grounded")) rigidbody2d.AddForce(new Vector2(0, 100f), ForceMode2D.Impulse);
            
            CheckBurning();

            Move();

            CheckDistanceWithPlayer(transform);

            // Check if falling too low
            if (rigidbody2d.position.y < -30) state = "dying";
        }
    }

    // Collisions events
    private void OnCollisionEnter2D(Collision2D collision)
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

    protected override bool isGrounded()
    {
        RaycastHit2D raycastHit2D = Physics2D.BoxCast(hitbox.bounds.center, hitbox.bounds.size, 0f, Vector2.down, 0.1f, blocksLayerMask);
        RaycastHit2D ninjaRaycastHit2D = Physics2D.BoxCast(hitbox.bounds.center, hitbox.bounds.size, 0f, Vector2.down, 0.1f, ninjaLayerMask);
        // HACK: If jumping on player, then collision is not triggered, so we take dmg here
        if (ninjaRaycastHit2D.collider != null) ninja.TakeDamage((int)(damagePerHit));
        return raycastHit2D.collider != null || ninjaRaycastHit2D.collider != null;
    }
}
