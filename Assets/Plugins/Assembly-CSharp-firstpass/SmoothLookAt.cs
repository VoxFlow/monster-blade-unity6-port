using UnityEngine;

public class SmoothLookAt : LookAt
{
	public float damping = 6f;

	public bool smooth = true;

	protected override void LateUpdate()
	{
		if ((bool)target)
		{
			if (smooth)
			{
				Quaternion to = Quaternion.LookRotation(target.position - base.transform.position, base.transform.up);
				base.transform.rotation = Quaternion.Slerp(base.transform.rotation, to, Time.deltaTime * damping);
			}
			else
			{
				base.LateUpdate();
			}
		}
	}
}
