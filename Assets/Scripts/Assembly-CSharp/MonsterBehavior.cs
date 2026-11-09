using System;

public class MonsterBehavior
{
	public class Trigger : IComparable
	{
		public float parameter;

		public string behavior = string.Empty;

		public Trigger()
		{
		}

		public Trigger(float p_parameter, string p_behavior)
		{
			parameter = p_parameter;
			behavior = p_behavior;
		}

		public override string ToString()
		{
			return string.Format("[Trigger Param: " + parameter + ", Behavior: " + behavior + "]");
		}

		public int CompareTo(object obj)
		{
			if (parameter < ((Trigger)obj).parameter)
			{
				return -1;
			}
			if (parameter == ((Trigger)obj).parameter)
			{
				return 0;
			}
			return 1;
		}
	}

	public string name { get; set; }

	public int comboCount { get; set; }

	public bool hasAdvanceAttack { get; set; }

	public bool hasSpecialAttack { get; set; }
}
