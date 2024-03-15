using System.Collections;
using System.Collections.Generic;
using System.Runtime.InteropServices.WindowsRuntime;
using UnityEngine;

public class Health : MonoBehaviour
{
    [SerializeField]

    private int health;
    private int maxHp;
    
    public void SetHealth(int maxHealth, int health)
    {
        this.maxHp = maxHealth;
        this.health = health;
    }


    public void Damage(int amount)
    {
        if (amount < 0)
        {
            return;
        }
        health -= amount;
        if (health <= 0)
        {
            Die();
        }
    }

    public bool Damage(int amount, bool isBullet)
    {
        if (isBullet)
        {
            if (amount < 0)
            {
            return false;
            }
                health -= amount;
            if (health <= 0)
            {
                return true;
            }
        }
        return false;
    }

    public void Heal(int amount)
    {
        if (amount < 0)
        {
            return;
        }
        if (health + amount > maxHp)
        {
            health = maxHp;
        }
        else
        {
            health += amount;
        }
        
    }

    private void Die()
    {
        Destroy(gameObject);
    }
}
