using UnityEngine;

public class UISpriteFadeOut : MonoBehaviour
{
	public Transform tweenTarget;

	private int timeCounter;

	private void Start()
	{
		if (tweenTarget != null)
		{
			UIWidget component = tweenTarget.gameObject.GetComponent<UIWidget>();
			Color color = component.color;
			Color color2 = new Color(color.r, color.g, color.b, 0.2f);
			component.color = color2;
		}
	}

	private void Update()
	{
		timeCounter++;
	}
}
