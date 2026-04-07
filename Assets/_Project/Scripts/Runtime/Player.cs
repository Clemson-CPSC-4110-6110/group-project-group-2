using UnityEngine;

public class Player
{
    private const float MaxHealth = 100f;
    private const int MaxAmmo = 100;
    private const float DefaultFireRate = 0.5f;
    private const float MaxFireRate = 15f;

    private float health;
    private int ammo;
    private float fireRate;
    private int score;

    public Player()
    {
        ResetPlayer();
        fireRate = DefaultFireRate;
    }

    public int getAmmo()
    {
        return ammo;
    }

    public float getHealth()
    {
        return health;
    }

    public float getMaxHealth()
    {
        return MaxHealth;
    }

    public float getFireRate()
    {
        return fireRate;
    }

    public int getScore()
    {
        return score;
    }

    public void TakeDamage(float damage)
    {
        if (damage <= 0f)
        {
            return;
        }

        health = Mathf.Max(0f, health - damage);
    }

    public void UseAmmo(int amount)
    {
        if (amount <= 0)
        {
            return;
        }

        ammo = Mathf.Max(0, ammo - amount);
    }

    public bool IsDead()
    {
        return health <= 0;
    }

    public void ResetPlayer()
    {
        health = MaxHealth;
        ammo = MaxAmmo;
        score = 0;
        fireRate = DefaultFireRate;
    }

    public void IncreaseFireRate(float amount)
    {
        if (amount < 0)
        {
            Debug.LogWarning("Fire rate increase amount must be positive.");
            return;
        }

        if (fireRate + amount > MaxFireRate)
        {
            Debug.LogWarning("Fire rate cannot exceed 15.");
            fireRate = MaxFireRate;
            return;
        }

        fireRate += amount;
    }
}
