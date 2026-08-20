using UnityEngine;
using UnityEngine.InputSystem;
using System.Collections;
using System.Collections.Generic;

[RequireComponent(typeof(Rigidbody2D))]

public class PlayerMovement : MonoBehaviour
{

    [Header("Dash & Phasing")]
    public float dashSpeed = 30f;
    public float dashDuration = 0.2f;
    public float dashCooldown = 1f;
    [SerializeField] int dashingLayerIndex = 8;
    int originalLayerIndex;
    bool canDash = true;
    bool isDashing = false;

    [Header("Movement & Input")]
    [SerializeField] float speed = 15f;
    [SerializeField] InputActionReference moveAction;
    [SerializeField] InputActionReference aimAction;
    [SerializeField] InputActionReference shootAction;
    [SerializeField] InputActionReference dashAction;
    [SerializeField] InputActionReference interactAction;

    Rigidbody2D rb;
    Vector2 moveInput;
    Camera mainCam;
    // Transform playerTransform;
    // Vector2 dir;

    [Header("Shooting")]
    [SerializeField] GameObject bullet;
    [SerializeField] float timeBetweenFiring;
    [SerializeField] Transform spawnPoint;
    float fireTimer;

    [Header("Health")]
    int maxHealth = 100;
    int currentHealth;
    [SerializeField] HealthBar healthBar;

    [Header("Interaction")]
    [SerializeField] GameObject interactionButton;
    [SerializeField] InteractionSlider interactionSlider;
    bool holdInteractable = false;
    bool interactable = false;

    void Awake(){
        // Get Rigidbody2D component
        rb = GetComponent<Rigidbody2D>();
        mainCam = Camera.main;
        originalLayerIndex = gameObject.layer;
    }
    
    void Start(){
        fireTimer = timeBetweenFiring;
        currentHealth = maxHealth;
        healthBar.SetMaxHealth(maxHealth);
        interactionButton.SetActive(false);
        interactionSlider.gameObject.SetActive(false);
    }

    void Update(){
        if (isDashing) return;

        HandleRotation();
        HandleShooting();
        HandleDash();
        HandleInteraction();
        // dir = (Camera.main.ScreenToWorldPoint(Mouse.current.position.value) - playerTransform.position).normalized;
        // float angle = Mathf.Atan2(dir.y, dir.x) * Mathf.Rad2Deg;
        // playerTransform.rotation = Quaternion.AngleAxis(angle - 90, Vector3.forward);
        // timer-=Time.deltaTime;
        // if (Mouse.current.leftButton.isPressed && timer<=0 && !isDashing){
        //     OnClick();
        //     timer = timeBetweenFiring;
        // }
        // if (Keyboard.current.spaceKey.wasPressedThisFrame && canDash){
        //     StartCoroutine(Dash());
        // }
        // if (Keyboard.current.pKey.wasPressedThisFrame){
        //     TakeDamage(20);
        // }
        // if (Keyboard.current.fKey.isPressed && holdInteractable && interactionSlider.timer<=interactionSlider.waitTimer){
        //     interactionSlider.gameObject.SetActive(true);
        //     interactionSlider.timer += Time.deltaTime;
        //     interactionSlider.SetSliderValue();
        //     if (interactionSlider.timer >= interactionSlider.waitTimer){
        //         interactionSlider.timer = interactionSlider.waitTimer;
        //         interactionSlider.gameObject.SetActive(false);
        //         interactionButton.SetActive(false);
        //         Debug.Log("Breaching Complete!");
        //         holdInteractable = false;
        //     }
        // }
        // else if (interactionSlider.timer>0 && holdInteractable){
        //     interactionSlider.timer -= Time.deltaTime;
        //     interactionSlider.SetSliderValue();
        //     if (interactionSlider.timer <= 0){
        //         interactionSlider.timer = 0;
        //         interactionSlider.gameObject.SetActive(false);
        //     }
        // }
        // if (Keyboard.current.fKey.wasPressedThisFrame && interactable){
        //     Debug.Log("Interacted!");
        // }
    }

    void FixedUpdate(){
        if (isDashing) return;
        //Using Rigidbody2D to change player's position
        moveInput = moveAction.action.ReadValue<Vector2>();
        rb.MovePosition(rb.position + moveInput.normalized * speed * Time.fixedDeltaTime);
    }

