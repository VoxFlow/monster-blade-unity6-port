using UnityEngine;

public class CollisionDetector : MonoBehaviour
{
	public delegate void CollisionCallback(Collider p_other);

	public CollisionCallback collisionCallback;

	private void OnTriggerEnter(Collider p_other)
	{
		if (collisionCallback != null && p_other.tag != base.tag)
		{
			collisionCallback(p_other);
		}
	}
}
