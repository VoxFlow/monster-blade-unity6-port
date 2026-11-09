using UnityEngine;

[RequireComponent(typeof(Rigidbody))]
[RequireComponent(typeof(BoxCollider))]
public class FireBreath : MonoBehaviour
{
	[SerializeField]
	protected float m_damageInterval = 0.8f;

	[SerializeField]
	protected float m_minDamageInterval = 0.3f;

	[SerializeField]
	protected float m_timeToTarget = 0.7f;

	[SerializeField]
	protected float m_damageMultiplier = 0.2f;

	protected Transform m_root;

	protected Transform m_target;

	protected GameObject m_flameThrowerFx;

	protected NBTimer m_coolDownTimer;

	protected float m_coolDownTime;

	protected ParticleSystem[] m_flameParticles;

	protected bool m_emitting = true;

	public bool isEmitting
	{
		get
		{
			return m_emitting;
		}
	}

	protected virtual void Awake()
	{
		m_flameParticles = MBHier.GetComponentsInChildren<ParticleSystem>(base.gameObject);
		StopFlame();
		m_coolDownTimer = base.gameObject.AddComponent<NBTimer>();
		m_coolDownTimer.Name = "Fire Breath Cool Down";
		base.GetComponent<Rigidbody>().isKinematic = true;
		base.GetComponent<Rigidbody>().collisionDetectionMode = CollisionDetectionMode.Continuous;
		base.GetComponent<Collider>().enabled = false;
		m_emitting = false;
	}

	protected virtual void OnEnable()
	{
		if (!m_emitting)
		{
			StopFlame();
		}
	}

	protected virtual void OnDisable()
	{
		if (null != m_coolDownTimer)
		{
			m_coolDownTimer.Stop();
		}
		StopFlame();
		base.GetComponent<Collider>().enabled = false;
		m_emitting = false;
	}

	public virtual void Emit(Transform p_root, Transform p_target)
	{
		m_root = p_root;
		m_target = p_target;
		m_coolDownTime = Mathf.Clamp(m_damageInterval, m_minDamageInterval, 2f);
		m_coolDownTimer.CountDown(m_timeToTarget, OnCoolDownFinished);
		EmitFlame();
		base.GetComponent<Collider>().enabled = false;
		m_emitting = true;
		base.enabled = true;
	}

	public virtual void Stop()
	{
		base.enabled = false;
	}

	protected virtual void OnTriggerEnter(Collider p_other)
	{
		if (p_other.tag == m_target.tag)
		{
			base.GetComponent<Collider>().enabled = false;
			m_coolDownTimer.CountDown(m_coolDownTime, OnCoolDownFinished);
		}
	}

	protected void OnCoolDownFinished()
	{
		base.GetComponent<Collider>().enabled = true;
	}

	protected virtual void Update()
	{
	}

	protected void EmitFlame()
	{
		if (m_flameParticles == null)
		{
			return;
		}
		for (int i = 0; i < m_flameParticles.Length; i++)
		{
			if (null != m_flameParticles[i])
			{
				m_flameParticles[i].Play();
			}
		}
	}

	protected void StopFlame()
	{
		if (m_flameParticles == null)
		{
			return;
		}
		for (int i = 0; i < m_flameParticles.Length; i++)
		{
			if (null != m_flameParticles[i])
			{
				m_flameParticles[i].Stop(true);
			}
		}
	}
}
