using System.Collections;
using UnityEngine;

public class EnemySpawner : MonoBehaviour
{
    [SerializeField] GameObject enemyPrefab;
    [SerializeField] GameObject player;

    [Header("Spawn")]
    [SerializeField] float spawnInterval = 1.5f;
    [SerializeField] float minSpawnInterval = 0.3f;

    [Header("Difficulty")]
    [SerializeField] float difficultyIncreaseInterval = 20f;
    [SerializeField] float speedIncreasePerStep = 0.2f;

    float difficultyMultiplier = 0f;

    void Start()
    {
        StartCoroutine(SpawnLoop());
        StartCoroutine(DifficultyLoop());
    }

    IEnumerator SpawnLoop()
    {
        while (true)
        {
            SpawnEnemy();
            yield return new WaitForSeconds(spawnInterval);
        }
    }

    IEnumerator DifficultyLoop()
    {
        while (true)
        {
            yield return new WaitForSeconds(difficultyIncreaseInterval);

            difficultyMultiplier += speedIncreasePerStep;
            spawnInterval = Mathf.Max(minSpawnInterval, spawnInterval - 0.1f);
        }
    }

    void SpawnEnemy()
    {
        Vector2 spawnPos = GetRandomSpawnPosition();
        GameObject enemy = Instantiate(enemyPrefab, spawnPos, Quaternion.identity);

        var health = enemy.GetComponent<EnemyHealth>();
        if (health != null)
        {
            health.baseSpeed += difficultyMultiplier;
        }
    }

    Vector2 GetRandomSpawnPosition()
    {
        Vector2 offset = player.transform.position;
        float camHeight = Camera.main.orthographicSize;
        float camWidth = camHeight * Camera.main.aspect;

        int side = Random.Range(0, 4);

        return side switch
        {
            0 => new Vector2(Random.Range(-camWidth, camWidth), camHeight + 1) + offset,
            1 => new Vector2(Random.Range(-camWidth, camWidth), -camHeight - 1) + offset,
            2 => new Vector2(-camWidth - 1, Random.Range(-camHeight, camHeight)) + offset,
            _ => new Vector2(camWidth + 1, Random.Range(-camHeight, camHeight)) + offset,
        };
    }
}