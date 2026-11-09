using UnityEngine;

public class SwapModels : MonoBehaviour
{
	public GameObject[] m_models;

	private GameObject m_selectedGao;

	private int m_selectedIndex;

	private void Start()
	{
		m_selectedIndex = 0;
		m_selectedGao = m_models[0];
	}

	private void ShowSelected()
	{
		for (int i = 0; i < m_models.Length; i++)
		{
			int num = m_selectedIndex % m_models.Length;
			if (num == i)
			{
				m_models[num].SetActiveRecursively(true);
				m_selectedGao = m_models[num];
			}
			else
			{
				m_models[i].SetActiveRecursively(false);
			}
		}
	}
}
