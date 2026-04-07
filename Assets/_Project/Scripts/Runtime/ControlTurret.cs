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
    public float pitchRange = 35f;
    public float handRange = 0.10f;
    public float deadZone = 0.08f;
    public float smoothTime = 0.08f;
    public bool invertYaw = false;
    public bool invertPitch = true;

    [Header("Shooting")]
    public Transform leftBulletSpawn;
    public Transform rightBulletSpawn;
    public Rigidbody bulletPrefab;
    public float bulletSpeed = 40f;
    public float fireCooldown = 0.15f;

    private Vector3 grabStartLocal;
    private float grabStartYaw;
    private float grabStartPitch;
    private bool wasTwoHanded;

    private float yawVelocity;
    private float pitchVelocity;
    private float leftNextFireTime;
    private float rightNextFireTime;

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
            grabStartPitch = GetAngleX(turretPitch);
            wasTwoHanded = true;
        }

        Vector3 delta = local - grabStartLocal;

        float x = Mathf.Clamp(delta.x / handRange, -1f, 1f);
        float y = Mathf.Clamp(delta.y / handRange, -1f, 1f);

        x = ApplyDeadZone(x);
        y = ApplyDeadZone(y);

        float yawInput = invertYaw ? -x : x;
        float pitchInput = invertPitch ? -y : y;

        float targetYaw = grabStartYaw + yawInput * yawRange;
        float targetPitch = grabStartPitch + pitchInput * pitchRange;

        if (turretYaw != null)
        {
            float currentYaw = GetAngleY(turretYaw);
            float smoothedYaw = Mathf.SmoothDampAngle(currentYaw, targetYaw, ref yawVelocity, smoothTime);
            turretYaw.localRotation = Quaternion.Euler(0f, smoothedYaw, 0f);
        }

        if (turretPitch != null)
        {
            float currentPitch = GetAngleX(turretPitch);
            float smoothedPitch = Mathf.SmoothDampAngle(currentPitch, targetPitch, ref pitchVelocity, smoothTime);
            turretPitch.localRotation = Quaternion.Euler(smoothedPitch, 0f, 0f);
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

    float GetAngleX(Transform t)
    {
        if (t == null) return 0f;
        float a = t.localEulerAngles.x;
        if (a > 180f) a -= 360f;
        return a;
    }
}