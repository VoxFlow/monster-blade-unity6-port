using UnityEngine;

public class LeaderboardResultSinglePlayer : MonoBehaviour
{
	[SerializeField]
	private UILabel m_rankText;

	[SerializeField]
	private UILabel m_playerName;

	[SerializeField]
	private UILabel m_playerTitle;

	[SerializeField]
	private UILabel m_score;

	private void Start()
	{
	}

	private void Update()
	{
	}

	public void SetInfo(LeaderPlayer m_playerData)
	{
		m_rankText.text = ((m_playerData.Rank <= 0) ? "#-" : ("#" + m_playerData.Rank));
		m_playerName.text = m_playerData.CharaName;
		m_playerTitle.text = m_playerData.Title;
		m_score.text = ((m_playerData.Score < 0) ? "0" : m_playerData.Score.ToString());
	}
}
