using UnityEngine;

public class BulletController : MonoBehaviour
{
    protected int direction = 1;
    protected int speed = 0;
    protected int shurikenDmg;
    protected float critRatio;
    protected Rigidbody2D rb;
    protected float lifetime = 2.0f;
    protected float lifetimeAfterCollision = 0.05f;

    protected bool active = true;

    protected float age = 0.0f;
    protected string type = "shuriken";
    private int numberOfEnemies = 0;
    private int maxNumberOfEnemies;

    // Start is called before the first frame update
    void Start()
    {
        //gameObject.GetComponent<Rigidbody2D>().angularVelocity = 720f;
    }

    // Update is called once per frame
    void FixedUpdate()
    {
        age += Time.deltaTime;
        if (age >= lifetime) Destroy(gameObject);

    }

    public void SetDirection(int newDir)
    {
        direction = newDir;
    }
    public void SetDamage(int dmg)
    {
        shurikenDmg = dmg;
    }
    public void SetCritRatio(float ratio)
    {
        critRatio = ratio;
    }
    public void SetSpeed(float speed, float vSpeed)
    {
        rb = gameObject.GetComponent<Rigidbody2D>();
        gameObject.GetComponent<Rigidbody2D>().velocity = new Vector2(speed * direction, vSpeed);
    }
    public void SetType(string t) => type = t;
    public void SetMaxNumberOfEnemies(int n) => maxNumberOfEnemies = n;


    // Collisions events
    protected virtual void OnTriggerEnter2D(Collider2D collision)
    {
        if (active)
        {
            if (collision.gameObject.CompareTag("Mommy"))
            {
                Enemy enemy = collision.gameObject.GetComponent<Enemy>();
                float rand = Random.Range(0.0f, 1.0f);
                if (rand < critRatio) enemy.TakeDamage(shurikenDmg * 2, true);
                else enemy.TakeDamage(shurikenDmg, false);
                enemy.SetTouched();
                // Fireball gives  a DOT
                if (type.Equals("fireball"))
                {
                    enemy.SetBurning();
                }
                numberOfEnemies++;
                if (numberOfEnemies >= maxNumberOfEnemies) Destroy(gameObject);
            }
            else if (!collision.gameObject.CompareTag("shuriken") && !collision.gameObject.name.Equals("RotatingBlockz"))
            {
                 Destroy(gameObject);
            }
        }
    }
}
