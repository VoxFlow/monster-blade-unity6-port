using UnityEngine;

public class MysteryCaveCameraController : MonoBehaviour
{
	public enum ShakeType
	{
		SHAKE = 0,
		RECOIL = 1
	}

	public string pathName = "CAMERA_PREMIUM";

	public float time = 1.5f;

	public iTween.EaseType easeType = iTween.EaseType.easeInOutSine;

	public ShakeType shakeType;

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
		CameraShakeFx componentInChildren = MBHier.GetComponentInChildren<CameraShakeFx>(this);
		if ((bool)componentInChildren)
		{
			componentInChildren.Amplitude = new Vector3(0.5f, 0.5f, 0f);
			componentInChildren.Frequency = 5f;
			componentInChildren.Duration = 0.5f;
		}
		CameraRecoilFx componentInChildren2 = MBHier.GetComponentInChildren<CameraRecoilFx>(this);
		if ((bool)componentInChildren2)
		{
			componentInChildren2.Amplitude = 10f;
			componentInChildren2.Period = 0.25f;
		}
	}

	public void PlayAnimation()
	{
		Animation componentInChildren = MBHier.GetComponentInChildren<Animation>(this);
		if (componentInChildren != null)
		{
			string text = componentInChildren.clip.name;
			componentInChildren[text].time = 0f;
			componentInChildren[text].speed = 1f;
			componentInChildren.Play();
		}
		iTween.MoveTo(base.gameObject, iTween.Hash("path", iTweenPath.GetPath(pathName), "time", time, "easetype", easeType));
	}

	public void PlayAnimationReverse()
	{
		Animation componentInChildren = MBHier.GetComponentInChildren<Animation>(this);
		if (componentInChildren != null)
		{
			string text = componentInChildren.clip.name;
			componentInChildren[text].time = componentInChildren.clip.length;
			componentInChildren[text].speed = -1f;
			componentInChildren.Play();
		}
		iTween.MoveTo(base.gameObject, iTween.Hash("path", iTweenPath.GetPathReversed(pathName), "time", time, "easeType", easeType));
	}

	public void Reset()
	{
		iTween.Stop(base.gameObject);
		Animation componentInChildren = MBHier.GetComponentInChildren<Animation>(this);
		if (componentInChildren != null)
		{
			componentInChildren.Stop();
		}
		base.transform.position = originalPosition;
		base.transform.rotation = originalRotation;
		Camera componentInChildren2 = MBHier.GetComponentInChildren<Camera>(this);
		if (componentInChildren2 != null)
		{
			componentInChildren2.transform.rotation = originalCameraRotation;
		}
	}

	public void Shake()
	{
		CameraFx cameraFx = null;
		switch (shakeType)
		{
		case ShakeType.SHAKE:
			cameraFx = MBHier.GetComponentInChildren<CameraShakeFx>(this);
			break;
		case ShakeType.RECOIL:
			cameraFx = MBHier.GetComponentInChildren<CameraRecoilFx>(this);
			break;
		}
		if (cameraFx != null)
		{
			if (!cameraFx.enabled)
			{
				cameraFx.enabled = true;
			}
			cameraFx.Play();
		}
	}
}
