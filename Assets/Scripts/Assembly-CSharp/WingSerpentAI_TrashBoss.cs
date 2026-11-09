public class WingSerpentAI_TrashBoss : AIController_TrashBoss
{
	protected override void Awake()
	{
		base.Awake();
		string[] p_list = new string[2] { "Att_Swipe_Down_Left", "Att_Swipe_Left" };
		string[] p_list2 = new string[2] { "Att_Swipe_Down_Right", "Att_Swipe_Right" };
		AssignSwipeAttackAnim(MonsterAttack.Zone.Left, p_list);
		AssignSwipeAttackAnim(MonsterAttack.Zone.Right, p_list2);
	}
}
