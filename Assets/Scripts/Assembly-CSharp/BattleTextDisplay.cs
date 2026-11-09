public class BattleTextDisplay : SingletonComponent<BattleTextDisplay>
{
	public delegate void Callback();

	public UISprite m_textBattle;

	public UISprite m_textBossBattle;

	public UISprite m_textOf;

	public UISprite[] m_numbers = new UISprite[4];

	private Callback m_endCallback;

	private void Awake()
	{
		UISprite[] numbers = m_numbers;
		foreach (UISprite uISprite in numbers)
		{
			if (uISprite == null)
			{
				Dbg.LogError("Missing number sprite.");
				break;
			}
		}
	}

	public void Show(int p_currentFight, int p_maxFight, bool p_bossBattle, Callback p_endCallback)
	{
		if (p_currentFight < 0 || p_maxFight < 0 || p_currentFight >= 100 || p_maxFight >= 100)
		{
			Dbg.LogError("Invalid number of fight: " + p_currentFight + "/" + p_maxFight);
			return;
		}
		base.gameObject.SetActiveRecursively(true);
		if (!p_bossBattle)
		{
			if (m_textBattle != null)
			{
				m_textBattle.gameObject.active = true;
			}
			if (m_textBossBattle != null)
			{
				m_textBossBattle.gameObject.active = false;
			}
			if (p_currentFight >= 10)
			{
				m_numbers[0].spriteName = GetSpriteName(p_currentFight / 10);
				m_numbers[0].gameObject.active = true;
			}
			else
			{
				m_numbers[0].gameObject.active = false;
			}
			if (p_maxFight >= 10)
			{
				m_numbers[2].spriteName = GetSpriteName(p_maxFight / 10);
				m_numbers[2].gameObject.active = true;
				m_numbers[3].spriteName = GetSpriteName(p_maxFight % 10);
				m_numbers[3].gameObject.active = true;
			}
			else
			{
				m_numbers[2].spriteName = GetSpriteName(p_maxFight % 10);
				m_numbers[3].gameObject.active = false;
			}
			m_numbers[1].spriteName = GetSpriteName(p_currentFight % 10);
			m_numbers[1].gameObject.active = true;
		}
		else
		{
			if (m_textBattle != null)
			{
				m_textBattle.gameObject.active = false;
			}
			if (m_textBossBattle != null)
			{
				m_textBossBattle.gameObject.active = true;
			}
			if (m_textOf != null)
			{
				m_textOf.gameObject.active = false;
			}
			UISprite[] numbers = m_numbers;
			foreach (UISprite uISprite in numbers)
			{
				if (uISprite != null)
				{
					uISprite.gameObject.active = false;
				}
			}
		}
		m_endCallback = p_endCallback;
	}

	private string GetSpriteName(int p_number)
	{
		string text = "battle_announcement/generic_";
		if (p_number < 10)
		{
			return text + p_number;
		}
		return text + "0";
	}

	private void Update()
	{
		if (!base.GetComponent<UnityEngine.Animation>().isPlaying)
		{
			if (m_endCallback != null)
			{
				m_endCallback();
			}
			base.gameObject.SetActiveRecursively(false);
		}
	}
}
