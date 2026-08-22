using UnityEngine;
using UnityEngine.InputSystem;
using System.Collections;
using System.Collections.Generic;
using Unity.Cinemachine;
using UnityEngine.Rendering;

[RequireComponent(typeof(Rigidbody2D))]

public class PlayerMovement : MonoBehaviour
{

[Header("Dash & Phasing")]
    public float dashSpeed = 50f;
    public float dashDuration = 0.2f;
    public float dashCooldown = 1f;
    public float dashDamage = 50f;
    public float dashHitboxRadius = 1f;
    [SerializeField] LayerMask unphasableLayers;
    [SerializeField] LayerMask enemyLayer;
    [SerializeField] int dashingLayerIndex = 8;
    int originalLayerIndex;
    bool canDash = true;
    public bool isDashing = false;
    HashSet<IDamageable> damagedDuringDash = new HashSet<IDamageable>();

    [Header("Movement & Input")]
    [SerializeField] float speed = 22f; // Giữ nguyên tốc độ 22f của bạn
    float originalSpeed;
    [SerializeField] InputActionReference moveAction;
    [SerializeField] InputActionReference aimAction;
    [SerializeField] InputActionReference shootAction;
    [SerializeField] InputActionReference dashAction;
    [SerializeField] InputActionReference interactAction;
    [SerializeField] InputActionReference slowMovementAction;

    Rigidbody2D rb;
    Vector2 moveInput;
    float targetAngle;
    Camera mainCam;

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

    [Header("Slow Movement")]
    SpriteRenderer sprite;
    [SerializeField] float fadeDuration;
    [SerializeField] Color shimmerColor = Color.white;
    public bool isShimmering;
    [SerializeField] GameObject playerCore;
    float fadeTimer = 0;
    [SerializeField] float slowSpeed = 1f;
    Color originalColor;
    bool colorInit = false;

    [Header("Screen Shake")]
    public float recoilForce = 0.5f;
    CinemachineImpulseSource impulseSource;

    [Header("Hit Stop Effect")]
    [SerializeField] float hitStopDuration = 0.05f;
    bool isHitStopping = false;
    public float hitRecoilForce = 0.3f;

    void Awake(){
        // Get Rigidbody2D component
        rb = GetComponent<Rigidbody2D>();
        mainCam = Camera.main;
        originalLayerIndex = gameObject.layer;
        sprite = GetComponent<SpriteRenderer>();
        impulseSource = GetComponent<CinemachineImpulseSource>();
    }
    
    void Start(){
        fireTimer = timeBetweenFiring;
        currentHealth = maxHealth;
        healthBar.SetMaxHealth(maxHealth);
        interactionButton.SetActive(false);
        interactionSlider.gameObject.SetActive(false);
        playerCore.SetActive(false);
        originalSpeed = speed;
    }

    void Update()
    {
        if (isDashing) return;

        // 1. Nhận Input
        moveInput = moveAction.action.ReadValue<Vector2>();
        Vector2 mouseScreenPos = aimAction.action.ReadValue<Vector2>();
        Vector3 mouseWorldPos = mainCam.ScreenToWorldPoint(mouseScreenPos);
        
        // 2. Tính hướng xoay
        Vector2 dir = ((Vector2)mouseWorldPos - (Vector2)transform.position).normalized;
        targetAngle = Mathf.Atan2(dir.y, dir.x) * Mathf.Rad2Deg - 90f;

        // 3. Xử lý kĩ năng
        HandleShooting();
        HandleDash();
        HandleInteraction();
        HandleSlowMovement();

        // 4. Xử lý va chạm nổ khi slow-mo (từ code cũ mang sang)
        if (Time.timeScale < 1f) {
            Collider2D[] hitColliders = Physics2D.OverlapCircleAll(transform.position, 0.2f);
            foreach (Collider2D hit in hitColliders) {
                if (hit.CompareTag("Explosive")) {
                    Debug.Log("Exploded!");
                    Destroy(hit.gameObject);
                }
            }
        }

        // 5. Xử lý khi chết
        if (currentHealth <= 0)
        {
            Time.timeScale = 1f;
            Destroy(gameObject);
            healthBar.gameObject.SetActive(false);
            interactionButton.SetActive(false);
            interactionSlider.gameObject.SetActive(false);
            Debug.Log("GAH! I AM DEAD!");
        }
    }

void FixedUpdate(){
        if (isDashing) {
            DetechDashHits();
            return;
        }

        // --- BỔ SUNG ĐOẠN NÀY ĐỂ DI CHUYỂN TRONG TIMESTOP ---
        if (Time.timeScale < 1f)
        {
            // Xóa sạch quán tính vật lý để tránh bị trượt
            rb.linearVelocity = Vector2.zero;
            
            // Tự dịch chuyển thủ công bằng thời gian thực ngoài đời (UnscaledDeltaTime)
            transform.position += (Vector3)(moveInput.normalized * speed * Time.unscaledDeltaTime);
        }
        else
        {
            // Khi thời gian bình thường, sử dụng hệ thống vật lý Rigidbody cũ
            rb.linearVelocity = moveInput.normalized * speed;
        }
        // ----------------------------------------------------

        rb.MoveRotation(targetAngle);
    }

