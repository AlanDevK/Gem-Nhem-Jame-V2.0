using UnityEngine;
using UnityEngine.InputSystem; // Hỗ trợ hệ thống Input mới

public class InfinityAbility : MonoBehaviour
{
    [Header("Infinity")]
    [Tooltip("Radius of the area where bullets will be affected by Infinity")]
    public float infinityRadius = 2f;
    
    [Tooltip("The tag of the bullets that will be affected by Infinity")]
    public string bulletTag = "EnemyBullets";

    [Header("Duration & Cooldown")]
    [Tooltip("Maximum duration of infinity ability")]
    public float maxActiveDuration = 6f;
    
    [Tooltip("Cooldown time after using the ability")]
    public float cooldownTime = 10f;

    // Các biến quản lý thời gian
    private float currentActiveTimer = 0f;
    private float currentCooldownTimer = 0f;
    private bool isInfinityActive = false;

    void Update()
    {
        if (currentCooldownTimer > 0)
        {
            currentCooldownTimer -= Time.unscaledDeltaTime;
        }

        bool isXPressed = Input.GetKey(KeyCode.X) || 
                        (Keyboard.current != null && Keyboard.current.xKey.isPressed);

        if (isXPressed && currentCooldownTimer <= 0)
        {
            if (!isInfinityActive)
            {
                isInfinityActive = true;
                currentActiveTimer = maxActiveDuration; 
                Debug.Log("DOMAIN EXPANSION: INFINITY VOID");
            }

            if (isInfinityActive)
            {
                TriggerInfinity();

                currentActiveTimer -= Time.unscaledDeltaTime;

                if (currentActiveTimer <= 0)
                {
                    DeactivateInfinity();
                    Debug.Log("The technique is now restricted due to time limit!");
                }
            }
        }
        else if (isInfinityActive && !isXPressed)
        {
            DeactivateInfinity();
            Debug.Log("The technique is now restricted due to time limit!");
        }
    }

    void TriggerInfinity()
    {
        Collider2D[] colliders = Physics2D.OverlapCircleAll(transform.position, infinityRadius);

        foreach (Collider2D col in colliders)
        {
            if (col.CompareTag("EnemyBullets"))
            {
                BulletMovement bulletScript = col.GetComponent<BulletMovement>();
                if (bulletScript != null)
                {
                    bulletScript.ApplyInfinitySlow();
                }
            }
        }
    }

    void DeactivateInfinity()
    {
        isInfinityActive = false;
        currentCooldownTimer = cooldownTime;
    }

    void OnDrawGizmosSelected()
    {
        Gizmos.color = Color.cyan;
        Gizmos.DrawWireSphere(transform.position, infinityRadius);
    }
}