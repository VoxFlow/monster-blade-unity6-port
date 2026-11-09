using UnityEngine;

public class StoneDragonArmorPlayerSkill : ArmorSetPlayerSkill
{
	[SerializeField]
	protected StoneDragonArmorBlockLogic m_skillLogicPrefab;

	[SerializeField]
	protected PlayerBuffFx m_buffFxPrefab;

	[SerializeField]
	protected float m_duration = 8f;

	[SerializeField]
	private float m_armourMultiplier = 1f;

	[SerializeField]
	protected int m_triggerCondition = 3;

	private int m_blockCount;

	private StoneDragonArmorBlockLogic m_blockLogic;

	private PlayerBuffFx m_buffFx;

	public StoneDragonArmorBlockLogic blockLogic
	{
		get
		{
			return m_blockLogic;
		}
	}

	protected override void OnBattleReady()
	{
		base.OnBattleReady();
		if (!base.enabled)
		{
			return;
		}
		bool flag = base.monster != null && base.monster is StoneDragon;
		m_blockCount = 0;
		if (null == m_blockLogic)
		{
			m_blockLogic = Object.Instantiate(m_skillLogicPrefab) as StoneDragonArmorBlockLogic;
		}
		if (null != m_blockLogic)
		{
			if (flag)
			{
				m_blockLogic.duration = ((StoneDragon)base.monster).petrifyDuration;
				m_blockLogic.armourMod = ((StoneDragon)base.monster).petrifiedArmourMod;
			}
			else
			{
				m_blockLogic.duration = m_duration;
				m_blockLogic.armourMod = m_armourMultiplier;
			}
			m_blockLogic.Init();
		}
	}

	private void OnBattleStart()
	{
		BlockSystem[] array = Object.FindObjectsOfType(typeof(BlockSystem)) as BlockSystem[];
		for (int i = 0; i < array.Length; i++)
		{
			if (array[i] != m_blockLogic)
			{
				array[i].enabled = false;
			}
		}
	}

	private void OnEnable()
	{
		if (null != m_blockLogic)
		{
			m_blockLogic.enabled = true;
		}
		Messenger.AddListener("perfect_block", OnPerfectBlock);
		Messenger.AddListener("battle_start", OnBattleStart);
	}

	private void OnDisable()
	{
		if (null != m_blockLogic)
		{
			m_blockLogic.enabled = false;
		}
		Messenger.RemoveListener("perfect_block", OnPerfectBlock);
		Messenger.RemoveListener("battle_start", OnBattleStart);
	}

	protected override void PreCutsceneInitialization()
	{
		m_blockLogic.skillEnabled = true;
		base.monster.Invincible = true;
	}

	protected override void PostCutsceneInitialization()
	{
		if (null != base.player && null != m_buffFxPrefab)
		{
			m_buffFx = PlayerBuffFx.InstantiateFx(m_buffFxPrefab.gameObject, base.player, "Sword_Bone", TransformAttachMode.RETAIN_LOCAL_TRANSFORM);
		}
		if (null != base.monster)
		{
			base.monster.Invincible = false;
		}
	}

	private void OnPerfectBlock()
	{
		if (base.enabled && !m_skillTriggered && !(null == base.monster) && !(null == SingletonComponent<CutsceneController>.Instance))
		{
			m_blockCount++;
			if (m_triggerCondition == m_blockCount)
			{
				TriggerSkill();
			}
		}
	}
}
