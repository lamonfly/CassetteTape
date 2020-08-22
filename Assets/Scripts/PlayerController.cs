using System.Collections;
using System.Collections.Generic;
using System.Security.Cryptography;
using System.Text.RegularExpressions;
using UnityEngine;
using UnityEngine.UI;

public class PlayerController : MonoBehaviour
{
    [Header("Horizontal Movement")]
    public float moveSpeed;
    public Vector2 direction;
    private bool facingRight = true;
    public Vector2 movingAxe;

    [Header("Vertical Movement")]
    public float jumpSpeed;
    public float jumpDelay;
    private float jumpTimer;
    private bool onAction = false;

    [Header("Physics")]
    public float maxSpeed;
    public float linearDrag;
    public float gravity;
    public float fallMultiplier;
    public bool hooked = false;

    [Header("Collision")]
    public LayerMask groundLayer;
    public bool grounded = false;
    public Vector3 colliderOffset;
    public Vector2 colliderBox;

    [Header("Component")]
    public GameObject characterHolder;
    private Rigidbody2D rb;
    private Animator anim;

    void Start()
    {
        // Get Components
        rb = GetComponent<Rigidbody2D>();
        anim = GetComponentInChildren<Animator>();
    }

    void Update()
    {
        // Inputs
        direction.x = Input.GetAxis("Horizontal");
        direction.y = Input.GetAxis("Vertical");

        if (Input.GetButtonDown("Jump"))
            jumpTimer = Time.time + jumpDelay;

        // Check for ground on both sides of collider
        bool wasGrounded = grounded;
        grounded = Physics2D.OverlapBox(transform.position + colliderOffset, colliderBox, 0, groundLayer);
        // Set direction of sideway movement
        if (grounded)
        {
            var target = transform.position + colliderOffset;
            target.y -= colliderBox.y * 2;
            var groundCollision = Physics2D.Linecast(transform.position, target, groundLayer);
            movingAxe = -Vector2.Perpendicular(groundCollision.normal).normalized;
        }
        else
        {
            movingAxe = Vector2.right;
        }
        anim.SetBool("Grounded", grounded);

        // Player just landed
        if (!wasGrounded && grounded)
        {
            StartCoroutine(jumpSqueeze(new Vector3(0.9f, 1.1f, 1), 0.1f));
            SoundManager.PlaySound(SoundManager.Sound.playerLanding, transform.position);
        }
    }

    void FixedUpdate()
    {
        moveCharacter(direction.x);
        if (jumpTimer > Time.time && grounded)
        {
            Jump();
        }
        modifyPhysics();
    }

    // Move character left or right
    void moveCharacter(float horizontal)
    {
        rb.AddForce(movingAxe * horizontal * moveSpeed);

        if ((horizontal > 0 && !facingRight) || (horizontal < 0 && facingRight))
        {
            Flip();
        }

        if (!hooked)
            rb.velocity = new Vector2(Mathf.Clamp(rb.velocity.x, -maxSpeed, maxSpeed), rb.velocity.y);

        anim.SetFloat("Horizontal", Mathf.Abs(rb.velocity.x));
        anim.SetFloat("Vertical", rb.velocity.y);
    }

    // Sends character up in the air
    void Jump()
    {
        SoundManager.PlaySound(SoundManager.Sound.playerJump, transform.position);
        rb.velocity = new Vector2(rb.velocity.x, 0); 
        rb.AddForce(Vector2.up * jumpSpeed, ForceMode2D.Impulse);
        jumpTimer = 0;
        StartCoroutine(jumpSqueeze(new Vector3(0.9f, 1.1f, 1), 0.1f));
        StartCoroutine(toAir());
    }

    // Change physics
    void modifyPhysics()
    {
        bool changingDirections = (direction.x > 0 && rb.velocity.x < 0) || (direction.x < 0 && rb.velocity.x > 0);

        if (grounded)
        {
            if (!onAction && (Mathf.Abs(direction.x) < 0.2f || changingDirections))
            {
                rb.drag = linearDrag * 4;
            }
            else
            {
                rb.drag = 0f;
            }
            rb.gravityScale = 0.3f;
        }
        else
        {
            rb.gravityScale = gravity;
            if (!hooked)
            {
                rb.drag = linearDrag * 0.15f;
                if (rb.velocity.y < 0)
                {
                    rb.gravityScale = gravity * fallMultiplier;
                }
                else if (rb.velocity.y > 0 && !Input.GetButton("Jump"))
                {
                    rb.gravityScale = gravity * (fallMultiplier / 2);
                }
            }
            else
            {
                rb.drag = linearDrag * 0.05f;
            }
        }
    }

    // Change scale animation for jumping
    public IEnumerator jumpSqueeze(Vector3 size, float duration)
    {
        iTween.ScaleTo(characterHolder, size, duration);
        yield return new WaitForSeconds(duration);
        iTween.ScaleTo(characterHolder, Vector3.one, duration);
    }

    // Wait until ground to stop character sideways movement with drag
    IEnumerator toAir()
    {
        onAction = true;
        yield return new WaitUntil(() => !grounded);
        onAction = false;
    }

    // Turn character depending on direction
    void Flip()
    {
        facingRight = !facingRight;
        characterHolder.transform.rotation = Quaternion.Euler(0, facingRight ? 0 : 180, 0);
    }

    // Launch player in a direction (for double jump hook)
    public void ThrowPlayer(Vector3 dir)
    {
        StartCoroutine(jumpSqueeze(new Vector3(0.9f, 1.1f, 1), 0.1f));
        rb.AddForce(dir * 15f, ForceMode2D.Impulse);
        StartCoroutine(toAir());
    }

    void OnDrawGizmos()
    {
        // Visual for grounding and direction
        Gizmos.color = new Color(1, 0, 0, 0.5f);
        Gizmos.DrawCube(transform.position + colliderOffset, colliderBox);
        Gizmos.color = Color.blue;
        var target = transform.position + colliderOffset;
        target.y -= colliderBox.y * 2;
        Gizmos.DrawLine(transform.position, target);
    }
}
