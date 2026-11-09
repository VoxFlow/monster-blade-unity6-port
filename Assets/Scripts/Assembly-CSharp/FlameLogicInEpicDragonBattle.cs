using UnityEngine;

public class FlameLogicInEpicDragonBattle : BattleLogic
{
	private MonsterFlameLogic[] m_flameLogics;

	private void OnEnable()
	{
		Messenger<EpicDragonAI.Phase>.AddListener("epicdragon_phase_change", OnPhaseChange);
	}

	private void OnDisable()
	{
		Messenger<EpicDragonAI.Phase>.RemoveListener("epicdragon_phase_change", OnPhaseChange);
	}

	protected override void OnBattleStart()
	{
		base.OnBattleStart();
		if (m_flameLogics == null)
		{
			m_flameLogics = Object.FindObjectsOfType(typeof(MonsterFlameLogic)) as MonsterFlameLogic[];
		}
		if (m_flameLogics == null)
		{
			Object.Destroy(this);
		}
	}

	protected override void OnCutsceneFinish(CutsceneController.CutsceneType p_type)
	{
		base.OnCutsceneFinish(p_type);
		if (p_type == CutsceneController.CutsceneType.EPICDRAGON_FINAL_PHASE)
		{
			BurnOut();
		}
	}

	private void OnPhaseChange(EpicDragonAI.Phase p_phase)
	{
		if (p_phase == EpicDragonAI.Phase.FINAL || p_phase == EpicDragonAI.Phase.FLYING)
		{
			BurnOut();
		}
	}

	private void BurnOut()
	{
		for (int i = 0; i < m_flameLogics.Length; i++)
		{
			if (null != m_flameLogics[i])
			{
				m_flameLogics[i].OnBurnOut();
			}
		}
	}
}
