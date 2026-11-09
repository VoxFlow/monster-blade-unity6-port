using System.Collections.Generic;
using UnityEngine;

public class CameraCutScene : MonoBehaviour
{
	public Camera[] m_cameras;

	public Camera[] m_cameraSequence;

	public Camera m_currentCamera;

	private bool m_playSequence;

	private int m_cameraIndex = 1;

	private bool m_sequenceEnded;

	public bool IsSequenceEnded
	{
		get
		{
			return m_sequenceEnded;
		}
	}

	public Camera[] Sequence
	{
		get
		{
			return m_cameraSequence;
		}
		set
		{
			m_cameraSequence = value;
		}
	}

	private void Start()
	{
		PlaySequence();
	}

	private void Update()
	{
		if (m_playSequence && m_cameraIndex >= 1 && m_cameraIndex <= m_cameraSequence.Length)
		{
			if (!m_cameraSequence[m_cameraIndex - 1].GetComponent<Animation>().isPlaying && m_cameraIndex <= m_cameraSequence.Length - 1)
			{
				PlayCameraAnim(m_cameraIndex);
			}
			if (m_cameraIndex == m_cameraSequence.Length && !m_cameraSequence[m_cameraIndex - 1].GetComponent<Animation>().isPlaying)
			{
				m_cameraSequence[m_cameraIndex - 1].enabled = false;
				Dbg.Log("Camera Sequence Just ended.");
				m_playSequence = false;
				m_sequenceEnded = true;
			}
		}
	}

	public void SelectCamera(int p_index)
	{
		if (m_cameras != null && p_index > 0 && p_index < m_cameras.Length)
		{
			SelectCamera(m_cameras[p_index].name);
		}
	}

	public void SelectCamera(string p_cameraName)
	{
		if (m_cameras == null)
		{
			return;
		}
		for (int i = 0; i < m_cameras.Length; i++)
		{
			m_cameras[i].enabled = false;
			if (m_cameras[i].name == p_cameraName)
			{
				m_cameras[i].enabled = true;
			}
		}
	}

	public string[] SearchCameras()
	{
		m_cameras = Camera.allCameras;
		List<string> list = new List<string>();
		for (int i = 0; i < m_cameras.Length; i++)
		{
			list.Add(m_cameras[i].name);
		}
		string[] array = list.ToArray();
		string text = "Camera Found : \n";
		for (int j = 0; j < array.Length; j++)
		{
			text = text + m_cameras[j].name + "\n";
		}
		Dbg.Log(text);
		return array;
	}

	public void EraseSequence()
	{
		if (m_cameraSequence != null)
		{
			for (int i = 0; i < m_cameraSequence.Length; i++)
			{
				m_cameraSequence[i] = null;
			}
		}
	}

	public void EnableAllCameras()
	{
		string text = "Cameras Enabled :\n";
		for (int i = 0; i < m_cameras.Length; i++)
		{
			m_cameras[i].enabled = true;
			text = text + m_cameras[i].name + "\n";
		}
		Dbg.Log(text);
	}

	public void PrintSequence()
	{
		string empty = string.Empty;
		empty += "Camera Sequence :\n";
		for (int i = 0; i < m_cameraSequence.Length; i++)
		{
			if (m_cameraSequence[i] != null)
			{
				empty = empty + m_cameraSequence[i].name + "\n";
			}
		}
		Dbg.Log(empty);
	}

	public void ResetCameraSequence()
	{
		Dbg.Log("Reseting the Cameras Sequence.");
		m_cameraSequence = Camera.allCameras;
	}

	public void PlaySequence()
	{
		if (m_cameraSequence != null && m_cameraSequence.Length > 0 && m_cameraSequence[0] != null)
		{
			PlayCameraAnim(0);
			m_playSequence = true;
		}
		else
		{
			Dbg.LogWarn("Wrong parameter for the sequence");
		}
	}

	public void PlayCameraAnim(int p_index)
	{
		if (m_currentCamera != null)
		{
			m_currentCamera.enabled = false;
		}
		m_currentCamera = m_cameraSequence[p_index];
		SelectCamera(m_currentCamera.name);
		m_currentCamera.GetComponent<Animation>().Play();
		m_currentCamera.enabled = true;
		m_cameraIndex = ++p_index;
	}
}
