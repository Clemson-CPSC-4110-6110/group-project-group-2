using UnityEngine;

public static class Player
{
    private const float MaxHealth = 100f;
    private const int MaxAmmo = 100;
    private const float DefaultFireRate = 0.5f;
    private const float MaxFireRate = 15f;
    private const float DefaultTurretFireCooldown = 0.15f;
    private const float DefaultTurretDamagePerShot = 1f;
    private const float DefaultTurretFireRateMultiplier = 1f;
    private const float DefaultTurretDamageMultiplier = 1f;

    private static float health;
    private static int ammo;
    private static float fireRate;
    private static float turretFireRateMultiplier;
    private static float turretDamageMultiplier;
    private static int score;

    public static int getAmmo()
    {
        return ammo;
    }

    public static float getHealth()
    {
        return health;
    }

    public static float getMaxHealth()
    {
        return MaxHealth;
    }

    public static float getFireRate()
    {
        return fireRate;
    }

    public static int getScore()
    {
        return score;
    }

    public static float GetTurretFireRateMultiplier()
    {
        return turretFireRateMultiplier;
    }

    public static float GetTurretShotsPerSecond()
    {
        return (1f / DefaultTurretFireCooldown) * turretFireRateMultiplier;
    }

    public static float GetTurretDamageMultiplier()
    {
        return turretDamageMultiplier;
    }

    public static float GetTurretDamagePerShot()
    {
        return DefaultTurretDamagePerShot * turretDamageMultiplier;
    }

    public static void SetTurretFireRateMultiplier(float multiplier)
    {
        turretFireRateMultiplier = Mathf.Max(1f, multiplier);
    }

    public static void SetTurretDamageMultiplier(float multiplier)
    {
        turretDamageMultiplier = Mathf.Max(1f, multiplier);
    }

    public static void AddScore(int amount)
    {
        if (amount <= 0)
        {
            return;
        }

        score += amount;
    }

    public static bool TrySpendScore(int amount)
    {
        if (amount <= 0)
        {
            return true;
        }

        if (score < amount)
        {
            return false;
        }

        score -= amount;
        return true;
    }

    public static void TakeDamage(float damage)
    {
        if (damage <= 0f)
        {
            return;
        }

        health = Mathf.Max(0f, health - damage);
    }

    public static void UseAmmo(int amount)
    {
        if (amount <= 0)
        {
            return;
        }

        ammo = Mathf.Max(0, ammo - amount);
    }

    public static bool IsDead()
    {
        return health <= 0;
    }

    public static void ResetPlayer()
    {
        Debug.Log("Reset Player");

        health = MaxHealth;
        ammo = MaxAmmo;
        score = 0;
        fireRate = DefaultFireRate;
        turretFireRateMultiplier = DefaultTurretFireRateMultiplier;
        turretDamageMultiplier = DefaultTurretDamageMultiplier;
    }

    public static void IncreaseFireRate(float amount)
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
