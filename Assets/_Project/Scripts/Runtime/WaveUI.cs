using System.Collections;
using TMPro;
using UnityEngine;

public class WaveUI : MonoBehaviour
{
    [SerializeField] private TextMeshProUGUI waveText;
    [SerializeField] private float showDuration = 1.2f;
    [SerializeField] private float fadeDuration = 0.35f;

    private Coroutine _fadeRoutine;

    private void Awake()
    {
        SetAlpha(0f);
    }

    public void ShowWave(int waveNumber)
    {
        ShowText($"Wave {waveNumber}");
    }

    public void ShowText(string text)
    {
        if (waveText == null)
            return;

        waveText.text = text;
        SetAlpha(1f);

        if (_fadeRoutine != null)
            StopCoroutine(_fadeRoutine);

        _fadeRoutine = StartCoroutine(FadeOut());
    }

    private IEnumerator FadeOut()
    {
        yield return new WaitForSeconds(showDuration);

        Color c = waveText.color;
        float t = 0f;

        while (t < fadeDuration)
        {
            t += Time.deltaTime;
            c.a = Mathf.Lerp(1f, 0f, t / fadeDuration);
            waveText.color = c;
            yield return null;
        }

        c.a = 0f;
        waveText.color = c;
        _fadeRoutine = null;
    }

    private void SetAlpha(float alpha)
    {
        if (waveText == null) return;

        Color c = waveText.color;
        c.a = alpha;
        waveText.color = c;
    }
}