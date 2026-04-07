using UnityEngine;

[RequireComponent(typeof(Rigidbody))]
public class HomingAsteroid : MonoBehaviour
{
    private Transform _seekTarget;
    private Rigidbody _rigidBody;
    private float _speedMultiplier = 1f;

    [Header("Movement Information")]
    [SerializeField] private float moveForce = 3f;
    [SerializeField] private float separationRadius = 3f;
    [SerializeField] private float separationForce = 4f;
    [SerializeField] private float maxSpeed = 4f;

    private readonly Collider[] _separationResults = new Collider[32];

    public void Initialize(Transform target, float speedMultiplier)
    {
        _seekTarget = target;
        _speedMultiplier = Mathf.Max(1f, speedMultiplier);
    }

    private void Awake()
    {
        _rigidBody = GetComponent<Rigidbody>();
    }

    private void FixedUpdate()
    {
        if (_seekTarget == null) return;

        MoveTowardTarget();
        ApplySeparation();
        ClampSpeed();
    }

    private void MoveTowardTarget()
    {
        Vector3 dir = (_seekTarget.position - transform.position).normalized;
        _rigidBody.AddForce(dir * moveForce * _speedMultiplier, ForceMode.Acceleration);
    }

    private void ApplySeparation()
    {
        int count = Physics.OverlapSphereNonAlloc(transform.position, separationRadius, _separationResults);

        for (int i = 0; i < count; i++)
        {
            Collider collider = _separationResults[i];

            if (collider == null) continue;
            if (collider.gameObject == gameObject) continue;
            if (!collider.CompareTag("Asteroid")) continue;

            Vector3 away = transform.position - collider.transform.position;
            _rigidBody.AddForce(away.normalized * separationForce, ForceMode.Acceleration);
        }
    }

    private void ClampSpeed()
    {
        float limit = maxSpeed * _speedMultiplier;

        if (_rigidBody.linearVelocity.sqrMagnitude > limit * limit)
        {
            _rigidBody.linearVelocity = _rigidBody.linearVelocity.normalized * limit;
        }
    }

    private void OnCollisionEnter(Collision other)
    {
        if (!other.gameObject.CompareTag("SpaceShip"))
            return;

        SpaceShip ship = other.gameObject.GetComponent<SpaceShip>();
        if (ship != null)
        {
            ship.TakeDamage(5f);
        }

        Destroy(gameObject);
    }
}