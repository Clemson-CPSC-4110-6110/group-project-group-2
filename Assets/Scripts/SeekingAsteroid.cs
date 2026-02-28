using UnityEngine;

[RequireComponent(typeof(Rigidbody))]
public class SeekingAsteroid : MonoBehaviour
{
    private Transform _seekTarget;
    private Rigidbody _rigidBody;

    [Header("Movement Information")]
    [SerializeField] private float moveForce = 20f;
    [SerializeField] private float separationRadius = 5f;
    [SerializeField] private float separationForce = 15f;
    [SerializeField] private float maxSpeed = 30f;

    [Header("Game Data")]
    [SerializeField] private float health = 100;

    private readonly Collider[] _separationResults = new Collider[32];

    public void Initialize(Transform target)
    {
        _seekTarget = target;
        _rigidBody.maxLinearVelocity = maxSpeed;
    }

    void Awake()
    {
        _rigidBody = GetComponent<Rigidbody>();
    }

    void FixedUpdate()
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

            if (collider.gameObject == gameObject) continue;
            if (!collider.CompareTag("Asteroid")) continue;

            Vector3 away = transform.position - collider.transform.position;
            _rigidBody.AddForce(away.normalized * separationForce, ForceMode.Acceleration);
        }
    }
}
