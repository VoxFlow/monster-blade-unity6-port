using UnityEngine;

public class HalfDungeonCutscene : MonoBehaviour
{
	protected GameObject m_playerGameObject;

	protected CutscenePlayer m_playingCutscene;

	protected GameManager m_gameMgr;

	protected virtual void Awake()
	{
		HalfDungeonCutsceneManager instance = SingletonComponent<HalfDungeonCutsceneManager>.Instance;
		if (instance != null)
		{
			if (instance.playerTemplate == null)
			{
				Dbg.LogWarn("Player template game-object has been destroyed!");
				if (m_gameMgr != null)
				{
					m_gameMgr.LoadScene("MainMenuInit", true, false);
				}
				return;
			}
			m_playerGameObject = Object.Instantiate(instance.playerTemplate) as GameObject;
		}
		m_gameMgr = SingletonComponent<GameManager>.Instance;
		m_playingCutscene = base.gameObject.GetComponent<CutscenePlayer>();
		if (m_playingCutscene != null)
		{
			m_playingCutscene.enabled = true;
		}
		else if (m_gameMgr != null)
		{
			m_gameMgr.LoadScene("MainMenuInit", true, false);
		}
		Messenger<object>.AddListener("cutscene_enabled", OnCutsceneEnabled);
	}

	protected virtual void OnDestroy()
	{
		HalfDungeonCutsceneManager instance = SingletonComponent<HalfDungeonCutsceneManager>.Instance;
		if (instance != null)
		{
			instance.CleanUp();
		}
		Messenger<object>.RemoveListener("cutscene_enabled", OnCutsceneEnabled);
	}

	protected virtual void OnCutsceneEnabled(object p_objectName)
	{
	}

	protected virtual void Update()
	{
		if ((m_playingCutscene == null || !m_playingCutscene.enabled) && m_gameMgr != null)
		{
			m_gameMgr.LoadScene("MainMenuInit", true, false);
		}
	}
}
