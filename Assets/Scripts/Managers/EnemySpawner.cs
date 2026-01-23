using System.Collections;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;
using UnityEngine.Tilemaps;

[System.Serializable]
public class EnemySpawnData
{
    public GameObject enemyPrefab;
    public int unlockLevel = 1;
    [Range(0f, 1f)] public float spawnWeight = 1f;
    [Tooltip("Ha true, ez az enemy típus mindig spawnolja a többi típust is")]
    public bool alwaysSpawn = false;
}

public class EnemySpawner : MonoBehaviour
{
    [SerializeField] List<EnemySpawnData> enemyTypes = new List<EnemySpawnData>();
    [SerializeField] GameObject player;

    [Header("Tilemap")]
    public Tilemap groundTilemap;

    [Header("Spawn Settings")]
    [SerializeField] float spawnInterval = 1.5f;
    [SerializeField] float minSpawnInterval = 0.3f;
    [SerializeField] int maxEnemiesOnScreen = 100;
    [SerializeField] int enemiesPerSpawn = 1;
    [SerializeField] float spawnDistanceFromPlayer = 12f;
    [SerializeField] bool preventSpawnInView = true;

    [Header("Difficulty Progression")]
    [Tooltip("Hány szintenként növekedjen a nehézség")]
    [SerializeField] int difficultyIncreaseEveryXLevels = 2;
    [SerializeField] float speedIncreasePerStep = 0.2f;
    [SerializeField] float healthIncreasePerStep = 0.15f;
    [SerializeField] bool increaseEnemiesPerSpawn = true;
    [SerializeField] float spawnIntervalDecreaseRate = 0.1f;

    [Header("Wave System (Optional)")]
    [SerializeField] bool useWaveSystem = false;
    [SerializeField] float timeBetweenWaves = 5f;
    [SerializeField] int baseEnemiesPerWave = 10;
    [SerializeField] int enemiesIncreasePerWave = 3;

    [Header("Elite Spawns")]
    [SerializeField] bool spawnElites = false;
    [SerializeField] float eliteSpawnChance = 0.1f;
    [SerializeField] float eliteHealthMultiplier = 3f;
    [SerializeField] float eliteSpeedMultiplier = 1.5f;
    [SerializeField] float eliteScaleMultiplier = 1.3f;

    // Private variables
    float difficultyMultiplier = 0f;
    int currentWave = 0;
    int lastDifficultyIncreaseLevel = 0;
    int lastCheckedPlayerLevel = 1;
    List<GameObject> activeEnemies = new List<GameObject>();
    bool isSpawning = true;
    List<EnemySpawnData> availableEnemies = new List<EnemySpawnData>();
    PlayerStats playerStats;

    void Start()
    {
        playerStats = player.GetComponent<PlayerStats>();
        if (playerStats == null)
        {
            Debug.LogError("PlayerStats component not found on player!");
        }

        UpdateAvailableEnemies();

        if (useWaveSystem)
            StartCoroutine(WaveSpawnSystem());
        else
            StartCoroutine(ContinuousSpawnSystem());

        StartCoroutine(CheckPlayerLevel());
        StartCoroutine(CleanupDestroyedEnemies());
    }

    IEnumerator CheckPlayerLevel()
    {
        while (true)
        {
            yield return new WaitForSeconds(0.5f);

            if (playerStats != null)
            {
                int currentPlayerLevel = playerStats.Level;

                // Ha a player szintje változott
                if (currentPlayerLevel != lastCheckedPlayerLevel)
                {
                    lastCheckedPlayerLevel = currentPlayerLevel;
                    UpdateAvailableEnemies();

                    // Nehézség növelés szintenként
                    CheckAndIncreaseDifficulty(currentPlayerLevel);

                    Debug.Log($"Player Level: {currentPlayerLevel}. Available enemies: {availableEnemies.Count}");
                }
            }
        }
    }

    void CheckAndIncreaseDifficulty(int currentPlayerLevel)
    {
        // Ellenőrizzük, hogy elértük-e a következő nehézség növelési szintet
        if (currentPlayerLevel - lastDifficultyIncreaseLevel >= difficultyIncreaseEveryXLevels)
        {
            lastDifficultyIncreaseLevel = currentPlayerLevel;

            difficultyMultiplier += speedIncreasePerStep;
            spawnInterval = Mathf.Max(minSpawnInterval, spawnInterval - spawnIntervalDecreaseRate);

            if (increaseEnemiesPerSpawn && !useWaveSystem)
            {
                if (difficultyMultiplier % 1f == 0)
                {
                    enemiesPerSpawn = Mathf.Min(enemiesPerSpawn + 1, 5);
                }
            }

            if (spawnElites)
            {
                eliteSpawnChance = Mathf.Min(eliteSpawnChance + 0.02f, 0.3f);
            }

            Debug.Log($"Difficulty increased at player level {currentPlayerLevel}! Speed: +{difficultyMultiplier}, Spawn interval: {spawnInterval}s");
        }
    }

    void UpdateAvailableEnemies()
    {
        availableEnemies.Clear();

        int currentLevel = playerStats != null ? playerStats.Level : 1;

        foreach (var enemyData in enemyTypes)
        {
            if (enemyData.enemyPrefab != null && currentLevel >= enemyData.unlockLevel)
            {
                availableEnemies.Add(enemyData);
            }
        }

        if (availableEnemies.Count == 0)
        {
            Debug.LogWarning("No enemies available to spawn! Check unlock levels.");
        }
    }

    IEnumerator ContinuousSpawnSystem()
    {
        while (isSpawning)
        {
            if (activeEnemies.Count < maxEnemiesOnScreen && availableEnemies.Count > 0)
            {
                for (int i = 0; i < enemiesPerSpawn; i++)
                {
                    SpawnEnemy();
                }
            }
            yield return new WaitForSeconds(spawnInterval);
        }
    }

