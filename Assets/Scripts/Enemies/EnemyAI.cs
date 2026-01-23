using UnityEngine;
using UnityEngine.AI;

public class EnemyAI : MonoBehaviour
{
    [Header("References")]
    private Transform player;
    private NavMeshAgent agent;
    private SpriteRenderer spriteRenderer;
    private Collider2D playerCollider;
    private EnemyHealth enemyHealth;

    [Header("Approach Mode")]
    [SerializeField] private ApproachMode approachMode = ApproachMode.Straight;
    [SerializeField] private float modeChangeInterval = 5f; // Time between random mode changes (if enabled)
    [SerializeField] private bool randomizeModeOnStart = false;
    [SerializeField] private bool changeModePeriodically = false;

    [Header("Straight Mode Settings")]
    [SerializeField] private float straightStoppingDistance = 0.5f;

    [Header("Circle Mode Settings")]
    [SerializeField] private float circleRadius = 3f;
    [SerializeField] private float circleSpeed = 2f;
    [SerializeField] private bool circleClockwise = true;
    private float circleAngle = 0f;

    [Header("Zigzag Mode Settings")]
    [SerializeField] private float zigzagAmplitude = 2f;
    [SerializeField] private float zigzagFrequency = 1f;
    private float zigzagTime = 0f;

    [Header("Strafe Mode Settings")]
    [SerializeField] private float strafeDistance = 4f;
    [SerializeField] private float strafeRange = 3f;
    [SerializeField] private float strafeChangeInterval = 2f;
    private float strafeTimer = 0f;
    private Vector2 strafeDirection;

    [Header("Ambush Mode Settings")]
    [SerializeField] private float ambushDistance = 6f;
    [SerializeField] private float ambushWaitTime = 1f;
    [SerializeField] private float ambushChargeSpeed = 8f;
    private bool isAmbushing = false;
    private float ambushTimer = 0f;
    private Vector3 ambushPosition;

    [Header("Retreat Mode Settings")]
    [SerializeField] private float retreatDistance = 5f;
    [SerializeField] private float retreatDuration = 2f;
    [SerializeField] private float retreatCooldown = 5f;
    private float retreatTimer = 0f;
    private float retreatCooldownTimer = 0f;
    private bool isRetreating = false;

    private float modeTimer = 0f;

    public enum ApproachMode
    {
        Straight,    // Direct path to player
        Circle,      // Circle around player
        Zigzag,      // Zigzag pattern while approaching
        Strafe,      // Move side to side while maintaining distance
        Ambush,      // Wait at distance then charge
        Retreat      // Periodically retreat and re-engage
    }

    void Awake()
    {
        agent = GetComponent<NavMeshAgent>();
        spriteRenderer = GetComponent<SpriteRenderer>();
        enemyHealth = GetComponent<EnemyHealth>();

        // Setup NavMeshAgent for 2D
        agent.updateRotation = false;
        agent.updateUpAxis = false;
    }

    void Start()
    {
        GameObject playerObj = GameObject.FindGameObjectWithTag("Player");

        if (playerObj)
        {
            player = playerObj.transform;
            playerCollider = playerObj.GetComponent<Collider2D>();
        }

        // Initialize random mode if enabled
        if (randomizeModeOnStart)
        {
            approachMode = (ApproachMode)Random.Range(0, System.Enum.GetValues(typeof(ApproachMode)).Length);
        }

        // Initialize strafe direction
        strafeDirection = Random.value > 0.5f ? Vector2.right : Vector2.left;

        // Set initial speed from EnemyHealth
        if (enemyHealth != null)
        {
            agent.speed = enemyHealth.baseSpeed;
        }
    }

