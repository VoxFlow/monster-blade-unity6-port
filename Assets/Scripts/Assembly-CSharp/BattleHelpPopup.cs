using UnityEngine;

public class BattleHelpPopup : MonoBehaviour
{
	[SerializeField]
	private UILabel m_socialPointsDesc;

	[SerializeField]
	private UILabel m_userNumDesc;

	private void Start()
	{
	}

	private void Update()
	{
	}

	public void SetupDisplay()
	{
		if (SingletonComponent<DailyManager>.Instance != null)
		{
			m_socialPointsDesc.text = SingletonComponent<DailyManager>.Instance.m_BattleHelpData.SocialPointsGained + " " + SingletonComponent<NBLocalization>.Instance.Get("social_point");
			m_userNumDesc.text = "[00FF00]" + SingletonComponent<DailyManager>.Instance.m_BattleHelpData.UserNum + "[-] " + SingletonComponent<NBLocalization>.Instance.Get("battle_help_desc1");
		}
	}
}
