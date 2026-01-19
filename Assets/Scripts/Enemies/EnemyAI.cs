using Unity.VisualScripting;
using UnityEngine;

public class EnemyAI : MonoBehaviour
{
    Transform player;
    Rigidbody2D rb;
    Collider2D playerCollider;

    [Header("Pathfinding")]
    [SerializeField] private float detectionDistance = 2f;
    [SerializeField] private LayerMask obstacleLayer;
    [SerializeField] private float avoidanceAngle = 45f;
    [SerializeField] private float stuckCheckTime = 1f;
    
    private Vector2 lastPosition;
    private float stuckTimer;

    void LookAtPlayer()
    {
        if (!player) return;

        Vector3 scale = transform.localScale; 
        float dir = player.position.x - transform.position.x;

        if (dir > 0)
            scale.x = Mathf.Abs(scale.x);      
        else if (dir < 0)
            scale.x = -Mathf.Abs(scale.x);     

        transform.localScale = scale;     
    }

    void Awake()
    {
        rb = GetComponent<Rigidbody2D>();
    }

    void Start()
    {
        GameObject playerObj = GameObject.FindGameObjectWithTag("Player");

        if (playerObj)
        {
            player = playerObj.transform;
            playerCollider = playerObj.GetComponent<Collider2D>();
        }

        lastPosition = rb.position;
    }

    void FixedUpdate()
    {
        if (!player || GameManagerScript.instance.FreezeGame) return;

        Vector2 playerTargetPos = playerCollider != null ? playerCollider.bounds.center : player.position;
        Vector2 targetDir = (playerTargetPos - (Vector2)transform.position).normalized;
        float distanceToPlayer = Vector2.Distance(transform.position, playerTargetPos);

        float checkDistance = Mathf.Min(detectionDistance, distanceToPlayer - 0.1f);

        RaycastHit2D directHit = Physics2D.Raycast(
            transform.position,
            targetDir,
            checkDistance,
            obstacleLayer
        );

        Vector2 moveDir;

        if (directHit.collider == null || directHit.distance > distanceToPlayer) // Nincs akadály - egyenesen a player felé
        {
            moveDir = targetDir;
            Debug.DrawRay(transform.position, targetDir * checkDistance, Color.blue);
        }
        else // Akadály - kerüljön
        {
            Debug.Log($"Obstacle hit: {directHit.collider.name}");
            moveDir = FindAvoidanceDirection(targetDir, checkDistance);
        }

        CheckIfStuck(moveDir);

        float speed = this.GetComponent<EnemyHealth>().currentSpeed;
        rb.linearVelocity = moveDir * speed;
    }

    Vector2 FindAvoidanceDirection(Vector2 targetDir, float checkDistance)
    {

        RaycastHit2D hitForward = Physics2D.Raycast(
            transform.position,
            targetDir,
            checkDistance,
            obstacleLayer
        );

        if (hitForward.collider == null)
        {
            Debug.DrawRay(transform.position, targetDir * checkDistance, Color.green, 0.1f);
            return targetDir;
        }

        Vector2 rightDir = Rotate(targetDir, avoidanceAngle);
        Vector2 leftDir = Rotate(targetDir, -avoidanceAngle);

        RaycastHit2D hitRight = Physics2D.Raycast(
            transform.position,
            rightDir,
            checkDistance,
            obstacleLayer
        );

        RaycastHit2D hitLeft = Physics2D.Raycast(
            transform.position,
            leftDir,
            checkDistance,
            obstacleLayer
        );

        Debug.DrawRay(transform.position, targetDir * checkDistance, Color.red, 0.1f);
        Debug.DrawRay(transform.position, rightDir * checkDistance, 
            hitRight.collider == null ? Color.green : Color.yellow, 0.1f);
        Debug.DrawRay(transform.position, leftDir * checkDistance, 
            hitLeft.collider == null ? Color.green : Color.yellow, 0.1f);


        if (hitLeft.collider == null && hitRight.collider == null)
        {
            Vector2 smallRightDir = Rotate(targetDir, avoidanceAngle / 2);
            Vector2 smallLeftDir = Rotate(targetDir, -avoidanceAngle / 2);

            RaycastHit2D hitSmallRight = Physics2D.Raycast(transform.position, smallRightDir, checkDistance, obstacleLayer);
            Debug.DrawRay(transform.position, smallRightDir * checkDistance, 
                hitSmallRight.collider == null ? Color.green : Color.yellow, 0.1f);
                
            RaycastHit2D hitSmallLeft = Physics2D.Raycast(transform.position, smallLeftDir, checkDistance, obstacleLayer);
            Debug.DrawRay(transform.position, smallLeftDir * checkDistance, 
                hitSmallLeft.collider == null ? Color.green : Color.yellow, 0.1f);

            if (hitSmallRight.collider == null)
            {
                return smallRightDir;
            }
            if (hitSmallLeft.collider == null)
            {
                return smallLeftDir;
            }

            // MHa a kis szögek nem szabadok cross product
            Vector2 playerPos = playerCollider != null ? playerCollider.bounds.center : player.position;
            Vector2 toPlayer = (playerPos - (Vector2)transform.position).normalized;
            
            float cross = targetDir.x * toPlayer.y - targetDir.y * toPlayer.x;

            return cross > 0 ? leftDir : rightDir;
        }
        else if (hitLeft.collider == null)
        {
            return leftDir;
        }
        else if (hitRight.collider == null)
        {
            return rightDir;
        }
        else
        {
            // Mindkét oldal zárva --> nagyobb kitérés
            Vector2 hardRight = Rotate(targetDir, avoidanceAngle * 2);
            Vector2 hardLeft = Rotate(targetDir, -avoidanceAngle * 2);
            
            RaycastHit2D hitHardRight = Physics2D.Raycast(transform.position, hardRight, checkDistance, obstacleLayer);
            RaycastHit2D hitHardLeft = Physics2D.Raycast(transform.position, hardLeft, checkDistance, obstacleLayer);
            
            if (hitHardLeft.collider == null) return hardLeft;
            if (hitHardRight.collider == null) return hardRight;
            
            // Végső megoldás: menj hátra
            return -hitForward.normal;
        }
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
                float randomAngle = Random.Range(-120f, 120f);
                Vector2 escapeDir = Rotate(moveDir, randomAngle);

                float speed = this.GetComponent<EnemyHealth>().currentSpeed;
                rb.linearVelocity = escapeDir * speed * 1.5f;

                rb.AddForce(escapeDir * speed * 2f, ForceMode2D.Impulse);

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