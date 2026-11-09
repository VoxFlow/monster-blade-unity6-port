using System.Collections;
using UnityEngine;

public class PetModel : MonoBehaviour
{
	[SerializeField]
	private bool m_useMouse = true;

	[SerializeField]
	private bool m_useTouch = true;

	[SerializeField]
	private float m_rotateMag = 0.3f;

	[SerializeField]
	private float m_outtroMoveDisPerFrame = 0.2f;

	[SerializeField]
	private float m_introMoveDisPerFrame = 0.2f;

	// PORT (CD-027)
	private Rect m_senseRange;

	private Vector2 m_lastTouchPos = new Vector2(-1f, -1f);

	private float m_xDiff;

	private bool m_mouseDownInRange;

	private int m_flingCounter;

	private float m_timer;

	private float m_lastxDiff;

	private float m_setLastXDiffTimer;

	private bool m_switchingPet;

	private NBAnimation m_nbAnim;

	private Hashtable m_rotArgs;

	private int m_tapCounter;

	private float m_tapAngryTimer;

	public bool m_isBored;

	public Transform m_cachedTransform;

	public float playRndIdleAnimTimer;

	private bool isPlayingDizzy;

	private bool feedSoundPlayed;

	private string originalAnimStr = string.Empty;

	private float accumRot;

	private float accumRotDizzyTimer;

	private bool needToCountTime;

	private readonly float dizzyTimeLimit = 7f;

	private string tapAnim = string.Empty;

	private bool rotating;

	private void Awake()
	{
		m_senseRange = new Rect((float)Screen.width * 0.3125f, (float)Screen.height * 0.2f, (float)Screen.width * 0.3542f, (float)Screen.height * 0.675f);
		m_cachedTransform = base.transform;
	}

	private void Start()
	{
		m_senseRange = new Rect((float)Screen.width * 0.3125f, (float)Screen.height * 0.2f, (float)Screen.width * 0.3542f, (float)Screen.height * 0.675f);
		m_nbAnim = GetComponent<NBAnimation>();
		if (m_rotArgs == null)
		{
			m_rotArgs = new Hashtable();
		}
		if (!m_rotArgs.ContainsKey("rotation"))
		{
			m_rotArgs.Add("rotation", new Vector3(0f, 90f, 0f));
		}
		if (!m_rotArgs.ContainsKey("time"))
		{
			m_rotArgs.Add("time", 0.8f);
		}
		if (!m_rotArgs.ContainsKey("oncomplete"))
		{
			m_rotArgs.Add("oncomplete", "OnOuttroRotComplete");
		}
		m_tapAngryTimer = 0f;
		m_tapCounter = 0;
	}

	private void CheckFling()
	{
	}

	public void DisableSenseMouseAndTouch()
	{
		m_useMouse = false;
		m_useTouch = false;
	}

	public void EnableSenseMouseAndTouch()
	{
		m_useMouse = true;
		m_useTouch = true;
	}

	private void FingerRotateModel(float p_xDiff)
	{
	}

	private void PreTapAnimRotate(string p_anim)
	{
	}

	private void OnPreTapRotComplete()
	{
		rotating = false;
		if (!string.IsNullOrEmpty(tapAnim))
		{
			m_nbAnim.Play(tapAnim);
		}
		tapAnim = string.Empty;
	}

	public void PlayAnim(string p_anim)
	{
	}
}
