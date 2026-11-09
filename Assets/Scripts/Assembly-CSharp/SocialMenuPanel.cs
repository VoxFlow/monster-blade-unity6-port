using UnityEngine;

public class SocialMenuPanel : StandardMenuPanel
{
	[SerializeField]
	private NBUILocalize m_connectString;

	public override void OnEnable()
	{
		base.OnEnable();
		EnableButtonsByFBLoginStatus();
		SocialMenuScreen socialMenuScreen = parentScreen as SocialMenuScreen;
		if (socialMenuScreen != null)
		{
			socialMenuScreen.UpdateRedIconNum();
		}
		if (null != m_connectString)
		{
			m_connectString.Key = "social_fb_connect";
		}
	}

	public void EnableButtonsByFBLoginStatus()
	{
		bool flag = SingletonComponent<SocialManager>.Instance.IsSNSLogin();
		TutorialManager instance = SingletonComponent<TutorialManager>.Instance;
		bool flag2 = instance != null && !instance.IsCompleted(Tutorial.State.All);
		for (int i = 0; i < buttons.Length; i++)
		{
			if (!(null == buttons[i]))
			{
				if (i == 4)
				{
					buttons[i].SetActiveRecursively(!flag);
				}
				else
				{
					buttons[i].SetActiveRecursively(flag);
				}
				if (flag2)
				{
					UIButton component = buttons[i].GetComponent<UIButton>();
					component.isLocked = true;
				}
			}
		}
	}
}
