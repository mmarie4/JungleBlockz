using UnityEngine;

class ThunderBoltController : MonoBehaviour
{
    private float timeSinceStart = 0.0f;
    private float livingDuration = 0.3f;
    private float dmg;

    void Start()
    {
        dmg = GameSettings.GetEnemyDamagePerHit("thundergod");
    }

    void FixedUpdate()
    {
        timeSinceStart += Time.deltaTime;
        if (timeSinceStart >= livingDuration) Destroy(gameObject);
    }

    // Collisions events
    private void OnCollisionStay2D(Collision2D collision)
    {

            if (collision.gameObject.CompareTag("Player"))
            {
                Ninja player = collision.gameObject.GetComponent<Ninja>();
                player.TakeDamage((int)dmg);
            }

            if (!collision.gameObject.CompareTag("shuriken"))
            {
                Destroy(gameObject);
            }
    }
}
