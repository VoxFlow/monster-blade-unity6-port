using UnityEngine;

public class TestEnvironment : MonoBehaviour
{
	public GameObject[] m_levels;

	private GameObject[] m_objects;

	public GameObject m_titleScreen;

	public GameObject m_player;

	public GameObject m_mob;

	private int m_index = -1;

	private void Awake()
	{
		m_levels = new GameObject[base.transform.childCount];
		for (int i = 0; i < m_levels.Length; i++)
		{
			m_levels[i] = base.transform.GetChild(i).gameObject;
			m_levels[i].SetActiveRecursively(false);
		}
		ChangeLevel(0);
	}

	private void Update()
	{
	}

	private void ChangeLevel(int p_index)
	{
		if (m_index >= 0)
		{
			m_levels[m_index].SetActiveRecursively(false);
		}
		if (p_index >= 0)
		{
			m_levels[p_index].SetActiveRecursively(true);
			CreateChildList(m_levels[p_index]);
		}
		m_index = p_index;
	}

	private void CreateChildList(GameObject p_parent)
	{
		m_objects = new GameObject[p_parent.transform.childCount];
		for (int i = 0; i < m_objects.Length; i++)
		{
			m_objects[i] = p_parent.transform.GetChild(i).gameObject;
		}
	}

	private void OnGUI()
	{
		GUILayout.Space(80f);
	}
}
