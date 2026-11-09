using UnityEngine;

public abstract class UISend : MonoBehaviour
{
	public GameObject target;

	public bool includeChildren;

	protected void Send(string funcName, int state)
	{
		GameObject gameObject = ((!(target != null)) ? base.gameObject : target);
		if (includeChildren)
		{
			Transform[] componentsInChildren = MBHier.GetComponentsInChildren<Transform>(gameObject);
			Transform[] array = componentsInChildren;
			foreach (Transform transform in array)
			{
				transform.gameObject.SendMessage(funcName, state, SendMessageOptions.DontRequireReceiver);
			}
		}
		else
		{
			gameObject.SendMessage(funcName, state, SendMessageOptions.DontRequireReceiver);
		}
	}

	protected void Send(int state)
	{
		Send("OnState", state);
	}
}
