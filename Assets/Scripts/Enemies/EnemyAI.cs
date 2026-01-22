using Unity.VisualScripting;
using UnityEngine;

public class EnemyAI : MonoBehaviour
{
    Transform player;
    Rigidbody2D rb;
    SpriteRenderer spriteRenderer;
    Collider2D playerCollider;
    Collider2D myCollider;
    EnemyHealth enemyHealth;

    [Header("References")]
    [SerializeField] private Transform feetPoint;

    [Header("Pathfinding")]
    [SerializeField] private float detectionDistance = 3f;
    [SerializeField] private LayerMask obstacleLayer;
    [SerializeField] private float avoidanceAngle = 30f;
    [SerializeField] private float obstacleAvoidanceDistance = 0.8f; // Mennyire tartson távolságot az akadálytól

    [Header("Stuck Detection")]
    [SerializeField] private float stuckCheckTime = 0.8f;
    [SerializeField] private float minMoveDistance = 0.08f;
    [SerializeField] private int maxStuckAttempts = 3;

    [Header("Movement Smoothing")]
    [SerializeField] private float rotationSmoothness = 6f;
    [SerializeField] private float accelerationSpeed = 10f;

    private Vector2 lastPosition;
    private float stuckTimer;
    private int stuckAttempts;
    private Vector2 currentMoveDir;
    private Vector2 currentVelocity;
    private Vector2 lastStuckEscapeDir;
    private float timeSinceLastStuck;

    private Vector2 CastOrigin
    {
        get
        {
            if (feetPoint != null) return feetPoint.position;

            // Fallback: collider alja-közepe
            if (myCollider != null)
            {
                var b = myCollider.bounds;
                return new Vector2(b.center.x, b.min.y);
            }

            // Végső fallback
            if (rb != null) return rb.position;
            return (Vector2)transform.position;
        }
    }

    void LookAtPlayer()
    {
        if (!player) return;

        float dir = player.position.x - transform.position.x;

        if (dir != 0)
            spriteRenderer.transform.localEulerAngles = new Vector3(0, dir < 0 ? 180f : 0f, 0);
    }

    void Awake()
    {
        rb = GetComponent<Rigidbody2D>();
        spriteRenderer = GetComponent<SpriteRenderer>();
        enemyHealth = GetComponent<EnemyHealth>();
        myCollider = GetComponent<Collider2D>();

        // Auto-find FeetPoint child, ha nincs Inspectorban beállítva
        if (feetPoint == null)
        {
            var t = transform.Find("FeetPoint");
            if (t != null) feetPoint = t;
        }
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
        currentMoveDir = Vector2.right;
    }

    void FixedUpdate()
    {
        if (!player) return;
        // if (GameManagerScript.instance.FreezeGame) return;

        timeSinceLastStuck += Time.fixedDeltaTime;

        Vector2 playerTargetPos = playerCollider != null ? playerCollider.bounds.center : player.position;
        Vector2 targetDir = (playerTargetPos - (Vector2)transform.position).normalized;
        float distanceToPlayer = Vector2.Distance(transform.position, playerTargetPos);

        // Saját collider méretének figyelembevétele
        float myRadius = myCollider != null ? Mathf.Max(myCollider.bounds.extents.x, myCollider.bounds.extents.y) : 0.5f;
        float checkDistance = Mathf.Min(detectionDistance, distanceToPlayer - myRadius);

        // Közvetlen útvonal ellenőrzése (feetpointból!)
        Vector2 rayOrigin = CastOrigin;

        RaycastHit2D directHit = Physics2D.CircleCast(
            rayOrigin,
            myRadius * 0.8f, // Kicsit kisebb mint az actual size
            targetDir,
            checkDistance,
            obstacleLayer
        );

        Vector2 desiredMoveDir;

        if (directHit.collider == null)
        {
            desiredMoveDir = targetDir;
            Debug.DrawRay(rayOrigin, targetDir * checkDistance, Color.blue, Time.fixedDeltaTime);
        }
        else
        {
            desiredMoveDir = FindAvoidanceDirection(targetDir, directHit, myRadius);
        }

        // Simított irányváltozás
        currentMoveDir = Vector2.Lerp(currentMoveDir, desiredMoveDir, Time.fixedDeltaTime * rotationSmoothness).normalized;

        bool isStuck = CheckIfStuck(currentMoveDir);

        // Simított gyorsítás/lassulás
        float speed = this.enemyHealth.currentSpeed;
        Vector2 targetVelocity = currentMoveDir * speed;

        // Ha megakadt, adjunk extra boost-ot
        if (isStuck && timeSinceLastStuck > 0.3f)
        {
            targetVelocity *= 1.3f;
        }

        currentVelocity = Vector2.Lerp(currentVelocity, targetVelocity, Time.fixedDeltaTime * accelerationSpeed);
        rb.linearVelocity = currentVelocity;

        LookAtPlayer();
    }

