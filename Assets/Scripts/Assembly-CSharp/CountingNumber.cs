using System.Collections;
using UnityEngine;

public class CountingNumber : MonoBehaviour
{
	[SerializeField]
	private UILabel m_numberLabel;

	public float m_numberPerSecond = 100f;

	private int m_value;

	private int m_newValue;

	private void Awake()
	{
		if (null == m_numberLabel)
		{
			m_numberLabel = MBHier.GetComponentInChildren<UILabel>(this);
		}
	}

	private void OnDisable()
	{
		StopCoroutine("Count");
		m_value = m_newValue;
		m_numberLabel.text = m_value.ToString();
	}

	public void SetValue(int p_value)
	{
		m_newValue = p_value;
		StartCoroutine("Count");
	}

	private IEnumerator Count()
	{
		if (!(null == m_numberLabel))
		{
			int dx = (int)Mathf.Sign(m_newValue - m_value);
			while (m_value != m_newValue)
			{
				m_value += dx;
				m_numberLabel.text = m_value.ToString();
				yield return new MBWaitForSeconds(1f / m_numberPerSecond);   // PORT (CD-059)
			}
			m_value = m_newValue;
			m_numberLabel.text = m_value.ToString();
		}
	}
}
