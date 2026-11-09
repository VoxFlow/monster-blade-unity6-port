using System.Collections;
using UnityEngine;

public class DefaultSuperAttack : SuperAttack
{
	protected PlayerCollisionData m_swordCollisionData;

	protected ParticleSystemController m_superTornadoFx;

	protected ParticleSystemController m_superAtkGeyser;

	public GameObject superFxPrefab;

	public GameObject geyserFxPrefab;

	public string[] attackAnimations;

	public PlayerAttack.Direction[] attackDirections;

	public float[] damageMultipliers;

	public float[] sequenceTimes;

	public float[] timeScales;

	public float[] slowMotionTimes;

	public override void Init()
	{
		base.Init();
		Transform transform = m_player.transform.FindInHierarchy("Sword_Bone");
		if (transform != null)
		{
			m_swordCollisionData = transform.gameObject.GetComponent<PlayerCollisionData>();
		}
		m_superTornadoFx = InstantiateFx(superFxPrefab);
		m_superAtkGeyser = InstantiateFx(geyserFxPrefab);
	}

	protected override void OnDestroy()
	{
		base.OnDestroy();
		StopCoroutine("TriggerSlowmotion");
		Time.timeScale = 1f;
	}

	protected override IEnumerator PlayAttackSequences()
	{
		if (attackAnimations == null || sequenceTimes == null || attackAnimations.Length != sequenceTimes.Length)
		{
			yield break;
		}
		m_player.ChangeState(Player.State.Attack);
		int index = 0;
		PlayerAttack attack = new PlayerAttack
		{
			forceCleanHit = true,
			interuptAnimation = true
		};
		m_superTornadoFx.Play();
		StartCoroutine("TriggerSlowmotion");
		for (; index < attackAnimations.Length; index++)
		{
			if (m_player.CurrentState != Player.State.Attack)
			{
				Dbg.Log("Super Attack was canceled while performing slashes combo.");
				m_superTornadoFx.Stop();
				break;
			}
			attack.direction = attackDirections[index];
			attack.type = PlayerAttack.Type.Super;
			attack.animation = attackAnimations[index];
			attack.multiplier = damageMultipliers[index];
			attack.comboIndex = 0;
			m_player.CurrentAttack = attack;
			if (m_swordCollisionData != null)
			{
				m_swordCollisionData.attack = attack;
			}
			float fadeTime = 0.3f;
			m_player.AnimPlayer.Play(attack.animation, fadeTime);
			Messenger<PlayerAttack>.Invoke("player_begin_atk", attack);
			while (null != m_player.AnimPlayer.CurrentAnim && m_player.AnimPlayer.CurrentAnim.time < sequenceTimes[index])
			{
				yield return null;
			}
		}
		if (m_monster.Data.Hp > 0f)
		{
			if (index < attackAnimations.Length || !m_monsterHit)
			{
				m_monsterAI.enabled = true;
			}
			else if (m_monster.AnimPlayer.enabled)
			{
				m_monster.TryPlayAnim("Stun");
			}
		}
		m_superTornadoFx.Stop();
		float geyserExplosionTime = 1.7f;
		while (m_player.AnimPlayer.CurrentAnim != null && m_player.AnimPlayer.CurrentAnim.time < geyserExplosionTime && m_player.CurrentState == Player.State.Attack)
		{
			yield return null;
		}
		if (m_player.CurrentState == Player.State.Attack && m_superAtkGeyser != null)
		{
			m_superAtkGeyser.Play();
		}
		else
		{
			Dbg.Log("Super Attack was canceled before geyser.");
		}
		while (!m_player.IsReadyToAttack)
		{
			yield return null;
		}
	}

	protected IEnumerator TriggerSlowmotion()
	{
		if (timeScales == null || slowMotionTimes == null || timeScales.Length != slowMotionTimes.Length)
		{
			yield break;
		}
		for (int index = 0; index < slowMotionTimes.Length; index++)
		{
			if (m_player.CurrentState != Player.State.Attack)
			{
				break;
			}
			Time.timeScale = timeScales[index];
			yield return new MBWaitForSeconds(slowMotionTimes[index]);   // PORT (CD-059)
		}
		Time.timeScale = 1f;
	}

	protected ParticleSystemController InstantiateFx(GameObject p_prefab)
	{
		if (p_prefab != null)
		{
			GameObject gameObject = Object.Instantiate(p_prefab) as GameObject;
			if (gameObject != null)
			{
				gameObject.transform.position += m_player.transform.position;
				gameObject.transform.parent = m_player.transform;
				return gameObject.gameObject.GetComponent<ParticleSystemController>();
			}
		}
		return null;
	}
}
