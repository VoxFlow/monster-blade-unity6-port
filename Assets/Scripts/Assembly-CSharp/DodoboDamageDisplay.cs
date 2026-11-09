public class DodoboDamageDisplay : MonsterDamageDisplay
{
	private Dodobo m_dodobo;

	protected override void Awake()
	{
		base.Awake();
		m_damageValueHUD = "DodoboDamageValue_Panel";
	}

	protected override void Start()
	{
		base.Start();
		m_dodobo = base.gameObject.GetComponent<Dodobo>();
	}

	public override void DisplayDamageValue(DamageData p_damage)
	{
		if (m_damageValue != null && m_dodobo != null)
		{
			((CoinsContainer)m_damageValue).DropCoin(m_dodobo.MoneyPerHit);
		}
	}
}
