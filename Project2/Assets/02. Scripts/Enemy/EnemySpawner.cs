using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class EnemySpawner : MonoBehaviour
{
    [Header("Spawn Settings")]
    [SerializeField] private GameObject enemyPrefab; //생성할 적 종류
    [SerializeField] private Transform[] spawnPoints; //스폰 위치들
    [SerializeField] private float spawnInterval = 5f; //생성 간격
    [SerializeField] private int poolSize = 20; //동시에 존재 가능한 최대 수

    [Header("웨이브 설정 (선택)")]
    [SerializeField] private bool useWave = false;
    [SerializeField] private int enemiesPerWave = 5;
    [SerializeField] private float waveDelay = 10f;

    private GameObject[] enemyPool;
    private bool spawning = false;

    void Start()
    {
        if (enemyPrefab == null || spawnPoints == null || spawnPoints.Length == 0)
        {
            Debug.LogWarning("EnemySpawner: EnemyPrefab 혹은 SpawnPoint가 설정되지 않았습니다.");
            return;
        }

        CreatePool();

        if (useWave)
            StartCoroutine(SpawnWaveRoutine());
        else
            StartCoroutine(SpawnContinuousRoutine());
    }

    void CreatePool()
    { 
        enemyPool=new GameObject[poolSize];
        for (int i = 0; i < poolSize; i++)
        {
            enemyPool[i] = Instantiate(enemyPrefab, transform);
            enemyPool[i].SetActive(false);
        }
    }

    IEnumerator SpawnContinuousRoutine()
    {
        spawning = true;
        while (spawning)
        {
            yield return new WaitForSeconds(spawnInterval);
            SpawnEnemy();
        }
    }

    IEnumerator SpawnWaveRoutine()
    {
        spawning = true;
        while (spawning)
        {
            
            for (int i = 0; i < enemiesPerWave; i++)
                SpawnEnemy();

            yield return new WaitForSeconds(waveDelay);
        }
    }

    void SpawnEnemy()
    {
        GameObject enemy = GetInactive();
        if (enemy == null)
            return;

        Transform point = spawnPoints[Random.Range(0, spawnPoints.Length)];
        enemy.transform.position = point.position;
        enemy.transform.rotation = point.rotation;
        enemy.SetActive(true);
    }

    private GameObject GetInactive()
    {
        for (int i = 0; i < enemyPool.Length; i++)
        { 
            if (!enemyPool[i].activeSelf)
                return enemyPool[i];
        }
        return null;
    }

    // 에디터 디버그용
    private void OnDrawGizmos()
    {
        if (spawnPoints == null) return;
        Gizmos.color = Color.green;
        foreach (var p in spawnPoints)
        {
            if (p != null)
                Gizmos.DrawWireSphere(p.position, 0.5f);
        }
    }
}
