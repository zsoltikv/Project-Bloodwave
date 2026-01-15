using Unity.VisualScripting;
using UnityEngine;

public class EnemyAI : MonoBehaviour
{
    Transform player;
    Rigidbody2D rb;

    [Header("Pathfinding")]
    [SerializeField] private float detectionDistance = 2f;
    [SerializeField] private LayerMask obstacleLayer;
    [SerializeField] private int rayCount = 7;
    [SerializeField] private float rayAngleRange = 90f;
    [SerializeField] private float stuckCheckTime = 1f;
    
    private Vector2 lastPosition;
    private float stuckTimer;
    

    void Awake()
    {
        rb = GetComponent<Rigidbody2D>();
    }

    void Start()
    {
        player = GameObject.FindGameObjectWithTag("Player")?.transform;
        lastPosition = rb.position;
    }

    void FixedUpdate()
    {
        if (!player || GameManagerScript.instance.FreezeGame) return;

        Vector2 targetDir = (player.position - transform.position).normalized;
        Vector2 bestDir = FindBestDirection(targetDir);

        CheckIfStuck(bestDir);

        float speed = this.GetComponent<EnemyHealth>().currentSpeed;
        rb.linearVelocity = bestDir * speed;
    }

    Vector2 FindBestDirection(Vector2 targetDir)
    {
        float bestScore = -999f;
        Vector2 bestDirection = targetDir;

        for (int i = 0; i < rayCount; i++)
        {
            float t = rayCount > 1 ? i / (float)(rayCount - 1) : 0.5f;
            float angle = Mathf.Lerp(-rayAngleRange, rayAngleRange, t);
            Vector2 dir = Rotate(targetDir, angle);
            
            RaycastHit2D hit = Physics2D.Raycast(
                transform.position, 
                dir, 
                detectionDistance, 
                obstacleLayer
            );
            
            // Pontozás: mennyire mutat a cél felé
            float score = Vector2.Dot(dir, targetDir) * 100f;
            
            if (hit.collider == null)
            {
                score += 50f;
            }
            else
            {
                // Közel van akadály = büntetés
                float distanceRatio = hit.distance / detectionDistance;
                score -= (1f - distanceRatio) * 80f;
            }

            // Debug ray
            Debug.DrawRay(transform.position, dir * detectionDistance, 
                hit.collider == null ? Color.green : Color.red);

            if (score > bestScore)
            {
                bestScore = score;
                bestDirection = dir;
            }
        }

        return bestDirection;
    }

    void CheckIfStuck(Vector2 moveDir)
    {
        float movedDistance = Vector2.Distance(transform.position, lastPosition);
        
        if (movedDistance < 0.1f)
        {
            stuckTimer += Time.fixedDeltaTime;
            
            if (stuckTimer > stuckCheckTime)
            {
                // Random irány ha megakadt
                float randomAngle = Random.Range(-90f, 90f);
                Vector2 escapeDir = Rotate(moveDir, randomAngle);
                rb.linearVelocity = escapeDir * GetComponent<EnemyHealth>().currentSpeed;
                stuckTimer = 0f;
            }
        }
        else
        {
            stuckTimer = 0f;
        }
        
        lastPosition = transform.position;
    }

        Vector2 Rotate(Vector2 v, float degrees)
    {
        float radians = degrees * Mathf.Deg2Rad;
        float sin = Mathf.Sin(radians);
        float cos = Mathf.Cos(radians);
        return new Vector2(cos * v.x - sin * v.y, sin * v.x + cos * v.y);
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