using UnityEngine;

[ExecuteInEditMode]
[RequireComponent(typeof(Camera))]
[AddComponentMenu("NGUI/UI/NBViewport Camera")]
public class NBUIViewport : MonoBehaviour
{
	public Vector3 tl = new Vector3(0f, 640f, 0f);

	public Vector3 br = new Vector3(960f, 0f, 0f);

	public float fullSize = 320f;

	private Camera mCam;

	private float mWidthRatio = 1f;

	private float mHeightRatio = 1f;

	private void Start()
	{
		mCam = base.GetComponent<Camera>();
		mWidthRatio = (float)Screen.width / GLOBAL.BASE_WIDTH;
		mHeightRatio = (float)Screen.height / GLOBAL.BASE_HEIGHT;
	}

	private void LateUpdate()
	{
		Rect rect = new Rect(tl.x / (float)Screen.width * mWidthRatio, br.y / (float)Screen.height * mHeightRatio, (br.x - tl.x) / (float)Screen.width * mWidthRatio, (tl.y - br.y) / (float)Screen.height * mHeightRatio);
		float num = fullSize * rect.height;
		if (rect != mCam.rect)
		{
			mCam.rect = rect;
		}
		if (mCam.orthographicSize != num)
		{
			mCam.orthographicSize = num;
		}
	}
}
