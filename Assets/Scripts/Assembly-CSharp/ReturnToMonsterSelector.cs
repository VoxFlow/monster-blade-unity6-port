using UnityEngine;

public class ReturnToMonsterSelector : MonoBehaviour
{
	private void Awake()
	{
		Object.DontDestroyOnLoad(this);
		base.enabled = false;
	}

	private void OnGUI()
	{
		if (GUI.Button(new Rect((float)Screen.width - (float)Screen.width * 0.1f, 0f, (float)Screen.width * 0.1f, (float)Screen.height * 0.1f), "Return"))
		{
			Debug.LogWarning("return");
			GameObject obj = GameObject.Find("Core");
			GameObject obj2 = GameObject.Find("Game");
			GameObject obj3 = GameObject.Find("ItemAndUIManagers");
			GameObject obj4 = GameObject.Find("UIPopupRoot");
			Object.DestroyImmediate(obj);
			Object.DestroyImmediate(obj2);
			Object.DestroyImmediate(obj3);
			Object.DestroyImmediate(obj4);
			Application.LoadLevelAsync("TestScene");
			Object.Destroy(base.gameObject);
		}
	}

	private void Start()
	{
	}

	private void Update()
	{
	}
}
