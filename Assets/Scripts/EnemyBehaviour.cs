using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.SceneManagement;

public class EnemyBehaviour : MonoBehaviour
{

    RuntimeAnimatorController deathAnim;
    Animator animator;
    SpriteRenderer sRenderer;

    [SerializeField]
    private int dmg;
    [SerializeField]
    private float speed;
    [SerializeField]
    public EnemyData data;

    public Rect spawnRoom;
    private GameObject player;
    private bool flipped = false;
    public bool isAlive = true;

     

    void Start()
    {
        player = GameObject.FindGameObjectWithTag("Player");
        sRenderer = gameObject.GetComponent<SpriteRenderer>();
        animator = gameObject.GetComponent<Animator>();
        deathAnim = Resources.Load("Animations/Goblin_Death_Controller") as RuntimeAnimatorController;
        SetEnemyData();
    }

    void Update()
    {
        if (isAlive)
        {
            LocatePlayer();
        } 
    }

    private void SetEnemyData()
    {
        GetComponent<Health>().SetHealth(data.hp, data.hp);
        dmg = data.dmg;
        speed = data.speed;
    }

    private void LocatePlayer()
    {
        if(player.transform.position.x < transform.position.x)
        {
            flipped = true;
        }
        else
        {
            flipped = false;
        }
        transform.position = Vector2.MoveTowards(transform.position, player.transform.position, speed * Time.deltaTime);
        sRenderer.flipX = flipped;
    }

    private void OnCollisionEnter2D(Collision2D collision)
    {
        gameObject.GetComponent<Rigidbody2D>().velocity = Vector3.zero;
        if (collision.collider.CompareTag("Player"))
        {
            if(collision.collider.GetComponent<Health>() != null)
            {
                collision.collider.GetComponent<Health>().Damage(dmg);
            }
        }
    }

    public void Die()
    {
        isAlive = false;
        animator.runtimeAnimatorController = deathAnim;
        StartCoroutine(DestroyEnemy(0.75f));
    }

    IEnumerator DestroyEnemy(float time)
    {
        yield return new WaitForSeconds(time);
        Destroy(gameObject);
    }

}