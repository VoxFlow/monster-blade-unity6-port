using System.Collections.Generic;
using UnityEngine;

public class LeaderboardResultPopup : MonoBehaviour
{
	[SerializeField]
	private LeaderboardResultSinglePlayer[] m_top3Players;

	[SerializeField]
	private LeaderboardResultSinglePlayer m_player;

	[SerializeField]
	private UILabel m_rewardText;

	private void Start()
	{
	}

	private void Update()
	{
	}

	public void SetLeaderboardResultInfo(Dictionary<int, LeaderPlayer> p_players, LeaderPlayer p_selfPlayer)
	{
		for (int i = 1; i < 4; i++)
		{
			if (p_players.ContainsKey(i))
			{
				m_top3Players[i - 1].SetInfo(p_players[i]);
			}
		}
		m_player.SetInfo(p_selfPlayer);
		m_rewardText.text = p_selfPlayer.AmountOfPgcReward + "[D]";
	}
}
