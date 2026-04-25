using UnityEngine;
using UnityEngine.XR;
using UnityEngine.XR.Interaction.Toolkit;
using UnityEngine.XR.Interaction.Toolkit.Interactables;

public class MountedJoystickTurret : MonoBehaviour
{
    [Header("Handle")]
    public XRSimpleInteractable handle;
    public bool requireHandleHeld = true;

    [Header("Turret")]
    public Transform turretYaw;
    public Transform turretPitch;

    public float yawSpeed = 720f;
    public float pitchSpeed = 480f;

    public float yawSharpness = 2.5f;
    public float pitchSharpness = 1.5f;

    public float minPitch = -45f;
    public float maxPitch = 45f;

    [Header("Joystick Visual")]
    public Transform joystickVisual;
    public float joystickTiltAmount = 20f;
    public float joystickReturnSpeed = 10f;

    [Header("Aim Reticle")]
    public Transform reticle;
    public float reticleDistance = 80f;

    [Header("Shooting")]
    public Transform bulletSpawn;
    public Rigidbody bulletPrefab;
    public float bulletSpeed = 40f;
    public float fireCooldown = 0.15f;

    private bool isHeld;
    private float currentYaw;
    private float currentPitch;
    private float nextFireTime;

    private InputDevice leftController;
    private InputDevice rightController;

    private Quaternion joystickStartRotation;

    void Awake()
    {
        if (handle == null)
        {
            GameObject joystickObject = GameObject.Find("Joystick");

            if (joystickObject != null)
                handle = joystickObject.GetComponent<XRSimpleInteractable>();
        }

        if (joystickVisual == null && handle != null)
            joystickVisual = handle.transform;

        if (joystickVisual != null)
            joystickStartRotation = joystickVisual.localRotation;
    }

    void OnEnable()
    {
        if (handle != null)
        {
            handle.selectEntered.AddListener(OnGrabbed);
            handle.selectExited.AddListener(OnReleased);
        }
    }

    void OnDisable()
    {
        if (handle != null)
        {
            handle.selectEntered.RemoveListener(OnGrabbed);
            handle.selectExited.RemoveListener(OnReleased);
        }
    }

    void Start()
    {
        currentYaw = GetAngleY(turretYaw);
        currentPitch = GetAngleX(turretPitch);

        leftController = InputDevices.GetDeviceAtXRNode(XRNode.LeftHand);
        rightController = InputDevices.GetDeviceAtXRNode(XRNode.RightHand);
    }

    void Update()
    {
        RefreshControllers();

        if (requireHandleHeld && !isHeld)
        {
            UpdateReticle();
            ResetJoystickVisual();
            return;
        }

        Vector2 stick = GetBestStickInput();

        if (stick.sqrMagnitude > 0.001f)
        {
            float yawInput = stick.x * yawSharpness;
            float pitchInput = stick.y * pitchSharpness;

            currentYaw += yawInput * yawSpeed * Time.deltaTime;
            currentPitch += pitchInput * pitchSpeed * Time.deltaTime;

            currentPitch = Mathf.Clamp(currentPitch, minPitch, maxPitch);

            if (turretYaw != null)
                turretYaw.localRotation = Quaternion.Euler(0f, currentYaw, 0f);

            if (turretPitch != null)
                turretPitch.localRotation = Quaternion.Euler(currentPitch, 0f, 0f);

            UpdateJoystickVisual(stick);
        }
        else
        {
            ResetJoystickVisual();
        }

        UpdateReticle();

        if (AnyTriggerPressed() && Time.time >= nextFireTime)
        {
            Fire();
            nextFireTime = Time.time + fireCooldown;
        }
    }

    void RefreshControllers()
    {
        if (!leftController.isValid)
            leftController = InputDevices.GetDeviceAtXRNode(XRNode.LeftHand);

        if (!rightController.isValid)
            rightController = InputDevices.GetDeviceAtXRNode(XRNode.RightHand);
    }

