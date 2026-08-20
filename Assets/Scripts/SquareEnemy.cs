using UnityEngine;
using UnityEngine.AI;
using System.Collections;
using System.Collections.Generic;

public class SquareEnemy : MonoBehaviour
{
    [SerializeField] Transform player;
    [SerializeField] float spareDistance = 2f;
    [SerializeField] GameObject bullet;
    [SerializeField] Transform spawnPointA;
    [SerializeField] Transform spawnPointB;
    [SerializeField] Transform spawnPointC;
    [SerializeField] Transform spawnPointD;
    [SerializeField] float rotationSpeed;
    float delaySeconds = 1f;
    float timer;
    bool canFire = false;
    [SerializeField] float health = 300f;
    [SerializeField] float timeBetweenFiring = 0.5f;

    NavMeshAgent agent;
    // Start is called once before the first execution of Update after the MonoBehaviour is created
    void Start()
    {
        timer = 0;
        agent = GetComponent<NavMeshAgent>();
        agent.updateRotation = false;
        agent.updateUpAxis = false;
    }

    // Update is called once per frame
    void Update()
    {
        timer -= Time.deltaTime;
        if (Vector2.Distance(transform.position, player.position) > spareDistance && !canFire){
            StopCoroutine(Firing());
            agent.isStopped = false;
            agent.SetDestination(player.position);
        }
        else{
            canFire = true;
            agent.isStopped = true;
            StartCoroutine(Firing());
        }
    }

    IEnumerator Firing(){
        yield return new WaitForSeconds(delaySeconds);
        transform.Rotate(0,0,rotationSpeed);
        if (timer<=0){
            Instantiate(bullet, spawnPointA.position, spawnPointA.rotation);
            Instantiate(bullet, spawnPointB.position, spawnPointB.rotation);
            Instantiate(bullet, spawnPointC.position, spawnPointC.rotation);
            Instantiate(bullet, spawnPointD.position, spawnPointD.rotation);
            timer = timeBetweenFiring;
        }
    }

    void OnTriggerEnter2D (Collider2D other){
        if (other.CompareTag("Borders")){
            StartCoroutine(Waiting());
            canFire = false;
            StopCoroutine(Firing());
            transform.rotation = Quaternion.Euler(0,0,0);
        }
        if (other.CompareTag("Bullets")){
            health-=10;
            if (health <= 0){
                gameObject.SetActive(false);
            }
        }
    }

    IEnumerator Waiting(){
        yield return new WaitForSeconds(delaySeconds);
    }
}
