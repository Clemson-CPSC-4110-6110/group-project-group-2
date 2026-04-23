using UnityEngine;

[RequireComponent(typeof(Rigidbody))]
public class TurretProjectile : MonoBehaviour
{
    [SerializeField] private float baseDamage = 1f;

    public void SetBaseDamage(float value)
    {
        baseDamage = Mathf.Max(0f, value);
    }

    private void OnCollisionEnter(Collision other)
    {
        float damage = baseDamage * Player.GetTurretDamageMultiplier();

        EnemyHealth enemyHealth = other.collider.GetComponent<EnemyHealth>();
        if (enemyHealth == null)
        {
            enemyHealth = other.collider.GetComponentInParent<EnemyHealth>();
        }

        if (enemyHealth != null)
        {
            enemyHealth.TakeDamage(damage);
        }

        Destroy(gameObject);
    }
}
