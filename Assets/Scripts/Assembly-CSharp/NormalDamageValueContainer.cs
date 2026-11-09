using UnityEngine;

public class NormalDamageValueContainer : DamageValueContainer
{
	public Color m_fireDamageColor = Color.white;

	public Color m_waterDamageColor = Color.white;

	public Color m_natureDamageColor = Color.white;

	public Color m_lightingDamageColor = Color.white;

	public Color m_metalDamageColor = Color.white;

	public Color m_healColor = Color.white;

	public void DisplayDamage(int p_value, int p_finalValue, DamageType p_damageType, Element.Type p_element)
	{
		if (p_damageType != DamageType.Heal || p_finalValue != 0)
		{
			Color p_color = Color.black;
			switch (p_element)
			{
			case Element.Type.Fire:
				p_color = m_fireDamageColor;
				break;
			case Element.Type.Water:
				p_color = m_waterDamageColor;
				break;
			case Element.Type.Earth:
				p_color = m_natureDamageColor;
				break;
			case Element.Type.Lightning:
				p_color = m_lightingDamageColor;
				break;
			case Element.Type.Metal:
				p_color = m_metalDamageColor;
				break;
			}
			string empty = string.Empty;
			string p_prefix = string.Empty;
			switch (p_damageType)
			{
			case DamageType.Heavy:
				empty = "damage_value_heavy";
				break;
			case DamageType.Critical:
				empty = "damage_value_critical";
				break;
			case DamageType.Heal:
				p_color = m_healColor;
				p_prefix = "+";
				empty = "damage_value_heal";
				break;
			default:
				empty = "damage_value_normal";
				break;
			}
			DisplayDamage(p_value, p_finalValue, p_color, empty, p_prefix);
		}
	}
}
