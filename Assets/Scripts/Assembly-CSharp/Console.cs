using System;
using System.Collections.Generic;
using UnityEngine;

public class Console : MonoBehaviour
{
	private struct ConsoleMessage
	{
		public readonly string message;

		public readonly string stackTrace;

		public readonly LogType type;

		public ConsoleMessage(string message, string stackTrace, LogType type)
		{
			this.message = message;
			this.stackTrace = stackTrace;
			this.type = type;
		}
	}

	private const int margin = 20;

	public static readonly Version version = new Version(1, 0);

	public KeyCode toggleKey = KeyCode.BackQuote;

	private List<ConsoleMessage> entries = new List<ConsoleMessage>();

	private Vector2 scrollPos;

	private bool show = true;

	private bool collapse;

	private bool freeze;

	private float fContentHeight;

	// PORT (CD-027)
	private Rect windowRect;

	// PORT (CD-027)
	private Rect scrollArea;

	private GUIContent clearLabel = new GUIContent("Clear", "Clear the contents of the console.");

	private GUIContent collapseLabel = new GUIContent("Collapse", "Hide repeated messages.");

	private GUIContent freezeLabel = new GUIContent("Freeze", "No autoscrolling");

	private void OnEnable()
	{
		Application.RegisterLogCallback(HandleLog);
	}

	private void OnDisable()
	{
		Application.RegisterLogCallback(null);
	}

	private void Start()
	{
		scrollArea = new Rect(Screen.width - 100, 0f, 100f, Screen.height);
		windowRect = new Rect(20f, 20f, Screen.width - 40, Screen.height - 40);
	}

	private void Update()
	{
		if (Input.GetKeyDown(toggleKey))
		{
			show = !show;
		}
		if (Input.touchCount > 0)
		{
			Touch touch = Input.touches[0];
			if (scrollArea.Contains(touch.position))
			{
				scrollPos = touch.position;
			}
		}
	}

	private void ConsoleWindow(int windowID)
	{
		fContentHeight = 0f;
		scrollPos = GUILayout.BeginScrollView(scrollPos);
		for (int i = 0; i < entries.Count; i++)
		{
			ConsoleMessage consoleMessage = entries[i];
			if (!collapse || i <= 0 || !(consoleMessage.message == entries[i - 1].message))
			{
				switch (consoleMessage.type)
				{
				case LogType.Error:
				case LogType.Exception:
					GUI.contentColor = Color.red;
					break;
				case LogType.Warning:
					GUI.contentColor = Color.yellow;
					break;
				default:
					GUI.contentColor = Color.white;
					break;
				}
				GUILayout.Label(consoleMessage.message);
				fContentHeight += GUILayoutUtility.GetLastRect().y;
			}
		}
		GUI.contentColor = Color.white;
		GUILayout.EndScrollView();
		while (!freeze && scrollPos.y < fContentHeight)
		{
			scrollPos.y += 100f;
		}
		GUILayout.BeginHorizontal();
		if (GUILayout.Button(clearLabel))
		{
			entries.Clear();
		}
		collapse = GUILayout.Toggle(collapse, collapseLabel, GUILayout.ExpandWidth(false));
		freeze = GUILayout.Toggle(freeze, freezeLabel, GUILayout.ExpandWidth(false));
		GUILayout.EndHorizontal();
		GUI.DragWindow(new Rect(0f, 0f, 10000f, 20f));
	}

	private void HandleLog(string message, string stackTrace, LogType type)
	{
		ConsoleMessage item = new ConsoleMessage(message, stackTrace, type);
		entries.Add(item);
	}
}
