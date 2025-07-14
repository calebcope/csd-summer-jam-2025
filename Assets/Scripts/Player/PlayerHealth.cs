using UnityEngine;
using UnityEngine.SceneManagement;

public class PlayerHealth : MonoBehaviour
{
    private Animator animator;
    [SerializeField]
    private float maxHealth = 6;
    private float health;

    private void Start()
    {
        animator = GetComponent<Animator>();
        health = maxHealth;
    }

    public float CurrentHealth => health;
    public float MaxHealth => maxHealth;
    public event System.Action OnHealthChanged;

    public void TakeDamage(float dmg)
    {
        health = Mathf.Max(0, health - dmg);
        animator.SetTrigger("Hit");
        OnHealthChanged?.Invoke();
        if (health == 0)
        {
            SceneManager.LoadScene(SceneManager.GetActiveScene().name);
        }
    }
    public void HealDamage(float amt)
    {
        health = Mathf.Min(maxHealth, health + amt);
        OnHealthChanged?.Invoke();
    }
}