    void Update()
    {
        if (!player || enemyHealth == null) return;

        // Update agent speed based on current speed
        //agent.speed = enemyHealth.currentSpeed;

        // Handle periodic mode changes
        if (changeModePeriodically)
        {
            modeTimer += Time.deltaTime;
            if (modeTimer >= modeChangeInterval)
            {
                modeTimer = 0f;
                approachMode = (ApproachMode)Random.Range(0, System.Enum.GetValues(typeof(ApproachMode)).Length);
            }
        }

        // Execute behavior based on current approach mode
        switch (approachMode)
        {
            case ApproachMode.Straight:
                HandleStraightApproach();
                break;
            case ApproachMode.Circle:
                HandleCircleApproach();
                break;
            case ApproachMode.Zigzag:
                HandleZigzagApproach();
                break;
            case ApproachMode.Strafe:
                HandleStrafeApproach();
                break;
            case ApproachMode.Ambush:
                HandleAmbushApproach();
                break;
            case ApproachMode.Retreat:
                HandleRetreatApproach();
                break;
        }

        LookAtPlayer();
    }

    void HandleStraightApproach()
    {
        // Simple direct approach to player
        Vector3 targetPos = GetPlayerPosition();
        agent.stoppingDistance = straightStoppingDistance;
        agent.SetDestination(targetPos);
    }

    void HandleCircleApproach()
    {
        // Approach player in a circular/spiral path instead of straight line
        Vector3 playerPos = GetPlayerPosition();
        Vector3 directionToPlayer = (playerPos - transform.position).normalized;
        float distanceToPlayer = Vector3.Distance(transform.position, playerPos);

        // Calculate perpendicular direction for circular motion
        float direction = circleClockwise ? 1f : -1f;
        Vector3 perpendicular = new Vector3(-directionToPlayer.y, directionToPlayer.x, 0f) * direction;

        // Mix forward movement with circular movement
        // As we get closer, reduce the circular component
        float circularWeight = Mathf.Clamp01(distanceToPlayer / circleRadius);
        Vector3 circularOffset = perpendicular * circleRadius * circularWeight;

        Vector3 targetPos = playerPos + circularOffset;
        agent.stoppingDistance = straightStoppingDistance;
        agent.SetDestination(targetPos);
    }

    void HandleZigzagApproach()
    {
        // Zigzag pattern while moving towards player
        Vector3 playerPos = GetPlayerPosition();
        Vector3 directionToPlayer = (playerPos - transform.position).normalized;

        // Calculate perpendicular direction for zigzag
        Vector3 perpendicular = new Vector3(-directionToPlayer.y, directionToPlayer.x, 0f);

        // Calculate zigzag offset
        zigzagTime += Time.deltaTime * zigzagFrequency;
        float offset = Mathf.Sin(zigzagTime) * zigzagAmplitude;

        Vector3 targetPos = playerPos + perpendicular * offset;
        agent.stoppingDistance = straightStoppingDistance;
        agent.SetDestination(targetPos);
    }

    void HandleStrafeApproach()
    {
        // Maintain distance while moving side to side
        Vector3 playerPos = GetPlayerPosition();
        float distanceToPlayer = Vector3.Distance(transform.position, playerPos);

        strafeTimer += Time.deltaTime;

        // Change strafe direction periodically
        if (strafeTimer >= strafeChangeInterval)
        {
            strafeTimer = 0f;
            strafeDirection = -strafeDirection; // Reverse direction
        }

        Vector3 directionToPlayer = (playerPos - transform.position).normalized;
        Vector3 perpendicular = new Vector3(-directionToPlayer.y, directionToPlayer.x, 0f);

        Vector3 targetPos;

        if (distanceToPlayer > strafeDistance + 0.5f)
        {
            // Too far, move closer while strafing
            targetPos = playerPos + perpendicular * strafeDirection.x * strafeRange - directionToPlayer * strafeDistance;
        }
        else if (distanceToPlayer < strafeDistance - 0.5f)
        {
            // Too close, back away while strafing
            targetPos = transform.position + perpendicular * strafeDirection.x * strafeRange + directionToPlayer * 0.5f;
        }
        else
        {
            // Good distance, just strafe
            targetPos = transform.position + perpendicular * strafeDirection.x * strafeRange;
        }

        agent.stoppingDistance = 0.1f;
        agent.SetDestination(targetPos);
    }

