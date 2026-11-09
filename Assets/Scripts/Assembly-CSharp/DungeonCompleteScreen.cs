using UnityEngine;

public class DungeonCompleteScreen : NBUIMenuScreen
{
	public override void OnEnable()
	{
		Init();
	}

	public override void Start()
	{
	}

	protected override void Reset()
	{
		ClearStack();
	}

	protected override void Init()
	{
		PushPanel(menuPanels[0]);
	}

	private void OnButtonClick(GameObject source)
	{
	}
}
