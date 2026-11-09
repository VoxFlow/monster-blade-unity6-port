using UnityEngine;

public class LeaderboardFakeColumn : MonoBehaviour
{
	[SerializeField]
	private UISprite m_columnBg;

	[SerializeField]
	private UILabel m_rank;

	[SerializeField]
	private UILabel m_name;

	[SerializeField]
	private UILabel m_score;

	[SerializeField]
	private UISprite m_icon;

	private void Start()
	{
	}

	private void Update()
	{
	}

	public void SetRankText(string p_rank)
	{
		m_rank.text = "#" + p_rank;
	}

	public void SetNameText(string p_name)
	{
		m_name.text = p_name;
	}

	public void SetScoreText(string p_score)
	{
		m_score.text = p_score;
	}
}
