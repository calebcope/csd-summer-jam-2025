using UnityEngine;

public class PlayerController : MonoBehaviour
{
    private Rigidbody2D rb;
    private Animator animator;
    private SpriteRenderer spriteRenderer;

    [Header("Swipe Hitboxes")]
    [SerializeField]
    private Transform upPoint, downPoint, leftPoint, rightPoint;
    [SerializeField]
    private LayerMask enemyLayer;

    [Header("Movement")]
    private Vector2 moveDirection;
    public float speed = 3f;

    [Header("Attack")]
    public Vector2 daggerHitboxSize = new Vector2(.6f, .2f);
    public float daggerDamage = 1f;
    public float daggerCooldown = 1f;
    float lastAttackTime = -Mathf.Infinity;

    private void Start()
    {
        rb = GetComponent<Rigidbody2D>();
        animator = GetComponent<Animator>();
        spriteRenderer = GetComponent<SpriteRenderer>();
    }

    void Update()
    {
        SetDirection();
        if (Input.GetMouseButtonDown(0) && Time.time - lastAttackTime >= daggerCooldown)
            Attack();
    }
    private void FixedUpdate()
    {
       MovePlayer();
    }

    void LateUpdate()
    {
        spriteRenderer.sortingOrder = -Mathf.RoundToInt(transform.position.y * 100);
    }

    public void SetDirection()
    {
        moveDirection = new Vector2(Input.GetAxisRaw("Horizontal"), Input.GetAxisRaw("Vertical")).normalized;

        if (moveDirection != Vector2.zero)
        {
            animator.SetFloat("XInput", moveDirection.x);
            animator.SetFloat("YInput", moveDirection.y);
        }
    }

    public void MovePlayer()
    {
        rb.linearVelocity = moveDirection * speed;

        animator.SetBool("Walking", moveDirection != Vector2.zero);
    }

    public void Attack() 
    {
        DaggerAttack();
        animator.SetTrigger("Attack");
    }

    public void DaggerAttack()
    {
        Transform hitPoint = moveDirection switch
        {
            var d when d == Vector2.up => upPoint,
            var d when d == Vector2.down => downPoint,
            var d when d == Vector2.left => leftPoint,
            var d when d == Vector2.right => rightPoint,
            _ => upPoint
        };

        float angle = Vector2.SignedAngle(Vector2.up, moveDirection);

        Collider2D[] hits = Physics2D.OverlapBoxAll(
            hitPoint.position,
            daggerHitboxSize,
            angle,
            enemyLayer
        );

        foreach (var enemyCol in hits)
        {
            enemyCol.GetComponent<EnemyHealth>()?.TakeDamage(daggerDamage);
        }

        lastAttackTime = Time.time;
    }

    void OnDrawGizmos()
    {
        Vector2 hitboxRotate = new Vector2(daggerHitboxSize.y, daggerHitboxSize.x);

        Gizmos.color = Color.red;
        Gizmos.DrawWireCube(upPoint.position, daggerHitboxSize);
        Gizmos.DrawWireCube(downPoint.position, daggerHitboxSize);
        Gizmos.DrawWireCube(leftPoint.position, hitboxRotate);
        Gizmos.DrawWireCube(rightPoint.position, hitboxRotate);
    }
}
