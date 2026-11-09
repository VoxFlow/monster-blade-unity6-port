using UnityEngine;

public class ApplicationSettingsInfo : MonoBehaviour
{
	public float m_desiredFpsCap = 60f;

	public void Awake()
	{
		Application.targetFrameRate = MBFramePacing.Cap((int)m_desiredFpsCap);   // PORT (CD-049)
	}
}
