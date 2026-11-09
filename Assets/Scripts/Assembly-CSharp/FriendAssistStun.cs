using System.Collections;

public class FriendAssistStun : FriendAssistAttack
{
	protected DamageData m_massiveDamageData = new DamageData();

	public override SkillEnum.AssistSkill assistSkillType
	{
		get
		{
			return SkillEnum.AssistSkill.Stun;
		}
	}

	public override void OnSkillTrigger()
	{
		if (base.friendData != null)
		{
			StartCoroutine(PlayMonsterStunReact());
		}
	}

	protected IEnumerator PlayMonsterStunReact()
	{
		float stunTime = base.friendData.FriendAssistValue;
		base.monster.enabled = false;
		base.monsterAI.enabled = false;
		PlayHitReaction();
		Waiter waiter = new Waiter();
		yield return StartCoroutine(waiter.WaitUntil(0.5f));
		if (base.monster.Data.Hp > 0f)
		{
			base.monster.AnimPlayer.Play("Idle_01");
			Dbg.Log("Friend Assist Stun: " + stunTime);
			SingletonComponent<StunLogic>.Instance.StunMonster(stunTime);
		}
		base.monster.enabled = true;
	}
}
