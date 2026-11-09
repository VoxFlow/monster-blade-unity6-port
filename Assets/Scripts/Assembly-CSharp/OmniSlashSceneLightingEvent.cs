using UnityEngine;

public class OmniSlashSceneLightingEvent : MonoBehaviour
{
	private GameObject m_levelBackgroud;

	private void Awake()
	{
		m_levelBackgroud = GameObject.FindGameObjectWithTag("Background");
		if (IsAbleToAnimate())
		{
			m_levelBackgroud.GetComponent<Animation>().Stop();
		}
		Messenger.AddListener("player_super_atk_end", LightenScene);
		base.enabled = false;
	}

	private void OnDestroy()
	{
		ResetLight();
		Messenger.RemoveListener("player_super_atk_end", LightenScene);
	}

	private void DimScene()
	{
		if (IsAbleToAnimate() && SuperAttack.Playing)
		{
			m_levelBackgroud.GetComponent<Animation>().Play("Anim_Dimming_Env");
		}
	}

	private void LightenScene()
	{
		if (IsAbleToAnimate())
		{
			m_levelBackgroud.GetComponent<Animation>().Play("Anim_Lightening_Env");
		}
		else
		{
			ResetLight();
		}
	}

	private void ResetLight()
	{
		if (IsAbleToAnimate())
		{
			m_levelBackgroud.GetComponent<Animation>().Stop();
			AnimateTintColor component = m_levelBackgroud.GetComponent<AnimateTintColor>();
			if (null != component)
			{
				component.SetColor(Color.white);
				component.enabled = false;
			}
		}
	}

	private bool IsAbleToAnimate()
	{
		return null != m_levelBackgroud && null != m_levelBackgroud.GetComponent<Animation>();
	}
}
