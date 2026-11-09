using System;
using System.Collections;
using UnityEngine;

public class BattleRankChestIcons : MonoBehaviour
{
	[SerializeField]
	private UISprite m_rankIcon;

	[SerializeField]
	private UISprite m_chestIcon;

	private string m_rankText = string.Empty;

	private bool canShine;

	private float chestShineTimer = (float)Math.PI;

	private void Start()
	{
		if (string.IsNullOrEmpty(m_rankText))
		{
			m_rankIcon.enabled = false;
			m_chestIcon.enabled = false;
		}
		else
		{
			m_rankIcon.enabled = true;
			m_chestIcon.enabled = true;
			m_rankIcon.spriteName = m_rankText;
		}
	}

	private void Update()
	{
		if (m_chestIcon != null && m_chestIcon.enabled && canShine)
		{
			chestShineTimer += Time.deltaTime;
			m_chestIcon.color = new Color(1f, 1f, 1f, Mathf.Sin(chestShineTimer * 4f));
		}
	}

	public void SetRank(string p_rank)
	{
		m_rankText = p_rank;
		if (string.IsNullOrEmpty(m_rankText))
		{
			m_rankIcon.gameObject.SetActiveRecursively(false);
			return;
		}
		m_rankIcon.enabled = true;
		m_rankIcon.spriteName = m_rankText;
	}

	public void ShowChest(bool p_enable)
	{
		m_chestIcon.enabled = p_enable;
	}

	public void ChestIconFlyIn()
	{
		canShine = false;
		Vector3 localScale = m_chestIcon.transform.localScale;
		m_chestIcon.transform.localScale *= 3f;
		Hashtable hashtable = new Hashtable();
		hashtable.Add("scale", localScale);
		hashtable.Add("time", 0.8f);
		iTween.ScaleTo(m_chestIcon.gameObject, hashtable);
		StartCoroutine(DelayEnableCanShine(1.2f));
	}

	private IEnumerator DelayEnableCanShine(float time)
	{
		yield return new MBWaitForSeconds(time);   // PORT (CD-059)
		canShine = true;
	}
}