    void HandleRotation(){
        Vector2 mouseScreenPos = aimAction.action.ReadValue<Vector2>();
        Vector3 mouseWorldPos = mainCam.ScreenToWorldPoint(mouseScreenPos);
        Vector2 dir = ((Vector2)mouseWorldPos - (Vector2)transform.position).normalized;
        float angle = Mathf.Atan2(dir.y, dir.x) * Mathf.Rad2Deg;
        transform.rotation = Quaternion.AngleAxis(angle - 90, Vector3.forward);
    }

    void HandleShooting(){
        fireTimer -= Time.deltaTime;
        if (shootAction.action.IsPressed() && fireTimer <= 0){
            Instantiate(bullet, spawnPoint.position, transform.rotation);
            fireTimer = timeBetweenFiring;
        }
    }

    void HandleDash(){
        if (dashAction.action.triggered && canDash){
            StartCoroutine(Dash());
        }
    }

    void HandleInteraction(){
        bool isHoldingInteract = interactAction.action.IsPressed();
        if (isHoldingInteract && holdInteractable && interactionSlider.timer <= interactionSlider.waitTimer){
            interactionSlider.gameObject.SetActive(true);
            interactionSlider.timer += Time.deltaTime;
            interactionSlider.SetSliderValue();
        }
        if (interactionSlider.timer >= interactionSlider.waitTimer){
            interactionSlider.timer = interactionSlider.waitTimer;
            interactionSlider.gameObject.SetActive(false);
            interactionButton.SetActive(false);
            Debug.Log("Breaching Complete!");
            holdInteractable = false;
        }
        else if (interactionSlider.timer > 0 && holdInteractable){
            interactionSlider.timer -= Time.deltaTime;
            interactionSlider.SetSliderValue();
            if (interactionSlider.timer <= 0){
                interactionSlider.timer = 0;
                interactionSlider.gameObject.SetActive(false);
            }
        }
        if (interactAction.action.triggered && interactable){
            Debug.Log("Interacted!");
        }
    }

    private IEnumerator Dash(){
        canDash = false;
        isDashing = true;
        gameObject.layer = dashingLayerIndex;
        rb.linearVelocity = transform.up * dashSpeed;
        yield return new WaitForSeconds(dashDuration);
        rb.linearVelocity = Vector2.zero;
        gameObject.layer = originalLayerIndex;
        isDashing = false;
        yield return new WaitForSeconds(dashCooldown);
        canDash = true;
    }

    void TakeDamage(int damage){
        currentHealth -= damage;
        healthBar.SetHealth(currentHealth);
    }

    void OnTriggerEnter2D (Collider2D other){
        if (other.CompareTag("DataCenter") && interactionSlider.timer < interactionSlider.waitTimer){
            interactionButton.SetActive(true);
            holdInteractable = true;
        }
        if (other.CompareTag("Interactives")){
            interactionButton.SetActive(true);
            interactable = true;
        }
    }

    void OnTriggerExit2D (Collider2D other){
        if (other.CompareTag("DataCenter")){
            if (interactionButton != null) interactionButton.SetActive(false);
            holdInteractable = false;
            if (interactionSlider.timer < interactionSlider.waitTimer){
                interactionSlider.timer = 0;
            }
            interactionSlider.gameObject.SetActive(false);
        }
        if (other.CompareTag("Interactives")){
            interactionButton.SetActive(false);
            interactable = false;
        }
    }
    void OnEnable(){
        //Enabling Input System
        // moveActionReference.action.Enable();
        // moveActionReference.action.performed += OnMovePerformed;
        // moveActionReference.action.canceled += OnMoveCanceled;
        moveAction.action.Enable();
        aimAction.action.Enable();
        shootAction.action.Enable();
        dashAction.action.Enable();
        interactAction.action.Enable();
    }

    void OnDisable(){
        // Disabling Input System
        // moveActionReference.action.performed -= OnMovePerformed;
        // moveActionReference.action.canceled -= OnMoveCanceled;
        // moveActionReference.action.Disable();
        moveAction.action.Disable();
        aimAction.action.Disable();
        shootAction.action.Disable();
        dashAction.action.Disable();
        interactAction.action.Disable();
    }

    // void OnMovePerformed(InputAction.CallbackContext ctx){
    //     // Get Vector2 input
    //     moveInput = ctx.ReadValue<Vector2>();
    // }

    // void OnMoveCanceled(InputAction.CallbackContext ctx){
    //     // No movement when no button is pressed
    //     moveInput = Vector2.zero;
    // }
}