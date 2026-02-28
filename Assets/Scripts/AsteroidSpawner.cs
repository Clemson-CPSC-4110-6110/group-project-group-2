using UnityEngine;

public class AsteroidSpawner : MonoBehaviour
{
    [SerializeField] private GameObject asteroidPrefab;
    [SerializeField] private Transform target;
    [SerializeField] private float spawnRadius = 150f;

    public void SpawnWave(int count)
    {
        for (int i = 0; i < count; i++)
        {
            Vector3 direction = Random.onUnitSphere;
            Vector3 position = target.position + direction * spawnRadius;

            GameObject asteroid = Instantiate(asteroidPrefab, position, Random.rotation);

            asteroid.GetComponent<SeekingAsteroid>().Initialize(target);
        }
    }
}
