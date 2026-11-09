using System.Collections.Generic;
using UnityEngine;

[RequireComponent(typeof(UISprite))]
public class LocalizedSprite : MonoBehaviour
{
	[SerializeField]
	private UIAtlas[] m_atlases;

	private static readonly Dictionary<string, int> s_keyIndexes = new Dictionary<string, int>
	{
		{
			NBLocalization.s_supportedLanguage[0],
			0
		},
		{
			NBLocalization.s_supportedLanguage[1],
			1
		},
		{
			NBLocalization.s_supportedLanguage[2],
			2
		},
		{
			NBLocalization.s_supportedLanguage[3],
			3
		},
		{
			NBLocalization.s_supportedLanguage[4],
			4
		}
	};

	private UISprite m_sprite;

	public UIAtlas this[string p_lang]
	{
		get
		{
			if (string.IsNullOrEmpty(p_lang))
			{
				return null;
			}
			if (m_atlases == null)
			{
				m_atlases = new UIAtlas[NBLocalization.s_supportedLanguage.Length];
			}
			UIAtlas uIAtlas = null;
			int value = 0;
			s_keyIndexes.TryGetValue(p_lang, out value);
			return m_atlases[value];
		}
		set
		{
			if (!string.IsNullOrEmpty(p_lang))
			{
				if (m_atlases == null)
				{
					m_atlases = new UIAtlas[NBLocalization.s_supportedLanguage.Length];
				}
				int value2 = 0;
				s_keyIndexes.TryGetValue(p_lang, out value2);
				m_atlases[value2] = value;
			}
		}
	}

	private void Awake()
	{
		m_sprite = base.gameObject.GetComponent<UISprite>();
	}

	private void Start()
	{
		if (SingletonComponent<SaveData>.Instance != null)
		{
			m_sprite.atlas.replacement = this[SingletonComponent<SaveData>.Instance.GameData.Language];
		}
	}
}
