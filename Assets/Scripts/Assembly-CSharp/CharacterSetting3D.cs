using UnityEngine;

public class CharacterSetting3D : MonoBehaviour
{
	[SerializeField]
	private Camera m_camera;

	[SerializeField]
	private Color m_mainDisabledColor = Color.white;

	[SerializeField]
	private Color m_flashDisabledColor = Color.white;

	[SerializeField]
	private Color m_ambientDisabledColor = Color.white;

	[SerializeField]
	private Color m_ambientLight = Color.white;

	public Color AmbientLight
	{
		get
		{
			return m_ambientLight;
		}
	}

	public Color GetMainDisableColor()
	{
		return m_mainDisabledColor;
	}

	public Color GetFlashDisableColor()
	{
		return m_flashDisabledColor;
	}

	public Color GetAmbientDisableColor()
	{
		return m_ambientDisabledColor;
	}

	public void AttachCamera(string p_nodeName)
	{
		AttachObject(m_camera.gameObject, p_nodeName);
	}

	public void AttachObject(GameObject p_object, string p_nodeName)
	{
		if (!(null == p_object) && !string.IsNullOrEmpty(p_nodeName))
		{
			Transform transform = base.transform.Find(p_nodeName);
			if (transform != null)
			{
				p_object.transform.parent = transform;
				p_object.transform.localPosition = Vector3.zero;
				p_object.transform.localRotation = Quaternion.identity;
			}
		}
	}
}
