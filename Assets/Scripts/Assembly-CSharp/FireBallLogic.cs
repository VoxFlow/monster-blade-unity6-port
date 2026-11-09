using UnityEngine;

public class FireBallLogic : MonoBehaviour
{
	public GameObject m_FlamePrefab;

	public float m_TimeToTarget = 0.3f;

	public GameObject m_Target;

	public PlayerData m_PlayerData;

	private Player m_player;

	private NBTimer m_FireBallTimer;

	private Vector3 m_EmitPos;

	private Vector3 m_TargetPos;

	private GameObject m_flameInstance;

	private WingChickenFlameLogic m_flameLogic;

	private Vector3 m_velocity = Vector3.zero;

	private bool m_bIsUsedFlame;

	public GameObject Target
	{
		set
		{
			m_Target = value;
			Vector3 vector = m_Target.transform.position - base.transform.position;
			float num = vector.magnitude / m_TimeToTarget;
			m_velocity = vector.normalized * num;
			base.GetComponent<Rigidbody>().linearVelocity = m_velocity;
			base.GetComponent<Rigidbody>().isKinematic = false;
		}
	}

	protected void Start()
	{
		if (SingletonComponent<StageManager>.Instance != null)
		{
			m_player = SingletonComponent<StageManager>.Instance.PlayerObject.GetComponent<Player>();
		}
		m_FireBallTimer = base.gameObject.AddComponent<NBTimer>();
		m_FireBallTimer.IsRealtime = false;
		base.gameObject.tag = "Untagged";
		CollisionDetector component = base.gameObject.GetComponent<CollisionDetector>();
		if (component != null)
		{
			component.collisionCallback = OnCollide;
		}
		base.GetComponent<Rigidbody>().linearVelocity = Vector3.zero;
		base.gameObject.SetActiveRecursively(false);
		m_flameInstance = Object.Instantiate(m_FlamePrefab) as GameObject;
		if (m_flameInstance != null)
		{
			m_flameLogic = m_flameInstance.GetComponent<WingChickenFlameLogic>();
		}
		m_flameInstance.active = false;
	}

	protected void OnDestroy()
	{
	}

	private void OnEnable()
	{
		if (null != base.GetComponent<Rigidbody>())
		{
			base.GetComponent<Rigidbody>().linearVelocity = m_velocity;
		}
		if (null != m_FireBallTimer)
		{
			m_FireBallTimer.Pause(false);
		}
	}

	private void OnDisable()
	{
		if (null != base.GetComponent<Rigidbody>())
		{
			base.GetComponent<Rigidbody>().linearVelocity = Vector3.zero;
		}
		if (null != m_FireBallTimer)
		{
			m_FireBallTimer.Pause(true);
		}
	}

	private void OnCollide(Collider p_other)
	{
		if (p_other.enabled && !(p_other.gameObject.tag != "Player"))
		{
			base.GetComponent<Collider>().enabled = false;
			if (m_player.CurrentState != Player.State.Strafe || !(m_player.AnimPlayer.GetCurrentAnimNormalizeTime() <= 0.5f))
			{
				MonsterCollisionData component = GetComponent<MonsterCollisionData>();
				Dbg.Log(string.Concat("OnCollide. Attackter: ", component.attacker, ". Collider: ", p_other));
				Messenger<MonsterCollisionData>.Invoke("player_get_hit", component);
				UseFlame(component);
			}
		}
	}

	private void UseFlame(MonsterCollisionData p_collData)
	{
		if (!m_bIsUsedFlame && !(m_flameInstance == null) && !(m_flameLogic == null))
		{
			m_flameInstance.transform.parent = m_Target.transform;
			m_flameInstance.transform.position = m_Target.transform.position;
			m_flameLogic.m_Target = m_Target;
			m_flameLogic.Burn();
			MonsterCollisionData component = m_flameInstance.GetComponent<MonsterCollisionData>();
			component.attackerData = p_collData.attackerData;
			component.attacker = p_collData.attacker;
			component.attack = p_collData.attack;
			PlayHitExplosion(base.gameObject.transform);
			base.gameObject.transform.position = new Vector3(0f, -10000f, 0f);
			base.GetComponent<Rigidbody>().linearVelocity = Vector3.zero;
			base.GetComponent<Rigidbody>().Sleep();
			m_bIsUsedFlame = true;
			SingletonComponent<SoundManager>.Instance.PlayPlaylist("list_mon_fall_light");
			SingletonComponent<SoundManager>.Instance.PlayPlaylist("list_mon_jump");
		}
	}

	protected void Update()
	{
	}

	public void OnFire()
	{
		m_EmitPos = base.gameObject.transform.position;
		if (m_Target != null)
		{
			m_TargetPos = m_Target.transform.position;
			base.gameObject.transform.LookAt(m_TargetPos, new Vector3(0f, 1f, 0f));
		}
		m_FireBallTimer.CountDown(m_TimeToTarget, "DestroySelf");
		m_bIsUsedFlame = false;
		base.gameObject.SetActiveRecursively(true);
	}

	private void DestroySelf()
	{
		m_FireBallTimer.enabled = false;
		if (!m_bIsUsedFlame)
		{
			base.gameObject.transform.position = m_TargetPos;
			PlayGroundExplosion(base.gameObject.transform);
		}
		m_velocity = Vector3.zero;
		base.gameObject.SetActiveRecursively(false);
	}

	public void PlayHitExplosion(Transform trans)
	{
		Messenger<Transform>.Invoke("fireball_PlayHitExplosion", trans);
	}

	public void PlayGroundExplosion(Transform trans)
	{
		Messenger<Transform>.Invoke("fireball_PlayGroundExplosion", trans);
	}
}
