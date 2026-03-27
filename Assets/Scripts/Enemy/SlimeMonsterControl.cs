using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class SlimeMonsterControl : MonoBehaviour
{
    Rigidbody2D enemyBody2D;
    public float enemySpeed;

    [Tooltip("Enemy'nin duvara �arp�p �arpmad���n�n bilgisini tutar.")]
    bool isGround = true;

    Transform groundCheck;
    const float GroundCheckRadius = .1f;

    [Tooltip("Duvar layer'�n� belirler.")]
    public LayerMask groundLayer;

    public bool moveRight;

    bool onEdge;
    Transform edgeCheck;

    void Start()
    {
        enemyBody2D = GetComponent<Rigidbody2D>();

        groundCheck = transform.Find("GroundCheck");
        edgeCheck = transform.Find("EdgeCheck");
    }


    void Update()
    {
        isGround = Physics2D.OverlapCircle(groundCheck.position, GroundCheckRadius, groundLayer);
        onEdge = Physics2D.OverlapCircle(edgeCheck.position, GroundCheckRadius, groundLayer);

        if (isGround || !onEdge)
            moveRight = !moveRight;

        enemyBody2D.linearVelocity = (moveRight) ? new Vector2(enemySpeed, 0) : new Vector2(-enemySpeed, 0);
        transform.localScale = (moveRight) ? new Vector2(-1, 1) : new Vector2(1, 1);
    }
}
