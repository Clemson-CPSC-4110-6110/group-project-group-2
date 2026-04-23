using UnityEngine;

public class EnemyHealth : MonoBehaviour
{
    [SerializeField] private float maxHealth = 3f;
    [SerializeField] private int scoreReward = 10;

    private float currentHealth;
    private bool isDead;

    private void Awake()
    {
        currentHealth = maxHealth;
    }

    public void TakeDamage(float damage)
    {
        if (isDead || damage <= 0f)
            return;

        currentHealth -= damage;

        if (currentHealth <= 0f)
        {
            Die();
        }
    }

    void Die()
    {
        if (isDead)
            return;

        isDead = true;
        Player.AddScore(scoreReward);
        Destroy(GetDestroyTarget());
    }

    GameObject GetDestroyTarget()
    {
        Transform root = transform.root;

        if (root != null && root.CompareTag("Asteroid"))
            return root.gameObject;

        return gameObject;
    }
}
