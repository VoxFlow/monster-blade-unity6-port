using UnityEngine;

public class LeaderboardCell : NBUIGridCell<LeaderPlayer>
{
	public float m_highlightedExtendRatio = 1.05f;

	[SerializeField]
	private UILabel m_rank;

	[SerializeField]
	private UILabel m_name;

	[SerializeField]
	private UILabel m_score;

	[SerializeField]
	private UISprite m_icon;

	[SerializeField]
	private UISprite m_background;

	private Transform cachedTransform;

	protected override void Awake()
	{
		base.Awake();
		cachedTransform = base.transform;
		Object.DestroyImmediate(GetComponent<UIPanel>());
	}

	protected override void Start()
	{
		base.Start();
	}

	private void Update()
	{
		if (m_background != null && m_background.spriteName.Contains("selected"))
		{
			SingletonComponent<LeaderboardManager>.Instance.m_PlayerColumnPosY = cachedTransform.position.y;
		}
	}

	public void SetEmpty()
	{
		m_rank.text = "...";
		m_name.text = string.Empty;
		m_score.text = string.Empty;
		if (m_icon != null)
		{
			m_icon.enabled = false;
		}
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

	public void SetBackground(bool p_isDark)
	{
		if (p_isDark)
		{
			m_background.spriteName = "name_separator_2";
		}
		else
		{
			m_background.spriteName = "name_separator";
		}
	}

	public void SetPlayerBackground()
	{
		m_background.spriteName = "leaderboard_player_selected";
		m_background.transform.localScale = new Vector3(m_background.transform.localScale.x, 68f, m_background.transform.localScale.z);
		m_background.MakePixelPerfect();
		m_background.transform.localScale = new Vector3(m_background.transform.localScale.x * m_highlightedExtendRatio, m_background.transform.localScale.y, m_background.transform.localScale.z);
		m_background.depth++;
	}
}
