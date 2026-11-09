using UnityEngine;

public class TestScene : MonoBehaviour
{
	public GameObject[] m_levels;

	private GameObject[] m_objects;

	private int m_index;

	private bool m_showLevel = true;

	private void Awake()
	{
		if (m_levels != null && m_levels.Length > 0)
		{
			Dbg.Log("Number of levels: " + m_levels.Length);
			for (int i = 1; i < m_levels.Length; i++)
			{
				m_levels[i].SetActiveRecursively(false);
			}
			ChangeLevel(0);
		}
	}

	private void Update()
	{
	}

	private void ChangeLevel(int p_index)
	{
		m_levels[m_index].SetActiveRecursively(false);
		m_levels[p_index].SetActiveRecursively(true);
		m_objects = new GameObject[m_levels[p_index].transform.childCount];
		for (int i = 0; i < m_objects.Length; i++)
		{
			m_objects[i] = m_levels[p_index].transform.GetChild(i).gameObject;
		}
		m_index = p_index;
	}

	private void OnGUI()
	{
		GUILayout.Space(100f);
	}
}