    void HandleAmbushApproach()
    {
        Vector3 playerPos = GetPlayerPosition();
        float distanceToPlayer = Vector3.Distance(transform.position, playerPos);

        if (!isAmbushing)
        {
            // Position at ambush distance
            if (distanceToPlayer > ambushDistance + 0.5f)
            {
                // Move to ambush distance
                Vector3 directionToPlayer = (playerPos - transform.position).normalized;
                Vector3 targetPos = playerPos - directionToPlayer * ambushDistance;
                agent.stoppingDistance = 0.5f;
                agent.SetDestination(targetPos);
            }
            else if (distanceToPlayer < ambushDistance - 0.5f)
            {
                // Too close, back away
                Vector3 awayFromPlayer = (transform.position - playerPos).normalized;
                Vector3 targetPos = playerPos + awayFromPlayer * ambushDistance;
                agent.stoppingDistance = 0.5f;
                agent.SetDestination(targetPos);
            }
            else
            {
                // At correct distance, wait before charging
                ambushTimer += Time.deltaTime;
                agent.isStopped = true;

                if (ambushTimer >= ambushWaitTime)
                {
                    isAmbushing = true;
                    ambushTimer = 0f;
                    ambushPosition = playerPos;
                    agent.isStopped = false;
                    agent.speed = ambushChargeSpeed;
                }
            }
        }
        else
        {
            // Charge at player
            agent.stoppingDistance = straightStoppingDistance;
            agent.SetDestination(ambushPosition);

            // Reset after reaching player or timeout
            ambushTimer += Time.deltaTime;
            if (distanceToPlayer < 1f || ambushTimer > 3f)
            {
                isAmbushing = false;
                ambushTimer = 0f;
                agent.speed = enemyHealth.baseSpeed;
            }
        }
    }

    void HandleRetreatApproach()
    {
        Vector3 playerPos = GetPlayerPosition();
        float distanceToPlayer = Vector3.Distance(transform.position, playerPos);

        retreatCooldownTimer += Time.deltaTime;

        if (!isRetreating && retreatCooldownTimer >= retreatCooldown)
        {
            // Start retreating
            isRetreating = true;
            retreatTimer = 0f;
            retreatCooldownTimer = 0f;
        }

        if (isRetreating)
        {
            retreatTimer += Time.deltaTime;

            // Move away from player
            Vector3 awayFromPlayer = (transform.position - playerPos).normalized;
            Vector3 targetPos = transform.position + awayFromPlayer * retreatDistance;
            agent.stoppingDistance = 0.1f;
            agent.SetDestination(targetPos);

            // Stop retreating after duration
            if (retreatTimer >= retreatDuration)
            {
                isRetreating = false;
                retreatTimer = 0f;
            }
        }
        else
        {
            // Normal approach when not retreating
            agent.stoppingDistance = straightStoppingDistance;
            agent.SetDestination(playerPos);
        }
    }

    Vector3 GetPlayerPosition()
    {
        if (playerCollider != null)
            return playerCollider.bounds.center;
        return player.position;
    }

    void LookAtPlayer()
    {
        if (!player || !spriteRenderer) return;

        float dir = player.position.x - transform.position.x;

        if (dir != 0)
            spriteRenderer.transform.localEulerAngles = new Vector3(0, dir < 0 ? 180f : 0f, 0);
    }

    // Public method to change approach mode at runtime
    public void SetApproachMode(ApproachMode newMode)
    {
        approachMode = newMode;

        // Reset mode-specific variables
        circleAngle = 0f;
        zigzagTime = 0f;
        strafeTimer = 0f;
        isAmbushing = false;
        isRetreating = false;
        ambushTimer = 0f;
        retreatTimer = 0f;
        agent.isStopped = false;
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