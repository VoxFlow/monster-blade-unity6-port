using System.Collections.Generic;

public class LandDragon : Monster
{
	public static readonly string RAKE_ATK_ANIM = "Att_Rake";

	public static readonly string SNAP_ATK_ANIM = "Att_Snap";

	public static readonly string SLAM_ATK_ANIM = "Att_Slam";

	public static readonly string GEYSER_ATK_ANIM = "Geyser";

	private List<KeyValuePair<string, MonsterAttack.Type>> m_attackAnimationSets;

	public bool slamming { get; set; }

	public bool diving { get; private set; }

	public bool fullyUnderGround { get; private set; }

	public float slamKnockSpeed
	{
		get
		{
			return 13f;
		}
		private set
		{
		}
	}

	public List<KeyValuePair<string, MonsterAttack.Type>> attackAnimationSets
	{
		get
		{
			return m_attackAnimationSets;
		}
	}

	public override void Idle()
	{
		if (ChangeState(State.Idle) && !diving && !slamming)
		{
			RandomIdleAnim(0.4f, true);
			SendMessage("OnIdle");
		}
	}

	public bool CanJump()
	{
		return CanBecomeIdle() && base.CurrentState != State.Cutscene;
	}

	public bool CanDive()
	{
		return CanBecomeIdle() && base.CurrentState != State.Cutscene;
	}

	public override bool CanPlayCutscene()
	{
		return IsReachableFromCurrent(State.Cutscene);
	}

	public override bool CanStun()
	{
		return base.CanStun() && !diving;
	}

	public void Dive()
	{
		if (ChangeState(State.Idle))
		{
			Dbg.Log("Diving");
			m_anim.Play("Dive");
			diving = true;
			fullyUnderGround = false;
			Messenger.Invoke(LandDragonEvent.LAND_DRAGON_DIVE);
		}
	}

	public override bool PlayIntro()
	{
		if (base.PlayIntro())
		{
			Invoke("EmergeIntro", 5f);
			return true;
		}
		return false;
	}

	private void EmergeIntro()
	{
		m_anim.Play("Emerge", 0f);
	}

	public void Emerge()
	{
		if (ChangeState(State.Idle))
		{
			fullyUnderGround = false;
			m_anim.Play("Emerge", 0f);
			Messenger.Invoke(LandDragonEvent.LAND_DRAGON_EMERGE);
		}
	}

	private void OnDiveEnd()
	{
		fullyUnderGround = true;
		EnableShadow(false);
	}

	protected void OnEmergeDown()
	{
		EnableShadow(true);
		diving = false;
	}

	public void Stunned()
	{
		SingletonComponent<StunLogic>.Instance.StunMonster(10f);
	}

	protected override void OnAnimEnded(string p_anim)
	{
		if (base.enabled && !diving)
		{
			base.OnAnimEnded(p_anim);
		}
	}

	protected override void InitAttackAnimations()
	{
		base.InitAttackAnimations();
		m_attackAnimationSets = new List<KeyValuePair<string, MonsterAttack.Type>>();
		List<string>[] attackAnimations = m_attackAnimations;
		foreach (List<string> list in attackAnimations)
		{
			int num = 0;
			foreach (string item in list)
			{
				m_attackAnimationSets.Add(new KeyValuePair<string, MonsterAttack.Type>(item, (MonsterAttack.Type)num));
				num++;
			}
		}
		List<string> list2 = m_attackAnimations[1];
		list2.Remove(SNAP_ATK_ANIM);
	}
}
