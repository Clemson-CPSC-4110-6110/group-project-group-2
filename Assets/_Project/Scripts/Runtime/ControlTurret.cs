using UnityEngine;
using UnityEngine.XR.Interaction.Toolkit;
using UnityEngine.XR.Interaction.Toolkit.Interactables;

public class TwoHandleTurretStick : MonoBehaviour
{
    [Header("Handles")]
    public XRSimpleInteractable leftHandle;
    public XRSimpleInteractable rightHandle;

    [Header("Turret")]
    public Transform turretYaw;
    public Transform turretPitch; // leave assigned if you want, but this script won't move it
    public float yawRange = 60f;
    public float handRange = 0.10f;
    public float deadZone = 0.08f;
    public float smoothTime = 0.08f;

    [Header("Shooting")]
    public Transform leftBulletSpawn;
    public Transform rightBulletSpawn;
    public Rigidbody bulletPrefab;
    public float bulletSpeed = 40f;
    public float fireCooldown = 0.15f;

    Vector3 grabStartLocal;
    float grabStartYaw;
    bool wasTwoHanded;

    float yawVelocity;
    float leftNextFireTime;
    float rightNextFireTime;

    void OnEnable()
    {
        if (leftHandle != null)
            leftHandle.activated.AddListener(OnLeftActivated);

        if (rightHandle != null)
            rightHandle.activated.AddListener(OnRightActivated);
    }

    void OnDisable()
    {
        if (leftHandle != null)
            leftHandle.activated.RemoveListener(OnLeftActivated);

        if (rightHandle != null)
            rightHandle.activated.RemoveListener(OnRightActivated);
    }

    void Update()
    {
        Transform left = GetHand(leftHandle);
        Transform right = GetHand(rightHandle);

        bool twoHanded = left != null && right != null;

        if (!twoHanded)
        {
            wasTwoHanded = false;
            return;
        }

        Vector3 midPoint = (left.position + right.position) * 0.5f;
        Vector3 local = transform.InverseTransformPoint(midPoint);

        if (!wasTwoHanded)
        {
            grabStartLocal = local;
            grabStartYaw = GetAngleY(turretYaw);
            wasTwoHanded = true;
        }

        Vector3 delta = local - grabStartLocal;

        float x = Mathf.Clamp(delta.x / handRange, -1f, 1f);
        x = ApplyDeadZone(x);

        float targetYaw = grabStartYaw + x * yawRange;

        if (turretYaw != null)
        {
            float currentYaw = GetAngleY(turretYaw);
            float smoothedYaw = Mathf.SmoothDampAngle(currentYaw, targetYaw, ref yawVelocity, smoothTime);
            turretYaw.localRotation = Quaternion.Euler(0f, smoothedYaw, 0f);
        }
    }

    void OnLeftActivated(ActivateEventArgs args)
    {
        if (!BothHandsHeld()) return;
        if (Time.time < leftNextFireTime) return;

        Fire(leftBulletSpawn);
        leftNextFireTime = Time.time + fireCooldown;
    }

    void OnRightActivated(ActivateEventArgs args)
    {
        if (!BothHandsHeld()) return;
        if (Time.time < rightNextFireTime) return;

        Fire(rightBulletSpawn);
        rightNextFireTime = Time.time + fireCooldown;
    }

    bool BothHandsHeld()
    {
        return GetHand(leftHandle) != null && GetHand(rightHandle) != null;
    }

    void Fire(Transform spawnPoint)
    {
        if (spawnPoint == null || bulletPrefab == null)
            return;

        Rigidbody bullet = Instantiate(bulletPrefab, spawnPoint.position, spawnPoint.rotation);
        bullet.linearVelocity = spawnPoint.forward * bulletSpeed;
    }

    Transform GetHand(XRSimpleInteractable handle)
    {
        if (handle == null || handle.interactorsSelecting.Count == 0)
            return null;

        if (handle.interactorsSelecting[0] is Component c)
            return c.transform;

        return null;
    }

    float ApplyDeadZone(float value)
    {
        float abs = Mathf.Abs(value);

        if (abs < deadZone)
            return 0f;

        float sign = Mathf.Sign(value);
        return sign * ((abs - deadZone) / (1f - deadZone));
    }

    float GetAngleY(Transform t)
    {
        if (t == null) return 0f;
        float a = t.localEulerAngles.y;
        if (a > 180f) a -= 360f;
        return a;
    }
}