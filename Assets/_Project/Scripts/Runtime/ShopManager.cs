using UnityEngine;
using TMPro;

public class ShopManager : MonoBehaviour
{
	[Header("Fire Rate Upgrade")]
    public TextMeshProUGUI fireRateUpgradeCostText;
	public TextMeshProUGUI fireRateStatText;
	[SerializeField] private int fireRateBaseCost = 50;
	[SerializeField] private int fireRateCostStep = 25;
	[SerializeField] private float fireRateMultiplierPerLevel = 0.2f;
	[SerializeField] private int maxFireRateLevel = 5;

	[Header("Damage Upgrade")]
    public TextMeshProUGUI damageUpgradeCostText;
	public TextMeshProUGUI damageStatText;
	[SerializeField] private int damageBaseCost = 50;
	[SerializeField] private int damageCostStep = 25;
	[SerializeField] private float damageMultiplierPerLevel = 0.25f;
	[SerializeField] private int maxDamageLevel = 5;

	private int fireRateLevel;
	private int damageLevel;

	private void Start()
	{
		ApplyUpgrades();
		UpdateUpgradeTexts();
		UpdateUpgradeStatTexts();
	}

	public void BuyFireRateUpgrade()
	{
		TryUpgradeFireRate();
	}

	public void BuyDamageUpgrade()
	{
		TryUpgradeDamage();
	}

	public bool TryUpgradeFireRate()
	{
		if (fireRateLevel >= maxFireRateLevel)
		{
			UpdatefireRateUpgradeCostText();
			return false;
		}

		int cost = GetFireRateUpgradeCost();
		if (!Player.TrySpendScore(cost))
		{
			UpdatefireRateUpgradeCostText();
			return false;
		}

		fireRateLevel++;
		ApplyUpgrades();
		UpdateUpgradeTexts();
		UpdateUpgradeStatTexts();
		return true;
	}

	public bool TryUpgradeDamage()
	{
		if (damageLevel >= maxDamageLevel)
		{
			UpdatedamageUpgradeCostText();
			return false;
		}

		int cost = GetDamageUpgradeCost();
		if (!Player.TrySpendScore(cost))
		{
			UpdatedamageUpgradeCostText();
			return false;
		}

		damageLevel++;
		ApplyUpgrades();
		UpdateUpgradeTexts();
		UpdateUpgradeStatTexts();
		return true;
	}

	public int GetFireRateUpgradeCost()
	{
		return fireRateBaseCost + (fireRateLevel * fireRateCostStep);
	}

	public int GetDamageUpgradeCost()
	{
		return damageBaseCost + (damageLevel * damageCostStep);
	}

	public int GetFireRateLevel()
	{
		return fireRateLevel;
	}

	public int GetDamageLevel()
	{
		return damageLevel;
	}

	void ApplyUpgrades()
	{
		float fireRateMultiplier = 1f + (fireRateLevel * fireRateMultiplierPerLevel);
		float damageMultiplier = 1f + (damageLevel * damageMultiplierPerLevel);

		Player.SetTurretFireRateMultiplier(fireRateMultiplier);
		Player.SetTurretDamageMultiplier(damageMultiplier);
	}

	public void UpdateUpgradeTexts()
	{
		UpdatefireRateUpgradeCostText();
		UpdatedamageUpgradeCostText();
	}

	public void UpdateUpgradeStatTexts()
	{
		UpdateFireRateStatText();
		UpdateDamageStatText();
	}

	public void UpdatefireRateUpgradeCostText()
	{
		if (fireRateUpgradeCostText == null)
			return;

		if (fireRateLevel >= maxFireRateLevel)
		{
			fireRateUpgradeCostText.text = "Fire Rate: MAX";
			return;
		}

		fireRateUpgradeCostText.text = $"Fire Rate L{fireRateLevel + 1} Cost: {GetFireRateUpgradeCost()}";
	}

	public void UpdatedamageUpgradeCostText()
	{
		if (damageUpgradeCostText == null)
			return;

		if (damageLevel >= maxDamageLevel)
		{
			damageUpgradeCostText.text = "Damage: MAX";
			return;
		}

		damageUpgradeCostText.text = $"Damage L{damageLevel + 1} Cost: {GetDamageUpgradeCost()}";
	}

	public void UpdateFireRateStatText()
	{
		if (fireRateStatText == null)
			return;

		fireRateStatText.text = $"Fire rate: {Player.GetTurretShotsPerSecond():0.##}/sec";
	}

	public void UpdateDamageStatText()
	{
		if (damageStatText == null)
			return;

		damageStatText.text = $"Damage: {Player.GetTurretDamagePerShot():0.##}/shot";
	}
}
