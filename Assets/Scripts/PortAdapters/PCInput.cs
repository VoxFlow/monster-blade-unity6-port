// PC controls: the mouse as one finger, keyboard shortcuts for the battle controls, all delivered as touch. Register CD-029.
using System;
using UnityEngine;

[DefaultExecutionOrder(-31990)]
public class PCInput : MonoBehaviour
{
	private TouchInjector m_keys;

	private bool m_mouseDown;

	private Vector2 m_mouseLast;

	private KeyCode m_heldKey = KeyCode.None;

	[RuntimeInitializeOnLoadMethod(RuntimeInitializeLoadType.BeforeSceneLoad)]
	private static void Boot()
	{
		MBInput.TouchPlatform = true;
		GameObject go = new GameObject("[PCInput]");
		DontDestroyOnLoad(go);
		go.AddComponent<PCInput>();
	}

	private void Awake()
	{
		m_keys = gameObject.AddComponent<TouchInjector>();
	}

	private void Update()
	{
		Keys();
		Vector2 pos = Input.mousePosition;
		TouchPhase phase;
		if (Input.GetMouseButtonDown(0))
		{
			phase = TouchPhase.Began;
			m_mouseDown = true;
		}
		else if (m_mouseDown && Input.GetMouseButtonUp(0))
		{
			phase = TouchPhase.Ended;
			m_mouseDown = false;
		}
		else if (m_mouseDown && Input.GetMouseButton(0))
		{
			phase = pos != m_mouseLast ? TouchPhase.Moved : TouchPhase.Stationary;
		}
		else
		{
			m_mouseDown = false;
			return;
		}
		Touch t = new Touch();
		t.fingerId = 0;
		t.position = pos;
		t.rawPosition = pos;
		t.deltaPosition = phase == TouchPhase.Began ? Vector2.zero : pos - m_mouseLast;
		t.deltaTime = Time.unscaledDeltaTime;
		t.tapCount = 1;
		t.phase = phase;
		m_mouseLast = pos;
		MBInput.SetTouches(new Touch[] { t });
	}

	private void Keys()
	{
		if (m_heldKey != KeyCode.None)
		{
			if (!Input.GetKey(m_heldKey))
			{
				m_heldKey = KeyCode.None;
				m_keys.Release();
			}
			return;
		}
		if (m_keys.Busy || m_mouseDown)
		{
			return;
		}
		if (Input.GetKeyDown(KeyCode.Tab))
		{
			CutsceneController cc = UnityEngine.Object.FindAnyObjectByType<CutsceneController>();
			Hold(cc != null ? cc.m_speedUpBtn : null, KeyCode.Tab);
			return;
		}
		BattleScreenUI ui = UnityEngine.Object.FindAnyObjectByType<BattleScreenUI>();
		if (ui == null || !ui.isActiveAndEnabled)
		{
			return;
		}
		if (Input.GetKeyDown(KeyCode.LeftArrow)) { Hold(ui.m_dodgeButtons[0] != null ? ui.m_dodgeButtons[0].gameObject : null, KeyCode.LeftArrow); return; }
		if (Input.GetKeyDown(KeyCode.RightArrow)) { Hold(ui.m_dodgeButtons[1] != null ? ui.m_dodgeButtons[1].gameObject : null, KeyCode.RightArrow); return; }
		if (Input.GetKeyDown(KeyCode.Space)) { Hold(ui.m_blockButton != null ? ui.m_blockButton.gameObject : null, KeyCode.Space); return; }
		if (Input.GetKeyDown(KeyCode.UpArrow)) { Tap(ui.m_superButton != null ? ui.m_superButton.gameObject : null); return; }
		if (Input.GetKeyDown(KeyCode.F)) { Tap(ui.m_assistButton != null ? ui.m_assistButton.gameObject : null); return; }
		Vector2 from = Vector2.zero;
		if (Input.GetKeyDown(KeyCode.W)) from = new Vector2(0f, 1f);
		else if (Input.GetKeyDown(KeyCode.X)) from = new Vector2(0f, -1f);
		else if (Input.GetKeyDown(KeyCode.A)) from = new Vector2(-1f, 0f);
		else if (Input.GetKeyDown(KeyCode.D)) from = new Vector2(1f, 0f);
		else if (Input.GetKeyDown(KeyCode.Q)) from = new Vector2(-1f, 1f);
		else if (Input.GetKeyDown(KeyCode.E)) from = new Vector2(1f, 1f);
		else if (Input.GetKeyDown(KeyCode.Z)) from = new Vector2(-1f, -1f);
		else if (Input.GetKeyDown(KeyCode.C)) from = new Vector2(1f, -1f);
		if (from != Vector2.zero)
		{
			Vector2 mid = new Vector2(Screen.width * 0.5f, Screen.height * 0.55f);
			Vector2 r = from.normalized * Screen.height * 0.15f;
			m_keys.Swipe(mid - r, mid + r);
		}
	}

	private void Hold(GameObject button, KeyCode key)
	{
		Vector2 pos;
		string why;
		if (MBScreen.Pos(button, out pos, out why))
		{
			m_heldKey = key;
			m_keys.Hold(pos);
		}
	}

	private void Tap(GameObject button)
	{
		Vector2 pos;
		string why;
		if (MBScreen.Pos(button, out pos, out why))
		{
			m_keys.Tap(pos);
		}
	}
}
