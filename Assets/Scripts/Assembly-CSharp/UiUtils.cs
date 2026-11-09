using UnityEngine;

public class UiUtils
{
	public static Vector3 GetScreenPos(Vector3 p_3dPos, Camera p_3dCamera, Camera p_uiCamera)
	{
		if (p_3dCamera == null || p_uiCamera == null)
		{
			return Vector3.zero;
		}
		Vector3 result = p_3dCamera.WorldToViewportPoint(p_3dPos);
		result.x -= 0.5f;
		result.y -= 0.5f;
		float num = p_uiCamera.orthographicSize * p_uiCamera.aspect * 2f;
		float num2 = p_uiCamera.orthographicSize * 2f;
		result.x *= num;
		result.y *= num2;
		result.z = 0f;
		return result;
	}
}
