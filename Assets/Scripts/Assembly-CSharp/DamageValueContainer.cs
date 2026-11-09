using UnityEngine;

public class DamageValueContainer : MonoBehaviour
{
	public float m_displayTime = 0.5f;

	public Camera m_uiCam;

	public Transform m_referencePosition;

	protected DamageValue[] m_damageValueScripts;

	public Camera UiCam
	{
		set
		{
			m_uiCam = value;
		}
	}

	public Camera BattleCamera { protected get; set; }

	public Transform ReferencePosition
	{
		set
		{
			m_referencePosition = value;
		}
	}

	public void Pause(bool p_flag)
	{
		DamageValue[] damageValueScripts = m_damageValueScripts;
		foreach (DamageValue damageValue in damageValueScripts)
		{
			if (damageValue.gameObject.active)
			{
				damageValue.enabled = !p_flag;
			}
		}
	}

	protected void DisplayDamage(int p_value, int p_finalValue, Color p_color, string p_animation, string p_prefix)
	{
		if (m_referencePosition == null)
		{
			Dbg.LogError("Reference position has not been specified");
			return;
		}
		Vector3 screenPos = UiUtils.GetScreenPos(m_referencePosition.position, BattleCamera, m_uiCam);
		DamageValue damageValue = null;
		DamageValue[] damageValueScripts = m_damageValueScripts;
		foreach (DamageValue damageValue2 in damageValueScripts)
		{
			if (!damageValue2.gameObject.active)
			{
				damageValue = damageValue2;
				break;
			}
		}
		if (damageValue != null)
		{
			damageValue.Show(p_value, p_finalValue, screenPos, m_displayTime, p_color, p_animation, p_prefix);
		}
		else
		{
			Dbg.LogWarn("There is no DamageValue text available. Try to add new DamageValue game-object or decrease the text display period");
		}
	}

	public virtual void HideAllDamageValue()
	{
		if (m_damageValueScripts != null)
		{
			DamageValue[] damageValueScripts = m_damageValueScripts;
			foreach (DamageValue damageValue in damageValueScripts)
			{
				if (damageValue != null && damageValue.gameObject != null && damageValue.gameObject.active)
				{
					damageValue.gameObject.SetActiveRecursively(false);
				}
			}
		}
		else
		{
			Dbg.LogWarn("m_damageValueScripts is null, unable to hide them");
		}
	}

	protected virtual void Start()
	{
		m_damageValueScripts = MBHier.GetComponentsInChildren<DamageValue>(base.transform);
		DamageValue[] damageValueScripts = m_damageValueScripts;
		foreach (DamageValue damageValue in damageValueScripts)
		{
			damageValue.gameObject.SetActiveRecursively(false);
		}
	}
}
