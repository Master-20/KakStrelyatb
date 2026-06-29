using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class EnemyManager : MonoBehaviour
{
    [Header("Настройка спавна")]
    public Enemy enemyPrefab;
    public Transform[] spawnPoints;
    public float respawnDelay = 1f;

    private int remaining;

    private void OnEnable()
    {
        Enemy.OnEnemyDestroyed += HandleEnemyDestroyed;
    }

    private void OnDisable()
    {
        Enemy.OnEnemyDestroyed -= HandleEnemyDestroyed;
    }

    private void Start()
    {
        SpawnAllEnemies();
    }

    private void HandleEnemyDestroyed()
    {
        remaining--;
        if (remaining <= 0)
        {
            StartCoroutine(RespawnAllRoutine());
        }
    }

    private IEnumerator RespawnAllRoutine()
    {
        yield return new WaitForSeconds(respawnDelay);
        SpawnAllEnemies();
    }

    private void SpawnAllEnemies()
    {
        foreach (Transform point in spawnPoints)
        {
            var old = point.GetComponentInChildren<Enemy>();
            if (old != null)
                Destroy(old.gameObject);
        }

        foreach (Transform point in spawnPoints)
        {
            Enemy e = Instantiate(enemyPrefab, point.position, point.rotation, point);
            e.health = 3;
            e.GetComponent<Renderer>().material.color = Color.green;
        }

        remaining = spawnPoints.Length;
    }
}
