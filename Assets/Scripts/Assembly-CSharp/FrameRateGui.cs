using UnityEngine;

public class FrameRateGui : MonoBehaviour
{
	private static float m_framesPerSecond;

	private float m_accFramesPerSecond;

	private int m_countedFramesPerSecond;

	private static float m_avgFramesPerSecond;

	private int m_frames;

	private float m_framesTimeAcc;

	private float m_framesUpdateInterval = 0.5f;

	private float m_framesNextUpdate;

	private static float m_secondsPerFrame;

	private float m_accSecondsPerFrame;

	private int m_countedSecondsPerFrame;

	private static float m_avgSecondsPerFrame;

	public static float FPS
	{
		get
		{
			return m_framesPerSecond;
		}
	}

	public static float AverageFPS
	{
		get
		{
			return m_avgFramesPerSecond;
		}
	}

	public static float SPF
	{
		get
		{
			return m_secondsPerFrame;
		}
	}

	public static float AverageSPF
	{
		get
		{
			return m_avgSecondsPerFrame;
		}
	}

	public void Awake()
	{
		Reset();
	}

	public void Reset()
	{
		m_framesPerSecond = 0f;
		m_accFramesPerSecond = 0f;
		m_countedFramesPerSecond = 0;
		m_avgFramesPerSecond = 0f;
		m_frames = 0;
		m_framesTimeAcc = 0f;
		m_framesUpdateInterval = 0.5f;
		m_framesNextUpdate = Time.time + m_framesUpdateInterval;
		m_secondsPerFrame = 0f;
		m_accSecondsPerFrame = 0f;
		m_countedSecondsPerFrame = 0;
		m_avgSecondsPerFrame = 0f;
	}

	public void Update()
	{
		m_frames++;
		m_framesTimeAcc += Time.timeScale / Time.deltaTime;
		if (Time.time >= m_framesNextUpdate)
		{
			m_framesPerSecond = m_framesTimeAcc / (float)m_frames;
			m_framesNextUpdate = Time.time + m_framesUpdateInterval;
			m_framesTimeAcc = 0f;
			m_frames = 0;
			m_accFramesPerSecond += m_framesPerSecond;
			m_countedFramesPerSecond++;
			m_avgFramesPerSecond = m_accFramesPerSecond / (float)m_countedFramesPerSecond;
		}
		m_secondsPerFrame = Time.deltaTime;
		m_accSecondsPerFrame += m_secondsPerFrame;
		m_countedSecondsPerFrame++;
		m_avgSecondsPerFrame = m_accSecondsPerFrame / (float)m_countedSecondsPerFrame;
	}
}
