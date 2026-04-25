using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using TMPro;

public class UIManager : MonoBehaviour
{
    [Header("Health UI")]
    public Image healthFill;
    public Slider healthBar;

    [Header("Ammo UI")]
    public TextMeshProUGUI ammoCounter;

    [Header("Score UI")]
    public List<TextMeshProUGUI> scoreTexts;

    [Header("Damage Feedback")]
    public Image damageOverlay;
    public float damageOverlayAlpha = 0.45f;
    public float damageFadeTime = 0.35f;

    [Header("Audio")]
    public AudioSource audioSource;
    public AudioClip hitSound;

    private float previousHealth;
    private Coroutine damageFlashRoutine;

    void Start()
    {
        healthBar.maxValue = Player.getMaxHealth();
        healthBar.value = Player.getHealth();

        previousHealth = Player.getHealth();

        ammoCounter.text = $"Ammo: {Player.getAmmo()}";
        UpdateScoreTexts();

        if (damageOverlay != null)
        {
            Color c = damageOverlay.color;
            c.a = 0f;
            damageOverlay.color = c;
        }
    }

    void Update()
    {
        float currentHealth = Player.getHealth();

        healthBar.value = currentHealth;

        if (currentHealth < previousHealth)
        {
            ShowDamageFeedback();
        }

        previousHealth = currentHealth;

        float healthPercent = healthBar.value / healthBar.maxValue;

        if (healthPercent > 0.5f)
        {
            healthFill.color = Color.green;
        }
        else if (healthPercent > 0.25f)
        {
            healthFill.color = Color.yellow;
        }
        else
        {
            healthFill.color = Color.red;
        }

        ammoCounter.text = $"Ammo: {Player.getAmmo()}";
        UpdateScoreTexts();
    }

    private void ShowDamageFeedback()
    {
        if (audioSource != null && hitSound != null)
        {
            audioSource.PlayOneShot(hitSound);
        }

        if (damageOverlay != null)
        {
            if (damageFlashRoutine != null)
                StopCoroutine(damageFlashRoutine);

            damageFlashRoutine = StartCoroutine(DamageFlash());
        }
    }

    private IEnumerator DamageFlash()
    {
        Color c = damageOverlay.color;
        c.a = damageOverlayAlpha;
        damageOverlay.color = c;

        float t = 0f;

        while (t < damageFadeTime)
        {
            t += Time.deltaTime;

            c.a = Mathf.Lerp(damageOverlayAlpha, 0f, t / damageFadeTime);
            damageOverlay.color = c;

            yield return null;
        }

        c.a = 0f;
        damageOverlay.color = c;

        damageFlashRoutine = null;
    }

    private void UpdateScoreTexts()
    {
        if (scoreTexts == null)
            return;

        string scoreLabel = $"Score: {Player.getScore()}";

        for (int i = 0; i < scoreTexts.Count; i++)
        {
            if (scoreTexts[i] != null)
            {
                scoreTexts[i].text = scoreLabel;
            }
        }
    }
}