    Vector2 GetBestStickInput()
    {
        Vector2 leftStick = Vector2.zero;
        Vector2 rightStick = Vector2.zero;

        if (leftController.isValid)
            leftController.TryGetFeatureValue(CommonUsages.primary2DAxis, out leftStick);

        if (rightController.isValid)
            rightController.TryGetFeatureValue(CommonUsages.primary2DAxis, out rightStick);

        if (leftStick.sqrMagnitude > rightStick.sqrMagnitude)
            return leftStick;

        return rightStick;
    }

    bool AnyTriggerPressed()
    {
        bool leftTrigger = false;
        bool rightTrigger = false;

        if (leftController.isValid)
            leftController.TryGetFeatureValue(CommonUsages.triggerButton, out leftTrigger);

        if (rightController.isValid)
            rightController.TryGetFeatureValue(CommonUsages.triggerButton, out rightTrigger);

        return leftTrigger || rightTrigger;
    }

    void UpdateJoystickVisual(Vector2 stick)
    {
        if (joystickVisual == null)
            return;

        float forwardBackTilt = -stick.y * joystickTiltAmount;
        float leftRightTilt = stick.x * joystickTiltAmount;

        joystickVisual.localRotation =
            joystickStartRotation *
            Quaternion.Euler(forwardBackTilt, 0f, leftRightTilt);
    }

    void ResetJoystickVisual()
    {
        if (joystickVisual == null)
            return;

        joystickVisual.localRotation = Quaternion.Lerp(
            joystickVisual.localRotation,
            joystickStartRotation,
            Time.deltaTime * joystickReturnSpeed
        );
    }

    void OnGrabbed(SelectEnterEventArgs args)
    {
        isHeld = true;
    }

    void OnReleased(SelectExitEventArgs args)
    {
        isHeld = false;
    }

    void UpdateReticle()
    {
        if (reticle == null || bulletSpawn == null)
            return;

        Ray ray = new Ray(bulletSpawn.position, bulletSpawn.forward);
        RaycastHit[] hits = Physics.RaycastAll(ray, reticleDistance);

        System.Array.Sort(hits, (a, b) => a.distance.CompareTo(b.distance));

        foreach (RaycastHit hit in hits)
        {
            if (ShouldIgnoreReticleHit(hit))
                continue;

            reticle.position = hit.point;
            reticle.rotation = Quaternion.LookRotation(hit.normal);
            return;
        }

        reticle.position = bulletSpawn.position + bulletSpawn.forward * reticleDistance;
        reticle.rotation = Quaternion.LookRotation(-bulletSpawn.forward);
    }

    bool ShouldIgnoreReticleHit(RaycastHit hit)
    {
        if (hit.collider == null)
            return true;

        Transform hitTransform = hit.collider.transform;

        if (reticle != null)
        {
            if (hitTransform == reticle || hitTransform.IsChildOf(reticle))
                return true;
        }

        if (hit.collider.GetComponentInParent<TurretProjectile>() != null)
            return true;

        Rigidbody hitBody = hit.collider.attachedRigidbody;

        if (hitBody != null && bulletPrefab != null)
        {
            string hitName = hitBody.gameObject.name.Replace("(Clone)", "").Trim();
            string prefabName = bulletPrefab.gameObject.name.Trim();

            if (hitName == prefabName)
                return true;
        }

        return false;
    }

    void Fire()
    {
        if (bulletSpawn == null || bulletPrefab == null)
            return;

        Rigidbody bullet = Instantiate(
            bulletPrefab,
            bulletSpawn.position,
            bulletSpawn.rotation
        );

        bullet.linearVelocity = bulletSpawn.forward * bulletSpeed;

        TurretProjectile projectile = bullet.GetComponent<TurretProjectile>();

        if (projectile == null)
            projectile = bullet.gameObject.AddComponent<TurretProjectile>();

        projectile.SetBaseDamage(Player.GetTurretDamagePerShot());
    }

    float GetAngleY(Transform t)
    {
        if (t == null) return 0f;

        float a = t.localEulerAngles.y;

        if (a > 180f)
            a -= 360f;

        return a;
    }

    float GetAngleX(Transform t)
    {
        if (t == null) return 0f;

        float a = t.localEulerAngles.x;

        if (a > 180f)
            a -= 360f;

        return a;
    }
}