    Vector2 FindAvoidanceDirection(Vector2 targetDir, RaycastHit2D obstacleHit, float myRadius)
    {
        Vector2 origin = CastOrigin;

        Vector2 playerPos = playerCollider != null ? playerCollider.bounds.center : player.position;
        Vector2 toPlayer = (playerPos - (Vector2)transform.position).normalized;

        // Ha túl közel vagyunk az akadályhoz, prioritizáljuk a távolodást
        bool tooClose = obstacleHit.distance < myRadius + obstacleAvoidanceDistance;

        // Szélesebb szögtartomány ha stuck vagyunk
        float maxAngle = stuckAttempts > 0 ? avoidanceAngle * 3f : avoidanceAngle * 2f;

        // Teszteljük az irányokat finomabb lépésekben
        float[] angleSteps = tooClose
            ? new float[] { 90f, -90f, 60f, -60f, 120f, -120f, 45f, -45f, 135f, -135f }
            : new float[] {
                avoidanceAngle * 0.5f, -avoidanceAngle * 0.5f,
                avoidanceAngle, -avoidanceAngle,
                avoidanceAngle * 1.5f, -avoidanceAngle * 1.5f,
                avoidanceAngle * 2f, -avoidanceAngle * 2f,
                avoidanceAngle * 2.5f, -avoidanceAngle * 2.5f
              };

        Vector2 bestDirection = Vector2.zero;
        float bestScore = -1000f;

        foreach (float angle in angleSteps)
        {
            if (Mathf.Abs(angle) > maxAngle) continue;

            Vector2 testDir = Rotate(targetDir, angle);

            // CircleCast használata hogy figyelembe vegye az ellenség méretét (feetpointból!)
            RaycastHit2D hit = Physics2D.CircleCast(
                origin,
                myRadius * 0.7f,
                testDir,
                detectionDistance,
                obstacleLayer
            );

            float score = 0f;

            if (hit.collider == null)
            {
                score += 20f; // Teljesen szabad út
            }
            else if (hit.distance > myRadius + obstacleAvoidanceDistance)
            {
                // Van akadály de elég messze
                score += 10f + (hit.distance * 2f);
            }
            else
            {
                // Túl közel az akadály
                score -= 20f;
                continue; // Skip this direction
            }

            // Játékos irányába mutatás
            float alignment = Vector2.Dot(testDir, toPlayer);
            score += alignment * 8f;

            // Kisebb szög preferálása
            score -= Mathf.Abs(angle) * 0.03f;

            // Smooth transitions - jelenlegi irányhoz hasonlóság
            float similarity = Vector2.Dot(testDir, currentMoveDir);
            score += similarity * 4f;

            // Ha nemrég megakadtunk, kerüljük azt az irányt
            if (lastStuckEscapeDir != Vector2.zero)
            {
                float avoidLastStuck = Vector2.Dot(testDir, lastStuckEscapeDir);
                if (avoidLastStuck < 0) // Ellenkező irány
                {
                    score += 3f;
                }
            }

            // Debug visualization (feetpointból!)
            Color debugColor = Color.green;
            if (hit.collider != null)
            {
                debugColor = hit.distance > myRadius + obstacleAvoidanceDistance ? Color.yellow : Color.red;
            }
            Debug.DrawRay(origin, testDir * detectionDistance, debugColor, Time.fixedDeltaTime);

            if (score > bestScore)
            {
                bestScore = score;
                bestDirection = testDir;
            }
        }

        // Ha nem találtunk jó irányt, menjünk az akadály normálisa mentén
        if (bestScore < 0 && obstacleHit.normal != Vector2.zero)
        {
            bestDirection = obstacleHit.normal;
        }
        else if (bestDirection == Vector2.zero)
        {
            // Végső megoldás: random irány
            bestDirection = Rotate(targetDir, Random.Range(-90f, 90f));
        }

        // Legjobb irány megjelölése (feetpointból!)
        Debug.DrawRay(origin, bestDirection * detectionDistance, Color.cyan, Time.fixedDeltaTime);

        return bestDirection;
    }

