using UnityEngine;
using UnityEngine.UI;
using TMPro;


public class UIManager : MonoBehaviour
{
    public Slider healthBar;
    public TextMeshProUGUI ammoCounter;
    public TextMeshProUGUI scoreText;

    private Player player;
    // Start is called once before the first execution of Update after the MonoBehaviour is created
    void Start()
    {
        healthBar.maxValue = player.getMaxHealth();
        healthBar.value = player.getHealth();

        ammoCounter.text = $"Ammo: {player.getAmmo()}";
        scoreText.text = $"Score: {player.getScore()}";
    }

    // Update is called once per frame
    void Update()
    {
        healthBar.value = player.getHealth();

        ammoCounter.text = $"Ammo: {player.getAmmo()}";
        scoreText.text = $"Score: {player.getScore()}";
    }
}
