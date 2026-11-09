using UnityEngine;

public class MascotController : MonoBehaviour
{
	private NBAnimation m_nbAnim;

	public void Show()
	{
		base.gameObject.SetActiveRecursively(true);
	}

	public void Hide()
	{
		base.gameObject.SetActiveRecursively(false);
	}

	public void PlayAnim(string p_anim)
	{
		if (m_nbAnim != null)
		{
			m_nbAnim.Play(p_anim);
		}
		else
		{
			Dbg.LogError("[Error] Unable to find \"m_nbAnim\"");
		}
	}

	private void Awake()
	{
		m_nbAnim = base.gameObject.GetComponent<NBAnimation>();
	}

	private void Start()
	{
	}

	private void Update()
	{
	}
}
