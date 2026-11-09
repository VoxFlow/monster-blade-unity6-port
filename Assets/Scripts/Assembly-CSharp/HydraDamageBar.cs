public class HydraDamageBar : DamageBarBehavior
{
	private Hydra m_hydra;

	protected override void Awake()
	{
		base.Awake();
		Messenger.AddListener("battle_ready", OnBattleReady);
	}

	protected override void OnDestroy()
	{
		base.OnDestroy();
		Messenger.RemoveListener("battle_ready", OnBattleReady);
	}

	private void OnBattleReady()
	{
		m_hydra = SingletonComponent<StageManager>.Instance.EnemyObject.GetComponent<Hydra>();
	}

	protected override void LateUpdate()
	{
		if (!(m_hydra == null) && m_watchingData != null && !(m_foregroundSlider == null) && !(m_backgroundSlider == null))
		{
			float hpPercent = m_watchingData.HpPercent;
			float num = m_hydra.GetAllHeadHpPercent() * hpPercent;
			m_foregroundSlider.TimeDelayToMove = 0f;
			m_foregroundSlider.ExpandAccel = 0f;
			if (!m_foregroundSlider.TargetSliderValue.Equals(num))
			{
				m_foregroundSlider.TargetSliderValue = num;
			}
			if (!m_backgroundSlider.TargetSliderValue.Equals(hpPercent))
			{
				SliderSmoothExpand backgroundSlider = m_backgroundSlider;
				float num2 = hpPercent;
				m_backgroundSlider.TargetSliderValue = num2;
				backgroundSlider.SliderValue = num2;
			}
		}
	}
}
