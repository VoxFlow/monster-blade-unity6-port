using UnityEngine;

[RequireComponent(typeof(ButtonScale))]
public class FriendAssistButtonPulsing : MonoBehaviour
{
	private enum PulsePhase
	{
		None = -1,
		Rest = 0,
		Pulsing = 1
	}

	private ButtonScale m_scaleAnim;

	private PlayerData m_playerData;

	private MonsterData m_monsterData;

	private NBTimer m_pulseTimer;

	private PulsePhase m_phase = PulsePhase.None;

	public float m_pulseTime = 5f;

	public float m_restTime = 10f;

	protected void Awake()
	{
		m_scaleAnim = base.gameObject.GetComponent<ButtonScale>();
		m_pulseTimer = base.gameObject.AddComponent<NBTimer>();
		m_pulseTimer.Name = "Pulse Timer";
		Messenger.AddListener("battle_ready", OnBattleReady);
	}

	protected void OnDestroy()
	{
		Messenger.RemoveListener("battle_ready", OnBattleReady);
	}

	protected void OnBattleReady()
	{
		StageManager instance = SingletonComponent<StageManager>.Instance;
		if (instance != null && instance.PlayerObject != null && instance.EnemyObject != null)
		{
			Player component = instance.PlayerObject.GetComponent<Player>();
			if (component != null)
			{
				m_playerData = component.Data;
			}
			Monster component2 = instance.EnemyObject.GetComponent<Monster>();
			if (component2 != null)
			{
				m_monsterData = component2.Data.m_monsterData;
			}
		}
		m_phase = PulsePhase.None;
	}

	protected void OnButtonFinishAnimate(GameObject p_button)
	{
		if (m_pulseTimer.Counter == 0f)
		{
			if (m_phase == PulsePhase.Pulsing)
			{
				m_pulseTimer.CountDown(m_restTime);
				m_phase = PulsePhase.Rest;
			}
		}
		else
		{
			m_scaleAnim.OnClicked(OnButtonFinishAnimate);
		}
	}

	protected void BeginPulse()
	{
		m_scaleAnim.OnClicked(OnButtonFinishAnimate);
		m_pulseTimer.CountDown(m_pulseTime);
		m_phase = PulsePhase.Pulsing;
	}

	protected void Update()
	{
		if (m_playerData == null || m_monsterData == null)
		{
			return;
		}
		if (m_phase == PulsePhase.None)
		{
			if (m_playerData.HpPercent <= 0.25f && m_monsterData.isBoss)
			{
				BeginPulse();
			}
			return;
		}
		if (m_playerData.HpPercent > 0.25f || !m_monsterData.isBoss)
		{
			m_pulseTimer.Stop();
			m_phase = PulsePhase.None;
		}
		if (m_phase == PulsePhase.Rest && m_pulseTimer.Counter == 0f)
		{
			BeginPulse();
		}
	}
}
