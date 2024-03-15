using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class BulletBehaviour : MonoBehaviour
{
    public int bulletDmg = 5;
   public void OnCollisionEnter2D(Collision2D collision)
    {
        bool isDead = false;
        if (collision.collider.CompareTag("Enemy"))
        {
            if (collision.collider.GetComponent<Health>() != null)
            {
                isDead = collision.collider.GetComponent<Health>().Damage(bulletDmg, true);
                collision.rigidbody.velocity = Vector3.zero;
            }
        }
        if (isDead)
        {
            collision.rigidbody.velocity = Vector3.zero;
            collision.rigidbody.isKinematic = false;
            collision.collider.GetComponent<EnemyBehaviour>().Die();
        }
        Destroy(gameObject);  
    }
}