using System.Collections.Generic;
using UnityEngine;

public class MommyController : Enemy
{
    private Sprite[] walkingSprites;
    private Sprite[] fallingSprites;
    private Sprite[] croutchingSprites;
    private Sprite[] jumpingSprites;
    private Sprite[] dyingSprites;

    private float timeSinceCroutch = 0.0f;
    private float crouchDuration = 0.4f;

    private float jumpForce = 65f;


    // Start is called before the first frame update
    void Start()
    {
        base.Start();
        InitBurningDamages();
        InitStats("mommy");
        rigidbody2d = GetComponent<Rigidbody2D>();
        hitbox = GetComponent<BoxCollider2D>();

        state = "walking";

        walkingSprites = Resources.LoadAll<Sprite>("Mommy/Walking");
        fallingSprites = Resources.LoadAll<Sprite>("Mommy/Falling");
        croutchingSprites = Resources.LoadAll<Sprite>("Mommy/Croutching");
        jumpingSprites = Resources.LoadAll<Sprite>("Mommy/Jumping");
        dyingSprites = Resources.LoadAll<Sprite>("EnemyDying");

        gameObject.GetInstanceID();

        spriteRenderer = gameObject.GetComponent<SpriteRenderer>();
        spriteMap = new Dictionary<string, Sprite[]>() {{ "falling", fallingSprites }, { "walking", walkingSprites }, { "jumping", jumpingSprites }, { "croutching", croutchingSprites }, { "dying", dyingSprites } };
    }


    void FixedUpdate()
    {
        if (!state.Equals("dying"))
        {
            if (rigidbody2d.velocity.y < 0.1 && state.Equals("walking")) state = "falling";
            if (isGrounded() && (state.Equals("falling") || state.Equals("jumping"))) state = "walking";

            if (state.Equals("walking")) moveHorizontal = ninja.transform.position.x > gameObject.transform.position.x ? 1 : -1;

            if (state.Equals("croutching"))
            {
                timeSinceCroutch += Time.deltaTime;
                if (timeSinceCroutch >= crouchDuration || rigidbody2d.velocity.y < -0.1)
                {
                    rigidbody2d.AddForce(new Vector2(0, jumpForce), ForceMode2D.Impulse);
                    state = "jumping";
                }
            }

            if (state.Equals("walking")) HandleBlock();
            HandleBlockUnder();
            if (state.Equals("jumping") || state.Equals("walking")) Move();

            CheckBurning();

            CheckDistanceWithPlayer(transform);

            // Check if falling too low
            if (rigidbody2d.position.y < -30) state = "dying";
        }
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

    // Jump when see block
    private void HandleBlock()
    {
        RaycastHit2D ninjaRaycastHit2D = Physics2D.Raycast(hitbox.bounds.center, spriteRenderer.flipX ? Vector2.left : Vector2.right, 1.0f, ninjaLayerMask);
        RaycastHit2D raycastHit2D = Physics2D.Raycast(hitbox.bounds.center, spriteRenderer.flipX ? Vector2.left : Vector2.right, 1.0f, blocksLayerMask);
        if (raycastHit2D.collider && ninjaRaycastHit2D.collider == null)
        {
            state = "croutching";
            rigidbody2d.velocity = new Vector2(0, 0);
            timeSinceCroutch = 0.0f;
        }
    }
}