    IEnumerator WaveSpawnSystem()
    {
        while (isSpawning)
        {
            currentWave++;
            int enemiesToSpawn = baseEnemiesPerWave + (enemiesIncreasePerWave * (currentWave - 1));

            Debug.Log($"Wave {currentWave} starting! Spawning {enemiesToSpawn} enemies.");

            for (int i = 0; i < enemiesToSpawn; i++)
            {
                if (activeEnemies.Count < maxEnemiesOnScreen && availableEnemies.Count > 0)
                {
                    SpawnEnemy();
                }
                yield return new WaitForSeconds(0.2f);
            }

            yield return new WaitForSeconds(timeBetweenWaves);
        }
    }

    void SpawnEnemy()
    {
        if (availableEnemies.Count == 0) return;

        // Több próbálkozás valid spawn pozícióra
        Vector2 spawnPos = Vector2.zero;
        bool foundValidPosition = false;

        for (int attempt = 0; attempt < 10; attempt++)
        {
            spawnPos = GetRandomSpawnPosition();
            Vector3Int cellPos = groundTilemap.WorldToCell(spawnPos);

            // Ellenőrizzük hogy van-e tile ezen a pozíción
            if (groundTilemap.GetTile(cellPos) == null)
                continue;

            // Ellenőrizzük hogy teljesen körbe van-e véve
            if (!IsFullySurrounded(cellPos))
                continue;

            foundValidPosition = true;
            break;
        }

        if (!foundValidPosition)
            return;

        // Weighted random selection
        EnemySpawnData selectedEnemy = SelectEnemyByWeight();
        if (selectedEnemy == null || selectedEnemy.enemyPrefab == null) return;

        GameObject enemy = Instantiate(selectedEnemy.enemyPrefab, spawnPos, Quaternion.identity);
        activeEnemies.Add(enemy);

        // Elite spawning
        bool isElite = spawnElites && Random.value < eliteSpawnChance;

        // Enemy stats beállítása
        var health = enemy.GetComponent<EnemyHealth>();
        if (health != null)
        {
            health.baseSpeed += difficultyMultiplier;
            health.maxHealth *= (1 + (difficultyMultiplier * healthIncreasePerStep));

            if (isElite)
            {
                health.maxHealth *= eliteHealthMultiplier;
                health.baseSpeed *= eliteSpeedMultiplier;
                enemy.transform.localScale *= eliteScaleMultiplier;

                // Vizuális jelzés (pl. szín változtatás)
                var renderer = enemy.GetComponent<SpriteRenderer>();
                if (renderer != null)
                {
                    renderer.color = Color.red;
                }
            }
        }
    }

    EnemySpawnData SelectEnemyByWeight()
    {
        // Always spawn enemyk kezelése
        var alwaysSpawnEnemies = availableEnemies.Where(e => e.alwaysSpawn).ToList();
        if (alwaysSpawnEnemies.Count > 0 && Random.value < 0.3f)
        {
            return alwaysSpawnEnemies[Random.Range(0, alwaysSpawnEnemies.Count)];
        }

        // Weighted selection
        float totalWeight = availableEnemies.Sum(e => e.spawnWeight);
        float randomValue = Random.Range(0f, totalWeight);
        float currentWeight = 0f;

        foreach (var enemy in availableEnemies)
        {
            currentWeight += enemy.spawnWeight;
            if (randomValue <= currentWeight)
            {
                return enemy;
            }
        }

        return availableEnemies[availableEnemies.Count - 1];
    }

    bool IsFullySurrounded(Vector3Int pos, string expectedName = "Wall_Middle")
    {
        if (groundTilemap == null) return false;

        var tile = groundTilemap.GetTile(pos);
        if (tile == null) return false;

        var data = new TileData();
        tile.GetTileData(pos, groundTilemap, ref data);

        return data.sprite != null && data.sprite.name == expectedName;
    }

    Vector2 GetRandomSpawnPosition()
    {
        Vector2 playerPos = player.transform.position;
        float camHeight = Camera.main.orthographicSize;
        float camWidth = camHeight * Camera.main.aspect;

        float spawnMargin = preventSpawnInView ? spawnDistanceFromPlayer : 1f;

        int side = Random.Range(0, 4);
        Vector2 spawnPos = side switch
        {
            0 => new Vector2(Random.Range(-camWidth, camWidth), camHeight + spawnMargin) + playerPos,
            1 => new Vector2(Random.Range(-camWidth, camWidth), -camHeight - spawnMargin) + playerPos,
            2 => new Vector2(-camWidth - spawnMargin, Random.Range(-camHeight, camHeight)) + playerPos,
            _ => new Vector2(camWidth + spawnMargin, Random.Range(-camHeight, camHeight)) + playerPos,
        };

        return spawnPos;
    }

    IEnumerator CleanupDestroyedEnemies()
    {
        while (true)
        {
            yield return new WaitForSeconds(2f);
            activeEnemies.RemoveAll(enemy => enemy == null);
        }
    }

    public void StopSpawning()
    {
        isSpawning = false;
        StopAllCoroutines();
    }

    public void ResumeSpawning()
    {
        isSpawning = true;
        if (useWaveSystem)
            StartCoroutine(WaveSpawnSystem());
        else
            StartCoroutine(ContinuousSpawnSystem());
    }

    public int GetCurrentLevel()
    {
        return playerStats != null ? playerStats.Level : 1;
    }

    public int GetActiveEnemyCount()
    {
        return activeEnemies.Count;
    }

    public float GetCurrentDifficulty()
    {
        return difficultyMultiplier;
    }
}