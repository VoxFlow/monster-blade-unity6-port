using System.Collections.Generic;
using UnityEngine;

public class MonsterSelector : MonoBehaviour
{
	[SerializeField]
	private GameObject defaultWolf;

	public string currentMonsterPrefabPath = string.Empty;

	private GUIContent[] mobComboBoxList = new GUIContent[13]
	{
		new GUIContent("Mob01"),
		new GUIContent("Mob02"),
		new GUIContent("Mob03"),
		new GUIContent("Mob05"),
		new GUIContent("Mob06"),
		new GUIContent("Mob08"),
		new GUIContent("Mob09"),
		new GUIContent("Mob10"),
		new GUIContent("Mob11"),
		new GUIContent("Mob12"),
		new GUIContent("Mob13"),
		new GUIContent("Mob14"),
		new GUIContent("Mob15")
	};

	private ComboBox mobComboBoxControl;

	private GUIContent[] tierComboBoxList = new GUIContent[4]
	{
		new GUIContent("T1"),
		new GUIContent("T2"),
		new GUIContent("T3"),
		new GUIContent("None")
	};

	private ComboBox tierComboBoxControl;

	private GUIContent[] elementComboBoxList = new GUIContent[6]
	{
		new GUIContent("Fire"),
		new GUIContent("Lightning"),
		new GUIContent("Metal"),
		new GUIContent("Nature"),
		new GUIContent("Water"),
		new GUIContent("None")
	};

	private ComboBox elementComboBoxControl;

	private bool showAnimationComboBox;

	private GUIContent[] animationComboBoxList;

	private ComboBox animationComboBoxControl;

	private GUIStyle listStyle = new GUIStyle();

	private GameObject currentMobGo;

	private NBAnimation currentNbAnim;

	private Camera mainCamera;

	private string msg = string.Empty;

	public bool useMouse = true;

	private bool mouseDown;

	private Vector2 initialMousePos = Vector2.zero;

	private Vector2 currentMousePos = Vector2.zero;

	private float currentMouseWheel;

	private bool twoFingerTap;

	private Vector2 initFingerPosDelta = Vector2.zero;

	private Vector3 cameraToMob = Vector3.zero;

	private Dictionary<string, string> mobVsCameraNameMap = new Dictionary<string, string>();

	private string[] mobObjNames = new string[16]
	{
		"Mob01", "Mob02", "Mob03", "Mob04", "Mob05", "Mob06", "Mob07", "Mob08", "Mob09", "Mob10",
		"Mob11", "Mob12", "Mob13", "Mob14", "Mob15", "Mob16"
	};

	private string[] mobCameraNames = new string[16]
	{
		"Bull Camera",
		"Rhino 1 Camera",
		"Wolf 1 Camera",
		string.Empty,
		"Hydra Cam",
		"Land Dragon Camera",
		string.Empty,
		"Wing Dragon 1 Camera",
		"Raptor Camera",
		"Winged Serpent Camera",
		"Dodobo 1 Camera",
		"Lizard 1 Camera",
		"Shark 1 Camera",
		"Rhino 2 Camera",
		"Lizard 1 Camera",
		"Winged Serpent 1 Camera"
	};

	private void OnGUI()
	{
		mobComboBoxControl.Show();
		tierComboBoxControl.Show();
		elementComboBoxControl.Show();
		if (showAnimationComboBox)
		{
			animationComboBoxControl.Show();
			if (GUI.Button(new Rect(Screen.width - 450, 0f, 150f, 40f), "Play Anim") && currentNbAnim != null)
			{
				currentNbAnim.Play(animationComboBoxList[animationComboBoxControl.SelectedItemIndex].text);
			}
		}
		GUI.Label(new Rect(Screen.width - 300, 42f, 300f, 50f), msg);
		if (GUI.Button(new Rect(Screen.width - 300, 0f, 150f, 40f), "Load Mob"))
		{
			if (defaultWolf != null)
			{
				defaultWolf.SetActiveRecursively(false);
			}
			if (currentMobGo != null)
			{
				Object.DestroyImmediate(currentMobGo);
			}
			string text = "Enemies/" + mobComboBoxList[mobComboBoxControl.SelectedItemIndex].text + "/Prefabs/" + mobComboBoxList[mobComboBoxControl.SelectedItemIndex].text;
			if (elementComboBoxList[elementComboBoxControl.SelectedItemIndex].text != "None")
			{
				text = text + "_" + elementComboBoxList[elementComboBoxControl.SelectedItemIndex].text;
			}
			if (tierComboBoxList[tierComboBoxControl.SelectedItemIndex].text != "None")
			{
				text = text + "_" + tierComboBoxList[tierComboBoxControl.SelectedItemIndex].text;
			}
			text = (currentMonsterPrefabPath = text + "_Prefab");
			Object obj = Resources.Load(text);
			if (obj == null)
			{
				Debug.LogWarning("The mob might have no element and tier");
				text = "Enemies/" + mobComboBoxList[mobComboBoxControl.SelectedItemIndex].text + "/Prefabs/" + mobComboBoxList[mobComboBoxControl.SelectedItemIndex].text + "_Prefab";
				elementComboBoxControl.SelectedItemIndex = elementComboBoxControl.LastIndex;
				tierComboBoxControl.SelectedItemIndex = tierComboBoxControl.LastIndex;
				currentMonsterPrefabPath = text;
				obj = Resources.Load(text);
			}
			if (obj == null)
			{
				Debug.LogWarning("Unable to find : " + text);
				msg = "Unable to find : " + text;
				showAnimationComboBox = false;
			}
			else
			{
				currentMobGo = Object.Instantiate(obj) as GameObject;
				if (currentMobGo != null)
				{
					currentMobGo.transform.localPosition = new Vector3(0f, 0f, 7f);
					Debug.Log("Load game object from : " + text);
					msg = "Load game object from : " + text;
					currentNbAnim = currentMobGo.GetComponent<NBAnimation>();
					if (currentNbAnim != null)
					{
						Debug.Log("Get NBAnimation!");
						animationComboBoxList = new GUIContent[currentNbAnim.AnimationList.Length];
						int num = 0;
						string[] animationList = currentNbAnim.AnimationList;
						foreach (string text2 in animationList)
						{
							animationComboBoxList[num] = new GUIContent(text2);
							num++;
						}
						showAnimationComboBox = true;
						animationComboBoxControl = new ComboBox(new Rect(Screen.width - 450, 80f, 150f, 30f), animationComboBoxList[0], animationComboBoxList, "button", "box", listStyle);
					}
				}
			}
		}
		if (GUI.Button(new Rect(Screen.width - 150, 0f, 150f, 40f), "Unload Mob") && currentMobGo != null)
		{
			Object.DestroyImmediate(currentMobGo);
			showAnimationComboBox = false;
			currentMonsterPrefabPath = string.Empty;
			animationComboBoxList = null;
		}
	}

