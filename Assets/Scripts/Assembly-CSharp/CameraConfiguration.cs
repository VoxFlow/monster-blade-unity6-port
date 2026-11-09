using System.Collections.Generic;
using UnityEngine;

public class CameraConfiguration : SingletonComponent<CameraConfiguration>
{
	public class ConfData
	{
		private string m_name = "Camera 0";

		private Vector3 m_position = Vector3.zero;

		private Vector3 m_rotAngles = Vector3.zero;

		private float m_fov = 45f;

		public string Name
		{
			get
			{
				return m_name;
			}
			set
			{
				m_name = value.Trim();
			}
		}

		public Vector3 Position
		{
			get
			{
				return m_position;
			}
			set
			{
				m_position = value;
			}
		}

		public Quaternion Rotation
		{
			get
			{
				return Quaternion.Euler(m_rotAngles);
			}
			set
			{
				m_rotAngles = value.eulerAngles;
			}
		}

		public Vector3 EulerAngles
		{
			get
			{
				return m_rotAngles;
			}
			set
			{
				m_rotAngles = value;
			}
		}

		public float Fov
		{
			get
			{
				return m_fov;
			}
			set
			{
				m_fov = value;
			}
		}

		public ConfData()
		{
		}

		public ConfData(ConfData p_rhs)
		{
			m_name = p_rhs.Name;
			m_position = p_rhs.Position;
			m_rotAngles = p_rhs.EulerAngles;
			m_fov = p_rhs.Fov;
		}
	}

	private static readonly string ms_cameraConfName = "Camera";

	private Dictionary<string, ConfData> m_cameras = new Dictionary<string, ConfData>();

	public static string CameraConfigName
	{
		get
		{
			return ms_cameraConfName;
		}
	}

	public Dictionary<string, ConfData> Cameras
	{
		get
		{
			return m_cameras;
		}
	}

	private CameraConfiguration()
	{
	}

	public ConfData GetCamera(string p_name)
	{
		if (!ConfirmDbInit())
		{
			return null;
		}
		ConfData result = null;
		string text = p_name.Trim();
		if (m_cameras.ContainsKey(text))
		{
			result = m_cameras[text];
		}
		else
		{
			Dbg.LogError("Could not find the camera with name : " + text);
		}
		return result;
	}

	public void AddCamera(ConfData p_newCam)
	{
		if (ConfirmDbInit())
		{
			if (m_cameras.ContainsKey(p_newCam.Name))
			{
				Dbg.LogWarn("Camera with the name '" + p_newCam.Name + "' already been added.");
			}
			else
			{
				m_cameras.Add(p_newCam.Name, p_newCam);
			}
		}
	}

	public void RemoveCamera(string p_name)
	{
		if (ConfirmDbInit())
		{
			string text = p_name.Trim();
			if (m_cameras.ContainsKey(text))
			{
				m_cameras.Remove(text);
			}
			else
			{
				Dbg.LogWarn("Could not find the camera with name : " + text);
			}
		}
	}

	private bool ConfirmDbInit()
	{
		if (m_cameras == null)
		{
			Dbg.LogError("Camera configuration database hasn't been initialized.");
			return false;
		}
		return true;
	}

	public void InitManager()
	{
		NBProfiler.BeginTask("[CAMERA_CONFIGURATION][INIT_MANAGER]");
		if (SingletonComponent<ParameterManager>.Instance == null)
		{
			if (!LoadConfig())
			{
				Dbg.LogError("Camera configuration initialization failed.");
			}
		}
		else
		{
			ParameterManager instance = SingletonComponent<ParameterManager>.Instance;
			NBBinaryFormat parametersFor = instance.GetParametersFor(ms_cameraConfName);
			int p_columnIndex = parametersFor.ColumnsNames["Position"];
			int p_columnIndex2 = parametersFor.ColumnsNames["Rotation"];
			int p_columnIndex3 = parametersFor.ColumnsNames["FOV"];
			m_cameras.Clear();
			for (int i = 0; i < parametersFor.RowsCount; i++)
			{
				ConfData confData = new ConfData();
				confData.Name = parametersFor.RowsNames[i];
				confData.Position = parametersFor.GetDataAsVector3(i, p_columnIndex);
				confData.EulerAngles = parametersFor.GetDataAsVector3(i, p_columnIndex2);
				float dataGeneric = parametersFor.GetDataGeneric<float>(i, p_columnIndex3);
				if (dataGeneric > 0f)
				{
					confData.Fov = dataGeneric;
				}
				m_cameras.Add(confData.Name, confData);
			}
		}
		Messenger<LoadingProgressManager.LoadProgress>.Invoke("progress_bar", LoadingProgressManager.LoadProgress.CameraConfig);
		NBProfiler.EndTask("[CAMERA_CONFIGURATION][INIT_MANAGER]");
	}

	public bool LoadConfig()
	{
		return false;
	}

	public void SaveConfig()
	{
	}
}
