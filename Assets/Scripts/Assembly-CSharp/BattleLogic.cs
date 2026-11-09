using UnityEngine;

public class BattleLogic : MonoBehaviour
{
	protected Player m_player;

	protected Monster m_monster;

	protected PlayerController m_playerController;

	protected AIController m_monsterAI;

	protected virtual void Awake()
	{
		Messenger.AddListener("battle_ready", OnBattleReady);
		Messenger.AddListener("battle_start", OnBattleStart);
		Messenger.AddListener("battle_end", OnBattleEnd);
		Messenger<bool>.AddListener("battle_pause", OnPause);
		Messenger<CutsceneController.CutsceneType>.AddListener(CutsceneMessage.CUTSCENE_START, OnCutsceneStart);
		Messenger<CutsceneController.CutsceneType>.AddListener(CutsceneMessage.CUTSCENE_END, OnCutsceneFinish);
		base.enabled = false;
	}

	protected virtual void Start()
	{
	}

	public virtual void Init()
	{
		StageManager instance = SingletonComponent<StageManager>.Instance;
		if (instance == null)
		{
			return;
		}
		GameObject playerObject = instance.PlayerObject;
		if (playerObject != null)
		{
			if (m_player == null)
			{
				m_player = playerObject.GetComponent<Player>();
			}
			if (m_playerController == null)
			{
				m_playerController = playerObject.GetComponent<PlayerController>();
			}
		}
		GameObject enemyObject = instance.EnemyObject;
		if (enemyObject != null)
		{
			m_monster = enemyObject.GetComponent<Monster>();
			m_monsterAI = enemyObject.GetComponent<AIController>();
		}
		base.enabled = true;
	}

	protected virtual void OnDestroy()
	{
		Messenger.RemoveListener("battle_ready", OnBattleReady);
		Messenger.RemoveListener("battle_start", OnBattleStart);
		Messenger.RemoveListener("battle_end", OnBattleEnd);
		Messenger<bool>.RemoveListener("battle_pause", OnPause);
		Messenger<CutsceneController.CutsceneType>.RemoveListener(CutsceneMessage.CUTSCENE_START, OnCutsceneStart);
		Messenger<CutsceneController.CutsceneType>.RemoveListener(CutsceneMessage.CUTSCENE_END, OnCutsceneFinish);
	}

	protected virtual void OnBattleReady()
	{
		Init();
	}

	protected virtual void OnBattleStart()
	{
	}

	protected virtual void OnBattleEnd()
	{
	}

	protected virtual void OnCutsceneStart(CutsceneController.CutsceneType p_type)
	{
	}

	protected virtual void OnCutsceneFinish(CutsceneController.CutsceneType p_type)
	{
	}

	protected virtual void OnPause(bool p_flag)
	{
	}

	protected virtual void Update()
	{
	}

	protected virtual void LateUpdate()
	{
	}
}
