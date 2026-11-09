using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class CamPath : MonoBehaviour
{
	public string pathName;

	public float timeTaken;

	public float timeDelay;

	public iTween.EaseType easeType = iTween.EaseType.linear;

	public bool playOnStart;

	private iTweenPath m_iTweenPath;

	private List<Vector3> m_nodes;

	private void Awake()
	{
		m_iTweenPath = MBHier.GetComponentInChildren<iTweenPath>(base.gameObject);
		m_nodes = new List<Vector3>();
		foreach (Vector3 node in m_iTweenPath.nodes)
		{
			m_nodes.Add(node);
		}
	}

	private IEnumerator Start()
	{
		if (playOnStart)
		{
			yield return new WaitForEndOfFrame();
			Move();
		}
	}

	public void Move()
	{
		for (int i = 0; i < m_iTweenPath.nodes.Count; i++)
		{
			m_iTweenPath.nodes[i] = base.transform.root.TransformPoint(m_nodes[i]);
		}
		base.transform.position = m_iTweenPath.nodes[0];
		iTween.MoveTo(base.gameObject, iTween.Hash("path", iTweenPath.GetPath(pathName), "time", timeTaken, "delay", timeDelay, "easetype", easeType));
	}
}
