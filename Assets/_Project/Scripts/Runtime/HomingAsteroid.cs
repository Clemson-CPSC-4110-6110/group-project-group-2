using UnityEngine;

[RequireComponent(typeof(Rigidbody))]
public class HomingAsteroid : MonoBehaviour
{
    private Transform _seekTarget;
    private Rigidbody _rigidBody;

    [Header("Movement Information")]
    [SerializeField] private float moveForce = 3f;
    [SerializeField] private float separationRadius = 3f;
    [SerializeField] private float separationForce = 4f;
    [SerializeField] private float maxSpeed = 1f;

    [Header("Game Data")]
    [SerializeField] private float health = 100f;

    private readonly Collider[] _separationResults = new Collider[32];

    public void Initialize(Transform target)
    {
        _seekTarget = target;
        _rigidBody.maxLinearVelocity = maxSpeed;
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
    }

    private void MoveTowardTarget()
    {
        Vector3 dir = (_seekTarget.position - transform.position).normalized;
        _rigidBody.AddForce(dir * moveForce, ForceMode.Acceleration);
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
}