using System.Collections;
using UnityEngine;
using UnityEngine.XR.Interaction.Toolkit;
using UnityEngine.XR.Interaction.Toolkit.Interactables;

public class AsteroidWaveSpawner : MonoBehaviour
{
    [Header("References")]
    [SerializeField] private GameObject asteroidPrefab;
    [SerializeField] private Transform target;
    [SerializeField] private Transform aimReference;
    [SerializeField] private WaveUI waveUI;
    [SerializeField] private XRSimpleInteractable joystick;

    [Header("Spawn Settings")]
    [SerializeField] private float spawnRadius = 60f;
    [SerializeField] private float timeBetweenWaves = 2f;
    [SerializeField] private int startAsteroidsPerWave = 1;
    [SerializeField] private int maxAsteroidsPerWave = 12;
    [SerializeField] private float horizontalHalfAngle = 35f;

    [Header("Difficulty Scaling")]
    [SerializeField] private int asteroidsAddedPerWave = 1;
    [SerializeField] private float asteroidSpeedIncreasePerWave = 0.10f;
    [SerializeField] private float maxSpeedMultiplier = 3f;

    private int _waveNumber = 0;
    private int _currentAsteroidsPerWave;
    private float _currentSpeedMultiplier = 1f;
    private bool _waveInProgress;
    private bool _joystickGrabbed;

    private void Awake()
    {
        if (joystick == null)
        {
            GameObject joystickObject = GameObject.Find("Joystick");

            if (joystickObject != null)
                joystick = joystickObject.GetComponent<XRSimpleInteractable>();
        }
    }

    private void OnEnable()
    {
        if (joystick != null)
            joystick.selectEntered.AddListener(OnJoystickGrabbed);
    }

    private void OnDisable()
    {
        if (joystick != null)
            joystick.selectEntered.RemoveListener(OnJoystickGrabbed);
    }

    private void Start()
    {
        _currentAsteroidsPerWave = startAsteroidsPerWave;

        if (waveUI != null)
            waveUI.ShowGrabJoystick();

        StartCoroutine(WaveLoop());
    }

    private void OnJoystickGrabbed(SelectEnterEventArgs args)
    {
        _joystickGrabbed = true;

        if (waveUI != null)
            waveUI.HideText();
    }

    private IEnumerator WaveLoop()
    {
        yield return new WaitUntil(() => _joystickGrabbed);

        while (true)
        {
            yield return new WaitUntil(() => !_waveInProgress);

            StartWave();

            yield return new WaitUntil(() => GameObject.FindGameObjectsWithTag("Asteroid").Length == 0);

            _waveInProgress = false;

            yield return new WaitForSeconds(timeBetweenWaves);
        }
    }

    private void StartWave()
    {
        if (asteroidPrefab == null || target == null || aimReference == null)
            return;

        _waveNumber++;

        _currentAsteroidsPerWave = Mathf.Min(
            maxAsteroidsPerWave,
            startAsteroidsPerWave + ((_waveNumber - 1) * asteroidsAddedPerWave)
        );

        _currentSpeedMultiplier = Mathf.Min(
            maxSpeedMultiplier,
            1f + ((_waveNumber - 1) * asteroidSpeedIncreasePerWave)
        );

        if (waveUI != null)
            waveUI.ShowWave(_waveNumber);

        for (int i = 0; i < _currentAsteroidsPerWave; i++)
        {
            Vector3 direction = GetSpawnDirection();
            Vector3 position = target.position + direction * spawnRadius;

            GameObject asteroid = Instantiate(asteroidPrefab, position, Random.rotation);

            HomingAsteroid homingAsteroid = asteroid.GetComponentInChildren<HomingAsteroid>();

            if (homingAsteroid != null)
            {
                homingAsteroid.Initialize(target, _currentSpeedMultiplier);

                EnemyHealth enemyHealth = homingAsteroid.GetComponent<EnemyHealth>();

                if (enemyHealth == null)
                    homingAsteroid.gameObject.AddComponent<EnemyHealth>();
            }
            else
            {
                EnemyHealth enemyHealth = asteroid.GetComponent<EnemyHealth>();

                if (enemyHealth == null)
                    asteroid.AddComponent<EnemyHealth>();
            }
        }

        _waveInProgress = true;
    }

    private Vector3 GetSpawnDirection()
    {
        Vector3 flatForward = Vector3.ProjectOnPlane(-aimReference.forward, Vector3.up);

        if (flatForward.sqrMagnitude < 0.001f)
            flatForward = Vector3.forward;

        flatForward.Normalize();

        float yawOffset = Random.Range(-horizontalHalfAngle, horizontalHalfAngle);
        Vector3 dir = Quaternion.AngleAxis(yawOffset, Vector3.up) * flatForward;

        return dir.normalized;
    }

    public void ResetToWaveOne()
    {
        Debug.Log("Resetting to Wave 1");
        StopAllCoroutines();

        GameObject[] asteroids = GameObject.FindGameObjectsWithTag("Asteroid");

        foreach (GameObject asteroid in asteroids)
            Destroy(asteroid);

        Debug.Log("Resetted to Wave 1");

        if (waveUI != null)
            waveUI.ShowText("You Lost");

        StartCoroutine(RestartAfterLoss());
    }

    private IEnumerator RestartAfterLoss()
    {
        _waveNumber = 0;
        _currentAsteroidsPerWave = startAsteroidsPerWave;
        _currentSpeedMultiplier = 1f;
        _waveInProgress = false;
        _joystickGrabbed = false;

        yield return new WaitForSeconds(5f);

        if (waveUI != null)
            waveUI.ShowGrabJoystick();

        StartCoroutine(WaveLoop());
    }
}