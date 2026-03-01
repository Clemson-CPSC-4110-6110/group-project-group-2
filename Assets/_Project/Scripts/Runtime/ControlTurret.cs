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
    public Transform turretPitch;
    public float yawRange = 60f;
    public float minPitch = -5f;
    public float maxPitch = 45f;
    public float handRange = 0.2f;

    [Header("Shooting")]
    public Transform leftBulletSpawn;
    public Transform rightBulletSpawn;
    public Rigidbody bulletPrefab;
    public float bulletSpeed = 40f;
    public float fireCooldown = 0.15f;

    Vector3 grabStartLocal;
    float grabStartYaw;
    float grabStartPitch;
    bool wasTwoHanded;

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

        // When both hands first grab, store that as neutral
        if (!wasTwoHanded)
        {
            grabStartLocal = local;
            grabStartYaw = GetAngleY(turretYaw);
            grabStartPitch = GetAngleX(turretPitch);
            wasTwoHanded = true;
        }

        Vector3 delta = local - grabStartLocal;

        float x = Mathf.Clamp(delta.x / handRange, -1f, 1f);
        float z = Mathf.Clamp(delta.z / handRange, -1f, 1f);

        if (turretYaw != null)
        {
            float yaw = grabStartYaw + x * yawRange;
            turretYaw.localRotation = Quaternion.Euler(0f, yaw, 0f);
        }

        if (turretPitch != null)
        {
            float pitch = grabStartPitch - z * (maxPitch - minPitch);
            pitch = Mathf.Clamp(pitch, minPitch, maxPitch);
            turretPitch.localRotation = Quaternion.Euler(pitch, 0f, 0f);
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

    float GetAngleX(Transform t)
    {
        if (t == null) return 0f;
        float a = t.localEulerAngles.x;
        if (a > 180f) a -= 360f;
        return a;
    }

    float GetAngleY(Transform t)
    {
        if (t == null) return 0f;
        float a = t.localEulerAngles.y;
        if (a > 180f) a -= 360f;
        return a;
    }
}