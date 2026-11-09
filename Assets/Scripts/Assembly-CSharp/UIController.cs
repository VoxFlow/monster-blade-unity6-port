using UnityEngine;

public class UIController : MonoBehaviour
{
	public virtual void Awake()
	{
	}

	public virtual void Start()
	{
		float uI_SCALE_FACTOR = GLOBAL.UI_SCALE_FACTOR;
		base.transform.localScale = new Vector3(uI_SCALE_FACTOR, uI_SCALE_FACTOR, uI_SCALE_FACTOR);
		foreach (UICamera allUICamera in UICamera.allUICameras)
		{
			if ((bool)allUICamera.cachedCamera)
			{
				allUICamera.cachedCamera.orthographicSize = 360f;
			}
		}
	}
}
