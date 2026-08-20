using UnityEngine;

public class BulletMovement : MonoBehaviour
{
    [SerializeField] float moveSpeed = 6f;
    Rigidbody2D rb;
    // Start is called once before the first execution of Update after the MonoBehaviour is created
    void Start()
    {
        rb = GetComponent<Rigidbody2D>();
    }

    // Update is called once per frame
    void Update()
    {
        rb.linearVelocity = transform.up * moveSpeed;
    }

    void OnTriggerEnter2D (Collider2D other){
        if (other.CompareTag("Walls")){
            Destroy(gameObject);
        }
        if (other.CompareTag("Borders")){
            Destroy(gameObject);
        }
        if (gameObject.CompareTag("Bullets") && other.CompareTag("Enemies")){
            Destroy(gameObject);
        }
        if (other.CompareTag("EnemyBullets")){
            Destroy(gameObject);
            Destroy(other.gameObject);
        }
    }
}
