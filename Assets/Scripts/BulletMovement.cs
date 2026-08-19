using UnityEngine;

public class BulletMovement : MonoBehaviour
{
    [SerializeField] float moveSpeed = 6f;
    // Start is called once before the first execution of Update after the MonoBehaviour is created
    void Start()
    {
    }

    // Update is called once per frame
    void Update()
    {
        transform.localPosition += transform.up * Time.deltaTime * moveSpeed;
    }

    void OnTriggerEnter2D (Collider2D other){
        if (other.CompareTag("Walls")){
            Destroy(gameObject);
        }
    }
}
