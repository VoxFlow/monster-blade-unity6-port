using System.Collections;
using UnityEngine;

public class NBUIMenuPanel : MonoBehaviour
{
	public NBUIMenuScreen parentScreen;

	public float exitTransitionDuration;

	public float enterTransitionDuration;

	public AnimationClip enterTransitionClip;

	public AnimationClip exitTransitionClip;

	public bool show
	{
		get
		{
			return base.gameObject.active;
		}
		set
		{
			if (!value)
			{
				PreHidePanel();
			}
			if (value != base.gameObject.active)
			{
				base.enabled = value;
				base.gameObject.SetActiveRecursively(value);
				if (value)
				{
					PostShowPanel();
				}
			}
		}
	}

	public virtual void PostShowPanel()
	{
		StartCoroutine(WaitForPanelReadyAndNotify());
	}

	private IEnumerator WaitForPanelReadyAndNotify()
	{
		Animation anim = base.GetComponent<Animation>();
		if (null != anim && anim.GetClipCount() > 0)
		{
			if (null != anim.clip)
			{
				yield return new MBWaitForSeconds(anim.clip.length);   // PORT (CD-059)
			}
			else if (null != enterTransitionClip)
			{
				yield return new MBWaitForSeconds(enterTransitionClip.length);   // PORT (CD-059)
			}
		}
		Messenger<NBUIMenuPanel>.Invoke("mainmenu_show_panel", this);
	}

	public virtual void PreHidePanel()
	{
		Messenger<NBUIMenuPanel>.Invoke("mainmenu_hide_panel", this);
	}

	public virtual void OnEnterTransition()
	{
	}

	public virtual void OnExitTransition()
	{
	}

	public virtual void OnPush()
	{
	}

	public virtual void OnPop()
	{
	}

	protected virtual void Awake()
	{
		Animation animation = base.GetComponent<Animation>();
		if (null != animation)
		{
			if (null != enterTransitionClip)
			{
				animation.AddClip(enterTransitionClip, "enter");
			}
			if (null != exitTransitionClip)
			{
				animation.AddClip(exitTransitionClip, "exit");
			}
		}
	}

	private void Start()
	{
	}

	protected virtual void Update()
	{
	}

	public virtual void Reset()
	{
	}

	public virtual void EnableButton(int p_index, bool p_enable)
	{
	}

	public virtual void LockButton(int p_index, bool p_locked)
	{
	}
}
