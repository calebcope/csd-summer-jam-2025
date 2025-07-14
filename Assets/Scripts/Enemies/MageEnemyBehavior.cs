using UnityEngine;

public class MageEnemyBehavior : MonoBehaviour
{
    private Animator animator;

    [Header("Target & Rotation")]
    [SerializeField] private Transform player;

    [Header("Shooting")]
    [SerializeField] private Transform firePoint;
    [SerializeField] private GameObject projectilePrefab;
    [SerializeField] private float fireInterval = 2f;
    [SerializeField] private float range = 10f;

    private float lastFireTime;

    void Start()
    {
        animator = GetComponent<Animator>();
        if (player == null)
            player = GameObject.FindGameObjectWithTag("Player")?.transform;
        lastFireTime = Time.time;
    }

    void Update()
    {
        float dist = Vector2.Distance(transform.position, player.position);
        if (dist < range)
        {
            RotateTowardPlayer();
            TryFireProjectile();
        }
    }

    private void RotateTowardPlayer()
    {
        if (player == null)
            return;

        Vector2 direction = (player.position - transform.position).normalized;
        animator.SetFloat("XDir", direction.x);
        animator.SetFloat("YDir", direction.y);
    }

    private void TryFireProjectile()
    {
        if (Time.time - lastFireTime < fireInterval)
            return;

        FireProjectile();
        lastFireTime = Time.time;
    }

    private void FireProjectile()
    {
        if (projectilePrefab == null || firePoint == null)
            return;

        Instantiate(projectilePrefab, firePoint.position, firePoint.rotation);
    }
}
