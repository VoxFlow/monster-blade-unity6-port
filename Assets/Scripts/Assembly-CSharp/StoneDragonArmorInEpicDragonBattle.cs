using UnityEngine;

[RequireComponent(typeof(EpicDragonBlockLogic))]
public class StoneDragonArmorInEpicDragonBattle : BattleLogic
{
	private EpicDragonAI m_dragonAI;

	private StoneDragonArmorPlayerSkill m_stoneDragonSkill;

	private EpicDragonBlockLogic m_blockLogic;

	private bool m_hasStoneDragonSkill;

	protected override void Awake()
	{
		base.Awake();
		m_blockLogic = base.gameObject.GetComponent<EpicDragonBlockLogic>();
	}

	private void OnEnable()
	{
		Messenger<EpicDragonAI.Phase>.AddListener("epicdragon_phase_change", EnableStoneDragonSkill);
		Messenger.AddListener("perfect_block", OnPerfectBlock);
	}

	private void OnDisable()
	{
		Messenger<EpicDragonAI.Phase>.RemoveListener("epicdragon_phase_change", EnableStoneDragonSkill);
		Messenger.RemoveListener("perfect_block", OnPerfectBlock);
	}

	protected override void OnBattleReady()
	{
		base.OnBattleReady();
		if (null == m_dragonAI)
		{
			m_dragonAI = (EpicDragonAI)m_monsterAI;
		}
	}

	protected override void OnBattleStart()
	{
		base.OnBattleStart();
		if (null == m_dragonAI)
		{
			base.enabled = false;
			return;
		}
		if (null == m_stoneDragonSkill)
		{
			m_stoneDragonSkill = Object.FindObjectOfType(typeof(StoneDragonArmorPlayerSkill)) as StoneDragonArmorPlayerSkill;
			m_hasStoneDragonSkill = null != m_stoneDragonSkill && m_stoneDragonSkill.DoesCharacterHaveSkill(m_player.Data);
		}
		if (!m_hasStoneDragonSkill)
		{
			base.enabled = false;
			return;
		}
		EnableStoneDragonSkill(m_dragonAI.phase);
		m_stoneDragonSkill.blockLogic.petrifiedLogic.enableDeathCutscene = true;
	}

	protected override void OnCutsceneFinish(CutsceneController.CutsceneType p_type)
	{
		if (m_hasStoneDragonSkill && !(null == m_stoneDragonSkill) && m_stoneDragonSkill.skillTriggered && p_type == CutsceneController.CutsceneType.EPICDRAGON_FINAL_PHASE && null != m_stoneDragonSkill.blockLogic)
		{
			m_stoneDragonSkill.blockLogic.enabled = true;
			m_blockLogic.enabled = false;
		}
	}

	private void EnableStoneDragonSkill(EpicDragonAI.Phase p_phase)
	{
		if (m_hasStoneDragonSkill)
		{
			m_stoneDragonSkill.enabled = p_phase == EpicDragonAI.Phase.GROUND;
			m_blockLogic.enabled = !m_stoneDragonSkill.enabled;
		}
	}

	private void OnPerfectBlock()
	{
		if (m_stoneDragonSkill.skillTriggered && null != m_stoneDragonSkill.blockLogic && m_dragonAI.phase == EpicDragonAI.Phase.FINAL)
		{
			m_stoneDragonSkill.blockLogic.PetrifyTarget();
		}
	}
}
