using UnityEngine;

public class AsteroidWaveSpawner : MonoBehaviour
{
    [SerializeField] private GameObject asteroidPrefab;
    [SerializeField] private Transform target;          // ship / thing they should fly toward
    [SerializeField] private Transform aimReference;    // TurretYaw
    [SerializeField] private float spawnRadius = 60f;
    [SerializeField] private float spawnInterval = 3f;
    [SerializeField] private int asteroidsPerWave = 1;
    [SerializeField] private int maxAliveAsteroids = 8;
    [SerializeField] private float horizontalHalfAngle = 35f;

    private float _timer;

    private void Start()
    {
        SpawnWave();
    }

    private void Update()
    {
        _timer += Time.deltaTime;

        if (_timer >= spawnInterval)
        {
            _timer = 0f;
            SpawnWave();
        }
    }

    private void SpawnWave()
    {
        if (asteroidPrefab == null || target == null || aimReference == null)
            return;

        int aliveCount = GameObject.FindGameObjectsWithTag("Asteroid").Length;
        if (aliveCount >= maxAliveAsteroids)
            return;

        for (int i = 0; i < asteroidsPerWave; i++)
        {
            Vector3 direction = GetSpawnDirection();
            Vector3 position = target.position + direction * spawnRadius;

            GameObject asteroid = Instantiate(asteroidPrefab, position, Random.rotation);

            // This is the important fix:
            // BroadcastMessage reaches scripts on the asteroid OR its children.
            asteroid.BroadcastMessage("Initialize", target, SendMessageOptions.DontRequireReceiver);
        }
    }

    private Vector3 GetSpawnDirection()
    {
        // Using negative forward because your current reference appears flipped.
        Vector3 flatForward = Vector3.ProjectOnPlane(-aimReference.forward, Vector3.up);

        if (flatForward.sqrMagnitude < 0.001f)
            flatForward = Vector3.forward;

        flatForward.Normalize();

        float yawOffset = Random.Range(-horizontalHalfAngle, horizontalHalfAngle);
        Vector3 dir = Quaternion.AngleAxis(yawOffset, Vector3.up) * flatForward;

        return dir.normalized;
    }
}