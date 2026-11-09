public class ItemType
{
	public readonly ItemEnum.Type type = ItemEnum.Type.NONE;

	public readonly int baseLevel = 1;

	public readonly float minDamage;

	public readonly float maxDamage;

	public readonly float critical;

	public readonly float armour;

	public readonly float hp;

	public ItemType()
	{
	}

	public ItemType(ItemEnum.Type _type = ItemEnum.Type.NONE, int _baseLevel = 1, float _minDamage = 0f, float _maxDamage = 0f, float _critical = 0f, float _armour = 0f, float _hp = 0f)
	{
		type = _type;
		baseLevel = _baseLevel;
		minDamage = _minDamage;
		maxDamage = _maxDamage;
		critical = _critical;
		armour = _armour;
		hp = _hp;
	}

	public ItemType(ItemType _csvItemType, ItemSkill _skill)
	{
		type = _csvItemType.type;
		baseLevel = _csvItemType.baseLevel;
		minDamage = _csvItemType.minDamage;
		maxDamage = _csvItemType.maxDamage;
		critical = _csvItemType.critical;
		armour = _csvItemType.armour;
		hp = _csvItemType.hp;
		if (_skill != null)
		{
			switch (_skill.selfAction)
			{
			case SkillEnum.SelfSkill.HitPoints:
				hp += _skill.directValue;
				break;
			case SkillEnum.SelfSkill.Damage:
				minDamage += _skill.directValue;
				maxDamage += _skill.directValue;
				break;
			case SkillEnum.SelfSkill.Critical:
				critical += _skill.directValue;
				break;
			case SkillEnum.SelfSkill.Armour:
				armour += _skill.directValue;
				break;
			}
		}
	}

	public override string ToString()
	{
		return string.Format("[ItemType]\ntype:{0}  baseLV:{1}    minD:{2}  maxD:{3}  crit:{4}  arm:{5}  hp:{6}\n", type, baseLevel, minDamage, maxDamage, critical, armour, hp);
	}
}
