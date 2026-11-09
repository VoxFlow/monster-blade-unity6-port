using UnityEngine;

public class MonsterAttack
{
	public enum Phase
	{
		None = -1,
		WindUp = 0,
		DamageWindow = 1,
		WindDown = 2
	}

	public enum Type
	{
		None = -1,
		Weak = 0,
		Heavy = 1,
		Signature = 2,
		Ranged = 3,
		Roar = 4,
		Combo = 5,
		Max = 6
	}

	public enum Zone
	{
		None = -1,
		Center = 0,
		Left = 1,
		Right = 2,
		Area = 3,
		Max = 4
	}

	public Type type = Type.None;

	public Zone zone = Zone.None;

	public Phase phase = Phase.None;

	public int comboIndex = 1;

	public int comboLength = 1;

	public string animationName = string.Empty;

	public string attackPoint = string.Empty;

	public float multiplier = 1f;

	public float knockSpeed;

	public MonsterAttack()
	{
	}

	public MonsterAttack(MonsterAttack p_rhs)
	{
		type = p_rhs.type;
		zone = p_rhs.zone;
		phase = p_rhs.phase;
		comboIndex = p_rhs.comboIndex;
		comboLength = p_rhs.comboLength;
		animationName = p_rhs.animationName;
		attackPoint = p_rhs.attackPoint;
		multiplier = p_rhs.multiplier;
		knockSpeed = p_rhs.knockSpeed;
	}

	public static Type GetTypeFromString(string p_type)
	{
		Type result = Type.None;
		string text = p_type.ToLower();
		if (text.Contains("light"))
		{
			result = Type.Weak;
		}
		else if (text.Contains("heavy"))
		{
			result = Type.Heavy;
		}
		else if (text.Contains("signature"))
		{
			result = Type.Signature;
		}
		else if (text.Contains("ranged"))
		{
			result = Type.Ranged;
		}
		else if (text.Contains("roar"))
		{
			result = Type.Roar;
		}
		else if (text.Contains("combo"))
		{
			result = Type.Combo;
		}
		return result;
	}

	public static Zone GetZoneFromString(string p_zone)
	{
		Zone result = Zone.None;
		string text = p_zone.ToLower();
		if (text.Contains("right"))
		{
			result = Zone.Right;
		}
		else if (text.Contains("left"))
		{
			result = Zone.Left;
		}
		else if (text.Contains("center"))
		{
			result = Zone.Center;
		}
		else if (text.Contains("area"))
		{
			result = Zone.Area;
		}
		return result;
	}

	public bool IsBlockable()
	{
		return type == Type.Weak || type == Type.Ranged || type == Type.Roar;
	}

	public bool IsInteruptable()
	{
		return phase == Phase.WindDown && comboIndex == comboLength;
	}

	public static Type RandomAttack(bool p_includeSignature)
	{
		Type type = Type.None;
		Type[] array = new Type[4]
		{
			Type.Weak,
			Type.Heavy,
			Type.Combo,
			Type.Signature
		};
		int max = ((!p_includeSignature) ? (array.Length - 1) : array.Length);
		int num = 0;
		do
		{
			int num2 = Random.Range(0, max);
			type = array[num2];
			num++;
		}
		while (type != Type.None && num < 10);
		if (type == Type.None)
		{
			type = Type.Weak;
		}
		return type;
	}

	public override string ToString()
	{
		string text = "[MonsterAttack: ";
		string text2 = text;
		text = string.Concat(text2, "Type=", type, ", ");
		text2 = text;
		text = string.Concat(text2, "Zone=", zone, ", ");
		text2 = text;
		text = text2 + "Combo Index=" + comboIndex + ", ";
		text2 = text;
		text = text2 + "Combo Length=" + comboLength + ", ";
		text = text + "Animation=" + animationName + ", ";
		text = text + "Attack Point=" + attackPoint + ", ";
		text2 = text;
		text = text2 + "Knock Speed =" + knockSpeed + ", ";
		return text + "Multiplier=" + multiplier;
	}
}
