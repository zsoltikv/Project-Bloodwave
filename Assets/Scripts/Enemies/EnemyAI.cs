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
        if (!player) return;

        Vector2 dir = (player.position - transform.position).normalized;
        rb.linearVelocity = dir * this.GetComponent<EnemyHealth>().currentSpeed;
    }
}