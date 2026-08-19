using UnityEngine;

public class SimpleMovement : MonoBehaviour
{
    [Header("Movement")]
    public float moveSpeed = 8f;
    public float jumpForce = 12f;

    [Header("Ground Check")]
    public Transform groundCheck;
    public LayerMask groundLayer;
    public float groundCheckRadius = 0.2f;

    [Header("Shooting")]
    public GameObject bulletPrefab;
    public Transform firePoint;
    public float bulletSpeed = 20f;

    [Header("Dash")]
    public float dashSpeed = 20f;
    public float dashDuration = 0.2f;
    public float dashCooldown = 1f;

    private Rigidbody2D rb;
    private SpriteRenderer spriteRenderer;
    private float horizontalInput;
    private bool isGrounded;

    private bool canDash = true;
    private bool isDashing;
    private float originalGravity;
    private float facingDirection = 1f; // 1 for right, -1 for left

    void Start()
    {
        rb = GetComponent<Rigidbody2D>();
        spriteRenderer = GetComponent<SpriteRenderer>();
        originalGravity = rb.gravityScale;
    }

    void Update()
    {
        if (isDashing) return;

        // Ground check using your GroundCheck transform
        isGrounded = Physics2D.OverlapCircle(groundCheck.position, groundCheckRadius, groundLayer);

        // Get left/right input
        horizontalInput = Input.GetAxis("Horizontal");

        // Track facing direction and flip sprite + firepoint
        if (horizontalInput != 0)
        {
            facingDirection = Mathf.Sign(horizontalInput);

            if (spriteRenderer != null)
            {
                spriteRenderer.flipX = (facingDirection < 0);
            }

            if (firePoint != null)
            {
                Vector3 firePointPos = firePoint.localPosition;
                firePointPos.x = Mathf.Abs(firePointPos.x) * facingDirection;
                firePoint.localPosition = firePointPos;
            }
        }

        // Jump when Spacebar is pressed and player is grounded
        if (Input.GetButtonDown("Jump") && isGrounded)
        {
            rb.linearVelocity = new Vector2(rb.linearVelocity.x, jumpForce);
        }

        // Shoot when Left Mouse Button is clicked
        if (Input.GetButtonDown("Fire1"))
        {
            Shoot();
        }

        // Dash when Left Shift is pressed
        if (Input.GetKeyDown(KeyCode.LeftShift) && canDash)
        {
            StartCoroutine(Dash());
        }
    }

    void FixedUpdate()
    {
        if (isDashing) return;

        // Move the player left/right
        rb.linearVelocity = new Vector2(horizontalInput * moveSpeed, rb.linearVelocity.y);
    }

    void Shoot()
    {
        if (bulletPrefab != null && firePoint != null)
        {
            GameObject bullet = Instantiate(bulletPrefab, firePoint.position, firePoint.rotation);
            Rigidbody2D bulletRb = bullet.GetComponent<Rigidbody2D>();

            if (bulletRb != null)
            {
                bulletRb.linearVelocity = new Vector2(facingDirection * bulletSpeed, 0f);
            }
        }
    }

    private System.Collections.IEnumerator Dash()
    {
        canDash = false;
        isDashing = true;

        rb.gravityScale = 0f;
        rb.linearVelocity = new Vector2(facingDirection * dashSpeed, 0f);

        yield return new WaitForSeconds(dashDuration);

        rb.gravityScale = originalGravity;
        isDashing = false;

        yield return new WaitForSeconds(dashCooldown);
        canDash = true;
    }
}