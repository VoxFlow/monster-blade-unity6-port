using UnityEngine;

[RequireComponent(typeof(Projector))]
public class ShadowProjector : MonoBehaviour
{
	public Transform m_reference;

	private Vector3 m_rotAngle = new Vector3(90f, 0f, 0f);

	private float m_yOffset;

	private float m_orignalYRef;

	private float m_maxFadeOutLength = 10f;

	private Projector m_projector;

	private Color m_shadowColor = new Color(0.25f, 0.25f, 0.25f, 1f);

	private SkinnedMeshRenderer[] m_meshRenderers;

	public Transform Reference
	{
		get
		{
			return m_reference;
		}
		set
		{
			m_reference = value;
			m_yOffset = Mathf.Abs(m_reference.position.y - base.transform.position.y);
			m_orignalYRef = base.transform.position.y;
			base.enabled = true;
		}
	}

	private void Awake()
	{
		m_projector = base.gameObject.GetComponent<Projector>();
		m_projector.material = new Material(m_projector.material);
		m_meshRenderers = MBHier.GetComponentsInChildren<SkinnedMeshRenderer>(base.transform.root.gameObject);
		if (m_reference != null)
		{
			m_yOffset = Mathf.Abs(m_reference.position.y - base.transform.position.y);
			m_orignalYRef = base.transform.position.y;
		}
		else
		{
			base.enabled = false;
		}
	}

	private void OnEnable()
	{
		if (GameManager.qualitySetting == GameManager.QualitySetting.Low || m_reference == null)
		{
			base.enabled = false;
		}
		else if (m_projector != null)
		{
			m_projector.enabled = true;
		}
	}

	private void OnDisable()
	{
		if (m_projector != null)
		{
			m_projector.enabled = false;
		}
	}

	private void UpdateFakeSoftShadow()
	{
		float num = 0f;
		num = ((!(base.transform.position.y < 0f)) ? Mathf.Clamp01((base.transform.position.y - m_orignalYRef) / m_maxFadeOutLength) : 1f);
		float num2 = 1f;
		if (m_meshRenderers != null)
		{
			for (int i = 0; i < m_meshRenderers.Length; i++)
			{
				if (!(null == m_meshRenderers[i]))
				{
					// PORT (CD-014)
					Material mat = m_meshRenderers[i].material;
					Color color = mat.HasProperty("_Color") ? mat.GetColor("_Color") : new Color(0f, 0f, 0f, 0f);
					if (color.a > num2)
					{
						num2 = color.a;
					}
				}
			}
			num2 = Mathf.Clamp01(1f - num2);
		}
		if (m_projector.material != null)
		{
			Color color2 = Color.Lerp(m_shadowColor, Color.white, num);
			Color color3 = Color.Lerp(m_shadowColor, Color.white, num2);
			Color color4 = ((!(color2.r > color3.r)) ? color3 : color2);
			m_projector.material.SetColor("_Color", color4);
		}
	}

	private void LateUpdate()
	{
		Vector3 zero = Vector3.zero;
		zero.x = m_reference.position.x;
		zero.y = m_reference.position.y + m_yOffset;
		zero.z = m_reference.position.z;
		base.transform.position = zero;
		UpdateFakeSoftShadow();
		base.transform.eulerAngles = m_rotAngle;
	}
}
