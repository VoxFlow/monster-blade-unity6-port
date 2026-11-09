using UnityEngine;

[RequireComponent(typeof(CollisionData))]
public class Explosion : MonoBehaviour
{
	[SerializeField]
	private float m_delay = 1f;

	private float m_minDamage;

	private float m_maxDamage;

	private NBTimer m_explodeTime;

	private PlayerCollisionData m_collData;

	private Callback m_explodeCb;

	public PlayerData attackerData
	{
		set
		{
			if (null != m_collData)
			{
				m_collData.attackerData = value;
			}
		}
	}

	public float damage
	{
		get
		{
			return Random.Range(m_minDamage, m_maxDamage);
		}
		set
		{
			m_minDamage = 0.9f * value;
			m_maxDamage = 1.1f * value;
		}
	}

	private void Awake()
	{
		m_explodeTime = base.gameObject.AddComponent<NBTimer>();
		m_explodeTime.Name = "Explosion Timer";
		m_collData = base.gameObject.GetComponent<PlayerCollisionData>();
		m_collData.attack = new PlayerAttack();
		m_collData.attack.type = PlayerAttack.Type.Skill;
		m_collData.attack.direction = PlayerAttack.Direction.BottomToTop;
		m_collData.attack.interuptAnimation = true;
		m_collData.attack.multiplier = 1f;
		m_collData.ignoreArmour = true;
		m_collData.directDamage = 0f;
	}

	private void OnEnable()
	{
		if (m_explodeTime.Counter > 0f)
		{
			m_explodeTime.Pause(true);
		}
	}

	private void OnDisable()
	{
		if (m_explodeTime.Counter > 0f)
		{
			m_explodeTime.Pause(false);
		}
	}

	public void Explode(Callback p_callback = null)
	{
		m_explodeCb = p_callback;
		m_explodeTime.CountDown(m_delay, OnTimerEnd);
	}

	private void OnTimerEnd()
	{
		m_collData.attack.type = PlayerAttack.Type.Skill;
		m_collData.directDamage = damage;
		Messenger<PlayerCollisionData>.Invoke("monster_get_hit", m_collData);
		if (m_explodeCb != null)
		{
			m_explodeCb();
		}
	}
}
