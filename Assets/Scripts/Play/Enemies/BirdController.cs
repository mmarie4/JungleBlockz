using System;
using System.Collections.Generic;
using UnityEngine;

public class BirdController : Enemy
{
    private Sprite[] flyingSprites;
    private Sprite[] dyingSprites;

    private int flyInertia = 0;


    // Start is called before the first frame update
    void Start()
    {
        base.Start();
        InitBurningDamages();
        InitStats("bird");
        rigidbody2d = GetComponent<Rigidbody2D>();
        // rigidbody2d.constraints = RigidbodyConstraints2D.FreezePositionX | RigidbodyConstraints2D.FreezePositionY | RigidbodyConstraints2D.FreezeRotation;
        hitbox = GetComponent<BoxCollider2D>();

        flyingSprites = Resources.LoadAll<Sprite>("Bird/Flying");
        dyingSprites = Resources.LoadAll<Sprite>("EnemyDying");
        state = "flying";
        spriteRenderer = gameObject.GetComponent<SpriteRenderer>();
        spriteMap = new Dictionary<string, Sprite[]>() { { "flying", flyingSprites }, { "dying", dyingSprites } };
    }

    void FixedUpdate()
    {
        if (!state.Equals("dying"))
        {
            Move();

            CheckDistanceWithPlayer(transform);

            CheckBurning();

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

    // Move
    protected override void Move()
    {
        if (state.Equals("flying") && Math.Abs(ninja.transform.position.x - gameObject.transform.position.x) > 2f) moveHorizontal = ninja.transform.position.x > gameObject.transform.position.x ? 1 : -1;
        float xSpeed = moveHorizontal * speed;
        float ySpeed = 0.0f;

        RaycastHit2D raycastHit2D = Physics2D.Raycast(hitbox.bounds.center, spriteRenderer.flipX ? Vector2.left : Vector2.right, 3.0f, blocksLayerMask);
        RaycastHit2D raycastHit2DDown = Physics2D.Raycast(hitbox.bounds.center, Vector2.down, 3.0f, blocksLayerMask);
        if (raycastHit2D.collider)
        {
            ySpeed = 2.0f;
            xSpeed = xSpeed / 2;
            flyInertia = 0;
        }
        else if (flyInertia < 20)
        {
            ySpeed = 2.0f;
            xSpeed = xSpeed / 2;
            flyInertia++;
        }
        else if (raycastHit2DDown.collider)
        {
            ySpeed = 0.8f;
        }

        rigidbody2d.velocity = new Vector2(xSpeed, ySpeed);
    }
}
