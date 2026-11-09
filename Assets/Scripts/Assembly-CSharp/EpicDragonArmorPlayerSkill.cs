using System.Collections;
using UnityEngine;

public class EpicDragonArmorPlayerSkill : ArmorSetPlayerSkill
{
	[SerializeField]
	private float m_delayDamageTime = 0.1f;

	[SerializeField]
	private float m_minHpLimit = 0.9f;

	[SerializeField]
	private PlayerBuffFx m_buffFxPrefab;

	private bool m_active;

	private bool m_battleStarted;

	private PlayerBuffFx m_buffFx;

	private MonsterDamageLogic m_monsterDamageLogic;

	private DamageData m_delayDamage = new DamageData();

	private CutsceneController m_cutsceneController;

	private bool skillActivated
	{
		get
		{
			return m_active;
		}
		set
		{
			if (value)
			{
				if (null != m_buffFxPrefab)
				{
					m_buffFx = PlayerBuffFx.InstantiateFx(m_buffFxPrefab.gameObject, base.player, "Sword_Bone", TransformAttachMode.RETAIN_LOCAL_TRANSFORM);
				}
			}
			else if (null != m_buffFx)
			{
				Object.Destroy(m_buffFx.gameObject);
			}
			m_active = value;
		}
	}

	protected override void Awake()
	{
		base.Awake();
		skillActivated = false;
		Messenger.AddListener("battle_start", OnBattleStart);
	}

	protected override void OnDestroy()
	{
		base.OnDestroy();
		Messenger.RemoveListener("battle_start", OnBattleStart);
	}

	protected override void OnBattleReady()
	{
		base.OnBattleReady();
		GameObject gameObject = GameObject.Find("BattleLogic");
		if (gameObject != null)
		{
			m_monsterDamageLogic = gameObject.GetComponent<MonsterDamageLogic>();
		}
		if (null != base.player && null != base.monster)
		{
			m_delayDamage.attacker = base.player.Data;
			m_delayDamage.defender = base.monster.Data;
			m_delayDamage.damageType = DamageType.Special;
		}
		if (null == m_cutsceneController)
		{
			m_cutsceneController = SingletonComponent<CutsceneController>.Instance;
		}
	}

	private void OnBattleStart()
	{
		m_battleStarted = true;
	}

	private void OnEnable()
	{
		Messenger<DamageData>.AddListener("monster_receive_dmg", OnMonsterGetHit);
	}

	private void OnDisable()
	{
		Messenger<DamageData>.RemoveListener("monster_receive_dmg", OnMonsterGetHit);
	}

	private void OnMonsterGetHit(DamageData p_damage)
	{
		if (skillActivated && p_damage != null && p_damage != m_delayDamage && p_damage.damageType != DamageType.Special)
		{
			StartCoroutine(DelayDamage(p_damage));
		}
	}

	private IEnumerator DelayDamage(DamageData p_damage)
	{
		yield return new MBWaitForSeconds(m_delayDamageTime);   // PORT (CD-059)
		if (!(null == m_monsterDamageLogic) && !(null == base.player) && !(null == base.monster) && base.player.Data.Hp != 0f && base.monster.Data.Hp != 0f)
		{
			float roundDamage = (int)p_damage.rawDamage;
			m_delayDamage.rawDamage = (m_delayDamage.finalDamage = roundDamage);
			Messenger<DamageData>.Invoke("monster_receive_dmg", m_delayDamage);
			m_monsterDamageLogic.DealDamageToMonster(m_delayDamage);
		}
	}

	private void Update()
	{
		if (!m_battleStarted)
		{
			return;
		}
		if (!skillTriggered && null != m_cutsceneController)
		{
			if (m_cutsceneController.enabled)
			{
				TriggerSkill();
			}
			return;
		}
		float hpPercent = base.player.Data.HpPercent;
		if (skillActivated && hpPercent < m_minHpLimit)
		{
			skillActivated = false;
		}
		else if (!skillActivated && hpPercent >= m_minHpLimit)
		{
			skillActivated = true;
		}
	}

	protected new virtual void PostCutsceneInitialization()
	{
		skillActivated = true;
	}
}
