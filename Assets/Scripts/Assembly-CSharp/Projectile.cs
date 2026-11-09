using System;
using UnityEngine;

[RequireComponent(typeof(MonsterCollisionData))]
[RequireComponent(typeof(Rigidbody))]
[RequireComponent(typeof(SphereCollider))]
[RequireComponent(typeof(PlayerCollisionData))]
public class Projectile : MonoBehaviour
{
	[SerializeField]
	protected float m_timeToTarget = 0.5f;

	[SerializeField]
	protected float m_selfDestroyTime = 10f;

	protected Vector3 m_velocity = Vector3.zero;

	protected Vector3 m_target = Vector3.zero;

	protected PlayerCollisionData m_playerCollisionData;

	protected MonsterCollisionData m_monsterCollisionData;

	public Action<Collider> onCollideCallback { get; set; }

	public Action<Projectile> onDestroyCallback { get; set; }

	public Vector3 target
	{
		protected get
		{
			return m_target;
		}
		set
		{
			m_target = value;
			Vector3 vector = m_target - base.transform.position;
			float num = vector.magnitude / m_timeToTarget;
			base.transform.forward = vector.normalized;
			m_velocity = base.transform.forward * num;
		}
	}

	public PlayerCollisionData playerCollisionData
	{
		get
		{
			if (null == m_playerCollisionData)
			{
				m_playerCollisionData = base.gameObject.GetComponent<PlayerCollisionData>();
			}
			return m_playerCollisionData;
		}
	}

	public MonsterCollisionData monsterCollisionData
	{
		get
		{
			if (null == m_monsterCollisionData)
			{
				m_monsterCollisionData = base.gameObject.GetComponent<MonsterCollisionData>();
			}
			return m_monsterCollisionData;
		}
	}

	protected virtual void Start()
	{
		MBDestroy.Destroy(base.gameObject, m_selfDestroyTime);   // PORT (CD-053)
	}

	protected virtual void OnDestroy()
	{
		if (onDestroyCallback != null)
		{
			onDestroyCallback(this);
		}
	}

	protected virtual void FixedUpdate()
	{
		base.transform.position += m_velocity * Time.fixedDeltaTime;
	}

	private void OnTriggerEnter(Collider p_other)
	{
		if (!(null == p_other) && p_other.enabled && !(p_other.gameObject.tag == base.gameObject.tag))
		{
			_OnTriggerEnter(p_other);
			if (onCollideCallback != null)
			{
				onCollideCallback(p_other);
			}
		}
	}

	protected virtual void _OnTriggerEnter(Collider p_other)
	{
	}
}
