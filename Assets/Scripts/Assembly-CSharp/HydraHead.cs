using UnityEngine;

public class HydraHead : MonoBehaviour
{
	public enum HeadEnum
	{
		None = -1,
		Left = 0,
		Right = 1,
		Middle = 2,
		Max = 3
	}

	public HeadEnum m_head = HeadEnum.None;

	public float m_hpRegenerateTime = 25f;

	private float m_hp = 1f;

	private float m_maxHp = 1f;

	private float m_regenRate;

	private NBTimer m_regenTimer;

	private NBTimer m_regenDelayTimer;

	public bool IsCutOff
	{
		get
		{
			return m_hp == 0f;
		}
	}

	public float MaxHp
	{
		set
		{
			m_maxHp = value;
			Hp = m_hp;
			m_regenRate = m_maxHp / m_hpRegenerateTime;
		}
	}

	public float Hp
	{
		get
		{
			return m_hp;
		}
		set
		{
			m_hp = value;
			m_hp = Mathf.Clamp(m_hp, 0f, m_maxHp);
			if (m_hp == 0f)
			{
				m_regenTimer.CountDown(m_hpRegenerateTime);
				Messenger<HeadEnum>.Invoke("hydra_head_cut_off", m_head);
			}
			else if (m_regenTimer.enabled)
			{
				m_regenTimer.Reset();
			}
		}
	}

	public float HpPercent
	{
		get
		{
			return m_hp / m_maxHp;
		}
	}

	private void Awake()
	{
		m_regenTimer = base.gameObject.AddComponent<NBTimer>();
		m_regenTimer.Name = "Regen Timer";
		m_regenTimer.IsRealtime = false;
		m_regenDelayTimer = base.gameObject.AddComponent<NBTimer>();
		m_regenDelayTimer.Name = "Regen Delay Timer";
		m_regenDelayTimer.IsRealtime = false;
		Messenger.AddListener("battle_ready", OnBattleReady);
		Messenger.AddListener("battle_start", OnBattleStart);
		Messenger.AddListener("battle_end", OnBattleEnd);
	}

	private void OnDestroy()
	{
		Messenger.RemoveListener("battle_ready", OnBattleReady);
		Messenger.RemoveListener("battle_start", OnBattleStart);
		Messenger.RemoveListener("battle_end", OnBattleEnd);
	}

	private void OnBattleReady()
	{
		m_hp = (m_maxHp = SingletonComponent<StageManager>.Instance.EnemyObject.GetComponent<Monster>().Data.MaxHp / 6f);
		m_regenRate = m_maxHp / m_hpRegenerateTime;
	}

	private void OnBattleStart()
	{
		base.enabled = true;
	}

	private void OnBattleEnd()
	{
		base.enabled = false;
	}

	public void Recover()
	{
		if (IsCutOff)
		{
			m_hp = m_maxHp;
			Messenger<HeadEnum>.Invoke("hydra_head_recover", m_head);
		}
	}

	public void OnMonsterDead()
	{
		m_regenTimer.Reset();
		base.enabled = false;
	}

	private void OnEnable()
	{
		m_regenTimer.Pause(false);
	}

	private void OnDisable()
	{
		m_regenTimer.Pause(true);
	}

	public void TakeDamage(float p_dmg)
	{
		Hp -= p_dmg;
		m_regenDelayTimer.CountDown(0.5f);
		Messenger<HeadEnum>.Invoke("hydra_head_hit", m_head);
	}

	public void Pause(bool p_pause)
	{
		m_regenTimer.Pause(p_pause);
		m_regenDelayTimer.Pause(p_pause);
	}

	private void LateUpdate()
	{
		if (!IsCutOff && m_hp != m_maxHp && m_regenDelayTimer.Counter == 0f)
		{
			Hp += m_regenRate * Time.deltaTime;
		}
		else if (IsCutOff && m_regenTimer.Counter == 0f)
		{
			Recover();
		}
	}
}
