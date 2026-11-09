using System.Collections;
using UnityEngine;

public class LandDragonGeyserAttack : MonoBehaviour
{
	private ParticleSystemController m_particleSystem;

	private MonsterCollisionData m_collData;

	private NBTimer m_delayTimer;

	private Vector3 m_explosionPos = Vector3.zero;

	public GameObject[] m_legacyParticles;

	private static readonly Vector3 m_invalidPos = new Vector3(float.NaN, float.NaN, float.NaN);

	private void Awake()
	{
		m_particleSystem = base.gameObject.GetComponent<ParticleSystemController>();
		m_collData = base.gameObject.GetComponent<MonsterCollisionData>();
		m_delayTimer = base.gameObject.AddComponent<NBTimer>();
		m_collData.attack = new MonsterAttack();
		m_collData.attack.type = MonsterAttack.Type.Signature;
		m_collData.attack.multiplier = 0.6f;
		m_collData.attack.animationName = LandDragon.GEYSER_ATK_ANIM;
		m_collData.attacker = base.transform.parent.gameObject;
		if (m_legacyParticles != null)
		{
			GameObject[] legacyParticles = m_legacyParticles;
			foreach (GameObject gameObject in legacyParticles)
			{
				gameObject.SetActiveRecursively(false);
			}
		}
	}

	public void Explode(Vector3 p_pos, float p_delay)
	{
		Dbg.Log(string.Concat("Explode, Position: ", m_explosionPos, ", Delay: ", p_delay));
		base.gameObject.active = true;
		m_explosionPos = p_pos;
		m_delayTimer.CountDown(p_delay, OnDelayEnd);
	}

	private void OnDelayEnd()
	{
		StartCoroutine(ActivateGeyserExplosion());
	}

	private IEnumerator ActivateGeyserExplosion()
	{
		Dbg.Log("Exploding");
		m_particleSystem.Play();
		if (m_legacyParticles != null)
		{
			GameObject[] legacyParticles = m_legacyParticles;
			foreach (GameObject legacyParticle in legacyParticles)
			{
				if (!(legacyParticle == null))
				{
					GameObject go = Object.Instantiate(legacyParticle, m_explosionPos, Quaternion.identity) as GameObject;
					go.SetActiveRecursively(true);
					legacyParticle.SetActiveRecursively(false);
				}
			}
		}
		Waiter waiter = new Waiter();
		yield return StartCoroutine(waiter.WaitUntil(1.2f));
		base.gameObject.GetComponent<Collider>().enabled = true;
		yield return StartCoroutine(waiter.WaitUntil(0.5f));
		base.gameObject.GetComponent<Collider>().enabled = false;
	}

	private void Update()
	{
		if (m_particleSystem.IsPlaying())
		{
			if (m_explosionPos == m_invalidPos)
			{
				m_particleSystem.Stop();
				Dbg.LogWarn("Invalid explosion position.");
			}
			else
			{
				base.transform.position = m_explosionPos;
				base.transform.rotation = Quaternion.identity;
			}
		}
		else if (base.gameObject.GetComponent<Collider>().enabled)
		{
			base.gameObject.GetComponent<Collider>().enabled = false;
		}
	}
}
