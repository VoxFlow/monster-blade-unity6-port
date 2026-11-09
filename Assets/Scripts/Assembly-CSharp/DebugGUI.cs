using System.Collections.Generic;
using UnityEngine;

public class DebugGUI : MonoBehaviour
{
	private static bool m_showFacebookMenu;

	private static bool m_showTwitterMenu;

	private void Unload<T>(List<T> _list) where T : Object
	{
		Dbg.LogWarn("Unload All Object of type " + typeof(T).ToString() + " not supported for Unity version lower than 3.5.5");
	}

	private void FacebookFunction()
	{
		m_showFacebookMenu = GUILayout.Toggle(m_showFacebookMenu, "Facebook Menu", GUILayout.Width(200f), GUILayout.Height(50f));
		if (m_showFacebookMenu)
		{
			if (GUILayout.Button("Login Facebook", GUILayout.Width(180f), GUILayout.Height(40f)))
			{
				SingletonComponent<SocialNetworkManager>.Instance.LoginSocialNetwork(SocialNetworkType.Facebook);
			}
			if (GUILayout.Button("Post on Facebook with SNS Manager", GUILayout.Width(180f), GUILayout.Height(40f)))
			{
				SingletonComponent<SocialNetworkManager>.Instance.PostToSocialNetwork(SocialNetworkType.Facebook, string.Empty);
			}
			if (GUILayout.Button("Get Friend List", GUILayout.Width(180f), GUILayout.Height(40f)))
			{
				SingletonComponent<SocialNetworkManager>.Instance.GetSocialNetworkFriends(SocialNetworkType.Facebook);
			}
			if (GUILayout.Button("Logout Facebook", GUILayout.Width(180f), GUILayout.Height(40f)))
			{
				SingletonComponent<SocialNetworkManager>.Instance.LogoutSocialNetwork(SocialNetworkType.Facebook);
			}
		}
	}

	private void TwitterFunction()
	{
		m_showTwitterMenu = GUILayout.Toggle(m_showTwitterMenu, "Twitter Menu", GUILayout.Width(200f), GUILayout.Height(50f));
		if (m_showTwitterMenu)
		{
			if (GUILayout.Button("Login Twitter", GUILayout.Width(180f), GUILayout.Height(40f)))
			{
				SingletonComponent<SocialNetworkManager>.Instance.LoginSocialNetwork(SocialNetworkType.Twitter);
			}
			if (GUILayout.Button("Post on Twitter", GUILayout.Width(180f), GUILayout.Height(40f)))
			{
				SingletonComponent<SocialNetworkManager>.Instance.PostToSocialNetwork(SocialNetworkType.Twitter, "Test Post on Twitter");
			}
		}
	}

	public static bool ToggleButton(string p_label, bool p_toggleState, float p_width, float p_height)
	{
		string[] texts = new string[2] { "No", "Yes" };
		int num = 0;
		GUILayout.BeginHorizontal();
		GUI.skin.label.fontSize = GUI.skin.button.fontSize;
		GUILayout.Label(p_label, GUILayout.MaxWidth(150f), GUILayout.MaxHeight(25f));
		num = (p_toggleState ? 1 : 0);
		num = GUILayout.SelectionGrid(num, texts, 2, GUILayout.Width(p_width * 2f), GUILayout.Height(p_height));
		p_toggleState = num == 1;
		GUILayout.EndHorizontal();
		return p_toggleState;
	}

	public static int ToggleStateButton(string p_label, string[] p_choices, int p_toggleState, float p_width, float p_height)
	{
		if (p_choices == null)
		{
			return 0;
		}
		int num = p_choices.Length;
		GUILayout.BeginHorizontal();
		GUI.skin.label.fontSize = GUI.skin.button.fontSize;
		GUILayout.Label(p_label, GUILayout.MaxWidth(150f), GUILayout.MaxHeight(25f));
		p_toggleState = GUILayout.SelectionGrid(p_toggleState, p_choices, num, GUILayout.Width(p_width * (float)num), GUILayout.Height(p_height));
		GUILayout.EndHorizontal();
		return p_toggleState;
	}
}
