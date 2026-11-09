using UnityEngine;

public class HighQualityFxSetting : MonoBehaviour
{
	public GameObject[] m_highQualityEffects;

	public GameObject[] m_mediumQualityEffects;

	private void Awake()
	{
	}

	private void DisableObjectInList(GameObject[] p_list)
	{
		if (p_list == null)
		{
			return;
		}
		for (int i = 0; i < p_list.Length; i++)
		{
			if (null != p_list[i] && p_list[i].active)
			{
				p_list[i].active = false;
			}
		}
	}

	private void Update()
	{
		if (GameManager.qualitySetting != GameManager.QualitySetting.High)
		{
			DisableObjectInList(m_highQualityEffects);
		}
		if (GameManager.qualitySetting == GameManager.QualitySetting.Low)
		{
			DisableObjectInList(m_mediumQualityEffects);
		}
	}
}
