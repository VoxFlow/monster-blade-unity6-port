using UnityEngine;

[AddComponentMenu("NGUI/UI/Image Button")]
[ExecuteInEditMode]
public class UIImageButton : MonoBehaviour
{
	public UISprite target;

	public string normalSprite;

	public string hoverSprite;

	public string pressedSprite;

	public bool m_locked;

	private void Start()
	{
		if (target == null)
		{
			target = MBHier.GetComponentInChildren<UISprite>(this);
		}
	}

	private void OnHover(bool isOver)
	{
		if (target != null && !m_locked)
		{
			target.spriteName = ((!isOver) ? normalSprite : hoverSprite);
		}
	}

	private void OnPress(bool pressed)
	{
		if (target != null && !m_locked)
		{
			target.spriteName = ((!pressed) ? normalSprite : pressedSprite);
		}
	}
}
