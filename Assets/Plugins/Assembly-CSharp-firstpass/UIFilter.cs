using UnityEngine;

public class UIFilter : MonoBehaviour
{
	public virtual bool Accept(GameObject go)
	{
		return true;
	}
}
