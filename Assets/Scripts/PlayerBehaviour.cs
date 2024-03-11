using System.Collections;
using System.Collections.Generic;
using Unity.Mathematics;
using UnityEngine;

public class PlayerBehaviour : MonoBehaviour
{
    SpriteRenderer sRenderer;
    Animator animator;
    RuntimeAnimatorController idleAnim;
    RuntimeAnimatorController walkAnim;
    RuntimeAnimatorController shootAnim;
    Vector3 mov;
    Vector3 aim;
    Vector2 fireDirection;
    public float runSpeed = 10f;
    private float fireRate = 0.5f;
    public Rigidbody2D rb;
    public GameObject crosshair;
    public GameObject bulletPrefab;
    bool flipped = false;
    bool isFiring;
    bool endOfFiring;


    private void Awake()
    {
        Cursor.lockState = CursorLockMode.Locked;
        Cursor.visible = false;
    }
    void Start()
    {
        sRenderer = GetComponent<SpriteRenderer>();
        animator = GetComponent<Animator>();
        idleAnim = Resources.Load("Animations/Shadow_Idle_Controller") as RuntimeAnimatorController;
        walkAnim= Resources.Load("Animations/Shadow_Walk_Controller") as RuntimeAnimatorController;
        shootAnim = Resources.Load("Animations/Shadow_Shoot_Controller") as RuntimeAnimatorController;
    }

    
    void Update()
    {
        ProcessInputs();
        Movement();
        Fire();
        ChangeAnimation();
    }

    private void ProcessInputs()
    {
        mov = new Vector3(Input.GetAxis("Horizontal"), Input.GetAxis("Vertical"), 0.0f);
        Vector3 mouseMov = new Vector3(Input.GetAxis("Mouse X"), Input.GetAxis("Mouse Y"), 0.0f);
        aim += mouseMov;
        flipped = CheckFlip(aim);
        sRenderer.flipX = flipped;
        isFiring = Input.GetButtonDown("Fire1");
        endOfFiring = Input.GetButtonUp("Fire1");

        if (mov.magnitude > 1.0f)
        {
            mov.Normalize();
        }
    }

    private void Fire()
    {
        fireDirection = new Vector2(aim.x, aim.y);
        if (aim.magnitude > 0.0f)
        {
            crosshair.transform.localPosition = aim * 0.4f;
            crosshair.SetActive(true);
            fireDirection.Normalize();
            if (isFiring)
            {
                InvokeRepeating("InstantiateBullet", 0.001f, fireRate);
            }else if (endOfFiring)
            {
                CancelInvoke("InstantiateBullet");
            }      
        }
        else
        {
            crosshair.SetActive(false);
        }
    }

    private void InstantiateBullet()
    {
                GameObject bullet = Instantiate(bulletPrefab, transform.position, Quaternion.identity);
                bullet.GetComponent<Rigidbody2D>().velocity = fireDirection * 10.0f;
                Destroy(bullet, 2.0f);
    }

    private void Movement()
    {
        rb.velocity = new Vector2(mov.x, mov.y) * runSpeed;
    }
     
    private void ChangeAnimation()
    {
        if (!Input.anyKey)
        {
            animator.runtimeAnimatorController = idleAnim;
        }
        if (IsFiring())
        {
            animator.runtimeAnimatorController = shootAnim;
        }
        if (mov.magnitude > 0 && !IsFiring())
        {
            animator.runtimeAnimatorController = walkAnim;
        }
    }

    private bool IsFiring()
    {
        if (Input.GetButton("Fire1"))
        {
            return true;
        }
        else
        {
            return false;
        }
    }

    private bool CheckFlip(Vector3 mouseMov)
    {
        if (mouseMov.x < transform.position.x)
        {
            return true;
        }
        return false;
    }
}