using System.Collections.Generic;
using UnityEngine;

[ExecuteInEditMode]
public class AnimateTintColor : MonoBehaviour
{
	private Material[] m_materials;

	[SerializeField]
	private bool m_useSharedMaterial = true;

	[SerializeField]
	private Color m_tintColor = Color.white;

	private Material[] materials
	{
		get
		{
			if (m_materials == null)
			{
				GetMaterials();
			}
			return m_materials;
		}
		set
		{
			m_materials = value;
		}
	}

	private void Awake()
	{
		GetMaterials();
	}

	private void GetMaterials()
	{
		MeshRenderer[] componentsInChildren = MBHier.GetComponentsInChildren<MeshRenderer>(base.gameObject);
		List<Material> list = new List<Material>();
		for (int i = 0; i < componentsInChildren.Length; i++)
		{
			if (null == componentsInChildren[i] || null == componentsInChildren[i].sharedMaterial || !componentsInChildren[i].sharedMaterial.HasProperty("_TintColor"))
			{
				continue;
			}
			if (m_useSharedMaterial)
			{
				if (list.IndexOf(componentsInChildren[i].sharedMaterial) < 0)
				{
					list.Add(componentsInChildren[i].sharedMaterial);
				}
			}
			else
			{
				list.Add(componentsInChildren[i].material);
			}
		}
		m_materials = list.ToArray();
	}

	public void SetColor(Color p_color)
	{
		if (NBContainerUtil.IsNullOrEmpty(m_materials))
		{
			return;
		}
		for (int i = 0; i < m_materials.Length; i++)
		{
			if (null != m_materials[i])
			{
				materials[i].SetColor("_TintColor", p_color);
			}
		}
	}

	private void LateUpdate()
	{
		if (NBContainerUtil.IsNullOrEmpty(m_materials))
		{
			base.enabled = false;
		}
		else
		{
			SetColor(m_tintColor);
		}
	}
}
