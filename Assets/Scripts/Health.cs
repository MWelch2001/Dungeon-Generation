using System.Collections;
using System.Collections.Generic;
using System.Runtime.InteropServices.WindowsRuntime;
using UnityEngine;
using UnityEngine.UI;

public class Health : MonoBehaviour
{
    [SerializeField]
    private Sprite[] healthBar;
    private Image healthUI;
    public int health;
    public int maxHp;


    private void Start()
    {
        healthUI = GameObject.Find("Image").GetComponent<Image>();
    }

    public void SetHealth(int maxHealth, int health)
    {
        this.maxHp = maxHealth;
        this.health = health;
    }


    public bool Damage(int amount)
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
        if (health > 0)
        {
            healthUI.sprite = healthBar[(health/ 5)];
        }
        return false;
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
}