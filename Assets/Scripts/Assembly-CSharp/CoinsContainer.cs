using UnityEngine;

public class CoinsContainer : DamageValueContainer
{
	public Color m_textColor = Color.white;

	private CoinsDrop[] m_coins;

	protected override void Start()
	{
		base.Start();
		m_coins = MBHier.GetComponentsInChildren<CoinsDrop>(this, true);
	}

	public void DropCoin(int p_value)
	{
		DisplayDamage(p_value, p_value, m_textColor, "damage_value_normal", string.Empty);
		CoinsDrop coinsDrop = null;
		CoinsDrop[] coins = m_coins;
		foreach (CoinsDrop coinsDrop2 in coins)
		{
			if (!coinsDrop2.gameObject.active)
			{
				coinsDrop = coinsDrop2;
				break;
			}
		}
		if (coinsDrop != null)
		{
			Vector3 screenPos = UiUtils.GetScreenPos(m_referencePosition.position, base.BattleCamera, m_uiCam);
			screenPos.y += 80f;
			coinsDrop.Show(screenPos);
		}
		else
		{
			Debug.LogWarning("There is no coin sprite available.");
		}
	}

	private void LateUpdate()
	{
		for (int i = 0; i < m_damageValueScripts.Length; i++)
		{
			DamageValue damageValue = m_damageValueScripts[i];
			CoinsDrop coinsDrop = m_coins[i];
			if (!damageValue.gameObject.active && coinsDrop.gameObject.active)
			{
				coinsDrop.Hide();
			}
		}
	}
}
