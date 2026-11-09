using UnityEngine;

public class BattleCamera : MonoBehaviour
{
	public enum CameraType
	{
		Main = 0,
		Intro = 1,
		Max = 2
	}

	public delegate void Callback();

	[SerializeField]
	private Transform m_root;

	[SerializeField]
	private Transform m_mainCamera;

	[SerializeField]
	private ShakeFx m_shakeRoot;

	private string m_currentCamera = string.Empty;

	private string m_previousCamera = string.Empty;

	private Vector3 m_startPos = Vector3.zero;

	private Vector3 m_targetPos = Vector3.zero;

	private Quaternion m_startRot = Quaternion.identity;

	private Quaternion m_targetRot = Quaternion.identity;

	private float m_startFov;

	private float m_targetFov;

	private float m_lerp;

	private float m_step;

	private Callback m_interpolateCallback;

	private bool m_interpolating;

	private LookAt m_lookAtScript;

	public static readonly string s_battleCameraName = "Fighting Camera";

	public static readonly string s_introCameraName = "Intro Camera";

	public string CurrentCamera
	{
		get
		{
			return m_currentCamera;
		}
	}

	public string PreviousCamera
	{
		get
		{
			return m_previousCamera;
		}
	}

	public static string DefaultCamera
	{
		get
		{
			return "Main Camera";
		}
	}

	public Transform Root
	{
		get
		{
			return m_root;
		}
	}

	public Transform MainCameraNode
	{
		get
		{
			return m_mainCamera;
		}
	}

	public ShakeFx ShakeNode
	{
		get
		{
			return m_shakeRoot;
		}
	}

	public bool Lock { get; set; }

	private bool DisableCamera(Transform p_cameraTransform, object p_param)
	{
		if (p_cameraTransform.GetComponent<Camera>() != null)
		{
			p_cameraTransform.GetComponent<Camera>().enabled = false;
		}
		return false;
	}

	private void Awake()
	{
		base.transform.Iterate(DisableCamera);
		m_mainCamera.GetComponent<Camera>().enabled = true;
		m_shakeRoot.transform.localPosition = Vector3.zero;
		m_shakeRoot.transform.localRotation = Quaternion.identity;
		if (null != m_mainCamera)
		{
			m_lookAtScript = m_mainCamera.GetComponent<LookAt>();
		}
		Lock = false;
		base.enabled = true;
		m_interpolating = false;
		Messenger<bool>.AddListener("battle_pause", OnPause);
	}

	private void OnDestroy()
	{
		Messenger<bool>.RemoveListener("battle_pause", OnPause);
	}

	private void OnDisable()
	{
		base.transform.localPosition = Vector3.zero;
		base.transform.localRotation = Quaternion.identity;
	}

	public Camera GetCamera(string p_target)
	{
		Transform transform = base.transform.FindInHierarchy(p_target);
		if (transform == null)
		{
			return null;
		}
		if (transform.GetComponent<Camera>() == null)
		{
			Dbg.LogWarn("There's no 'Camera' component attached to " + p_target + ".");
		}
		return transform.GetComponent<Camera>();
	}

	public void InitForCurrentBattle()
	{
		string[] array = new string[2] { s_battleCameraName, s_introCameraName };
		string p_name = SingletonComponent<StageManager>.Instance.EnemyObject.name;
		for (int i = 0; i < 2; i++)
		{
			string text = Singleton<MonsterFactory>.Instance.RandomCamera(p_name, (CameraType)i);
			Dbg.LogEx("Selected camera setup parameter for '{0}' is '{1}'.", Dbg.NBLogType.COMMON_LOG, array[i], text);
			SetCameraTransform(array[i], text);
		}
		for (int j = 0; j < base.transform.childCount; j++)
		{
			Transform child = base.transform.GetChild(j);
			if (!NBContainerUtil.IsContain(array, child.name) && child != m_root)
			{
				Object.Destroy(child.gameObject);
			}
		}
		string[] extraCameras = Singleton<MonsterFactory>.Instance.GetExtraCameras(p_name);
		if (extraCameras == null)
		{
			return;
		}
		for (int k = 0; k < extraCameras.Length; k++)
		{
			CameraConfiguration.ConfData confData = SingletonComponent<CameraConfiguration>.Instance.GetCamera(extraCameras[k]);
			if (confData != null)
			{
				GameObject gameObject = new GameObject(extraCameras[k]);
				Transform transform = gameObject.transform;
				transform.parent = base.transform;
				transform.localPosition = confData.Position;
				transform.localRotation = confData.Rotation;
				gameObject.AddComponent<Camera>();
				transform.GetComponent<Camera>().fieldOfView = confData.Fov;
				transform.GetComponent<Camera>().enabled = false;
				transform.GetComponent<Camera>().cullingMask &= ~(1 << LayerMask.NameToLayer("PopUpLayer"));
			}
		}
	}

