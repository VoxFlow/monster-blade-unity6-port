using UnityEngine;

public class UpdateComboCounter : MonoBehaviour
{
	private UISprite m_comboText1;

	private UISprite m_comboText2;

	private UISprite m_comboText3;

	private Color m_alphaColor;

	private Color m_normalColor;

	private int counter;

	private int printValue;

	private void Start()
	{
		GameObject gameObject = GameObject.Find("comboText1");
		if (gameObject != null)
		{
			m_comboText1 = gameObject.GetComponent<UISprite>();
		}
		GameObject gameObject2 = GameObject.Find("comboText2");
		if (gameObject2 != null)
		{
			m_comboText2 = gameObject2.GetComponent<UISprite>();
		}
		GameObject gameObject3 = GameObject.Find("comboText3");
		if (gameObject3 != null)
		{
			m_comboText3 = gameObject3.GetComponent<UISprite>();
		}
		m_alphaColor = new Color(m_comboText1.color.r, m_comboText1.color.g, m_comboText1.color.b, 0f);
		m_normalColor = new Color(m_comboText1.color.r, m_comboText1.color.g, m_comboText1.color.b, m_comboText1.color.a);
		m_comboText1.color = m_alphaColor;
		m_comboText2.color = m_alphaColor;
		m_comboText3.color = m_alphaColor;
	}

	private void Update()
	{
	}

	private void UpdateComboText(int comboCount)
	{
		int num = comboCount % 10;
		comboCount /= 10;
		int num2 = comboCount % 10;
		comboCount /= 10;
		int num3 = comboCount;
		if (num3 > 0)
		{
			m_comboText1.color = m_normalColor;
			string spriteName = "text_combo_" + num3 + ".png";
			m_comboText1.spriteName = spriteName;
		}
		else
		{
			m_comboText1.color = m_alphaColor;
		}
		if (num2 > 0)
		{
			m_comboText2.color = m_normalColor;
			string spriteName2 = "text_combo_" + num2 + ".png";
			m_comboText2.spriteName = spriteName2;
		}
		else
		{
			m_comboText2.color = m_alphaColor;
		}
		if (num3 > 0)
		{
			m_comboText2.color = m_normalColor;
			string spriteName3 = "text_combo_" + num2 + ".png";
			m_comboText2.spriteName = spriteName3;
		}
		string spriteName4 = "text_combo_" + num + ".png";
		m_comboText3.spriteName = spriteName4;
	}
}
