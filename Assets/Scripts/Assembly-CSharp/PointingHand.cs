using System.Collections;
using UnityEngine;

public class PointingHand : MonoBehaviour
{
	public enum Direction
	{
		Up = 1,
		Down = 2,
		Left = 4,
		Right = 8,
		DownLeft = 6,
		DownRight = 10,
		UpLeft = 5,
		UpRight = 9
	}

	public float m_tweenDistance = 20f;

	public float m_tweenSpeed = 5f;

	private UISprite m_sprite;

	[SerializeField]
	private Direction m_direction = Direction.Left;

	private Vector3 m_pointLocation;

	public UISprite sprite
	{
		get
		{
			return m_sprite;
		}
	}

	public Direction direction
	{
		get
		{
			return m_direction;
		}
		set
		{
			m_direction = value;
			int num = 1;
			int num2 = 1;
			if ((m_direction & Direction.Left) != 0)
			{
				num2 = -1;
			}
			if ((bool)m_sprite)
			{
				Vector3 localScale = m_sprite.transform.localScale;
				localScale.x = (float)num * Mathf.Abs(localScale.x);
				localScale.y = (float)num2 * Mathf.Abs(localScale.y);
				m_sprite.transform.localScale = localScale;
			}
		}
	}

	private void OnEnable()
	{
	}

	private void OnDisable()
	{
		Reset();
	}

	private void Reset()
	{
		if ((bool)m_sprite)
		{
			iTween.StopImmediate(m_sprite.gameObject);
		}
	}

	private void Awake()
	{
		m_sprite = MBHier.GetComponentInChildren<UISprite>(this);
		Dbg.Assert(m_sprite);
	}

	private void Start()
	{
	}

	public bool HasDirection(Direction p_direction)
	{
		return (m_direction & p_direction) != 0;
	}

	public void PointTo(Vector3 p_location, Direction p_direction = Direction.DownRight)
	{
		if ((bool)m_sprite)
		{
			Reset();
			direction = p_direction;
			m_pointLocation = p_location;
			int num = 0;
			if (HasDirection(Direction.Left))
			{
				num = -1;
			}
			else if (HasDirection(Direction.Right))
			{
				num = 1;
			}
			int num2 = 0;
			if (HasDirection(Direction.Up))
			{
				num2 = 1;
			}
			else if (HasDirection(Direction.Down))
			{
				num2 = -1;
			}
			Vector3 vector = Vector3.right * num + Vector3.up * num2;
			Vector3 vector2 = vector * Mathf.Abs(m_sprite.transform.localScale.y) * 0.5f;
			float z = Mathf.Atan2(vector.y, vector.x) * 57.29578f;
			base.transform.localRotation = Quaternion.Euler(0f, 0f, z);
			base.transform.localPosition = p_location - vector2;
			m_sprite.transform.localPosition = Vector3.zero;
			Hashtable hashtable = new Hashtable();
			hashtable.Add("position", -Vector3.right * m_tweenDistance);
			hashtable.Add("time", 1f / m_tweenSpeed);
			hashtable.Add("looptype", iTween.LoopType.pingPong);
			hashtable.Add("ignoretimescale", true);
			hashtable.Add("islocal", true);
			iTween.MoveTo(m_sprite.gameObject, hashtable);
			Color color = m_sprite.color;
			color.a = 1f;
			m_sprite.alpha = 0f;
			TweenColor.Begin(m_sprite.gameObject, 0.2f, color);
			base.enabled = true;
		}
	}
}
