using System.Collections;
using UnityEngine;

[RequireComponent(typeof(UIButton))]
public class UIButtonMove : MonoBehaviour
{
	public Vector3 startPosition;

	public float startDelay;

	public float enterDuration;

	public float exitDuration;

	public UITweener.Method enterEaseType;

	public UITweener.Method exitEaseType;

	private Vector3 _initPosition;

	private UIButton _button;

	private void Awake()
	{
		_initPosition = base.transform.localPosition;
		base.transform.localPosition = startPosition;
		_button = GetComponent<UIButton>();
	}

	private void Start()
	{
	}

	private void OnDisable()
	{
		Reset();
	}

	private void Reset()
	{
		base.transform.localPosition = startPosition;
	}

	private void OnEnter()
	{
		if ((bool)base.GetComponent<Collider>())
		{
			base.GetComponent<Collider>().enabled = false;
		}
		if (startDelay > 0f)
		{
			StartCoroutine(DoEnterAnimation(startDelay));
		}
		else
		{
			EnterAnimation();
		}
	}

	private IEnumerator DoEnterAnimation(float delay)
	{
		yield return new MBWaitForSeconds(delay);   // PORT (CD-059)
		EnterAnimation();
	}

	private void EnterAnimation()
	{
		TweenPosition tweenPosition = TweenPosition.Begin(base.gameObject, enterDuration, _initPosition);
		tweenPosition.method = enterEaseType;
		tweenPosition.eventReceiver = base.gameObject;
		tweenPosition.callWhenFinished = "OnEnterDone";
	}

	private void OnEnterDone()
	{
		if ((bool)base.GetComponent<Collider>() && !_button.isLocked)
		{
			base.GetComponent<Collider>().enabled = true;
		}
	}

	private void OnExit()
	{
		if ((bool)base.GetComponent<Collider>())
		{
			base.GetComponent<Collider>().enabled = false;
		}
		if (startDelay > 0f)
		{
			StartCoroutine(DoExitAnimation(startDelay));
		}
		else
		{
			ExitAnimation();
		}
	}

	private void ExitAnimation()
	{
		TweenPosition tweenPosition = TweenPosition.Begin(base.gameObject, exitDuration, startPosition);
		tweenPosition.method = exitEaseType;
	}

	private IEnumerator DoExitAnimation(float delay)
	{
		yield return new MBWaitForSeconds(delay);   // PORT (CD-059)
		ExitAnimation();
	}
}
