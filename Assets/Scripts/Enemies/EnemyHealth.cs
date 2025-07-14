using UnityEngine;

public class EnemyHealth : MonoBehaviour
{
    private Animator animator;
    [SerializeField]
    private float maxHealth = 3;
    private float health;

    private void Start()
    {
        animator = GetComponent<Animator>();
        health = maxHealth;
    }

    public void TakeDamage(float damage)
    {
        health -= damage;
        animator.SetTrigger("Hit");

        if (health < 0)
        {
            health = 0;
            GameObject.Destroy(gameObject);
        }

        Debug.Log("Enemy: " + health);
    }
}
