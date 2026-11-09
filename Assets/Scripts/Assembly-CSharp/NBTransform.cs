using UnityEngine;

public class NBTransform
{
	private Vector3 m_position;

	private Quaternion m_rotation;

	private Vector3 m_scale;

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
			return m_rotation;
		}
		set
		{
			m_rotation = value;
		}
	}

	public Vector3 Scale
	{
		get
		{
			return m_scale;
		}
		set
		{
			m_scale = value;
		}
	}

	public void SetRotation(Vector4 p_rotation)
	{
		m_rotation.x = p_rotation.x;
		m_rotation.y = p_rotation.y;
		m_rotation.z = p_rotation.z;
		m_rotation.w = p_rotation.w;
	}

	public void CopyFromTransform(Transform p_transform)
	{
		if (!(p_transform == null))
		{
			m_position = p_transform.position;
			m_rotation = p_transform.rotation;
			m_scale = p_transform.localScale;
		}
	}

	public void CopyFromTransform(NBTransform p_nbTransform)
	{
		if (p_nbTransform != null)
		{
			m_position = p_nbTransform.Position;
			m_rotation = p_nbTransform.Rotation;
			m_scale = p_nbTransform.Scale;
		}
	}

	public void CopyToTransform(Transform p_transform)
	{
		if (!(p_transform == null))
		{
			p_transform.position = m_position;
			p_transform.rotation = m_rotation;
			p_transform.localScale = m_scale;
		}
	}
}
