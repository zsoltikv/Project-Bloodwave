using System.Collections;
using UnityEngine;

public class EnemySpawner : MonoBehaviour
{
    [SerializeField] GameObject enemyPrefab;
    [SerializeField] float spawnInterval = 1f;
    [HideInInspector] public int enemiesToSpawn = 0;
    public GameObject Player;

    void Start()
    {
        StartCoroutine(SpawnLoop());
    }

    IEnumerator SpawnLoop()
    {
        while (true)
        {
            if (enemiesToSpawn > 0)
            {
                SpawnEnemy();
                enemiesToSpawn--;
            }

            yield return new WaitForSeconds(spawnInterval);
        }
    }

    void SpawnEnemy()
    {
        Vector2 spawnPos = GetRandomSpawnPosition();
        Instantiate(enemyPrefab, spawnPos, Quaternion.identity);
    }

    Vector2 GetRandomSpawnPosition()
    {
        Vector2 offset = Player.transform.position;
        float camHeight = Camera.main.orthographicSize;
        float camWidth = camHeight * Camera.main.aspect;

        int side = Random.Range(0, 4);
        Vector2 pos = Vector2.zero;

        switch (side)
        {
            case 0: pos = new Vector2(Random.Range(-camWidth, camWidth), camHeight + 1) + offset; break;
            case 1: pos = new Vector2(Random.Range(-camWidth, camWidth), -camHeight - 1) + offset; break;
            case 2: pos = new Vector2(-camWidth - 1, Random.Range(-camHeight, camHeight)) + offset; break;
            case 3: pos = new Vector2(camWidth + 1, Random.Range(-camHeight, camHeight)) + offset; break;
        }

        return pos;
    }
}