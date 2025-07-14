using UnityEngine;

public class MageProjectile : MonoBehaviour
{
    private Rigidbody2D rb;
    private GameObject player;
    [SerializeField]
    private float speed = 3f;
    [SerializeField]
    private float timeToDespawn = 4f;
    [SerializeField]
    private float damage = 2f;
    float spawnTime;
    

    void Start()
    {
        spawnTime = Time.time;
        rb = GetComponent<Rigidbody2D>();
        player = GameObject.FindGameObjectWithTag("Player");
    }

    void Update()
    {
        if (Time.time - spawnTime >= timeToDespawn)
        {
            Destroy(gameObject);
            return;
        }

        // 2) Move the projectile
        Vector2 dir = (player.transform.position - transform.position).normalized;
        rb.linearVelocity = dir * speed;
    }

    private void OnTriggerEnter2D(Collider2D collision)
    {
        if (collision == player.GetComponent<Collider2D>())
        {
            player.GetComponent<PlayerHealth>()?.TakeDamage(damage);
            Destroy(gameObject);
        }
    }
}
