using System;
using System.Collections;
using UnityEngine;
using UnityEngine.EventSystems;

public class DaggerEnemyBehavior : MonoBehaviour
{
    enum State { Idle, Chase, Attack }
    State state = State.Idle;

    private Transform player;
    private SpriteRenderer spriteRenderer;
    private Rigidbody2D rb;
    private Animator animator;

    [Header("Swipe Hitboxes")]
    [SerializeField] private Transform upPoint, downPoint, leftPoint, rightPoint;
    [SerializeField] private Vector2 daggerHitboxSize = new Vector2(.6f, .2f);
    [SerializeField] private LayerMask playerLayer;
    [SerializeField] private float daggerDamage = 1f;

    [Header("Movement")]
    public float speed = 2f;
    public float agroRadius = 5f;
    public float attackRadius = 1.5f;

    [Header("Attack")]
    public float attackCooldown = 1f;
    float lastAttackTime = -Mathf.Infinity;

    void Start()
    {
        player = GameObject.FindGameObjectWithTag("Player").transform;
        spriteRenderer = GetComponent<SpriteRenderer>();
        rb = GetComponent<Rigidbody2D>();
        animator = GetComponent<Animator>();
    }

    void Update()
    {
        float dist = Vector2.Distance(transform.position, player.position);

        switch (state)
        {
            case State.Idle:
                EnterChase(dist);
                break;

            case State.Chase:
                EnterIdle(dist);
                EnterAttack(dist);
                break;

            case State.Attack:
                break;
        }
    }

    void FixedUpdate()
    {
        if (state == State.Chase)
            Chase();
        else
            rb.linearVelocity = Vector2.zero;
    }

    void LateUpdate()
    {
        spriteRenderer.sortingOrder = -Mathf.RoundToInt(transform.position.y * 100);
    }
    void EnterChase(float dist)
    {
        if (dist <= agroRadius)
        {
            state = State.Chase;
            animator.SetBool("Walking", true);
        }
    }

    void EnterIdle(float dist)
    {
        if (dist > agroRadius)
        {
            state = State.Idle;
            animator.SetBool("Walking", false);
        }
    }

    void EnterAttack(float dist)
    {
        if (dist <= attackRadius && Time.time - lastAttackTime >= attackCooldown)
            Attack();
    }

    void Chase()
    {
        Vector2 dir = (player.position - transform.position).normalized;
        rb.linearVelocity = dir * speed;

        animator.SetFloat("XDirection", dir.x);
        animator.SetFloat("YDirection", dir.y);
        animator.SetBool("Walking", true);
    }

    private void Attack()
    {
        StopAllCoroutines();
        StartCoroutine(AttackRoutine());
    }

    private IEnumerator AttackRoutine()
    {
        state = State.Attack;
        rb.linearVelocity = Vector2.zero;
        animator.SetBool("Walking", false);

        animator.SetTrigger("Attack");

        DaggerAttack();
        lastAttackTime = Time.time;
        
        var animState = animator.GetCurrentAnimatorStateInfo(0);
        yield return new WaitForSeconds(0.2f);

        state = State.Chase;
        animator.SetBool("Walking", true);
    }

    public void DaggerAttack()
    {
        Vector2 dir = (player.position - transform.position).normalized;

        Transform hitPoint = dir switch
        {
            var d when Vector2.Dot(d, Vector2.up) > 0.7f => upPoint,
            var d when Vector2.Dot(d, Vector2.down) > 0.7f => downPoint,
            var d when Vector2.Dot(d, Vector2.left) > 0.7f => leftPoint,
            var d when Vector2.Dot(d, Vector2.right) > 0.7f => rightPoint,
            _ => upPoint
        };
        float angle = Vector2.SignedAngle(Vector2.up, dir);

        Collider2D[] hits = Physics2D.OverlapBoxAll(
            hitPoint.position,
            daggerHitboxSize,
            angle,
            playerLayer
        );

        foreach (var c in hits)
            c.GetComponent<PlayerHealth>()?.TakeDamage(daggerDamage);
    }
}