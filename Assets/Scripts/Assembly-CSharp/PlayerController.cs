using UnityEngine;

public class PlayerController : MonoBehaviour
{
	public enum Button
	{
		None = -1,
		LeftArrow = 0,
		RightArrow = 1,
		Shield = 2
	}

	private Player m_player;

	private BattleManager m_battleManager;

	private Button m_prevButtonState = Button.None;

	private Button m_currButtonState = Button.None;

	public float m_inputSensitivity = 0.25f;

	private NBTimer m_atkRecognitionTimer;

	private float m_atkHoldTime;

	public float m_heavyAtkRecognitionTime = 0.25f;

	private Pointer m_pointer;

	private Vector2 m_swipeVec = Vector2.zero;

	public float m_minRecognitionDistance = 40f;

	public float m_maxRecognitionDistance = 60f;

	public float m_smallMoveDistance = 5f;

	private float m_stationaryTime;

	public float m_maxStationaryTime = 0.1f;

	private bool m_isInputConsume;

	private bool m_enableDodge = true;

	private bool m_enableBlock = true;

	private bool m_enableAttack = true;

	public bool DodgeEnable
	{
		get
		{
			return m_enableDodge;
		}
		set
		{
			m_enableDodge = value;
			Messenger<bool>.Invoke("enable_dodge", value);
		}
	}

	public bool BlockEnable
	{
		get
		{
			return m_enableBlock;
		}
		set
		{
			m_enableBlock = value;
			Messenger<bool>.Invoke("enable_block", value);
		}
	}

	public bool AttackEnable
	{
		get
		{
			return m_enableAttack;
		}
		set
		{
			m_enableAttack = value;
			SuperAttackSystem instance = SingletonComponent<SuperAttackSystem>.Instance;
			if (null != instance && null != instance.superAttackLogic)
			{
				if (m_enableAttack)
				{
					instance.superAttackLogic.data.EnableSkill(this);
				}
				else
				{
					instance.superAttackLogic.data.DisableSkill(this);
				}
			}
			SkillSystem instance2 = SingletonComponent<SkillSystem>.Instance;
			if (null != instance2 && null != instance2.friendSkill)
			{
				if (m_enableAttack)
				{
					instance2.friendSkill.skillData.EnableSkill(this);
				}
				else
				{
					instance2.friendSkill.skillData.DisableSkill(this);
				}
			}
		}
	}

	public Pointer Pointer
	{
		get
		{
			return m_pointer;
		}
	}

	private void Awake()
	{
		m_pointer = base.gameObject.AddComponent<Pointer>();
		m_atkRecognitionTimer = base.gameObject.AddComponent<NBTimer>();
		Messenger.AddListener("player_dead", OnDead);
		Messenger.AddListener("battle_init", OnPause);
		Messenger.AddListener("battle_start", OnUnpause);
		Messenger.AddListener("battle_end", OnPause);
		Messenger<CutsceneController.CutsceneType>.AddListener(CutsceneMessage.CUTSCENE_START, OnCutsceneStart);
		Messenger<CutsceneController.CutsceneType>.AddListener(CutsceneMessage.CUTSCENE_END, OnCutsceneEnd);
	}

	private void Start()
	{
		m_player = base.gameObject.GetComponent<Player>();
		m_pointer.m_id = 0;
		m_minRecognitionDistance *= m_minRecognitionDistance;
		m_maxRecognitionDistance *= m_maxRecognitionDistance;
		m_smallMoveDistance *= m_smallMoveDistance;
	}

	private void OnDestroy()
	{
		Messenger.RemoveListener("player_dead", OnDead);
		Messenger.RemoveListener("battle_init", OnPause);
		Messenger.RemoveListener("battle_start", OnUnpause);
		Messenger.RemoveListener("battle_end", OnPause);
		Messenger<CutsceneController.CutsceneType>.RemoveListener(CutsceneMessage.CUTSCENE_START, OnCutsceneStart);
		Messenger<CutsceneController.CutsceneType>.RemoveListener(CutsceneMessage.CUTSCENE_END, OnCutsceneEnd);
	}

