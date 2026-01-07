using System.Collections;
using UnityEngine;

public class WaveManager : MonoBehaviour
{
    [SerializeField] EnemySpawner spawner;
    [SerializeField] float timeBetweenWaves = 10f;
    [SerializeField] int waveIndex = 0;
    [SerializeField] int startingEnemies = 10;
    [SerializeField] int enemiesIncrement = 5;

    private int previousWaveEnemies = 0;

    void Start()
    {
        StartCoroutine(WaveLoop());
    }

    IEnumerator WaveLoop()
    {
        int enemiesThisWave = startingEnemies;

        while (true)
        {
            waveIndex++;
            Debug.Log($"Wave {waveIndex} started! Enemies to spawn: {enemiesThisWave}");

            spawner.enemiesToSpawn += enemiesThisWave;

            foreach (var enemy in GameObject.FindGameObjectsWithTag("Enemy"))
            {
                var ai = enemy.GetComponent<EnemyAI>();
                if (ai != null)
                    ai.speed += 0.2f;
            }

            previousWaveEnemies = enemiesThisWave;
            enemiesThisWave = previousWaveEnemies + enemiesIncrement;

            yield return new WaitForSeconds(timeBetweenWaves);
        }
    }
}