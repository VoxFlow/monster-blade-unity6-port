using UnityEngine;

[ExecuteInEditMode]
[AddComponentMenu("NGUI/Interaction/Drag Camera")]
public class UIDragCamera : IgnoreTimeScale
{
	public UIDraggableCamera draggableCamera;

	[SerializeField]
	[HideInInspector]
	public Component target;

	public Transform rootForBounds;

	public Vector2 scale;

	public Vector3 Offset;

	public bool IsPressed;

	public Vector2 Momentum;

	public void SetTransform()
	{
	}

	public bool IsStopped()
	{
		return false;
	}

	public void ForceStop()
	{
	}

	private void Awake()
	{
		if (target != null)
		{
			if (draggableCamera == null)
			{
				draggableCamera = target.GetComponent<UIDraggableCamera>();
				if (draggableCamera == null)
				{
					draggableCamera = target.gameObject.AddComponent<UIDraggableCamera>();
				}
			}
			target = null;
		}
		else if (draggableCamera == null)
		{
			draggableCamera = NGUITools.FindInParents<UIDraggableCamera>(base.gameObject);
		}
	}

	private void OnPress(bool isPressed)
	{
		if (base.enabled && base.gameObject.active && draggableCamera != null)
		{
			draggableCamera.Press(isPressed);
		}
	}

	private void OnDrag(Vector2 delta)
	{
		if (base.enabled && base.gameObject.active && draggableCamera != null)
		{
			draggableCamera.Drag(delta);
		}
	}

	private void OnScroll(float delta)
	{
		if (base.enabled && base.gameObject.active && draggableCamera != null)
		{
			draggableCamera.Scroll(delta);
		}
	}
}
