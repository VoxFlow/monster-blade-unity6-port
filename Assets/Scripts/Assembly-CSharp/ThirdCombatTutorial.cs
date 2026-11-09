using UnityEngine;

public class ThirdCombatTutorial : Tutorial
{
	private Monster m_monster;

	private TutorialMonsterAi m_monsterAI;

	protected override void OnEnable()
	{
		base.OnEnable();
		Messenger.AddListener("battle_start", OnBattleStart);
		Messenger.AddListener("battle_end", OnBattleEnd);
		Messenger<PowerMove.MoveEnum>.AddListener("power_move", OnPowerMove);
	}

	protected override void OnDisable()
	{
		base.OnDisable();
		Messenger.RemoveListener("battle_start", OnBattleStart);
		Messenger.RemoveListener("battle_end", OnBattleEnd);
		Messenger<PowerMove.MoveEnum>.RemoveListener("power_move", OnPowerMove);
	}

	public override void Start()
	{
		base.Start();
		m_monster = SingletonComponent<StageManager>.Instance.EnemyObject.GetComponent<Monster>();
		m_monsterAI = m_monster.GetComponent<TutorialMonsterAi>();
		SingletonComponent<BattleManager>.Instance.enabled = true;
		m_monsterAI.enabled = false;
		m_monsterAI.isImmortal = true;
		SingletonComponent<BattleScreenUI>.Instance.Initialize();
		SingletonComponent<BattleScreenUI>.Instance.ShowBattleUiButtons(false);
		SingletonComponent<BattleManager>.Instance.ChangeState(BattleManager.State.Battle);
	}

	private void OnPowerMove(PowerMove.MoveEnum p_move)
	{
		string triggerKey = "power_move";
		TryAdvance(triggerKey);
	}

	private void OnBattleStart()
	{
		string triggerKey = "battle_start";
		TryAdvance(triggerKey);
	}

	private void OnBattleEnd()
	{
		string triggerKey = "battle_end";
		TryAdvance(triggerKey);
	}

	public override void ShowSubtitle(Action action)
	{
		base.ShowSubtitle(action);
	}

	private void ShowGestures(Action action)
	{
		SingletonComponent<TutorialManager>.Instance.EndSubtitle();
		SingletonComponent<BattleScreenUI>.Instance.ShowBattleUiButtons(true);
		m_monsterAI.enabled = true;
		m_monsterAI.allowAttack = false;
		base.gameObject.AddComponent<TutorialPowerMoveHint>();
	}

	private void FreeBattle()
	{
		Object.Destroy(GetComponent<TutorialPowerMoveHint>());
		SingletonComponent<TutorialManager>.Instance.EndSubtitle();
		SingletonComponent<BattleScreenUI>.Instance.ShowBattleUiButtons(true);
		SingletonComponent<BattleManager>.Instance.enabled = true;
		m_monsterAI.enabled = true;
		m_monsterAI.isImmortal = false;
		m_monsterAI.allowAttack = true;
	}

	private void ThirdCombatEnd()
	{
		Messenger<State>.Invoke("tutorial_completed", State.Combat3);
	}
}
