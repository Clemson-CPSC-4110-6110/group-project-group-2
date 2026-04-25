using UnityEngine;

[RequireComponent(typeof(Rigidbody))]
public class TurretProjectile : MonoBehaviour
{
    [SerializeField] private float baseDamage = 1f;

    [Header("Impact Force")]
    [SerializeField] private float impactForce = 25f;

    public void SetBaseDamage(float value)
    {
        baseDamage = Mathf.Max(0f, value);
    }

    private void OnCollisionEnter(Collision other)
    {
        Rigidbody hitRigidbody = other.collider.attachedRigidbody;

        if (hitRigidbody != null)
        {
            Vector3 forceDirection = GetComponent<Rigidbody>().linearVelocity.normalized;

            hitRigidbody.AddForceAtPosition(
                forceDirection * impactForce,
                other.contacts[0].point,
                ForceMode.Impulse
            );
        }

        float damage = baseDamage * Player.GetTurretDamageMultiplier();

        EnemyHealth enemyHealth = other.collider.GetComponent<EnemyHealth>();

        if (enemyHealth == null)
            enemyHealth = other.collider.GetComponentInParent<EnemyHealth>();

        if (enemyHealth != null)
            enemyHealth.TakeDamage(damage);

        Destroy(gameObject);
    }
}