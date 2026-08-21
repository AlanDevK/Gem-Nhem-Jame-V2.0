using UnityEngine;

public class BulletMovement : MonoBehaviour
{
    [Header("Pushback ability")]
    [SerializeField] float moveSpeed = 100f;
    [SerializeField] float knockbackForce = 15f;
    Rigidbody2D rb;
    // Start is called once before the first execution of Update after the MonoBehaviour is created
    void Start()
    {
        rb = GetComponent<Rigidbody2D>();
    }

    void Update()
    {
        transform.localPosition += transform.up * Time.unscaledDeltaTime * moveSpeed;
    }

    void OnTriggerEnter2D (Collider2D other){
        if (other.CompareTag("Walls")){
            Destroy(gameObject);
        }
        if (other.CompareTag("Borders")){
            Destroy(gameObject);
            return;
        }

        if (gameObject.CompareTag("Bullets")){
            
            if (other.CompareTag("Enemies")){
                Destroy(gameObject);
            }
            else if (other.CompareTag("EnemyBullets")){
                Destroy(gameObject);
                Destroy(other.gameObject);
            }
            else if (other.CompareTag("PushableEnemy")){
                Rigidbody2D targetRb = other.GetComponent<Rigidbody2D>();
                if (targetRb != null){
                    Vector2 pushDirection = transform.up;
                    targetRb.AddForce(pushDirection * knockbackForce, ForceMode2D.Impulse);
                }
                Destroy(gameObject);
            }
        }
        
        else if (gameObject.CompareTag("EnemyBullets")){
            
            PlayerMovement player = other.GetComponentInParent<PlayerMovement>();
            if (player != null){
                if (other.CompareTag("Player") && !player.isShimmering && !player.isDashing){
                    player.TakeDamage(10);
                    Destroy(gameObject);
                } 
                else if (other.CompareTag("Core") && player.isShimmering){
                    player.TakeDamage(20);
                    Destroy(gameObject);
                    Debug.Log("My core is hit! GAHH!!");
                }
            }
        }
    } 
        }

