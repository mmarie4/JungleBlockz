using UnityEngine;

public class NecroBulletController : BulletController
{

    void Start()
    {
        lifetime = 10f;
    }

    // Collisions events
    private void OnCollisionEnter2D(Collision2D collision)
    {
        if (active)
        {
            if (collision.gameObject.CompareTag("Player"))
            {
                Ninja player = collision.gameObject.GetComponent<Ninja>();
                float rand = Random.Range(0.0f, 1.0f);
                if (rand < critRatio) player.TakeDamage(shurikenDmg * 2);
                else player.TakeDamage(shurikenDmg);
            }

            if (!collision.gameObject.CompareTag("shuriken"))
            {
                active = false;
                Destroy(gameObject);
            }
        }
    }
}
