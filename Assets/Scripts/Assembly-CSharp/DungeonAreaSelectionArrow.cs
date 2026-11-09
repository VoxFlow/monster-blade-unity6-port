using System.Collections;
using UnityEngine;

public class DungeonAreaSelectionArrow : MonoBehaviour
{
	public DungeonAreaSelectPanel ParentPanel;

	[SerializeField]
	private UISprite[] m_arrows;

	private float next = 0.3f;

	public bool Shine;

	private float timer;

	private int index;

	private void OnEnable()
	{
		Shine = true;
	}

	private void OnDisable()
	{
		Shine = false;
	}

	private void Start()
	{
		UISprite[] arrows = m_arrows;
		foreach (UISprite uISprite in arrows)
		{
			uISprite.color = new Color(0.6f, 0.6f, 0.6f, 1f);
		}
	}

	private void Update()
	{
		if (!Shine)
		{
			return;
		}
		timer += Time.deltaTime;
		if (!(timer > next))
		{
			return;
		}
		index++;
		index %= m_arrows.Length;
		for (int i = 0; i < m_arrows.Length; i++)
		{
			if (i == index)
			{
				m_arrows[index].color = Color.white;
			}
			else
			{
				m_arrows[i].color = new Color(0.6f, 0.6f, 0.6f, 1f);
			}
		}
		timer = 0f;
	}

	public void FadeOut(float p_delay = 3f)
	{
		if (base.gameObject.active)
		{
			StartCoroutine(StartFadeOut(p_delay, 0.8f));
		}
	}

	private IEnumerator StartFadeOut(float p_delay, float p_speed)
	{
		float delayTimer = 0f;
		while (delayTimer < p_delay)
		{
			delayTimer += Time.deltaTime;
			yield return null;
		}
		Shine = false;
		UISprite[] arrows = m_arrows;
		foreach (UISprite temp in arrows)
		{
			temp.color = new Color(0.6f, 0.6f, 0.6f, 1f);
		}
		float alpha = 1f;
		while (alpha > 0f)
		{
			UISprite[] arrows2 = m_arrows;
			foreach (UISprite temp2 in arrows2)
			{
				temp2.color = new Color(0.6f, 0.6f, 0.6f, alpha);
			}
			alpha -= Time.deltaTime * p_speed;
			yield return null;
		}
		ParentPanel.IsScrolled = true;
		base.gameObject.SetActiveRecursively(false);
	}
}
