using UnityEngine;

public class CameraEffects : MonoBehaviour
{
	public enum CameraEffectTypes
	{
		None = 0,
		Shake = 1,
		Tilt = 2
	}

	private enum ShakeStates
	{
		Impact = 0,
		ImpactReverse = 1,
		Finish = 2
	}

	private enum TiltStates
	{
		Impact = 0,
		Recover = 1,
		Finish = 2
	}

	public enum LerpModes
	{
		Position = 0,
		Rotation = 1
	}

	private Transform m_myTransform;

	private CameraEffectTypes m_cameraEffectMode;

	private LerpModes m_lerpMode;

	private Vector3 m_targetPosition = Vector3.zero;

	private Quaternion m_targetRotation = Quaternion.identity;

	private Vector3 m_originalPosition = Vector3.zero;

	private Quaternion m_originalRotation = Quaternion.identity;

	private Vector3 m_direction = Vector3.zero;

	private float m_riseup = 3f;

	private float m_cooldown = 1f;

	private float m_time;

	private TiltStates m_tiltState = TiltStates.Finish;

	private void Awake()
	{
		m_myTransform = GetComponent<Transform>();
		CopyToOriginalTransform(m_myTransform);
	}

	public CameraEffectTypes GetCameraEffectMode()
	{
		return m_cameraEffectMode;
	}

	private void Update()
	{
		if (m_cameraEffectMode != CameraEffectTypes.None)
		{
			if (m_cameraEffectMode == CameraEffectTypes.Shake)
			{
				UpdateShake();
			}
			else if (m_cameraEffectMode == CameraEffectTypes.Tilt)
			{
				UpdateTilt();
			}
		}
	}

	private void UpdateShake()
	{
	}

	private void UpdateTilt()
	{
		if (m_tiltState == TiltStates.Impact)
		{
			m_time += Time.deltaTime * m_riseup;
			if (m_lerpMode == LerpModes.Position)
			{
				m_myTransform.position = Vector3.Lerp(m_originalPosition, m_targetPosition, m_time);
			}
			else
			{
				m_myTransform.rotation = Quaternion.Lerp(m_originalRotation, m_targetRotation, m_time);
			}
			if (m_time >= 1f)
			{
				m_time = 0f;
				m_tiltState = TiltStates.Recover;
			}
		}
		else if (m_tiltState == TiltStates.Recover)
		{
			m_time += Time.deltaTime * m_cooldown;
			if (m_lerpMode == LerpModes.Position)
			{
				m_myTransform.position = Vector3.Lerp(m_targetPosition, m_originalPosition, m_time);
			}
			else
			{
				m_myTransform.rotation = Quaternion.Lerp(m_targetRotation, m_originalRotation, m_time);
			}
			if (m_time >= 1f)
			{
				ApplyOriginalTransform();
				SendMessage("OnTiltComplete", null, SendMessageOptions.DontRequireReceiver);
				m_tiltState = TiltStates.Finish;
				m_cameraEffectMode = CameraEffectTypes.None;
			}
		}
	}

	public void StopCameraEffect()
	{
		Debug.Log("StopCameraEffect");
		ApplyOriginalTransform();
		m_cameraEffectMode = CameraEffectTypes.None;
		m_tiltState = TiltStates.Finish;
		iTween.Stop();
		SendMessage("OnEffectCancelled", m_cameraEffectMode, SendMessageOptions.DontRequireReceiver);
	}

	private void iTweenShake(Vector3 p_direction, float p_duration, LerpModes p_lerpMode)
	{
		if (m_cameraEffectMode != CameraEffectTypes.None && m_cameraEffectMode != CameraEffectTypes.Shake)
		{
			ApplyOriginalTransform();
		}
		switch (p_lerpMode)
		{
		case LerpModes.Position:
			iTween.ShakePosition(base.gameObject, iTween.Hash("amount", p_direction, "time", p_duration, "islocal", true, "oncomplete", "iTweenCompleteCallback"));
			break;
		case LerpModes.Rotation:
			iTween.ShakeRotation(base.gameObject, iTween.Hash("amount", p_direction, "time", p_duration, "islocal", true, "oncomplete", "iTweenCompleteCallback"));
			break;
		}
		m_cameraEffectMode = CameraEffectTypes.Shake;
	}

	private void iTweenCompleteCallback()
	{
		m_cameraEffectMode = CameraEffectTypes.None;
	}

	public void iTweenShakePosition(Vector3 p_direction, float p_duration)
	{
		iTweenShake(p_direction, p_duration, LerpModes.Position);
	}

	public void iTweenShakeRotation(Vector3 p_direction, float p_duration)
	{
		iTweenShake(p_direction, p_duration, LerpModes.Rotation);
	}

	public void Tilt(Vector3 p_direction, float p_riseup, float p_cooldown, LerpModes p_lerpMode)
	{
		if (m_cameraEffectMode == CameraEffectTypes.None || m_cameraEffectMode == CameraEffectTypes.Tilt)
		{
			if (m_cameraEffectMode == CameraEffectTypes.None)
			{
				CopyToOriginalTransform(m_myTransform);
			}
			m_cameraEffectMode = CameraEffectTypes.Tilt;
			m_direction = p_direction;
			m_riseup = p_riseup;
			m_cooldown = p_cooldown;
			m_tiltState = TiltStates.Impact;
			m_lerpMode = p_lerpMode;
			m_time = 0f;
			if (m_lerpMode == LerpModes.Position)
			{
				m_targetPosition = m_direction + m_myTransform.position;
				m_targetRotation = m_myTransform.rotation;
			}
			else
			{
				Vector3 forward = m_myTransform.forward;
				forward.Normalize();
				Vector3 lookRotation = forward + m_direction;
				m_targetPosition = m_myTransform.position;
				m_targetRotation = Quaternion.identity;
				m_targetRotation.SetLookRotation(lookRotation);
			}
			SendMessage("OnTiltBegin", null, SendMessageOptions.DontRequireReceiver);
		}
	}

	public void TiltPosition(Vector3 p_direction, float p_riseup, float p_cooldown)
	{
		Tilt(p_direction, p_riseup, p_cooldown, LerpModes.Position);
	}

	public void TiltRotation(Vector3 p_direction, float p_riseup, float p_cooldown)
	{
		Tilt(p_direction, p_riseup, p_cooldown, LerpModes.Rotation);
	}

	private void ApplyOriginalTransform()
	{
		m_myTransform.position = m_originalPosition;
		m_myTransform.rotation = m_originalRotation;
	}

	private void CopyToOriginalTransform(Transform p_transform)
	{
		m_originalPosition = p_transform.position;
		m_originalRotation = p_transform.rotation;
	}
}
