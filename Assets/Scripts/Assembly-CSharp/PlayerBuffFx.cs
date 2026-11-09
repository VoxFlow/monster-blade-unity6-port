using System.Collections;
using UnityEngine;

public class PlayerBuffFx : MonoBehaviour
{
	protected static readonly float frameUpdateRate = 1f / 30f;

	protected float m_elapsed;

	public Player player { get; set; }

	protected virtual void Awake()
	{
		player = null;
		m_elapsed = 0f;
		base.enabled = false;
	}

	protected virtual void OnEnable()
	{
		ShowEffect(true);
	}

	protected virtual void OnDisable()
	{
		ShowEffect(false);
	}

	protected void Start()
	{
		StartCoroutine("PeriodicUpdate");
	}

	protected void OnDestroy()
	{
		StopCoroutine("PeriodicUpdate");
	}

	protected IEnumerator PeriodicUpdate()
	{
		while (true)
		{
			m_elapsed += Time.deltaTime;
			if (m_elapsed >= frameUpdateRate)
			{
				if (null != player)
				{
					if ((!player.IsVisible() || !player.gameObject.active) && base.enabled)
					{
						base.enabled = false;
					}
					else if (player.IsVisible() && player.gameObject.active && !base.enabled)
					{
						base.enabled = true;
					}
				}
				m_elapsed = 0f;
			}
			yield return null;
		}
	}

	protected void ShowEffect(bool p_show)
	{
		int childCount = base.transform.childCount;
		for (int i = 0; i < childCount; i++)
		{
			base.transform.GetChild(i).gameObject.active = p_show;
		}
	}

	public static PlayerBuffFx InstantiateFx(GameObject p_prefab, Player p_player, string p_rootBone, TransformAttachMode p_mode = TransformAttachMode.PARENT_ORIGIN)
	{
		if (null == p_prefab || null == p_player)
		{
			return null;
		}
		GameObject gameObject = Object.Instantiate(p_prefab) as GameObject;
		PlayerBuffFx component = gameObject.GetComponent<PlayerBuffFx>();
		if (null != component)
		{
			component.transform.AttachTo(p_player.transform.FindInHierarchy(p_rootBone), p_mode);
			component.player = p_player;
			component.enabled = true;
		}
		return component;
	}
}
