using System;
using System.Collections;
using UnityEngine;

public class EpicDragon : Monster
{
	public const string TAKE_OFF_ANIM = "TakeOff";

	public const string LANDING_ANIM = "TakeOff_Recovery";

	public const string FLYING_FIREBALL_ANIM = "Flight_Att_Fireball";

	public const string FLYING_FIREBALL_RAPID_ANIM = "Flight_Att_Fireball_Rapid";

	public const string FLYING_FLAME_THROWER_ANIM = "Flight_Att_FlameThrower";

	public const string GROUND_FLAME_THROWER_COMBO = "Flight_Att_Combo";

	public const string GROUND_FLAME_THROWER_ANIM = "Att_FlameThrower";

	public const string GLIDE_IDLE_ANIM = "Glide_Idle";

	public const string GLIDE_HIT_LEFT_ANIM = "Glide_Hit_Left";

	public const string GLIDE_HIT_RIGHT_ANIM = "Glide_Hit_Right";

	[SerializeField]
	private Transform m_flyingTarget;

	private static readonly string[] ms_flightFireballAnims = new string[2] { "Flight_Att_Fireball", "Flight_Att_Fireball_Rapid" };

	private static readonly string[] ms_flightIdleAnims = new string[1] { "Flight_Idle_01" };

	private static readonly string[] ms_flightHitAnims = new string[2] { "Flight_Hit_Left", "Flight_Hit_Right" };

	private static readonly string[] ms_glideAttackAnims = new string[2] { "Glide_Att_Left", "Glide_Att_Right" };

	private static readonly string[] ms_glideHitAnims = new string[2] { "Glide_Hit_Left", "Glide_Hit_Right" };

	private static readonly string[] ms_groundFireballAnims = new string[2] { "Att_Fireball", "Att_Fireball_02" };

	private bool m_tumble;

	public static string[] FLYING_IDLE_ANIMS
	{
		get
		{
			return ms_flightIdleAnims;
		}
	}

	public static string[] GLIDE_ATTACK_ANIMS
	{
		get
		{
			return ms_glideAttackAnims;
		}
	}

	public static string[] GROUND_FIREBALL_ANIMS
	{
		get
		{
			return ms_groundFireballAnims;
		}
	}

	public bool flying { get; private set; }

	public bool takingOff { get; private set; }

	public bool gliding { get; set; }

	public Transform flyingTarget
	{
		get
		{
			return m_flyingTarget;
		}
	}

	public static string RandomFlyingIdleAnim()
	{
		return NBContainerUtil.RandomElement(ms_flightIdleAnims);
	}

	public static string RandomFlyingFireballAnim()
	{
		return NBContainerUtil.RandomElement(ms_flightFireballAnims);
	}

	public static string RandomFlyingHitReactAnim()
	{
		return NBContainerUtil.RandomElement(ms_flightHitAnims);
	}

	public static bool IsFlyingIdleAnim(string p_anim)
	{
		return Array.IndexOf(ms_flightIdleAnims, p_anim) >= 0;
	}

	public static bool IsFlyingFireballAnim(string p_anim)
	{
		return Array.IndexOf(ms_flightFireballAnims, p_anim) >= 0;
	}

	public static bool IsFlyingHitReactAnim(string p_anim)
	{
		return Array.IndexOf(ms_flightHitAnims, p_anim) >= 0;
	}

	protected override void Awake()
	{
		base.Awake();
		CollisionDetector collisionDetector = m_bodyCollider;
		collisionDetector.collisionCallback = (CollisionDetector.CollisionCallback)Delegate.Combine(collisionDetector.collisionCallback, new CollisionDetector.CollisionCallback(OnTriggerEnter));
	}

	protected override void InitAttackAnimations()
	{
		base.InitAttackAnimations();
		RemoveAttackAnimations(ms_glideAttackAnims);
		RemoveAttackAnimations(ms_flightFireballAnims);
		RemoveAttackAnimations(new string[1] { "Flight_Att_FlameThrower" });
	}

