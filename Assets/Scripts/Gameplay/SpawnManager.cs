using System.Collections;
using System.Collections.Generic;
using UnityEngine;

/// <summary>
/// Sinh thiên thạch và sao trong giới hạn camera; thỉnh thoảng bổ sung thiên thạch.
/// </summary>
public sealed class SpawnManager : MonoBehaviour
{
    [Header("Prefab")]
    [SerializeField] private GameObject asteroidPrefab;
    [SerializeField] private GameObject starPrefab;

    [Header("Số lượng")]
    [SerializeField, Min(0)] private int initialAsteroids = 9;
    [SerializeField, Min(1)] private int maxAsteroids = 13;
    [SerializeField, Min(0)] private int starCount = 14;
    [SerializeField, Min(0.5f)] private float asteroidSpawnInterval = 4.5f;

    private readonly HashSet<AsteroidMotion> activeAsteroids = new HashSet<AsteroidMotion>();
    private Transform player;

    private void Start()
    {
        PlayerController playerController = FindFirstObjectByType<PlayerController>();
        if (playerController != null)
            player = playerController.transform;

        for (int i = 0; i < initialAsteroids; i++)
            SpawnAsteroid();
        for (int i = 0; i < starCount; i++)
            SpawnStar();

        StartCoroutine(SpawnMoreAsteroids());
    }

    private IEnumerator SpawnMoreAsteroids()
    {
        var wait = new WaitForSeconds(asteroidSpawnInterval);
        while (true)
        {
            yield return wait;
            if (activeAsteroids.Count < maxAsteroids)
                SpawnAsteroid();
        }
    }

    private Vector3 RandomSpawnPosition(float avoidPlayerDistance)
    {
        Camera cam = Camera.main;
        float halfHeight = cam != null ? cam.orthographicSize : 5f;
        float halfWidth = cam != null ? halfHeight * cam.aspect : 8.5f;

        Vector3 point = Vector3.zero;
        for (int attempt = 0; attempt < 24; attempt++)
        {
            point = new Vector3(
                Random.Range(-halfWidth + 0.65f, halfWidth - 0.65f),
                Random.Range(-halfHeight + 0.65f, halfHeight - 0.65f),
                0f);

            if (player == null || Vector2.Distance(point, player.position) >= avoidPlayerDistance)
                return point;
        }
        return point;
    }

    private void SpawnAsteroid()
    {
        if (asteroidPrefab == null)
            return;

        GameObject instance = Instantiate(asteroidPrefab, RandomSpawnPosition(2.25f), Quaternion.Euler(0f, 0f, Random.Range(0f, 360f)));
        AsteroidMotion asteroid = instance.GetComponent<AsteroidMotion>();
        if (asteroid != null)
        {
            asteroid.SetOwner(this);
            activeAsteroids.Add(asteroid);
        }

        float scale = Random.Range(0.72f, 1.22f);
        instance.transform.localScale = Vector3.one * scale;
    }

    private void SpawnStar()
    {
        if (starPrefab == null)
            return;

        GameObject instance = Instantiate(starPrefab, RandomSpawnPosition(1.25f), Quaternion.identity);
        float scale = Random.Range(0.7f, 1.05f);
        instance.transform.localScale = Vector3.one * scale;
    }

    public void NotifyAsteroidDestroyed(AsteroidMotion asteroid)
    {
        activeAsteroids.Remove(asteroid);
    }
}
