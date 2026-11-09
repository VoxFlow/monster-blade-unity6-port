using UnityEngine;

public class SimplifiedLeaderboard : MonoBehaviour
{
	public enum RankMovement
	{
		Down = 0,
		NoChange = 1,
		Up = 2
	}

	[SerializeField]
	private UILabel m_rank;

	[SerializeField]
	private UISprite m_rankUpDownTex;

	[SerializeField]
	private UILabel m_rankUpDown;

	[SerializeField]
	private UILabel m_playerName;

	[SerializeField]
	private UILabel m_playerTitle;

	[SerializeField]
	private UILabel m_playerBattleScore;

	private string[] m_arrowSpriteName = new string[3] { "icon_small_arrow_down", "icon_small_arrow_nochange", "icon_small_arrow_up" };

	public void SetInfo()
	{
		if (SingletonComponent<LeaderboardManager>.Instance != null)
		{
			if (SingletonComponent<LeaderboardManager>.Instance.m_player != null)
			{
				if (SingletonComponent<LeaderboardManager>.Instance.m_player.Rank > 0)
				{
					m_rank.text = "#" + SingletonComponent<LeaderboardManager>.Instance.m_player.Rank;
					if (m_rankUpDownTex != null)
					{
						int lastRankInLocal = SingletonComponent<LeaderboardManager>.Instance.GetLastRankInLocal();
						if (lastRankInLocal <= 0)
						{
							m_rankUpDown.text = string.Empty;
							m_rankUpDownTex.enabled = false;
						}
						else
						{
							RankMovement rankMovement = RankMovement.NoChange;
							int num = 0;
							num = SingletonComponent<LeaderboardManager>.Instance.m_player.Rank - lastRankInLocal;
							if (num > 0)
							{
								rankMovement = RankMovement.Down;
								m_rankUpDown.text = "[FF0000]" + num + "[-]";
							}
							else if (num == 0)
							{
								rankMovement = RankMovement.NoChange;
								m_rankUpDown.text = string.Empty;
							}
							else
							{
								rankMovement = RankMovement.Up;
								m_rankUpDown.text = "[00FF00]" + Mathf.Abs(num) + "[-]";
							}
							float x = m_rank.transform.localPosition.x + m_rank.transform.localScale.x / 2f * (float)m_rank.text.Length + 10f;
							m_rankUpDownTex.transform.localPosition = new Vector3(x, m_rankUpDownTex.transform.localPosition.y, m_rankUpDownTex.transform.localPosition.z);
							m_rankUpDownTex.spriteName = m_arrowSpriteName[(int)rankMovement];
							if (rankMovement == RankMovement.NoChange)
							{
								m_rankUpDownTex.enabled = false;
							}
							else
							{
								m_rankUpDownTex.enabled = true;
							}
							float x2 = m_rankUpDownTex.transform.localPosition.x + m_rankUpDownTex.transform.localScale.x + 10f;
							m_rankUpDown.transform.localPosition = new Vector3(x2, m_rankUpDown.transform.localPosition.y, m_rankUpDown.transform.localPosition.z);
						}
					}
				}
				else
				{
					m_rank.text = "# -";
					m_rankUpDownTex.enabled = false;
					m_rankUpDown.text = string.Empty;
				}
				if (SingletonComponent<LeaderboardManager>.Instance.m_player.Score > 0)
				{
					m_playerBattleScore.text = SingletonComponent<LeaderboardManager>.Instance.m_player.Score.ToString();
				}
				else
				{
					m_playerBattleScore.text = "0";
					SingletonComponent<LeaderboardManager>.Instance.SetLastRankInLocal(0);
				}
				m_playerName.text = SingletonComponent<LeaderboardManager>.Instance.m_player.CharaName;
				m_playerTitle.text = SingletonComponent<LeaderboardManager>.Instance.m_player.Title;
			}
			else
			{
				Dbg.LogWarn("Unable to find the self character leaderboard data");
			}
		}
		else
		{
			Dbg.LogError("LeaderboardManager.Instance is null, should shut down the game");
		}
	}
}