	public void SetCameraTransform(string p_target, string p_confName)
	{
		CameraConfiguration.ConfData confData = SingletonComponent<CameraConfiguration>.Instance.GetCamera(p_confName);
		if (confData == null)
		{
			confData = SingletonComponent<CameraConfiguration>.Instance.GetCamera(DefaultCamera);
		}
		Transform transform = base.transform.Find(p_target);
		if (null != transform && base.transform != transform)
		{
			transform.localPosition = confData.Position;
			transform.localRotation = confData.Rotation;
			transform.GetComponent<Camera>().fieldOfView = confData.Fov;
		}
		else
		{
			Dbg.LogError("Could not modify target : " + p_target + " " + p_confName);
		}
	}

	public void SetMainCameraTransform(string p_confName)
	{
		if (!(m_root == null) && !(m_mainCamera == null))
		{
			CameraConfiguration.ConfData confData = SingletonComponent<CameraConfiguration>.Instance.GetCamera(p_confName);
			if (confData == null)
			{
				confData = SingletonComponent<CameraConfiguration>.Instance.GetCamera(DefaultCamera);
			}
			m_root.localPosition = confData.Position;
			m_root.localRotation = confData.Rotation;
			m_mainCamera.transform.localPosition = Vector3.zero;
			m_mainCamera.transform.localRotation = Quaternion.identity;
			m_mainCamera.GetComponent<Camera>().fieldOfView = confData.Fov;
		}
	}

	public void ChangeCamera(string p_target)
	{
		if (!Lock)
		{
			Camera camera = GetCamera(p_target);
			if (camera == null || base.transform == m_root || base.transform == m_mainCamera)
			{
				Dbg.LogError("Wrong Parameter set for ChangeCamera");
				return;
			}
			Dbg.Log("Camera is changing from " + m_currentCamera + " to " + p_target);
			CameraFx.StopPlayingEffects();
			m_root.localPosition = camera.transform.localPosition;
			m_root.localRotation = camera.transform.localRotation;
			m_mainCamera.transform.localPosition = Vector3.zero;
			m_mainCamera.transform.localRotation = Quaternion.identity;
			m_mainCamera.GetComponent<Camera>().fieldOfView = camera.fieldOfView;
			m_previousCamera = m_currentCamera;
			m_currentCamera = p_target;
			m_interpolating = false;
		}
	}

	public void InterpolateCamera(string p_target, float p_time, bool p_transformLocal, Callback p_endCallback = null)
	{
		if (Lock)
		{
			return;
		}
		Camera camera = GetCamera(p_target);
		if (camera == null)
		{
			return;
		}
		if (p_time <= 0f)
		{
			if (p_endCallback != null)
			{
				p_endCallback();
			}
			return;
		}
		Dbg.Log("Camera is interpolating from " + m_currentCamera + " to " + p_target);
		m_startPos = m_root.localPosition;
		m_startRot = m_root.localRotation;
		m_startFov = m_mainCamera.GetComponent<Camera>().fieldOfView;
		m_targetPos = camera.transform.localPosition;
		m_targetRot = camera.transform.localRotation;
		m_targetFov = camera.fieldOfView;
		m_step = 1f / p_time;
		m_lerp = 0f;
		m_interpolateCallback = p_endCallback;
		m_previousCamera = m_currentCamera;
		m_currentCamera = p_target;
		m_interpolating = true;
	}

	public void StopInterpolating()
	{
		m_root.localPosition = m_targetPos;
		m_root.localRotation = m_targetRot;
		m_mainCamera.GetComponent<Camera>().fieldOfView = m_targetFov;
		if (m_interpolateCallback != null)
		{
			m_interpolateCallback();
			m_interpolateCallback = null;
		}
		m_interpolating = false;
	}

	public void ShakeCamera(CameraShakeData p_shakeData)
	{
		if (m_shakeRoot == null)
		{
			Dbg.LogWarn("Shake Root hasn't been specified.");
			return;
		}
		m_shakeRoot.Duration = p_shakeData.duration;
		m_shakeRoot.Frequency = p_shakeData.frequency;
		m_shakeRoot.Amplitude = p_shakeData.amplitude;
		m_shakeRoot.SmoothFade = p_shakeData.smoothFade;
		m_shakeRoot.Play();
	}

	public void SetLookAtTarget(Transform p_target)
	{
		if (null != m_lookAtScript)
		{
			m_lookAtScript.target = p_target;
		}
	}

	private void OnPause(bool p_pause)
	{
	}

	private void Update()
	{
		base.transform.localPosition = Vector3.zero;
		base.transform.localRotation = Quaternion.identity;
		if (m_interpolating)
		{
			m_lerp += m_step * Time.deltaTime;
			m_root.localPosition = Vector3.Lerp(m_startPos, m_targetPos, m_lerp);
			m_root.localRotation = Quaternion.Slerp(m_startRot, m_targetRot, m_lerp);
			m_mainCamera.GetComponent<Camera>().fieldOfView = Mathf.Lerp(m_startFov, m_targetFov, m_lerp);
			if (m_lerp >= 1f)
			{
				StopInterpolating();
			}
		}
	}
}
