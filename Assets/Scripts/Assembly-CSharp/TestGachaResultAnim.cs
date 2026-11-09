using UnityEngine;

public class TestGachaResultAnim : MonoBehaviour
{
	public AnimatedUVs uvBackground;

	public GameObject particles;

	public GameObject particles2;

	public GameObject background1;

	public GameObject spriteHeader;

	public UISprite spriteFooter;

	public UISprite spriteStats;

	public UISprite spriteResistance;

	public ItemInfo itemInfo;

	public FrameRateGui fpsGui;

	public MemoryInfo memInfo;

	public GameObject m_attachGachaHere;

	public bool hideGui;

	public bool skipTouch;

	private bool _uvBackgroundFlag = true;

	private bool _background1Flag = true;

	private bool _headerFooterFlag = true;

	private bool _itemInfoFlag = true;

	private bool _particlesFlag = true;

	private bool _particles2Flag = true;

	private bool _fpsGuiFlag = true;

	private bool _memInfoFlag = true;

	private string _uvBackgroundStr;

	private string _background1Str;

	private string _headerFooterStr;

	private string _itemInfoStr;

	private string _particlesStr;

	private string _particles2Str;

	private string _fpsGuiStr;

	private string _memInfoStr;

	private GameObject m_gachaObject;

	private void Start()
	{
		SetUpGachaAnimation();
		_uvBackgroundStr = "UV Background " + ((!_uvBackgroundFlag) ? "Off" : "On");
		_background1Str = "Background " + ((!_background1Flag) ? "Off" : "On");
		_headerFooterStr = "Header/Footer " + ((!_headerFooterFlag) ? "Off" : "On");
		_itemInfoStr = "Item Info " + ((!_itemInfoFlag) ? "Off" : "On");
		_particlesStr = "Element Particles " + ((!_particlesFlag) ? "Off" : "On");
		_particles2Str = "Common Particles " + ((!_particles2Flag) ? "Off" : "On");
		_fpsGuiStr = "Fps GUI " + ((!_fpsGuiFlag) ? "Off" : "On");
		_memInfoStr = "Memory Info " + ((!_memInfoFlag) ? "Off" : "On");
	}

	private void SetUpGachaAnimation()
	{
		if (m_gachaObject != null)
		{
			Object.Destroy(m_gachaObject);
			m_gachaObject = null;
		}
		if (!(m_attachGachaHere != null))
		{
			return;
		}
		GameObject gameObject = Resources.Load(GLOBAL.GACHA_RESULT_ANIM_PATH) as GameObject;
		if (gameObject != null)
		{
			m_gachaObject = Object.Instantiate(gameObject, Vector3.zero, Quaternion.identity) as GameObject;
			m_gachaObject.transform.parent = m_attachGachaHere.GetComponent<UIPanel>().transform;
			m_gachaObject.transform.localScale = Vector3.one;
			m_gachaObject.transform.localPosition = Vector3.zero;
			Transform transform = m_gachaObject.transform.Find("BG_EnergyFX");
			if ((bool)transform)
			{
				uvBackground = transform.GetComponent<AnimatedUVs>();
			}
		}
	}

	private void OnGUI()
	{
		if (!hideGui)
		{
			GUILayout.BeginArea(new Rect(50f, 50f, 800f, 500f));
			GUILayout.BeginVertical();
			if (GUILayout.Button("Reset", GUILayout.Width(250f), GUILayout.Height(40f)))
			{
				SetUpGachaAnimation();
			}
			GUILayout.Space(10f);
			if (GUILayout.Button(_uvBackgroundStr, GUILayout.Width(250f), GUILayout.Height(40f)))
			{
				_uvBackgroundFlag = !_uvBackgroundFlag;
				_uvBackgroundStr = "UV Background " + ((!_uvBackgroundFlag) ? "Off" : "On");
				uvBackground.gameObject.active = _uvBackgroundFlag;
			}
			GUILayout.Space(10f);
			if (GUILayout.Button(_background1Str, GUILayout.Width(250f), GUILayout.Height(40f)))
			{
				_background1Flag = !_background1Flag;
				_background1Str = "Background " + ((!_background1Flag) ? "Off" : "On");
				background1.gameObject.active = _background1Flag;
			}
			GUILayout.Space(10f);
			if (GUILayout.Button(_headerFooterStr, GUILayout.Width(250f), GUILayout.Height(40f)))
			{
				_headerFooterFlag = !_headerFooterFlag;
				_headerFooterStr = "Header/Footer " + ((!_headerFooterFlag) ? "Off" : "On");
				spriteHeader.gameObject.active = _headerFooterFlag;
				spriteFooter.gameObject.active = _headerFooterFlag;
				spriteResistance.gameObject.active = _headerFooterFlag;
				spriteStats.gameObject.active = _headerFooterFlag;
			}
			GUILayout.Space(10f);
			if (GUILayout.Button(_itemInfoStr, GUILayout.Width(250f), GUILayout.Height(40f)))
			{
				_itemInfoFlag = !_itemInfoFlag;
				_itemInfoStr = "Item Info " + ((!_itemInfoFlag) ? "Off" : "On");
				itemInfo.gameObject.SetActiveRecursively(_itemInfoFlag);
			}
			GUILayout.Space(10f);
			if (GUILayout.Button(_particlesStr, GUILayout.Width(250f), GUILayout.Height(40f)))
			{
				_particlesFlag = !_particlesFlag;
				_particlesStr = "Element Particles " + ((!_particlesFlag) ? "Off" : "On");
				particles.gameObject.SetActiveRecursively(_particlesFlag);
			}
			GUILayout.Space(10f);
			if (GUILayout.Button(_particles2Str, GUILayout.Width(250f), GUILayout.Height(40f)))
			{
				_particles2Flag = !_particles2Flag;
				_particles2Str = "Common Particles " + ((!_particles2Flag) ? "Off" : "On");
				particles2.gameObject.SetActiveRecursively(_particles2Flag);
			}
			GUILayout.Space(10f);
			if (GUILayout.Button(_fpsGuiStr, GUILayout.Width(250f), GUILayout.Height(40f)))
			{
				_fpsGuiFlag = !_fpsGuiFlag;
				_fpsGuiStr = "Fps GUI " + ((!_fpsGuiFlag) ? "Off" : "On");
				fpsGui.enabled = _fpsGuiFlag;
			}
			GUILayout.Space(10f);
			if (GUILayout.Button(_memInfoStr, GUILayout.Width(250f), GUILayout.Height(40f)))
			{
				_memInfoFlag = !_memInfoFlag;
				_memInfoStr = "Memory Info " + ((!_memInfoFlag) ? "Off" : "On");
				memInfo.enabled = _memInfoFlag;
			}
			GUILayout.Space(10f);
			if (GUILayout.Button("Hide GUI", GUILayout.Width(250f), GUILayout.Height(40f)))
			{
				hideGui = true;
				skipTouch = true;
			}
			GUILayout.Space(10f);
			GUILayout.EndVertical();
			GUILayout.EndArea();
		}
	}

	private void Update()
	{
		if (hideGui)
		{
			if (skipTouch)
			{
				skipTouch = false;
			}
			else if (Input.touchCount > 0 && Input.GetTouch(0).phase == TouchPhase.Ended)
			{
				hideGui = false;
			}
		}
	}
}
