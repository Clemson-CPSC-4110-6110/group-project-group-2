using System;
using System.Collections.Generic;
using UnityEngine;

public class GameManager : MonoBehaviour
{
    [Header("Level Settings")]
    [SerializeField] private int maxLevel = 5;
    [SerializeField] private float levelDuration = 180f;

    [Header("Wave Scaling")]
    [SerializeField] private int baseAsteroidCount = 5;
    [SerializeField] private int asteroidsPerStep = 3;

    [Header("Dependencies")]
    [SerializeField] private AsteroidSpawner spawner;

    private float _elapsed;
    private int _lastPercentStep;
    private int _level = 1;
    private bool _isRunning;

    private void Start()
    {
        SpawnWave(10);
    }

    public void StartGame()
    {
        _level = 1;
        _isRunning = true;
        ResetLevelState();
    }

    void Update()
    {
        if (!_isRunning)
            return;

        _elapsed += Time.deltaTime;

        if (_elapsed >= levelDuration)
        {
            CompleteLevel();
            return;
        }

        float percent = _elapsed / levelDuration;
        int step = Mathf.FloorToInt(percent * 10f);

        if (step > _lastPercentStep)
        {
            _lastPercentStep = step;
            SpawnWave(step);
        }
    }

    private void SpawnWave(int step)
    {
        int asteroidCount = baseAsteroidCount + step * asteroidsPerStep;
        spawner.SpawnWave(asteroidCount);
    }

    private void CompleteLevel()
    {
        _level++;

        if (_level > maxLevel)
        {
            EndGame();
            return;
        }

        ResetLevelState();
    }

    private void ResetLevelState()
    {
        _elapsed = 0f;
        _lastPercentStep = 0;
    }

    private void EndGame()
    {
        _isRunning = false;
        Debug.Log("Game Complete");
    }
}