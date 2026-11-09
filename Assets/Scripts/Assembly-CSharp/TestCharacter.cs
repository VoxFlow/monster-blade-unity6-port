using UnityEngine;

public class TestCharacter : MonoBehaviour
{
	private Pointer m_pointer;

	private Vector2 m_swipeVec = Vector2.zero;

	public float m_minRecognitionDistance = 15f;

	public float m_maxRecognitionDistance = 40f;

	public float m_smallMoveDistance = 5f;

	private bool m_isInputConsume;

	public SlashEffectEmitter[] m_slashEffects;

	private int m_effectIndex;

	private int m_slashIndex;

	public ParticleSystemController m_perfectBlockFx;

	public ParticleSystemController m_healFx;

	public ParticleSystemController m_lifeStealFx;

	public ParticleSystemController m_superMoveFx;

	private void Awake()
	{
		m_pointer = base.gameObject.AddComponent<Pointer>();
		m_minRecognitionDistance *= m_minRecognitionDistance;
		m_maxRecognitionDistance *= m_maxRecognitionDistance;
		m_smallMoveDistance *= m_smallMoveDistance;
	}

	private void Start()
	{
		if (m_perfectBlockFx != null)
		{
			m_perfectBlockFx.Stop();
		}
		if (m_healFx != null)
		{
			m_healFx.Stop();
		}
		if (m_lifeStealFx != null)
		{
			m_lifeStealFx.Stop();
		}
		if (m_superMoveFx != null)
		{
			m_superMoveFx.Stop();
		}
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
		string[] array = new string[8] { "Att_Top", "Att_Down", "Att_Left", "Att_Right", "Att_Lefttop", "Att_Righttop", "Att_Leftdown", "Att_Rightdown" };
		PlayerAttack.Direction direction = EvaluateAttack(m_swipeVec);
		base.GetComponent<Animation>().CrossFadeQueued(array[(int)direction], 0.1f, QueueMode.CompleteOthers);
		CallSlashEmitter((SlashEffectEmitter.FxType)m_slashIndex, direction);
		if (m_lifeStealFx != null)
		{
			m_lifeStealFx.Play();
		}
		m_swipeVec = Vector2.zero;
		m_isInputConsume = true;
	}

	private void CallSlashEmitter(SlashEffectEmitter.FxType p_fxType, PlayerAttack.Direction p_dir)
	{
		if (m_slashEffects == null || m_slashEffects.Length == 0)
		{
			Dbg.LogWarn("Slash emitter is null");
			return;
		}
		Vector3 vector = Vector3.zero;
		switch (p_dir)
		{
		case PlayerAttack.Direction.TopToBottom:
			vector = base.transform.up * -1f;
			break;
		case PlayerAttack.Direction.BottomToTop:
			vector = base.transform.up;
			break;
		case PlayerAttack.Direction.LeftToRight:
			vector = base.transform.right;
			break;
		case PlayerAttack.Direction.RightToLeft:
			vector = base.transform.right * -1f;
			break;
		case PlayerAttack.Direction.TopLeftToBottomRight:
			vector = (base.transform.up - base.transform.right) * -1f;
			break;
		case PlayerAttack.Direction.BottomRightToTopLeft:
			vector = base.transform.up - base.transform.right;
			break;
		case PlayerAttack.Direction.TopRightToBottomLeft:
			vector = (base.transform.right + base.transform.up) * -1f;
			break;
		case PlayerAttack.Direction.BottomLeftToTopRight:
			vector = base.transform.right + base.transform.up;
			break;
		}
		if (vector != Vector3.zero)
		{
			SlashEffectEmitter slashEffectEmitter = m_slashEffects[m_effectIndex];
			slashEffectEmitter.EmitSlash(p_fxType, slashEffectEmitter.transform.position, vector);
		}
	}

	private void Update()
	{
		if (m_pointer.m_state == Pointer.State.None)
		{
			return;
		}
		if (m_pointer.m_state == Pointer.State.Down)
		{
			m_swipeVec = Vector2.zero;
		}
		if (m_pointer.m_state == Pointer.State.Move)
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
			m_isInputConsume = false;
		}
	}

	private void OnGUI()
	{
	}
}
