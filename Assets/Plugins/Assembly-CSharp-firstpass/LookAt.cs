using UnityEngine;

public class LookAt : MonoBehaviour
{
	public Transform target;

	[HideInInspector]
	public string targetTagName;

	[HideInInspector]
	public string targetPath;

	protected virtual void Start()
	{
		if ((bool)base.GetComponent<Rigidbody>())
		{
			base.GetComponent<Rigidbody>().freezeRotation = true;
		}
	}

	protected virtual void LateUpdate()
	{
		base.transform.LookAt(target);
	}
}