	private PlayerAttack.Direction EvaluateAttack(Vector2 p_pointerMoveVec)
	{
		float num = Mathf.Atan2(p_pointerMoveVec.y, p_pointerMoveVec.x) * 57.29578f;
		if (num > -165f && num <= -105f)
		{
			return PlayerAttack.Direction.TopRightToBottomLeft;
		}
		if (num > -105f && num <= -75f)
		{
			return PlayerAttack.Direction.TopToBottom;
		}
		if (num > -75f && num <= -15f)
		{
			return PlayerAttack.Direction.TopLeftToBottomRight;
		}
		if (num > -15f && num <= 15f)
		{
			return PlayerAttack.Direction.LeftToRight;
		}
		if (num > 15f && num <= 75f)
		{
			return PlayerAttack.Direction.BottomLeftToTopRight;
		}
		if (num > 75f && num <= 105f)
		{
			return PlayerAttack.Direction.BottomToTop;
		}
		if (num > 105f && num <= 165f)
		{
			return PlayerAttack.Direction.BottomRightToTopLeft;
		}
		return PlayerAttack.Direction.RightToLeft;
	}

	private void InterpretPointerMoved()
	{
		if (m_enableAttack)
		{
			Messenger<PlayerAttack.Direction>.Invoke("attack_swipe_interpreted", EvaluateAttack(m_swipeVec.normalized));
			m_stationaryTime = 0f;
			m_atkHoldTime = 0f;
			m_swipeVec = Vector2.zero;
			m_isInputConsume = true;
			m_atkRecognitionTimer.CountDown(m_inputSensitivity);
		}
	}

	private void InterpretPointerInput()
	{
		if (m_pointer.m_state == Pointer.State.None)
		{
			return;
		}
		if (m_pointer.m_state == Pointer.State.Down)
		{
			m_swipeVec = Vector2.zero;
		}
		else if (m_pointer.m_state == Pointer.State.Held)
		{
			m_stationaryTime += Time.deltaTime;
			m_atkHoldTime += Time.deltaTime;
			if (!m_isInputConsume && m_stationaryTime >= m_maxStationaryTime && m_swipeVec.sqrMagnitude >= m_minRecognitionDistance)
			{
				InterpretPointerMoved();
			}
		}
		else if (m_pointer.m_state == Pointer.State.Move)
		{
			if (m_pointer.MoveVector.sqrMagnitude > m_smallMoveDistance)
			{
				m_swipeVec += m_pointer.MoveVector;
			}
			if (m_swipeVec.sqrMagnitude >= m_minRecognitionDistance && !m_isInputConsume)
			{
				InterpretPointerMoved();
			}
		}
		else if (m_pointer.m_state == Pointer.State.Up || m_pointer.m_state == Pointer.State.None)
		{
			m_stationaryTime = 0f;
			m_atkHoldTime = 0f;
			m_isInputConsume = false;
		}
	}

	private void InterpretKeyboardInput()
	{
		if (m_enableAttack)
		{
			PlayerAttack.Direction direction = PlayerAttack.Direction.None;
			if (Input.GetKeyDown("x"))
			{
				direction = PlayerAttack.Direction.BottomToTop;
			}
			else if (Input.GetKeyDown("w"))
			{
				direction = PlayerAttack.Direction.TopToBottom;
			}
			else if (Input.GetKeyDown("d"))
			{
				direction = PlayerAttack.Direction.RightToLeft;
			}
			else if (Input.GetKeyDown("a"))
			{
				direction = PlayerAttack.Direction.LeftToRight;
			}
			else if (Input.GetKeyDown("q"))
			{
				direction = PlayerAttack.Direction.TopLeftToBottomRight;
			}
			else if (Input.GetKeyDown("e"))
			{
				direction = PlayerAttack.Direction.TopRightToBottomLeft;
			}
			else if (Input.GetKeyDown("z"))
			{
				direction = PlayerAttack.Direction.BottomLeftToTopRight;
			}
			else if (Input.GetKeyDown("c"))
			{
				direction = PlayerAttack.Direction.BottomRightToTopLeft;
			}
			if (direction != PlayerAttack.Direction.None)
			{
				m_atkRecognitionTimer.CountDown(m_inputSensitivity);
				Messenger<PlayerAttack.Direction>.Invoke("attack_swipe_interpreted", direction);
			}
		}
	}

	private bool IsButtonPressed()
	{
		return m_prevButtonState == Button.None && m_currButtonState != Button.None;
	}

	private bool IsButtonReleased()
	{
		return m_prevButtonState != Button.None && m_currButtonState == Button.None;
	}

	private bool IsButtonHeld()
	{
		return m_prevButtonState != Button.None && m_currButtonState != Button.None;
	}

