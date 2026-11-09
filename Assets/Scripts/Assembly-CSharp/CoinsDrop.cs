using UnityEngine;

public class CoinsDrop : MonoBehaviour
{
	public float m_ascendSpeed = 150f;

	public float m_ascendDecel = 2f;

	private Vector3 m_ascendVec = Vector3.zero;

	private Vector3 m_position = Vector3.zero;

	private void Awake()
	{
		base.gameObject.SetActiveRecursively(false);
	}

	public void Show(Vector3 p_position)
	{
		m_position = p_position;
		m_ascendVec = Vector3.zero;
		m_ascendVec.y = m_ascendSpeed;
		base.gameObject.SetActiveRecursively(true);
	}

	public void Hide()
	{
		base.gameObject.SetActiveRecursively(false);
	}

	private void LateUpdate()
	{
		m_position += m_ascendVec * Time.deltaTime;
		m_ascendVec.y -= m_ascendDecel * Time.deltaTime;
		m_ascendVec.y = Mathf.Max(2f, m_ascendVec.y);
		base.transform.position = m_position;
	}
}
