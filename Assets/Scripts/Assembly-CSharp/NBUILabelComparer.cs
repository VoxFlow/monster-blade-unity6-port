using UnityEngine;

public class NBUILabelComparer : MonoBehaviour
{
	public UILabel m_leftOperand;

	public UILabel m_comparerSign;

	public UILabel m_rightOperand;

	public Color m_inferiorColor;

	public Color m_equalColor;

	public Color m_superiorColor;

	public int leftOperand;

	public int rightOperand;

	public bool setLabel;

	private void Start()
	{
		setLabel = true;
	}

	private void Update()
	{
		if (setLabel)
		{
			SetLabels(leftOperand, rightOperand);
			setLabel = false;
		}
	}

	public void SetLabels(int _leftOperand, int _rightOperand, bool _showComparer = true)
	{
		m_leftOperand.text = _leftOperand.ToString();
		m_rightOperand.text = _rightOperand.ToString();
		if (_leftOperand < _rightOperand)
		{
			m_leftOperand.color = m_inferiorColor;
			m_comparerSign.text = "<";
			m_rightOperand.color = m_superiorColor;
		}
		else if (_leftOperand > _rightOperand)
		{
			m_leftOperand.color = m_superiorColor;
			m_comparerSign.text = ">";
			m_rightOperand.color = m_inferiorColor;
		}
		else
		{
			m_leftOperand.color = m_equalColor;
			m_comparerSign.text = "=";
			m_rightOperand.color = m_equalColor;
		}
		m_comparerSign.enabled = _showComparer;
	}
}
