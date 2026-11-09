using UnityEngine;

public class UIAlphaCutoffProgressBar : MonoBehaviour
{
	public UISprite m_foregroundSprite;

	private float m_progress = 1f;

	public float Progress
	{
		get
		{
			return m_progress;
		}
		set
		{
			m_progress = Mathf.Max(value, 0f);
			m_progress = Mathf.Min(value, 1f);
		}
	}

	private void Start()
	{
		if (m_foregroundSprite == null)
		{
			Debug.LogError(string.Concat(base.gameObject, " foreground sprite cannot be omitted."));
			base.enabled = false;
		}
		else
		{
			m_foregroundSprite.material = new Material(m_foregroundSprite.material);
			m_foregroundSprite.material.shader = Shader.Find("Unlit/Transparent Cutout");
		}
	}

	private void Update()
	{
		m_foregroundSprite.material.SetFloat("_Cutoff", m_progress);
	}
}