	private static bool IsDodgeButton(Button p_button)
	{
		return p_button == Button.LeftArrow || p_button == Button.RightArrow;
	}

	private void InterpretButtonInput()
	{
		BattleScreenUI.Button pressingButton = SingletonComponent<BattleScreenUI>.Instance.PressingButton;
		m_prevButtonState = m_currButtonState;
		switch (pressingButton)
		{
		case BattleScreenUI.Button.DodgeLeft:
			m_currButtonState = Button.LeftArrow;
			break;
		case BattleScreenUI.Button.DodgeRight:
			m_currButtonState = Button.RightArrow;
			break;
		case BattleScreenUI.Button.Block:
			m_currButtonState = Button.Shield;
			break;
		default:
			m_currButtonState = Button.None;
			break;
		}
		if (m_enableDodge && (m_currButtonState == Button.LeftArrow || m_currButtonState == Button.RightArrow))
		{
			if (IsButtonHeld() && m_player.IsReachableFromCurrent(Player.State.SideStep) && m_player.CanStrafe())
			{
				Player.DodgeDirection p_dir = ((m_currButtonState != Button.LeftArrow) ? Player.DodgeDirection.Right : Player.DodgeDirection.Left);
				m_player.SideStep(p_dir);
			}
		}
		else if (m_enableBlock && m_currButtonState == Button.Shield && IsButtonHeld() && !m_player.IsBlocking() && m_player.IsMinBlockPeriodEnded())
		{
			if (m_player.CurrentState == Player.State.Attack && (m_player.CurrentAttack == null || (m_player.CurrentAttack.type != PlayerAttack.Type.Ripost && m_player.CurrentAttack.type != PlayerAttack.Type.Special && m_player.CurrentAttack.type != PlayerAttack.Type.Ripost)))
			{
				m_player.IsReady = true;
			}
			m_player.Block();
		}
		if (IsButtonPressed())
		{
			Messenger<Button>.Invoke("battle_ui_button_pressed", m_currButtonState);
		}
		bool flag = m_player.CurrentState == Player.State.SideStep && !IsDodgeButton(m_currButtonState);
		if (flag | (m_currButtonState != Button.Shield && m_player.IsBlocking() && m_player.IsMinBlockPeriodEnded()))
		{
			m_player.IsReady = true;
			m_player.Idle();
		}
	}

	private void FixedUpdate()
	{
		if (m_battleManager == null)
		{
			m_battleManager = SingletonComponent<BattleManager>.Instance;
		}
	}

	private void LateUpdate()
	{
		InterpretButtonInput();
		InterpretPointerInput();
	}

	private void OnIdle()
	{
	}

	private void OnMove()
	{
	}

	private void OnFinishMove()
	{
	}

	private void OnDead()
	{
		base.enabled = false;
	}

	private void OnPause()
	{
		base.enabled = false;
	}

	private void OnUnpause()
	{
		base.enabled = true;
	}

	private void OnCutsceneStart(CutsceneController.CutsceneType p_type)
	{
		base.enabled = false;
	}

	private void OnCutsceneEnd(CutsceneController.CutsceneType p_type)
	{
		if (!CutsceneController.IsMobIntroCutscene(p_type) && !CutsceneController.IsMobDeathCutscene(p_type))
		{
			base.enabled = true;
		}
	}

	private void OnEnable()
	{
		Dbg.Log("Enabled PlayerController");
		AttackEnable = true;
		BlockEnable = true;
		DodgeEnable = true;
		m_pointer.enabled = true;
		if (m_player != null)
		{
			m_player.IsReady = true;
		}
		OnEnableOrDisable();
	}

	private void OnDisable()
	{
		Dbg.Log("Disabled PlayerController");
		AttackEnable = false;
		BlockEnable = false;
		DodgeEnable = false;
		m_pointer.enabled = false;
		OnEnableOrDisable();
	}

	private void OnEnableOrDisable()
	{
		if (m_player != null)
		{
			m_player.ClearAttackList();
			if (m_player.CurrentState == Player.State.SideStep || m_player.IsBlocking())
			{
				m_player.IsReady = true;
				m_player.Idle();
			}
		}
		m_pointer.Reset();
		m_prevButtonState = (m_currButtonState = Button.None);
		Messenger<bool>.Invoke("player_controller_enabled", base.enabled);
	}
}
