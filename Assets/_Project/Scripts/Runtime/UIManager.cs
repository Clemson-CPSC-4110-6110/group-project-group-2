using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using TMPro;


public class UIManager : MonoBehaviour
{
    public Image healthFill;
    public Slider healthBar;
    public TextMeshProUGUI ammoCounter;
    public List<TextMeshProUGUI> scoreTexts;


    // Start is called once before the first execution of Update after the MonoBehaviour is created
    void Start()
    {
        healthBar.maxValue = Player.getMaxHealth();
        healthBar.value = Player.getHealth();

        ammoCounter.text = $"Ammo: {Player.getAmmo()}";
        UpdateScoreTexts();
    }

    // Update is called once per frame
    void Update()
    {
        healthBar.value = Player.getHealth();

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
