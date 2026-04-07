using UnityEngine;

[RequireComponent(typeof(AudioSource))]
[RequireComponent(typeof(Rigidbody))]
public class BulletAudio : MonoBehaviour
{
    [Header("Audio")]
    [SerializeField] private AudioClip fireClip;
    [SerializeField] private float volume = 1f;
    [SerializeField] private bool randomizePitch = true;
    [SerializeField] private float minPitch = 0.95f;
    [SerializeField] private float maxPitch = 1.05f;

    [Header("Lifetime")]
    [SerializeField] private float lifeTime = 5f;

    private AudioSource _source;

    private void Awake()
    {
        _source = GetComponent<AudioSource>();
        _source.playOnAwake = false;
        _source.spatialBlend = 1f;
    }

    private void Start()
    {
        if (randomizePitch)
            _source.pitch = Random.Range(minPitch, maxPitch);

        if (fireClip != null)
            _source.PlayOneShot(fireClip, volume);

        Destroy(gameObject, lifeTime);
    }
}