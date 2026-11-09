using UnityEngine;

public class AchievementBtn : MonoBehaviour
{
	[SerializeField]
	private UISprite m_btnTexture;

	private TweenColor m_tweenColor;

	private void Awake()
	{
		m_tweenColor = m_btnTexture.GetComponent<TweenColor>();
	}

	private void Start()
	{
		CheckShining();
	}

	public void CheckShining()
	{
		if (m_tweenColor != null)
		{
			m_tweenColor.enabled = SingletonComponent<AchievementManager>.Instance.m_NeedToShineAchievementBtn;
		}
		if (!SingletonComponent<AchievementManager>.Instance.m_NeedToShineAchievementBtn)
		{
			m_btnTexture.color = Color.white;
		}
	}
}
