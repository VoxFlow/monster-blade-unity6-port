using System.Collections.Generic;
using UnityEngine;

public class PlayerAttackLogic : BattleLogic
{
	public float m_inputExpTime = 5f;

	public static bool m_blockSlashEnabled = true;

	private List<float> m_expTimes = new List<float>();

	private List<PowerMove.Action> m_inputs = new List<PowerMove.Action>();

	protected override void Awake()
	{
		base.Awake();
		Messenger<PlayerAttack.Direction>.AddListener("attack_swipe_interpreted", OnPlayerSwipe);
		Messenger<PlayerController.Button>.AddListener("battle_ui_button_pressed", OnUiButtonPressed);
		Messenger<PlayerAttack>.AddListener("player_begin_atk", OnPlayerBeginAttack);
	}

	protected override void OnDestroy()
	{
		base.OnDestroy();
		Messenger<PlayerAttack.Direction>.RemoveListener("attack_swipe_interpreted", OnPlayerSwipe);
		Messenger<PlayerController.Button>.RemoveListener("battle_ui_button_pressed", OnUiButtonPressed);
		Messenger<PlayerAttack>.RemoveListener("player_begin_atk", OnPlayerBeginAttack);
	}

	private bool FindMatchCommand(PowerMove.Action[] p_commandList)
	{
		int num = p_commandList.Length;
		int num2 = m_inputs.Count - num;
		for (int i = 0; i <= num2; i++)
		{
			bool flag = true;
			for (int j = 0; j < num; j++)
			{
				if (m_inputs[i + j] != p_commandList[j])
				{
					flag = false;
					break;
				}
			}
			if (flag)
			{
				return true;
			}
		}
		return false;
	}

	protected bool DetectPowerMove(PowerMove.Action p_action)
	{
		if (!m_player.IsReachableFromCurrent(Player.State.Attack))
		{
			return false;
		}
		m_inputs.Add(p_action);
		int num = -1;
		for (int i = 0; i < PowerMove.commands.Length; i++)
		{
			if (FindMatchCommand(PowerMove.commands[i]))
			{
				num = i;
				break;
			}
		}
		if (num != -1)
		{
			m_inputs.Clear();
			m_expTimes.Clear();
			PlayerAttack playerAttack = new PlayerAttack();
			playerAttack.animation = PowerMove.moves[num];
			playerAttack.type = PlayerAttack.Type.PowerMove;
			playerAttack.direction = PowerMove.attackDirections[num];
			playerAttack.multiplier = PowerMove.dmgMultipliers[num];
			playerAttack.forceCleanHit = true;
			playerAttack.interuptAnimation = true;
			m_player.RegisterAttackCommand(playerAttack);
			m_player.IsReady = true;
			m_player.Idle();
			Messenger<PowerMove.MoveEnum>.Invoke("power_move", (PowerMove.MoveEnum)num);
			return true;
		}
		m_expTimes.Add(m_inputExpTime);
		return false;
	}

	protected virtual void OnPlayerSwipe(PlayerAttack.Direction p_atkDir)
	{
		if (p_atkDir != PlayerAttack.Direction.None && !DetectPowerMove(PowerMove.GetActionFromDirection(p_atkDir)))
		{
			PlayerAttack playerAttack = new PlayerAttack();
			playerAttack.type = PlayerAttack.Type.None;
			playerAttack.direction = p_atkDir;
			m_player.RegisterAttackCommand(playerAttack);
			if (m_blockSlashEnabled && m_player.IsBlocking())
			{
				m_player.IsReady = true;
			}
		}
	}

	protected virtual void OnUiButtonPressed(PlayerController.Button p_button)
	{
		PowerMove.Action p_action = PowerMove.Action.None;
		switch (p_button)
		{
		case PlayerController.Button.LeftArrow:
			p_action = PowerMove.Action.DodgeLeft;
			break;
		case PlayerController.Button.RightArrow:
			p_action = PowerMove.Action.DodgeRight;
			break;
		case PlayerController.Button.Shield:
			p_action = PowerMove.Action.Block;
			break;
		}
		DetectPowerMove(p_action);
	}

	protected virtual void OnPlayerBeginAttack(PlayerAttack p_attack)
	{
		PowerMove.MoveEnum moveEnum = PowerMove.GetMoveEnum(p_attack.animation);
		if (moveEnum != PowerMove.MoveEnum.None)
		{
			m_player.Stamina -= PowerMove.staminaCosts[(int)moveEnum];
		}
	}

	protected override void Update()
	{
		for (int num = m_inputs.Count - 1; num >= 0; num--)
		{
			List<float> expTimes;
			List<float> list = (expTimes = m_expTimes);
			int index2;
			int index = (index2 = num);
			float num2 = expTimes[index2];
			list[index] = num2 - Time.deltaTime;
			if (m_expTimes[num] <= 0f)
			{
				m_inputs.RemoveAt(num);
				m_expTimes.RemoveAt(num);
			}
		}
	}
}
