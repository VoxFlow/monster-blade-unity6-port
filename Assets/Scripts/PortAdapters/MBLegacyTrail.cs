// Unity 4.0.1's trail renderer: per-camera mesh and linear width taper. Register CD-018.
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Rendering;

[DefaultExecutionOrder(10000)]
[RequireComponent(typeof(TrailRenderer))]
public class MBLegacyTrail : MonoBehaviour
{
	private TrailRenderer m_trail;

	private float m_time, m_start, m_end, m_minDist;

	private bool m_autodestruct, m_wasDrawing;

	private readonly uint[] m_colors = new uint[5];

	private readonly List<Vector3> m_points = new List<Vector3>();

	private readonly List<double> m_times = new List<double>();

	private readonly Dictionary<Camera, Mesh> m_meshes = new Dictionary<Camera, Mesh>();

	private void Awake()
	{
		m_trail = GetComponent<TrailRenderer>();
		m_time = m_trail.time;
		m_minDist = m_trail.minVertexDistance;
		m_autodestruct = m_trail.autodestruct;
		Keyframe[] k = m_trail.widthCurve.keys;
		m_start = m_trail.widthMultiplier * (k.Length > 0 ? k[0].value : 1f);
		m_end = m_trail.widthMultiplier * (k.Length > 0 ? k[k.Length - 1].value : 1f);
		Gradient g = m_trail.colorGradient;
		for (int i = 0; i < 5; i++)
		{
			Color32 c = g.Evaluate(i / 4f);
			m_colors[i] = (uint)(c.r | (c.g << 8) | (c.b << 16) | (c.a << 24));
		}
		m_trail.emitting = false;
		m_trail.autodestruct = false;
		m_trail.Clear();
		transform.hasChanged = false;
	}

	private void OnEnable()
	{
		Camera.onPreCull += Draw;
	}

	private void OnDisable()
	{
		Camera.onPreCull -= Draw;
	}

	private void OnDestroy()
	{
		foreach (Mesh m in m_meshes.Values) Destroy(m);
	}

	private void LateUpdate()
	{
		if (!m_trail.enabled) return;
		double now = Time.timeAsDouble;
		while (m_points.Count > 0 && now > m_times[m_times.Count - 1] + m_time)
		{
			m_points.RemoveAt(m_points.Count - 1);
			m_times.RemoveAt(m_times.Count - 1);
		}
		if (transform.hasChanged)
		{
			Vector3 pos = transform.position;
			if (m_points.Count == 0 || m_minDist * m_minDist < (m_points[0] - pos).sqrMagnitude)
			{
				m_points.Insert(0, pos);
				m_times.Insert(0, now);
			}
			transform.hasChanged = false;
		}
		if (m_points.Count <= 1)
		{
			if (m_autodestruct && m_wasDrawing) Destroy(gameObject);
		}
		else
		{
			m_wasDrawing = true;
		}
	}

	private void Draw(Camera cam)
	{
		int n = m_points.Count;
		if (n <= 1 || !m_trail.enabled || !isActiveAndEnabled || (cam.cullingMask & (1 << gameObject.layer)) == 0) return;
		Material mat = m_trail.sharedMaterial;
		if (mat == null) return;
		Mesh mesh;
		if (!m_meshes.TryGetValue(cam, out mesh) || mesh == null)
		{
			mesh = new Mesh { name = "LegacyTrail" };
			mesh.MarkDynamic();
			m_meshes[cam] = mesh;
		}
		Matrix4x4 view = cam.worldToCameraMatrix, back = cam.cameraToWorldMatrix;
		var pv = new Vector3[n];
		for (int i = 0; i < n; i++) pv[i] = view.MultiplyPoint3x4(i == 0 ? transform.position : m_points[i]);
		var verts = new Vector3[2 * n];
		var uvs = new Vector2[2 * n];
		var cols = new Color32[2 * n];
		Vector3 d = pv[0] - pv[1];
		Vector3 lo = new Vector3(float.MaxValue, float.MaxValue, float.MaxValue), hi = -lo;
		for (int i = 0; i < n; i++)
		{
			float t = (float)i / (n - 1);
			Vector3 p = pv[i];
			if (i + 1 < n) d = p - pv[i + 1];
			float a = d.x * p.z - d.z * p.x;
			float b = d.z * p.y - d.y * p.z;
			float len2 = a * a + b * b;
			float half = ((1f - t) * m_start + t * m_end) * 0.5f;
			float ox = 0f, oy = 0f;
			if (len2 != 0f)
			{
				float inv = 1f / Mathf.Sqrt(len2);
				oy = half * a * inv;
				ox = half * b * inv;
			}
			verts[2 * i] = back.MultiplyPoint3x4(new Vector3(p.x - ox, p.y - oy, p.z));
			verts[2 * i + 1] = back.MultiplyPoint3x4(new Vector3(p.x + ox, p.y + oy, p.z));
			uvs[2 * i] = new Vector2(t, 1f);
			uvs[2 * i + 1] = new Vector2(t, 0f);
			Color32 c = ColorAt(t);
			cols[2 * i] = c;
			cols[2 * i + 1] = c;
			Vector3 w = i == 0 ? transform.position : m_points[i];
			lo = Vector3.Min(lo, w);
			hi = Vector3.Max(hi, w);
		}
		var tris = new int[(2 * n - 2) * 3];
		for (int k = 0; k < 2 * n - 2; k++)
		{
			bool even = (k & 1) == 0;
			tris[3 * k] = even ? k : k + 1;
			tris[3 * k + 1] = even ? k + 1 : k;
			tris[3 * k + 2] = k + 2;
		}
		mesh.Clear();
		mesh.vertices = verts;
		mesh.uv = uvs;
		mesh.colors32 = cols;
		mesh.triangles = tris;
		float pad = Mathf.Max(m_start, m_end) * 0.5f;
		var bounds = new Bounds();
		bounds.SetMinMax(lo - Vector3.one * pad, hi + Vector3.one * pad);
		mesh.bounds = bounds;
		Graphics.DrawMesh(mesh, Matrix4x4.identity, mat, gameObject.layer, cam, 0, null, ShadowCastingMode.Off, false);
	}

	public Mesh MeshFor(Camera cam)
	{
		Mesh m;
		return m_meshes.TryGetValue(cam, out m) ? m : null;
	}

	private Color32 ColorAt(float t)
	{
		uint v = (uint)(t * 262140f);
		int key = (int)(v >> 16);
		int f = (int)((v >> 8) & 0xFF);
		int c0 = (int)m_colors[key], c1 = (int)m_colors[key + 1];
		int rb = ((c0 & 0xFF00FF) + ((((c1 & 0xFF00FF) - (c0 & 0xFF00FF)) * f) >> 8)) & 0xFF00FF;
		int ga = (int)(((uint)c0 & 0xFF00FF00u) + (uint)((((int)(((uint)c1 >> 8) & 0xFF00FF)) - (int)(((uint)c0 & 0xFF00FF00u) >> 8)) * f)) & unchecked((int)0xFF00FF00u);
		uint packed = (uint)(rb | ga);
		return new Color32((byte)packed, (byte)(packed >> 8), (byte)(packed >> 16), (byte)(packed >> 24));
	}
}
