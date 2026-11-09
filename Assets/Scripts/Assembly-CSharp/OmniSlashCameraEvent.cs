using System;
using System.Collections;
using UnityEngine;

public class OmniSlashCameraEvent : MonoBehaviour
{
	public Action onFinishIntroCameraSetup;

	public Action onFinishResetCameraSetup;

	private Transform m_cameraTweenTargetRoot;

	private Transform m_battleCameraRoot;

	private Vector3 m_defaultPosition = Vector3.zero;

	private Quaternion m_defaultRotation = Quaternion.identity;

	private bool m_wasMoved;

	public Transform cameraTweenTargetRoot
	{
		private get
		{
			return m_cameraTweenTargetRoot;
		}
		set
		{
			m_cameraTweenTargetRoot = value;
			if (null == m_cameraTweenTargetRoot)
			{
				return;
			}
			for (int i = 0; i < cameraTweenTargetRoot.childCount; i++)
			{
				Transform child = cameraTweenTargetRoot.GetChild(i);
				if (null != child && child.gameObject.GetComponent<Camera>().enabled)
				{
					child.gameObject.GetComponent<Camera>().enabled = false;
				}
			}
		}
	}

	public Transform battleCameraRoot
	{
		private get
		{
			return m_battleCameraRoot;
		}
		set
		{
			m_battleCameraRoot = value;
		}
	}

	private void Awake()
	{
		Messenger.AddListener("player_super_atk_end", OnSuperAttackEnd);
		base.enabled = false;
	}

	private void OnDestroy()
	{
		Messenger.RemoveListener("player_super_atk_end", OnSuperAttackEnd);
	}

	private void OnSuperAttackEnd()
	{
		if (m_wasMoved)
		{
			ResetCameraTransfrom(0.5f);
		}
	}

	private void SetupIntroCamera(float p_seconds)
	{
		if (!(null == cameraTweenTargetRoot) && !(null == battleCameraRoot) && SuperAttack.Playing)
		{
			Transform closestTweenTarget = GetClosestTweenTarget();
			if (!(null == closestTweenTarget))
			{
				m_wasMoved = true;
				m_defaultPosition = battleCameraRoot.localPosition;
				m_defaultRotation = battleCameraRoot.localRotation;
				StartCoroutine(Tween(p_seconds, closestTweenTarget.localPosition, closestTweenTarget.localRotation, battleCameraRoot, onFinishIntroCameraSetup));
			}
		}
	}

	private void ResetCameraTransfrom(float p_seconds)
	{
		if (!(null == cameraTweenTargetRoot) && !(null == battleCameraRoot) && m_wasMoved)
		{
			StartCoroutine(Tween(p_seconds, m_defaultPosition, m_defaultRotation, battleCameraRoot, OnFinishResetCameraSetup));
		}
	}

	private void OnFinishResetCameraSetup()
	{
		m_wasMoved = false;
		if (onFinishResetCameraSetup != null)
		{
			onFinishResetCameraSetup();
		}
	}

	public IEnumerator Tween(float p_seconds, Vector3 p_targetPosition, Quaternion p_targetRotation, Transform p_cameraRoot, Action p_onTweenEnd)
	{
		if (!(null == p_cameraRoot) && p_seconds != 0f)
		{
			float time = 0f;
			float lerp = 0f;
			float minSeconds = 1f / 30f;
			Vector3 srcPosition = p_cameraRoot.localPosition;
			Quaternion srcRotation = p_cameraRoot.localRotation;
			while (lerp < 1f)
			{
				time += Mathf.Min(minSeconds, Time.deltaTime);
				lerp = time / p_seconds;
				p_cameraRoot.localPosition = Vector3.Lerp(srcPosition, p_targetPosition, lerp);
				p_cameraRoot.localRotation = Quaternion.Slerp(srcRotation, p_targetRotation, lerp);
				yield return null;
			}
			if (p_onTweenEnd != null)
			{
				p_onTweenEnd();
			}
		}
	}

	private Transform GetClosestTweenTarget()
	{
		if (null == cameraTweenTargetRoot || null == battleCameraRoot)
		{
			return null;
		}
		float num = float.MaxValue;
		Transform result = null;
		for (int i = 0; i < cameraTweenTargetRoot.childCount; i++)
		{
			Transform child = cameraTweenTargetRoot.GetChild(i);
			float sqrMagnitude = (battleCameraRoot.position - child.position).sqrMagnitude;
			if (sqrMagnitude < num)
			{
				num = sqrMagnitude;
				result = child;
			}
		}
		return result;
	}
}
