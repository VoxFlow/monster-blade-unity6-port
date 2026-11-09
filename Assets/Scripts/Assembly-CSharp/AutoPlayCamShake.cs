using UnityEngine;

public class AutoPlayCamShake : MonoBehaviour
{
	private void OnEnable()
	{
		CameraShakeFx[] components = base.gameObject.GetComponents<CameraShakeFx>();
		CameraShakeFx[] array = components;
		foreach (CameraShakeFx cameraShakeFx in array)
		{
			cameraShakeFx.Play();
		}
	}

	private void Start()
	{
		CameraShakeFx[] components = base.gameObject.GetComponents<CameraShakeFx>();
		CameraShakeFx[] array = components;
		foreach (CameraShakeFx cameraShakeFx in array)
		{
			cameraShakeFx.Play();
		}
	}
}
