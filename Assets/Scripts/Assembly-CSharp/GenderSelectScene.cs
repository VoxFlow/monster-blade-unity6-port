using UnityEngine;

public class GenderSelectScene : MonoBehaviour
{
	public GameObject[] m_selections;

	private void Start()
	{
		if (SingletonComponent<SaveData>.Instance.GameData.IsGenderChosen())
		{
			Exit();
		}
	}

	private void OnClick(GameObject p_go)
	{
		Dbg.Log(p_go.name);
		if (p_go == m_selections[0])
		{
			SingletonComponent<SaveData>.Instance.GameData.SetMale(true);
		}
		else if (p_go == m_selections[1])
		{
			SingletonComponent<SaveData>.Instance.GameData.SetMale(false);
		}
		SingletonComponent<SaveData>.Instance.SaveDataToPlayerPrefs();
		Exit();
	}

	private void Exit()
	{
		Application.LoadLevel("SystemInit");
	}
}