	public IEnumerator TakeOff()
	{
		takingOff = true;
		flying = true;
		yield return StartCoroutine(m_anim.PlayAndWait("TakeOff"));
		takingOff = false;
		m_anim.Play(RandomFlyingIdleAnim());
	}

	public IEnumerator Land()
	{
		Messenger.Invoke("epicdragon_landing");
		m_invincible = true;
		yield return StartCoroutine(m_anim.PlayAndWait("TakeOff_Recovery"));
		m_invincible = false;
		flying = false;
		Idle();
	}

	public override void RandomIdleAnim(float p_fadeLength, bool p_loop)
	{
		if (!takingOff)
		{
			if (flying)
			{
				m_anim.Play(RandomFlyingIdleAnim(), p_fadeLength);
			}
			else if (m_tumble)
			{
				m_tumble = false;
				m_anim.Play("Recovery", p_fadeLength);
			}
			else if (gliding)
			{
				m_anim.Play("Glide_Idle", p_fadeLength);
			}
			else
			{
				base.RandomIdleAnim(p_fadeLength, p_loop);
			}
		}
	}

	public override void PlayStumbleAnim()
	{
		base.PlayStumbleAnim();
		m_tumble = true;
	}

	public override void Turn(float p_angle)
	{
		if (!flying)
		{
			base.Turn(p_angle);
		}
		else if (!m_tumble)
		{
			m_rotor.BeginRotate(p_angle, 1f, OnFinishOrient);
		}
	}

	public override bool PlayHitAnim(PlayerAttack.Direction p_dir)
	{
		if (flying)
		{
			m_anim.Play(RandomFlyingHitReactAnim());
			return true;
		}
		if (gliding)
		{
			string empty = string.Empty;
			string[] p_animList = new string[8]
			{
				string.Empty,
				string.Empty,
				"Glide_Hit_Right",
				"Glide_Hit_Left",
				"Glide_Hit_Right",
				"Glide_Hit_Left",
				"Glide_Hit_Right",
				"Glide_Hit_Left"
			};
			empty = PickHitAnimation(p_dir, p_animList);
			if (string.IsNullOrEmpty(empty))
			{
				empty = NBContainerUtil.RandomElement(ms_glideHitAnims);
			}
			StartCoroutine(m_anim.PlaySequence(empty, 0.06f));
			return true;
		}
		if (!m_tumble)
		{
			return base.PlayHitAnim(p_dir);
		}
		return false;
	}

	public override bool PlayHitHeavyAnim(PlayerAttack.Direction p_dir)
	{
		if (flying || gliding)
		{
			return PlayHitAnim(p_dir);
		}
		if (!m_tumble)
		{
			return base.PlayHitHeavyAnim(p_dir);
		}
		return false;
	}

	private void OnDragonTakeOff()
	{
		if (takingOff)
		{
			Messenger.Invoke("epicdragon_take_off");
		}
	}

	public override bool IsPlayingIdleAnimation()
	{
		return null != m_anim && m_anim.IsPlaying && (IsFlyingIdleAnim(m_anim.CurrentAnimName) || m_anim.CurrentAnimName == "Glide_Idle" || base.IsPlayingIdleAnimation());
	}

	public static bool IsFireBallAttack(MonsterAttack p_attack)
	{
		if (p_attack == null)
		{
			return false;
		}
		return IsFlyingFireballAnim(p_attack.animationName) || Array.IndexOf(ms_groundFireballAnims, p_attack.animationName) >= 0;
	}

	public static bool IsFireBreathAttack(MonsterAttack p_attack)
	{
		if (p_attack == null)
		{
			return false;
		}
		return p_attack.animationName == "Flight_Att_FlameThrower" || p_attack.animationName == "Att_FlameThrower" || (p_attack.animationName.Contains("Flight_Att_Combo") && p_attack.zone == MonsterAttack.Zone.Area);
	}
}
