using UnityEngine;

public class PlayEffects : MonoBehaviour
{
	public GameObject[] allEffects;

	private void Awake()
	{
	}

	private void Start()
	{
	}

	private void Update()
	{
		if (Input.GetMouseButtonUp(0))
		{
			GameObject[] array = allEffects;
			foreach (GameObject gameObject in array)
			{
				gameObject.SetActiveRecursively(true);
			}
		}
	}
}
