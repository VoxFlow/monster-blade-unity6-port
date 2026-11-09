using UnityEngine;

public abstract class Skill : MonoBehaviour
{
	protected Player player { get; private set; }

	protected Monster monster { get; private set; }

	protected PlayerController playerController { get; private set; }

	protected AIController monsterAI { get; private set; }

	public abstract bool DoesCharacterHaveSkill(PlayerData p_data);

	protected virtual void Awake()
	{
		base.enabled = false;
		Messenger.AddListener("battle_ready", OnBattleReady);
	}

	protected virtual void OnDestroy()
	{
		Messenger.RemoveListener("battle_ready", OnBattleReady);
	}

	protected virtual void OnBattleReady()
	{
		StageManager instance = SingletonComponent<StageManager>.Instance;
		if (instance == null)
		{
			return;
		}
		GameObject playerObject = instance.PlayerObject;
		if (playerObject != null)
		{
			if (player == null)
			{
				player = playerObject.GetComponent<Player>();
			}
			if (playerController == null)
			{
				playerController = playerObject.GetComponent<PlayerController>();
			}
		}
		GameObject enemyObject = instance.EnemyObject;
		if (enemyObject != null)
		{
			monster = enemyObject.GetComponent<Monster>();
			monsterAI = enemyObject.GetComponent<AIController>();
		}
	}
}
