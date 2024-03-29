﻿using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class FinalMonster : MonoBehaviour
{
    [SerializeField] float moveSpeed;
    [SerializeField] Transform groundSensor;
    [SerializeField] Transform wallSensor;
    [SerializeField] Collider2D damageCollider;
    [SerializeField] int maxHP = 3;
    [SerializeField] float invulnerableDuration = 1.0f;
    [SerializeField] GameObject deathPrefab;

    Rigidbody2D rigidBody;
    SpriteRenderer sprite;
    int currentHP;
    float invulnerabilityTimer;
    public int health;
    public GameObject bloodEffect;
    public bool move;

    bool isInvulnerable
    {
        get
        {
            if (invulnerabilityTimer > 0.0f)
            {
                return true;
            }

            return false;
        }
    }

    private void OnTriggerEnter2D(Collider2D other)
    {
        if (other.gameObject.CompareTag("Player"))
        {
            move = true;
        }
    }

    void Start()
    {
        rigidBody = GetComponent<Rigidbody2D>();
        sprite = GetComponent<SpriteRenderer>();

        currentHP = maxHP;
    }

    void FixedUpdate()
    {
        if (move == true)
        {
            Vector3 currentVelocity = rigidBody.velocity;

            currentVelocity.x = -transform.right.x * moveSpeed;

            rigidBody.velocity = currentVelocity;

            if (Mathf.Abs(currentVelocity.y) < 0.1f)
            {
                Collider2D groundCollider = Physics2D.OverlapCircle(groundSensor.position, 2.0f, LayerMask.GetMask("Ground"));
                if (groundCollider == null)
                {
                    transform.rotation = transform.rotation * Quaternion.AngleAxis(180.0f, Vector3.up);
                }
            }

            Collider2D wallCollider = Physics2D.OverlapCircle(wallSensor.position, 2.0f, LayerMask.GetMask("Ground"));
            if (wallCollider)
            {
                transform.rotation = transform.rotation * Quaternion.AngleAxis(180.0f, Vector3.up);
            }

            if (damageCollider)
            {
                ContactFilter2D filter = new ContactFilter2D();
                filter.ClearLayerMask();
                filter.SetLayerMask(LayerMask.GetMask("Player"));

                Collider2D[] results = new Collider2D[8];

                int nCollision = Physics2D.OverlapCollider(damageCollider, filter, results);

                if (nCollision > 0)
                {
                    foreach (var collider in results)
                    {
                        if (collider)
                        {
                            Wad wad = collider.GetComponent<Wad>();
                            if (wad)
                            {
                                Vector3 hitDirection = (wad.transform.position - transform.position).normalized;
                                hitDirection.y = 1.0f;

                                wad.DamagePlayer(1);
                            }
                        }
                    }
                }
            }
        }      
    }

    private void Update()
    {
        if (invulnerabilityTimer > 0.0f)
        {
            invulnerabilityTimer -= Time.deltaTime;

            if (invulnerabilityTimer > 0.0f)
            {
                if ((Mathf.FloorToInt(invulnerabilityTimer * 10.0f) % 2) == 0) sprite.color = Color.red;
                else sprite.color = Color.white;
            }
            else
            {
                sprite.color = Color.white;
            }
        }

        if (health <= 0)
        {
            Destroy(gameObject);
        }
    }

    public void DealDamage(int damagePoints, Vector2 hitDirection)
    {
        if (isInvulnerable) return;

        currentHP = currentHP - damagePoints;

        if (currentHP <= 0)
        {
            Destroy(gameObject);
            Instantiate(deathPrefab, transform.position, transform.rotation);
        }

        invulnerabilityTimer = invulnerableDuration;
    }

    public void TakeDamage(int damage)
    {
        Instantiate(bloodEffect, transform.position, Quaternion.identity);
        health -= damage;
    }

    private void OnDrawGizmos()
    {
        if (groundSensor)
        {
            Gizmos.color = Color.red;
            Gizmos.DrawSphere(groundSensor.position, 2.0f);
        }

        if (wallSensor)
        {
            Gizmos.color = Color.yellow;
            Gizmos.DrawSphere(wallSensor.position, 2.0f);
        }
    }
}