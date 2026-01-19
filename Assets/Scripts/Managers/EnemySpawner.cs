using System.Collections;
using UnityEngine;
using UnityEngine.Tilemaps;
using static UnityEditor.PlayerSettings;

public class EnemySpawner : MonoBehaviour
{
    [SerializeField] GameObject enemyPrefab;
    [SerializeField] GameObject player;

    [Header("Tilemap")]
    public Tilemap groundTilemap;

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

        if (groundTilemap.GetTile(groundTilemap.WorldToCell(spawnPos)) == null)
            return;

        if (!IsFullySurrounded(spawnPos))
            return;

        GameObject enemy = Instantiate(enemyPrefab, spawnPos, Quaternion.identity);

        var health = enemy.GetComponent<EnemyHealth>();
        if (health != null)
        {
            health.baseSpeed += difficultyMultiplier;
        }
    }

    bool IsFullySurrounded(Vector3Int pos)
    {
        Vector3Int[] neighbors =
        {
            Vector3Int.up,
            Vector3Int.down,
            Vector3Int.left,
            Vector3Int.right,

            new Vector3Int(1, 1, 0),
            new Vector3Int(-1, 1, 0),
            new Vector3Int(1, -1, 0),
            new Vector3Int(-1, -1, 0)
        };

        foreach (var dir in neighbors)
        {
            if (groundTilemap.GetTile(pos + dir) == null)
                return false;
        }

        return true;
    }

    bool IsFullySurrounded(Vector2 worldPos)
    {
        Vector3Int cellPos = groundTilemap.WorldToCell(worldPos);
        return IsFullySurrounded(cellPos);
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