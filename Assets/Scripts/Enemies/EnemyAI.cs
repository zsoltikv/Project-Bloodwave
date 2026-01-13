using UnityEngine;

public class EnemyAI : MonoBehaviour
{
    Transform player;
    Rigidbody2D rb;
    

    void Awake()
    {
        rb = GetComponent<Rigidbody2D>();
    }

    void Start()
    {
        player = GameObject.FindGameObjectWithTag("Player")?.transform;
    }

    void FixedUpdate()
    {
        if (!player || GameManagerScript.instance.FreezeGame) return;

        Vector2 dir = (player.position - transform.position).normalized;
        rb.linearVelocity = dir * this.GetComponent<EnemyHealth>().currentSpeed;
    }

    private void OnTriggerEnter2D(Collider2D collision)
    {
        if (collision.CompareTag("Player"))
        {
            var playerStats = collision.GetComponent<PlayerStats>();
            if (playerStats != null)
            {
                playerStats.TakeDamage(10f);
            }
            Destroy(this.gameObject);
        } 
    }
}   