    bool CheckIfStuck(Vector2 moveDir)
    {
        float movedDistance = Vector2.Distance(transform.position, lastPosition);

        if (movedDistance < minMoveDistance)
        {
            stuckTimer += Time.fixedDeltaTime;

            if (stuckTimer > stuckCheckTime)
            {
                stuckAttempts++;

                // Progresszíven drasztikusabb megoldások
                float escapeAngle;
                if (stuckAttempts == 1)
                {
                    // Első próbálkozás: enyhe korrekció
                    escapeAngle = Random.Range(70f, 110f) * (Random.value > 0.5f ? 1f : -1f);
                }
                else if (stuckAttempts == 2)
                {
                    // Második próbálkozás: nagyobb szög
                    escapeAngle = Random.Range(120f, 150f) * (Random.value > 0.5f ? 1f : -1f);
                }
                else
                {
                    // Harmadik+: random irány, akár vissza is
                    escapeAngle = Random.Range(-180f, 180f);
                }

                Vector2 escapeDir = Rotate(moveDir, escapeAngle);

                // Ellenőrizzük hogy az escape irány szabad-e (feetpointból!)
                float myRadius = myCollider != null ? Mathf.Max(myCollider.bounds.extents.x, myCollider.bounds.extents.y) : 0.5f;
                Vector2 origin = CastOrigin;

                RaycastHit2D escapeCheck = Physics2D.CircleCast(
                    origin,
                    myRadius * 0.7f,
                    escapeDir,
                    1f,
                    obstacleLayer
                );

                if (escapeCheck.collider != null && escapeCheck.distance < myRadius + 0.5f)
                {
                    // Ha ez az irány is zárva, próbáljuk az ellenkező irányba
                    escapeDir = -escapeDir;
                }

                currentVelocity = escapeDir * enemyHealth.currentSpeed * 1.4f;
                currentMoveDir = escapeDir;
                lastStuckEscapeDir = escapeDir;

                stuckTimer = 0f;
                timeSinceLastStuck = 0f;

                // Reset stuck attempts ha túl sokszor próbálkoztunk
                if (stuckAttempts >= maxStuckAttempts)
                {
                    stuckAttempts = 0;
                }

                lastPosition = transform.position;
                return true;
            }
        }
        else
        {
            // Fokozatosan csökkentjük a timer-t és az attempts-et ha mozgunk
            stuckTimer = Mathf.Max(0f, stuckTimer - Time.fixedDeltaTime * 2f);

            if (movedDistance > minMoveDistance * 3f) // Jól mozgunk
            {
                stuckAttempts = Mathf.Max(0, stuckAttempts - 1);
            }
        }

        lastPosition = transform.position;
        return false;
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
                playerStats.TakeDamage(enemyHealth.baseDamage);
            }
            Destroy(this.gameObject);
        }
    }
}
