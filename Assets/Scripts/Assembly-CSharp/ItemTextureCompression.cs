using System.Collections.Generic;
using UnityEngine;

public class ItemTextureCompression : MonoBehaviour
{
	public GameObject[] m_objectToSwap;

	private List<UISprite> m_imagesList;

	private List<string> m_spriteList;

	private int m_spriteIndex;

	private string m_selectedTexture = string.Empty;

	private void Start()
	{
		m_spriteIndex = 0;
		m_imagesList = new List<UISprite>();
		GameObject[] objectToSwap = m_objectToSwap;
		foreach (GameObject gameObject in objectToSwap)
		{
			UISprite[] componentsInChildren = MBHier.GetComponentsInChildren<UISprite>(gameObject, true);
			UISprite[] array = componentsInChildren;
			foreach (UISprite uISprite in array)
			{
				if (uISprite.name.ToLower().Contains("background"))
				{
					m_imagesList.Add(uISprite);
				}
			}
		}
		m_spriteList = m_imagesList[0].atlas.GetListOfSprites();
	}

	private void ShowImage()
	{
		m_selectedTexture = m_spriteList[m_spriteIndex];
		foreach (UISprite images in m_imagesList)
		{
			images.spriteName = m_selectedTexture;
		}
	}

	private void OnGUI()
	{
		GUILayout.Space(50f);
		GUILayout.BeginHorizontal();
		if (GUILayout.Button("Next Texture", GUILayout.Width(120f), GUILayout.Height(60f)))
		{
			m_spriteIndex++;
			if (m_spriteIndex >= m_spriteList.Count)
			{
				m_spriteIndex = 0;
			}
			ShowImage();
		}
		if (GUILayout.Button("Previous Texture", GUILayout.Width(120f), GUILayout.Height(60f)))
		{
			m_spriteIndex--;
			if (m_spriteIndex < 0)
			{
				m_spriteIndex = m_spriteList.Count - 1;
			}
			ShowImage();
		}
		GUILayout.Label(m_selectedTexture, GUILayout.Width(120f), GUILayout.Height(60f));
		GUILayout.EndHorizontal();
	}
}
