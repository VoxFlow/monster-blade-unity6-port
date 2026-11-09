using UnityEngine;

public class MonsterFlameFx : FlameFx
{
	private MonsterSpecialFx m_monsterFx;

	private Color m_defaultColor = Color.white;

	public Monster monster
	{
		set
		{
			m_monsterFx = value.GetComponent<MonsterSpecialFx>();
			if (null != m_monsterFx)
			{
				m_defaultColor = m_monsterFx.mainColor;
			}
		}
	}

	public override void Burn()
	{
		base.gameObject.SetActiveRecursively(true);
		for (int i = 0; i < m_flameParticles.Length; i++)
		{
			m_flameParticles[i].Play();
		}
		if (null != m_monsterFx)
		{
			m_monsterFx.mainColor = m_color;
		}
		base.enabled = true;
	}

	public override void OnBurnOut()
	{
		for (int i = 0; i < m_flameParticles.Length; i++)
		{
			m_flameParticles[i].Stop();
		}
		if (null != m_monsterFx)
		{
			m_monsterFx.mainColor = m_defaultColor;
		}
		base.enabled = false;
	}
}
