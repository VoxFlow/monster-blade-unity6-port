using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class NBUIMenuScreen : MonoBehaviour
{
	public NBUIMenuPanel[] menuPanels;

	public GameObject backButton;

	public UIButton m_backButton;

	private Stack<NBUIMenuPanel> _stack = new Stack<NBUIMenuPanel>();

	protected NBUIMenuPanel _currentPanelInTransition;

	private UICamera _input;

	private Callback m_exitTransitionCallback;

	private Callback m_enterTransitionCallback;

	public NBUIMenuPanel currentPanel
	{
		get
		{
			if (_stack.Count > 0)
			{
				return _stack.Peek();
			}
			return null;
		}
	}

	public bool inputEnabled
	{
		set
		{
			if ((bool)_input)
			{
				_input.useTouch = (_input.useMouse = value);
			}
		}
	}

	private void Awake()
	{
		for (int i = 0; i < menuPanels.Length; i++)
		{
			if (Dbg.Assert(menuPanels[i], "Screen missing!"))
			{
				continue;
			}
			if (MainMenu.Instance != null)
			{
				if (menuPanels[i] is CharacterPanelPlaceholder)
				{
					menuPanels[i] = MainMenu.Instance.m_sharedCharacterInfoPanel;
				}
				else if (menuPanels[i] is FriendPanelPlaceholder)
				{
					menuPanels[i] = MainMenu.Instance.m_sharedFriendSelectPanel;
				}
			}
			menuPanels[i].show = false;
		}
		_input = UICamera.eventHandler;
	}

	public virtual void OnEnable()
	{
		NBUIMenuPanel[] array = menuPanels;
		foreach (NBUIMenuPanel nBUIMenuPanel in array)
		{
			if (nBUIMenuPanel == null)
			{
				Dbg.LogError("No Panel:" + base.gameObject);
			}
			else
			{
				nBUIMenuPanel.parentScreen = this;
			}
		}
	}

	public virtual void OnDisable()
	{
		if ((bool)currentPanel)
		{
			currentPanel.show = false;
		}
	}

	public virtual void Start()
	{
	}

	protected virtual void Reset()
	{
	}

	protected virtual void Init()
	{
	}

	public virtual void Home()
	{
		Reset();
		Init();
	}

	public virtual void PushPanel(NBUIMenuPanel newPanel)
	{
		if (!newPanel)
		{
			return;
		}
		if (_stack.Count > 0)
		{
			NBUIMenuPanel nBUIMenuPanel = _stack.Peek();
			if ((bool)nBUIMenuPanel && currentPanel != newPanel)
			{
				nBUIMenuPanel.show = false;
			}
		}
		_stack.Push(newPanel);
		if (newPanel.show)
		{
			return;
		}
		Dbg.LogWarn("push = " + newPanel.name);
		newPanel.show = true;
		newPanel.OnPush();
		Animation animation = newPanel.GetComponent<Animation>();
		if (null != animation && animation.GetClipCount() > 0)
		{
			inputEnabled = false;
			newPanel.OnEnterTransition();
			AnimationState animationState = animation["enter"];
			if (null != animationState)
			{
				animation.Play("enter");
				StartCoroutine(DoAfterEnterTransitionDone(animationState.length));
			}
			else
			{
				animation.Play();
				StartCoroutine(DoAfterEnterTransitionDone(animation.clip.length));
			}
		}
		else if (newPanel.enterTransitionDuration > 0f)
		{
			inputEnabled = false;
			newPanel.OnEnterTransition();
			StartCoroutine(DoAfterEnterTransitionDone(newPanel.enterTransitionDuration));
		}
		else
		{
			inputEnabled = true;
			_currentPanelInTransition = null;
		}
	}

	private IEnumerator DoAfterEnterTransitionDone(float delay)
	{
		yield return new MBWaitForSeconds(delay);   // PORT (CD-059)
		inputEnabled = true;
		_currentPanelInTransition = null;
		if (m_enterTransitionCallback != null)
		{
			m_enterTransitionCallback();
		}
	}

	public bool PushPanelWithTransitions(NBUIMenuPanel newPanel, Callback p_callBack = null)
	{
		if (!newPanel)
		{
			return false;
		}
		if ((bool)_currentPanelInTransition)
		{
			Dbg.LogWarn("Transition queue not supported yet, don't push new panel while the old one is still transitioning.");
			return false;
		}
		m_enterTransitionCallback = p_callBack;
		_currentPanelInTransition = newPanel;
		if (_stack.Count > 0)
		{
			NBUIMenuPanel nBUIMenuPanel = _stack.Peek();
			Animation animation = nBUIMenuPanel.GetComponent<Animation>();
			if (null != animation && animation.GetClipCount() > 0 && null != animation["exit"])
			{
				inputEnabled = false;
				nBUIMenuPanel.OnExitTransition();
				AnimationState animationState = animation["exit"];
				animation.Play("exit");
				StartCoroutine(DoPush(animationState.length, newPanel));
			}
			else if (nBUIMenuPanel.exitTransitionDuration > 0f)
			{
				inputEnabled = false;
				nBUIMenuPanel.OnExitTransition();
				if (base.gameObject.active)
				{
					StartCoroutine(DoPush(nBUIMenuPanel.exitTransitionDuration, newPanel));
				}
				else
				{
					_currentPanelInTransition = null;
					if (m_enterTransitionCallback != null)
					{
						m_enterTransitionCallback();
					}
				}
			}
			else
			{
				StartCoroutine(DoPushNextFrame(newPanel));
			}
		}
		else
		{
			StartCoroutine(DoPushNextFrame(newPanel));
		}
		return true;
	}

	private IEnumerator DoPushNextFrame(NBUIMenuPanel newPanel)
	{
		yield return null;
		PushPanel(newPanel);
	}

	private IEnumerator DoPush(float delay, NBUIMenuPanel newPanel)
	{
		yield return new MBWaitForSeconds(delay);   // PORT (CD-059)
		PushPanel(newPanel);
	}

	private IEnumerator DoPop(float delay, bool showPrevious)
	{
		yield return new MBWaitForSeconds(delay);   // PORT (CD-059)
		PopPanel(showPrevious);
		inputEnabled = true;
	}

	public void ClearStack()
	{
		int count = _stack.Count;
		for (int i = 0; i < count; i++)
		{
			PopPanel(false);
		}
		_stack.Clear();
	}

	public void PopPanel(bool showPrevious = true)
	{
		if (_stack.Count <= 0)
		{
			return;
		}
		NBUIMenuPanel nBUIMenuPanel = _stack.Peek();
		_stack.Pop();
		if ((bool)nBUIMenuPanel)
		{
			nBUIMenuPanel.OnPop();
			nBUIMenuPanel.show = false;
		}
		_currentPanelInTransition = null;
		if (showPrevious && _stack.Count > 0)
		{
			nBUIMenuPanel = _stack.Peek();
			nBUIMenuPanel.show = true;
			Animation animation = nBUIMenuPanel.GetComponent<Animation>();
			if (null != animation && animation.GetClipCount() > 0)
			{
				nBUIMenuPanel.OnEnterTransition();
				_currentPanelInTransition = nBUIMenuPanel;
				AnimationState animationState = animation["enter"];
				if (null != animationState)
				{
					animation.Play("enter");
					StartCoroutine(DoAfterEnterTransitionDone(animationState.length));
				}
				else
				{
					animation.Play();
					StartCoroutine(DoAfterEnterTransitionDone(animation.clip.length));
				}
			}
			else
			{
				nBUIMenuPanel.OnEnterTransition();
			}
		}
		if (m_exitTransitionCallback != null)
		{
			m_exitTransitionCallback();
		}
	}

	public bool PopPanelWithTransitions(bool showPrevious = true, Callback p_exitTransitionCallback = null, Callback p_enterTransitionCallback = null)
	{
		if (_stack.Count <= 0)
		{
			return false;
		}
		if ((bool)_currentPanelInTransition)
		{
			Dbg.Log("Transition queue not supported yet, dont pop while the current panel is still transitioning.");
			return false;
		}
		m_enterTransitionCallback = p_enterTransitionCallback;
		m_exitTransitionCallback = p_exitTransitionCallback;
		NBUIMenuPanel nBUIMenuPanel = (_currentPanelInTransition = _stack.Peek());
		Animation animation = nBUIMenuPanel.GetComponent<Animation>();
		if (null != animation && animation.GetClipCount() > 0 && null != animation["exit"])
		{
			AnimationState animationState = animation["exit"];
			animation.Play("exit");
			inputEnabled = false;
			nBUIMenuPanel.OnExitTransition();
			StartCoroutine(DoPop(animationState.length, showPrevious));
		}
		else if (nBUIMenuPanel.exitTransitionDuration > 0f)
		{
			inputEnabled = false;
			nBUIMenuPanel.OnExitTransition();
			StartCoroutine(DoPop(nBUIMenuPanel.exitTransitionDuration, showPrevious));
		}
		else
		{
			PopPanel(showPrevious);
		}
		return true;
	}

	public int GetCurrentPanelIndex()
	{
		for (int i = 0; i < menuPanels.Length; i++)
		{
			if (currentPanel == menuPanels[i])
			{
				return i;
			}
		}
		return -1;
	}

	public virtual void UpdateRedIconNum()
	{
	}

	public virtual void StandardPanelOnEnablePostProcess()
	{
	}
}
