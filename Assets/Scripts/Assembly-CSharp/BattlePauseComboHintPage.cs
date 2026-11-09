using UnityEngine;

public class BattlePauseComboHintPage : MonoBehaviour
{
	private void Start()
	{
	}

	private void Update()
	{
	}

	public void Show()
	{
		base.gameObject.SetActiveRecursively(true);
	}

	public void Hide()
	{
		base.gameObject.SetActiveRecursively(false);
	}
}
