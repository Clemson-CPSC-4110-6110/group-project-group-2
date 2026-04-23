using UnityEngine;

public class SpaceShip : MonoBehaviour
{
    [SerializeField] private AsteroidWaveSpawner waveSpawner;

    // Start is called once before the first execution of Update after the MonoBehaviour is created
    void Start()
    {
        Player.ResetPlayer();
    }

    public void TakeDamage(float damage)
    {
        Player.TakeDamage(damage);

        if (Player.getHealth() <= 0f)
        {
            LoseGame();
        }
    }

    private void LoseGame()
    {
        Debug.Log("Game Lost");

        waveSpawner.ResetToWaveOne();
        Player.ResetPlayer();
    }

    public void UseAmmo(int amount)
    {
        Player.UseAmmo(amount);
    }
}
