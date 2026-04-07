using UnityEngine;
using UnityEngine.UI;
using TMPro;


public class UIManager : MonoBehaviour
{
    public Slider healthBar;
    public TextMeshProUGUI ammoCounter;
    public TextMeshProUGUI scoreText;
    // Start is called once before the first execution of Update after the MonoBehaviour is created
    void Start()
    {
        healthBar.maxValue = Player.getMaxHealth();
        healthBar.value = Player.getHealth();

        ammoCounter.text = $"Ammo: {Player.getAmmo()}";
        scoreText.text = $"Score: {Player.getScore()}";
    }

    // Update is called once per frame
    void Update()
    {
        healthBar.value = Player.getHealth();

        ammoCounter.text = $"Ammo: {Player.getAmmo()}";
        scoreText.text = $"Score: {Player.getScore()}";
    }
}
