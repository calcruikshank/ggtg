using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using System.Linq;

public class Bullet : MonoBehaviour
{
    public bool isCrit = false;
    public float velocity;
    public Vector3 targetPosition;
    public GameObject critCanvas;

    public Vector3 startPosition;
    private float distMoved;
    public float attackDamage;
    private float projectileSize;
    private float projectileRange;
    private GameObject homingTarget = null;
    public PlayerController playerOwningBullet;
    public bool isHoming;
    bool hasHitPlayer = false;
    public GameObject puffParticles;
    // Start is called before the first frame update
  

    public void ShootAt(Vector3 endPosition)
    {
        startPosition = transform.localPosition;
        targetPosition = endPosition;
        distMoved = 0;
    }


    internal void Init(PlayerController playerOwningGun, Vector3 target, bool isCrit = false)
    {

        playerOwningBullet = playerOwningGun;
        projectileRange = 50;
        Vector3 endPosition = target + transform.forward * projectileRange;
        isHoming = false;
        ShootAt(endPosition);
        velocity = 20;
        attackDamage = 10;
        projectileSize = 1;
        this.isCrit = isCrit;
        if (isCrit)
        {
            attackDamage = attackDamage * 2;
            projectileSize = projectileSize * 1.5f;
        }
        transform.localScale = new Vector3(projectileSize, projectileSize, projectileSize);
    }

    // Update is called once per frame
    void Update()
    {
        if (homingTarget != null)
        {
            targetPosition = homingTarget.transform.position;
        }
        distMoved += Time.deltaTime * velocity;
        //float percentMoved = distMoved/Vector3.Distance(targetPosition,startPosition);
        //transform.localPosition = Vector3.Lerp(startPosition, targetPosition, percentMoved);
        transform.localPosition = Vector3.MoveTowards(transform.localPosition, targetPosition, Time.deltaTime * velocity);
        if (Vector3.Distance(transform.localPosition, targetPosition) < 0.001f || distMoved >= projectileRange)
        {
            Die();
        }
    }

    private void OnTriggerEnter(Collider other)
    {
        if (hasHitPlayer) return;
        if (other.GetComponent<PlayerController>() != null)
        {
            hasHitPlayer = true;
            var objectHit = other.GetComponent<PlayerController>();
            if (objectHit != playerOwningBullet)
            {
                objectHit.TakeDamage(attackDamage);
                if (critCanvas != null)
                {
                    GameObject oof = Instantiate(critCanvas, transform.position, Camera.main.transform.rotation);
                   
                }
                Die();
            }
        }
    }

    private void OnCollisionEnter(Collision collision)
    {
    }

    public void Die()
    {
        Instantiate(puffParticles, transform.position, Quaternion.identity, null);
        Destroy(this.gameObject);
    }
}