using UnityEngine;

public class Bullet : MonoBehaviour
{
    public float lifetime = 3f; // Destroys itself after 3 seconds

    void Start()
    {
        Destroy(gameObject, lifetime);
    }

    private void OnTriggerEnter2D(Collider2D collision)
    {
        // Destroy the bullet when it hits anything with a collider (like ground or enemies)
        Destroy(gameObject);
    }
}