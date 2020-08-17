using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class KamikazController : Enemy
{
    private Sprite[] runningSprites;
    private Sprite[] spawningSprites;
    private Sprite[] fallingSprites;
    private Sprite[] jumpingSprites;
    private Sprite[] dyingSprites;

    private float jumpForce = 65f;
    private bool needsJump = false;


    // Start is called before the first frame update
    void Start()
    {
        base.Start();
        InitBurningDamages();
        InitStats("kamikaz");
        rigidbody2d = GetComponent<Rigidbody2D>();
        // rigidbody2d.constraints = RigidbodyConstraints2D.FreezePositionX | RigidbodyConstraints2D.FreezePositionY | RigidbodyConstraints2D.FreezeRotation;
        hitbox = GetComponent<BoxCollider2D>();
        runningSprites = Resources.LoadAll<Sprite>("Kamikaz/Running");
        fallingSprites = Resources.LoadAll<Sprite>("Kamikaz/Falling");
        jumpingSprites = Resources.LoadAll<Sprite>("Kamikaz/Jumping");
        dyingSprites = Resources.LoadAll<Sprite>("EnemyDying");
        state = "falling";
        spriteRenderer = gameObject.GetComponent<SpriteRenderer>();
        spriteMap = new Dictionary<string, Sprite[]>() { { "falling", fallingSprites }, { "running", runningSprites }, { "jumping", jumpingSprites }, { "dying", dyingSprites } };
    }

    void FixedUpdate()
    {
        if (!state.Equals("dying"))
        {
            if (rigidbody2d.velocity.y < 0.1 && state.Equals("running")) state = "falling";
            if (isGrounded() && (state.Equals("falling") || state.Equals("jumping"))) state = "running";

            if (state.Equals("running")) moveHorizontal = ninja.transform.position.x > gameObject.transform.position.x ? 1 : -1;

            if (state.Equals("running")) HandleBlock();
            if (state.Equals("jumping") || state.Equals("running")) Move();

            HandleBlockUnder();

            CheckBurning();

            // Jump
            if (needsJump)
            {
                needsJump = false;
                rigidbody2d.AddForce(new Vector2(0, jumpForce), ForceMode2D.Impulse);
                state = "jumping";
            }

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
            needsJump = true;
            rigidbody2d.velocity = new Vector2(0, 0);
        }
    }
}
