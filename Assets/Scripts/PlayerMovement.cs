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
    bool isDashing = false;

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

    [Header("Health")]
    int maxHealth = 100;
    int currentHealth;
    [SerializeField] HealthBar healthBar;

    [Header("Interaction")]
    [SerializeField] GameObject interactionButton;
    bool holdInteractable = false;
    [SerializeField] InteractionSlider interactionSlider;
    bool interactable = false;

    void Awake(){
        // Get Rigidbody2D component
        rb = GetComponent<Rigidbody2D>();
    }
    
    void Start(){
        playerTransform = this.transform;
        timer = timeBetweenFiring;
        currentHealth = maxHealth;
        healthBar.SetMaxHealth(maxHealth);
        interactionButton.SetActive(false);
        interactionSlider.gameObject.SetActive(false);
    }

    void Update(){
        if (isDashing) return;
        dir = (Camera.main.ScreenToWorldPoint(Mouse.current.position.value) - playerTransform.position).normalized;
        float angle = Mathf.Atan2(dir.y, dir.x) * Mathf.Rad2Deg;
        playerTransform.rotation = Quaternion.AngleAxis(angle - 90, Vector3.forward);
        timer-=Time.deltaTime;
        if (Mouse.current.leftButton.isPressed && timer<=0 && !isDashing){
            OnClick();
            timer = timeBetweenFiring;
        }
        if (Keyboard.current.spaceKey.wasPressedThisFrame && canDash){
            StartCoroutine(Dash());
        }
        if (Keyboard.current.pKey.wasPressedThisFrame){
            TakeDamage(20);
        }
        if (Keyboard.current.fKey.isPressed && holdInteractable && interactionSlider.timer<=interactionSlider.waitTimer){
            interactionSlider.gameObject.SetActive(true);
            interactionSlider.timer += Time.deltaTime;
            interactionSlider.SetSliderValue();
            if (interactionSlider.timer >= interactionSlider.waitTimer){
                interactionSlider.timer = interactionSlider.waitTimer;
                interactionSlider.gameObject.SetActive(false);
                interactionButton.SetActive(false);
                Debug.Log("Breaching Complete!");
                holdInteractable = false;
            }
        }
        else if (interactionSlider.timer>0 && holdInteractable){
            interactionSlider.timer -= Time.deltaTime;
            interactionSlider.SetSliderValue();
            if (interactionSlider.timer <= 0){
                interactionSlider.timer = 0;
                interactionSlider.gameObject.SetActive(false);
            }
        }
        if (Keyboard.current.fKey.wasPressedThisFrame && interactable){
            Debug.Log("Interacted!");
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
            if (interactionButton != null){
                interactionButton.SetActive(false);
            }
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
}