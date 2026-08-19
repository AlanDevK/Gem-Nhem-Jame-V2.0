using UnityEngine;

public class TopDownMovement : MonoBehaviour
{
    [Header("Movement & Dash Speeds")]
    public float moveSpeed = 6f;
    public float dashSpeed = 15f;

    [Header("Dash Timers")]
    public float dashDuration = 0.15f;
    public float dashCooldown = 1f;

    private Rigidbody2D rb;
    private Vector2 movementInput;

    private float dashTimeCounter;
    private float coolDownCounter;

    void Start()
    {
        rb = GetComponent<Rigidbody2D>();
        rb.gravityScale = 0f; // Turn off gravity for top-down
    }

    void Update()
    {
        // 1. Get basic movement input
        movementInput.x = Input.GetAxisRaw("Horizontal");
        movementInput.y = Input.GetAxisRaw("Vertical");
        movementInput = movementInput.normalized;

        // 2. Handle Dash Input (Q key)
        if (Input.GetKeyDown(KeyCode.Q))
        {
            // If cooldown is finished and we are actually trying to move
            if (coolDownCounter <= 0 && movementInput != Vector2.zero)
            {
                dashTimeCounter = dashDuration;
                coolDownCounter = dashCooldown;
            }
        }

        // 3. Count down the timers
        if (dashTimeCounter > 0)
        {
            dashTimeCounter -= Time.deltaTime;
        }

        if (coolDownCounter > 0)
        {
            coolDownCounter -= Time.deltaTime;
        }
    }

    void FixedUpdate()
    {
        // 4. Apply movement or dash velocity
        if (dashTimeCounter > 0)
        {
            // Dashing: use high speed
            rb.linearVelocity = movementInput * dashSpeed;
        }
        else
        {
            // Normal walking: use regular speed
            rb.linearVelocity = movementInput * moveSpeed;
        }
    }
}