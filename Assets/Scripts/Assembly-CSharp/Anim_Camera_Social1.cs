using UnityEngine;

public class Anim_Camera_Social1 : MonoBehaviour
{
	private Vector3 originalPosition;

	private Quaternion originalRotation;

	private Quaternion originalCameraRotation;

	private void Awake()
	{
		originalPosition = base.transform.position;
		originalRotation = base.transform.rotation;
		originalCameraRotation = MBHier.GetComponentInChildren<Camera>(this).transform.rotation;
	}

	private void Start()
	{
	}

	public void PlayAnimation()
	{
		Animation componentInChildren = MBHier.GetComponentInChildren<Animation>(this);
		if ((bool)componentInChildren)
		{
			componentInChildren.Play();
		}
		iTween.MoveTo(base.gameObject, iTween.Hash("path", iTweenPath.GetPath("CAMERA_SOCIAL"), "time", 1.5, "easetype", iTween.EaseType.easeInOutSine));
	}

	public void Reset()
	{
		base.transform.position = originalPosition;
		base.transform.rotation = originalRotation;
		Camera componentInChildren = MBHier.GetComponentInChildren<Camera>(this);
		if (componentInChildren != null)
		{
			componentInChildren.transform.rotation = originalCameraRotation;
		}
	}

	private void Update()
	{
	}
}
