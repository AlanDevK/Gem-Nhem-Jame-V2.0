using UnityEngine;
using UnityEngine.InputSystem;
using System.Collections;
using System.Collections.Generic;

[RequireComponent(typeof(Rigidbody2D))]

public class PlayerMovement : MonoBehaviour
{

    [Header("Dash")]
    public float dashSpeed = 20f;
    public float dashDuration = 0.2f;
    public float dashCooldown = 1f;
    bool canDash = true;
    bool isDashing;

    [Header("Movement")]
    [SerializeField] float speed = 6f;
    [SerializeField] InputActionReference moveActionReference;
    Rigidbody2D rb;
    Vector2 moveInput;
    Transform playerTransform;
    Vector2 dir;

    [Header("Shooting")]
    [SerializeField] GameObject bullet;
    [SerializeField] float timeBetweenFiring;
    [SerializeField] Transform spawnPoint;
    float timer;

    void Awake(){
        // Get Rigidbody2D component
        rb = GetComponent<Rigidbody2D>();
    }
    
    void Start(){
        playerTransform = this.transform;
        timer = timeBetweenFiring;
        canDash = true;
        isDashing = false;
    }

    void Update(){
        if (isDashing) return;
        dir = (Camera.main.ScreenToWorldPoint(Mouse.current.position.value) - playerTransform.position).normalized;
        float angle = Mathf.Atan2(dir.y, dir.x) * Mathf.Rad2Deg;
        playerTransform.rotation = Quaternion.AngleAxis(angle - 90, Vector3.forward);
        timer-=Time.deltaTime;
        if (Mouse.current.leftButton.isPressed && timer<=0){
            OnClick();
            timer = timeBetweenFiring;
        }
        if (Keyboard.current.spaceKey.wasPressedThisFrame && canDash){
            StartCoroutine(Dash());
        }
    }

    void FixedUpdate(){
        if (isDashing) return;
        //Using Rigidbody2D to change player's position
        rb.MovePosition(rb.position + moveInput.normalized * speed * Time.fixedDeltaTime);
    }

    void OnEnable(){
        //Enabling Input System
        moveActionReference.action.Enable();
        moveActionReference.action.performed += OnMovePerformed;
        moveActionReference.action.canceled += OnMoveCanceled;
    }

    void OnDisable(){
        // Disabling Input System
        moveActionReference.action.performed -= OnMovePerformed;
        moveActionReference.action.canceled -= OnMoveCanceled;
        moveActionReference.action.Disable();
    }

    void OnMovePerformed(InputAction.CallbackContext ctx){
        // Get Vector2 input
        moveInput = ctx.ReadValue<Vector2>();
    }

    void OnMoveCanceled(InputAction.CallbackContext ctx){
        // No movement when no button is pressed
        moveInput = Vector2.zero;
    }

    void OnClick(){
        Instantiate(bullet, spawnPoint.position, transform.rotation);
    }

    private IEnumerator Dash(){
        canDash = false;
        isDashing = true;
        rb.linearVelocity = transform.up * dashSpeed;
        yield return new WaitForSeconds(dashDuration);
        isDashing = false;
        yield return new WaitForSeconds(dashCooldown);
        canDash = true;
    }
}