	private void Start()
	{
		InitialComboListUi();
		GenerateMobCameraMap();
		GameObject gameObject = GameObject.Find("Main Camera");
		if (gameObject != null)
		{
			mainCamera = gameObject.GetComponent<Camera>();
		}
	}

	private void Update()
	{
		if (!(mainCamera != null) || !(currentMobGo != null))
		{
			return;
		}
		if (Input.GetMouseButtonDown(0))
		{
			if (!mouseDown)
			{
				if (mainCamera != null)
				{
					mainCamera.transform.LookAt(currentMobGo.transform.position);
				}
				initialMousePos = Input.mousePosition;
				mouseDown = true;
			}
		}
		else if (Input.GetMouseButtonUp(0))
		{
			mouseDown = false;
			initialMousePos = Vector2.zero;
			currentMousePos = Vector2.zero;
		}
		if (Input.touchCount == 1 || (useMouse && mouseDown))
		{
			currentMousePos = Input.mousePosition;
			Vector2 vector = Vector2.zero;
			if (mouseDown)
			{
				vector = currentMousePos - initialMousePos;
			}
			else if (Input.touchCount == 1)
			{
				vector = Input.GetTouch(0).deltaPosition;
			}
			mainCamera.transform.RotateAround(currentMobGo.transform.position, Vector3.up, vector.x * 0.25f);
			mainCamera.transform.RotateAround(currentMobGo.transform.position, Vector3.right, vector.y * -0.1f);
			initialMousePos = Input.mousePosition;
		}
		if (useMouse)
		{
			currentMouseWheel = Input.GetAxis("Mouse ScrollWheel");
			cameraToMob = currentMobGo.transform.position - mainCamera.transform.position;
			if (cameraToMob.magnitude > 7f)
			{
				mainCamera.transform.localPosition += cameraToMob.normalized * currentMouseWheel * 3f;
			}
			else
			{
				mainCamera.transform.localPosition -= cameraToMob.normalized * currentMouseWheel * 6f;
			}
		}
		else if (Input.touchCount == 2)
		{
			if (!twoFingerTap)
			{
				twoFingerTap = true;
				initFingerPosDelta = Input.GetTouch(0).position - Input.GetTouch(1).position;
			}
			if (twoFingerTap)
			{
				cameraToMob = currentMobGo.transform.position - mainCamera.transform.position;
				if (cameraToMob.magnitude > 7f)
				{
					Vector2 vector2 = Input.GetTouch(0).position - Input.GetTouch(1).position;
					mainCamera.transform.localPosition += cameraToMob.normalized * (vector2.magnitude - initFingerPosDelta.magnitude) * 0.005f;
					initFingerPosDelta = Input.GetTouch(0).position - Input.GetTouch(1).position;
				}
				else
				{
					Debug.Log("toooooo near");
					mainCamera.transform.localPosition -= cameraToMob.normalized * 0.03f;
				}
			}
		}
		else
		{
			twoFingerTap = false;
			initFingerPosDelta = Vector2.zero;
		}
	}

	private void InitialComboListUi()
	{
		listStyle.normal.textColor = Color.white;
		listStyle.hover.background = new Texture2D(2, 2);
		listStyle.padding.top = 8;
		listStyle.padding.left = 8;
		listStyle.padding.right = 4;
		listStyle.padding.bottom = 8;
		mobComboBoxControl = new ComboBox(new Rect(Screen.width - 300, 80f, 100f, 30f), mobComboBoxList[0], mobComboBoxList, "button", "box", listStyle);
		tierComboBoxControl = new ComboBox(new Rect(Screen.width - 200, 80f, 100f, 30f), tierComboBoxList[0], tierComboBoxList, "button", "box", listStyle);
		elementComboBoxControl = new ComboBox(new Rect(Screen.width - 100, 80f, 100f, 30f), elementComboBoxList[0], elementComboBoxList, "button", "box", listStyle);
	}

	private void GenerateMobCameraMap()
	{
		for (int i = 0; i < mobObjNames.Length; i++)
		{
			mobVsCameraNameMap.Add(mobObjNames[i], mobCameraNames[i]);
		}
	}
}