    void DetechDashHits(){
        Collider2D[] hits = Physics2D.OverlapCircleAll(rb.position, dashHitboxRadius, enemyLayer);
        bool hitSomethingThisFrame = false;
        foreach (Collider2D hit in hits){
            if (hit.TryGetComponent(out IDamageable enemy)){
                if (!damagedDuringDash.Contains(enemy)){
                    Debug.Log("Enemy is hit by dashing!");
                    enemy.TakeDamage(dashDamage);
                    damagedDuringDash.Add(enemy);
                    hitSomethingThisFrame = true;
                }
            }
        }
        if (hitSomethingThisFrame && !isHitStopping){
            StartCoroutine(HitStopRoutine());
            if (impulseSource != null){
                impulseSource.GenerateImpulseWithVelocity(rb.linearVelocity.normalized * hitRecoilForce);
            }
        }
    }
    void HandleSlowMovement(){
        if (!colorInit){
            originalColor = sprite.color;
            colorInit = true;
        }
        isShimmering = slowMovementAction.action.IsPressed();
        if (isShimmering){
            if (fadeTimer<fadeDuration){
                fadeTimer+=Time.deltaTime;
            }
            float t = fadeTimer/fadeDuration;
            sprite.color = Color.Lerp(originalColor, shimmerColor, t);
            speed = slowSpeed;
            playerCore.SetActive(true);
            canDash = false;
        } else{
            fadeTimer = 0;
            sprite.color = originalColor;
            playerCore.SetActive(false);
            speed = originalSpeed;
            canDash = true;
        }
    }

    void HandleShooting(){
        fireTimer -= Time.deltaTime;
        if (shootAction.action.IsPressed() && fireTimer <= 0){
            Instantiate(bullet, spawnPoint.position, transform.rotation);
            if (impulseSource != null){
                impulseSource.GenerateImpulseWithVelocity(-transform.up * recoilForce);
            }
            fireTimer = timeBetweenFiring;
        }
    }

    void HandleDash(){
        if (dashAction.action.triggered && canDash){
            Vector2 mouseScreenPos = aimAction.action.ReadValue<Vector2>();
            Vector3 mouseWorldPos = mainCam.ScreenToWorldPoint(mouseScreenPos);
            Vector2 dashDir = ((Vector2)mouseWorldPos - (Vector2)transform.position).normalized;
            float expectedDashDistance = dashSpeed * dashDuration;
            float actualDashDuration = dashDuration;
            RaycastHit2D hit = Physics2D.CircleCast(transform.position, 0.5f, dashDir, expectedDashDistance, unphasableLayers);
            if (hit.collider != null){
                float safeDistance = Mathf.Max(0, hit.distance - 0.1f);
                actualDashDuration = safeDistance / dashSpeed;
                if (actualDashDuration <= 0) return;
            }
            StartCoroutine(Dash(dashDir, actualDashDuration));
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
    } // Đóng ngoặc kết thúc hàm HandleInteraction()

    void OnEnable(){
        // Bật toàn bộ Input System
        moveAction.action.Enable();
        aimAction.action.Enable();
        shootAction.action.Enable();
        dashAction.action.Enable();
        interactAction.action.Enable();
        slowMovementAction.action.Enable();

        // Đăng ký sự kiện di chuyển
        moveAction.action.performed += OnMovePerformed;
        moveAction.action.canceled += OnMoveCanceled;
        }

    void OnDisable(){
        // Tắt toàn bộ Input System
        moveAction.action.Disable();
        aimAction.action.Disable();
        shootAction.action.Disable();
        dashAction.action.Disable();
        interactAction.action.Disable();
        slowMovementAction.action.Disable();

        // Hủy đăng ký sự kiện di chuyển
        moveAction.action.performed -= OnMovePerformed;
        moveAction.action.canceled -= OnMoveCanceled;
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

    private IEnumerator Dash(Vector2 dashDir, float actualDuration)
    {
        damagedDuringDash.Clear();
        canDash = false;
        isDashing = true;
        gameObject.layer = dashingLayerIndex; // Chuyển layer để xuyên tường
        float startTime = Time.unscaledTime;
        
        while (Time.unscaledTime < startTime + actualDuration)
        {
            if (Time.timeScale < 1f)
            {
                rb.linearVelocity = Vector2.zero;
                transform.position += (Vector3)dashDir * dashSpeed * Time.unscaledDeltaTime;
            }
            else
            {
                rb.linearVelocity = dashDir * dashSpeed;
            }
            yield return null;
        }

        rb.linearVelocity = Vector2.zero;
        gameObject.layer = originalLayerIndex; // Trả lại layer cũ
        isDashing = false;

        yield return new WaitForSeconds(dashCooldown);
        canDash = true;
    }
IEnumerator HitStopRoutine(){
        isHitStopping = true;
        Time.timeScale = 0;
        yield return new WaitForSecondsRealtime(hitStopDuration);
        Time.timeScale = 1f;
        isHitStopping = false;
    }

    public void TakeDamage(int damage){
        Debug.Log("Taking damages! AH!");
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
}