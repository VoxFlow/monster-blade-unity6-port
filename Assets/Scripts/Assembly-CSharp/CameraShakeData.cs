using UnityEngine;

public class CameraShakeData : MonoBehaviour
{
	public string referenceName = string.Empty;

	public Vector3 amplitude = Vector3.zero;

	public float frequency;

	public float duration;

	public float delay;

	public bool smoothFade = true;

	private void Awake()
	{
		base.enabled = false;
	}
}
