using UnityEngine;

public class Background3D : MonoBehaviour
{
	[SerializeField]
	private AnimatedUVs m_groundAnimUv;

	[SerializeField]
	private AnimatedUVs m_skyAnimUv;

	private void Awake()
	{
	}

	private void OnEnable()
	{
		Dbg.Log("[Info] Pet hunting background " + base.gameObject.name + " enabled");
	}

	private void OnDisable()
	{
		Dbg.Log("[Info] Pet hunting background disabled");
	}

	private void Start()
	{
	}

	public void EnableRolling(bool p_enable)
	{
		if (m_groundAnimUv != null)
		{
			m_groundAnimUv.enabled = p_enable;
		}
		if (m_skyAnimUv != null)
		{
			m_skyAnimUv.enabled = p_enable;
		}
	}
}
