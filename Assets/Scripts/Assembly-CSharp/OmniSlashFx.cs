using System.Collections;
using UnityEngine;

public class OmniSlashFx : MonoBehaviour
{
	public SelfDestroy weaponFlareFxPrefab;

	public SelfDestroy flareFxPrefab;

	public SelfDestroy geyserFxPrefab;

	public SelfDestroy[] slashEffects;

	public Shader radiantSwordShader;

	public Color radiantSwordColor = Color.white;

	private PlayerBuffFx m_weaponGlowFxPrefab;

	private int m_comboCount;

	private int m_weaponMaterialIndex;

	private PlayerBuffFx m_weaponGlowFx;

	private Transform m_swordBone;

	private Shader m_defaultShader;

	private Player m_player;

	public PlayerBuffFx weaponGlowFxPrefab
	{
		private get
		{
			return m_weaponGlowFxPrefab;
		}
		set
		{
			m_weaponGlowFxPrefab = value;
			if (null != m_weaponGlowFxPrefab && null != swordBone)
			{
				GameObject gameObject = InstantiateFx(m_weaponGlowFxPrefab.gameObject, swordBone);
				if (null != gameObject)
				{
					gameObject.active = true;
					m_weaponGlowFx = gameObject.GetComponent<PlayerBuffFx>();
				}
			}
		}
	}

	public Player player
	{
		get
		{
			return m_player;
		}
		set
		{
			if (null != m_weaponGlowFx)
			{
				m_weaponGlowFx.player = value;
				m_weaponGlowFx.enabled = true;
			}
			m_player = value;
		}
	}

	private Transform swordBone
	{
		get
		{
			if (null == m_swordBone)
			{
				m_swordBone = base.transform.FindInHierarchy("Sword_Bone");
			}
			return m_swordBone;
		}
	}

	private void Awake()
	{
		m_weaponMaterialIndex = Equipment.GetMaterialIndex(Equipment.Part.Weapon);
		Messenger<PlayerAttack>.AddListener("player_begin_atk", OnPlayerBeginAttack);
		Messenger<PlayerAttack>.AddListener("player_damage_window", OnPlayerDamageWindow);
		Messenger<PlayerCollisionData>.AddListener("monster_get_hit", OnMonsterGetHit);
		Messenger.AddListener("player_super_atk_end", OnSuperAttackEnd);
	}

	private void OnDestroy()
	{
		if (null != m_weaponGlowFx)
		{
			StopParticle(m_weaponGlowFx.gameObject);
		}
		ResetSwordShader();
		Messenger<PlayerAttack>.RemoveListener("player_begin_atk", OnPlayerBeginAttack);
		Messenger<PlayerAttack>.RemoveListener("player_damage_window", OnPlayerDamageWindow);
		Messenger<PlayerCollisionData>.RemoveListener("monster_get_hit", OnMonsterGetHit);
		Messenger.RemoveListener("player_super_atk_end", OnSuperAttackEnd);
	}

	private void OnPlayerBeginAttack(PlayerAttack p_atk)
	{
		if (p_atk.type == PlayerAttack.Type.Super)
		{
			m_comboCount = 0;
		}
	}

	private void OnPlayerDamageWindow(PlayerAttack p_atk)
	{
		if (p_atk != null && p_atk.type == PlayerAttack.Type.Super && slashEffects != null && slashEffects.Length > m_comboCount)
		{
			InstantiateFx(slashEffects[m_comboCount++].gameObject, base.transform);
		}
	}

	private void OnMonsterGetHit(PlayerCollisionData p_collisionData)
	{
		if (p_collisionData != null && p_collisionData.attack != null && p_collisionData.attack.type == PlayerAttack.Type.Super && null != flareFxPrefab)
		{
			InstantiateFx(flareFxPrefab.gameObject, base.transform);
		}
	}

	private void OnSuperAttackEnd()
	{
		ResetSwordShader();
	}

	private void TriggerLastAttackSlowMotion()
	{
		SingletonComponent<SlowMotionSystem>.Instance.LinearFade(10f, 1f);
	}

	private void ChangeAnimationSpeed(float p_speed)
	{
		if (null != player && null != player.AnimPlayer)
		{
			player.AnimPlayer.TimeScale = p_speed;
		}
	}

	private void TriggerRadiantFx()
	{
		if (null != base.GetComponent<Renderer>() && null != radiantSwordShader && base.GetComponent<Renderer>().materials.Length > m_weaponMaterialIndex && m_weaponMaterialIndex >= 0)
		{
			InstantiateFx(weaponFlareFxPrefab.gameObject, swordBone);
		}
	}

	private void TriggerParticelGeyser()
	{
		InstantiateFx(geyserFxPrefab.gameObject, base.transform);
	}

	private GameObject InstantiateFx(GameObject p_prefab, Transform p_root)
	{
		if (null == p_prefab || null == p_root)
		{
			return null;
		}
		GameObject gameObject = Object.Instantiate(p_prefab) as GameObject;
		if (null == gameObject)
		{
			return null;
		}
		gameObject.transform.AttachTo(p_root, TransformAttachMode.RETAIN_LOCAL_TRANSFORM);
		return gameObject;
	}

	private void StopParticle(GameObject p_fx)
	{
		if (null != p_fx)
		{
			ParticleSystem[] componentsInChildren = MBHier.GetComponentsInChildren<ParticleSystem>(p_fx);
			for (int i = 0; i < componentsInChildren.Length; i++)
			{
				componentsInChildren[i].Stop();
			}
		}
	}

	private void ResetSwordShader()
	{
		if (base.gameObject.active)
		{
			StartCoroutine(ResetSwordColor());
		}
	}

	private IEnumerator ResetSwordColor()
	{
		if (!(null == base.GetComponent<Renderer>()) && !(null == radiantSwordShader) && (base.GetComponent<Renderer>().materials.Length < m_weaponMaterialIndex || m_weaponMaterialIndex >= 0))
		{
			InstantiateFx(weaponFlareFxPrefab.gameObject, swordBone);
		}
		yield break;
	}
}
