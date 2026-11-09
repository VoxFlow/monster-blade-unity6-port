using UnityEngine;

public class GameInitScene : MonoBehaviour
{
	private void Start()
	{
		GameObject gameObject = GameObject.Find("SystemInit_Prefab");
		if (gameObject != null)
		{
			gameObject.GetComponent<SystemInitScene>().DoInitialize();
		}
	}

	private void Update()
	{
	}
}
