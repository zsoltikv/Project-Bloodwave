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
    public bool alwaysSpawn = false;

    [Header("Player Relative Scaling")]
    public float healthScaleToPlayerDamage = 1.0f;
    public float damageScaleToPlayerHealth = 0.1f;
    public float speedScaleToPlayer = 1.0f;
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
    [SerializeField] float eliteSpawnChance = 0.025f;
    [SerializeField] float eliteHealthMultiplier = 3f;
    [SerializeField] float eliteSpeedMultiplier = 1.5f;
    [SerializeField] float eliteScaleMultiplier = 1.3f;
    [SerializeField] float eliteCoinMultiplier = 2f;
    [SerializeField] float eliteDamageMultiplier = 1.05f;

    float speedDifficultyMultiplier = 0f;
    float healthDifficultyMultiplier = 0f;
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

                if (currentPlayerLevel != lastCheckedPlayerLevel)
                {
                    lastCheckedPlayerLevel = currentPlayerLevel;
                    UpdateAvailableEnemies();

                    CheckAndIncreaseDifficulty(currentPlayerLevel);

                    Debug.Log($"Player Level: {currentPlayerLevel}. Available enemies: {availableEnemies.Count}");
                }
            }
        }
    }

    void CheckAndIncreaseDifficulty(int currentPlayerLevel)
    {
        if (currentPlayerLevel - lastDifficultyIncreaseLevel >= difficultyIncreaseEveryXLevels)
        {
            lastDifficultyIncreaseLevel = currentPlayerLevel;

            speedDifficultyMultiplier += speedIncreasePerStep;
            healthDifficultyMultiplier += healthIncreasePerStep;
            spawnInterval = Mathf.Max(minSpawnInterval, spawnInterval - spawnIntervalDecreaseRate);

            if (increaseEnemiesPerSpawn && !useWaveSystem)
            {
                if (speedDifficultyMultiplier % 1f == 0 || healthDifficultyMultiplier % 1f == 0)
                {
                    enemiesPerSpawn = Mathf.Min(enemiesPerSpawn + 1, 5);
                }
            }

            if (spawnElites)
            {
                eliteSpawnChance = Mathf.Min(eliteSpawnChance + 0.02f, 0.3f);
            }

            Debug.Log(
                $"Difficulty increased at player level {currentPlayerLevel}! " +
                $"Speed multiplier: +{speedDifficultyMultiplier}, " +
                $"Health multiplier: +{healthDifficultyMultiplier}, " +
                $"Spawn interval: {spawnInterval}s"
            );
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

        Vector2 spawnPos = Vector2.zero;
        bool foundValidPosition = false;

        for (int attempt = 0; attempt < 10; attempt++)
        {
            spawnPos = GetRandomSpawnPosition();
            Vector3Int cellPos = groundTilemap.WorldToCell(spawnPos);

            if (groundTilemap.GetTile(cellPos) == null)
                continue;

            if (!IsFullySurrounded(cellPos))
                continue;

            foundValidPosition = true;
            break;
        }

        if (!foundValidPosition)
            return;

        EnemySpawnData selectedEnemy = SelectEnemyByWeight();
        if (selectedEnemy == null || selectedEnemy.enemyPrefab == null) return;

        GameObject enemy = Instantiate(selectedEnemy.enemyPrefab, spawnPos, Quaternion.identity);
        activeEnemies.Add(enemy);

        bool isElite = spawnElites && Random.value < eliteSpawnChance;

        var health = enemy.GetComponent<EnemyHealth>();
        if (health != null && playerStats != null)
        {
            float playerMaxHealth = playerStats.MaxHealth;
            float avgWeaponDamage = GetAverageWeaponDamage();

            float normalizedDamage = avgWeaponDamage / 10f;
            float originalHP = health.maxHealth;
            float bonusHP = originalHP * (normalizedDamage - 1f) * selectedEnemy.healthScaleToPlayerDamage;
            health.maxHealth = originalHP + Mathf.Max(0, bonusHP);

            float originalDamage = health.baseDamage;
            health.baseDamage = originalDamage + (playerMaxHealth * selectedEnemy.damageScaleToPlayerHealth);

            health.baseSpeed *= selectedEnemy.speedScaleToPlayer;

            health.maxHealth *= 1 + healthDifficultyMultiplier;
            health.baseDamage *= 1 + (healthDifficultyMultiplier * 0.5f);

            if (isElite)
            {
                health.maxHealth *= eliteHealthMultiplier;
                health.baseDamage *= eliteDamageMultiplier;
                health.baseSpeed *= eliteSpeedMultiplier;
                health.coinReward = Mathf.RoundToInt(health.coinReward * eliteCoinMultiplier);
                enemy.transform.localScale *= eliteScaleMultiplier;

                var renderer = enemy.GetComponent<SpriteRenderer>();
                if (renderer != null)
                {
                    renderer.color = Color.red;
                }
            }

            health.currentHealth = health.maxHealth;

            health.baseSpeed += speedDifficultyMultiplier;
            health.currentSpeed = health.baseSpeed;
        }
    }

    EnemySpawnData SelectEnemyByWeight()
    {
        var alwaysSpawnEnemies = availableEnemies.Where(e => e.alwaysSpawn).ToList();
        if (alwaysSpawnEnemies.Count > 0 && Random.value < 0.3f)
        {
            return alwaysSpawnEnemies[Random.Range(0, alwaysSpawnEnemies.Count)];
        }

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

    public float GetCurrentSpeedDifficulty()
    {
        return speedDifficultyMultiplier;
    }

    public float GetCurrentHealthDifficulty()
    {
        return healthDifficultyMultiplier;
    }

    private float GetAverageWeaponDamage()
    {
        var weaponController = player.GetComponent<WeaponController>();
        if (weaponController == null) return 1f;

        var weapons = weaponController.GetWeapons();
        if (weapons == null || weapons.Count == 0) return 1f;

        float totalDamage = 0f;
        int weaponCount = 0;

        foreach (var weapon in weapons)
        {
            if (weapon != null && weapon.definition != null)
            {
                totalDamage += weapon.GetDamage();
                weaponCount++;
            }
        }

        if (weaponCount == 0) return 1f;

        return totalDamage / weaponCount;
    }
}