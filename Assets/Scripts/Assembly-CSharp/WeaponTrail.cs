using System.Collections.Generic;
using UnityEngine;

[AddComponentMenu("FX/Weapon Trail")]
[RequireComponent(typeof(MeshFilter))]
public class WeaponTrail : MonoBehaviour
{
	public Transform attchObj;

	public float height = 2f;

	public float time = 2f;

	public bool alwaysUp;

	public float minDistance = 0.1f;

	public float timeTransitionSpeed = 1f;

	public float desiredTime = 2f;

	public Color startColor = Color.white;

	public Color endColor = new Color(1f, 1f, 1f, 0f);

	private Vector3 position;

	private float now;

	private TronTrailSection currentSection;

	private Matrix4x4 localSpaceTransform;

	private Mesh mesh;

	private Vector3[] vertices;

	private Color[] colors;

	private Vector2[] uv;

	private MeshRenderer meshRenderer;

	private Material trailMaterial;

	private List<TronTrailSection> sections = new List<TronTrailSection>();

	private void Awake()
	{
		MeshFilter meshFilter = GetComponent(typeof(MeshFilter)) as MeshFilter;
		mesh = meshFilter.mesh;
		meshRenderer = GetComponent(typeof(MeshRenderer)) as MeshRenderer;
		trailMaterial = meshRenderer.material;
	}

	public void StartTrail(float timeToTweenTo, float fadeInTime)
	{
		desiredTime = timeToTweenTo;
		if (time != desiredTime)
		{
			timeTransitionSpeed = Mathf.Abs(desiredTime - time) / fadeInTime;
		}
		if (time <= 0f)
		{
			time = 0.01f;
		}
	}

	public void SetTime(float trailTime, float timeToTweenTo, float tweenSpeed)
	{
		time = trailTime;
		desiredTime = timeToTweenTo;
		timeTransitionSpeed = tweenSpeed;
		if (time <= 0f)
		{
			ClearTrail();
		}
	}

	public void FadeOut(float fadeTime)
	{
		desiredTime = 0f;
		if (time > 0f)
		{
			timeTransitionSpeed = time / fadeTime;
		}
	}

	public void SetTrailColor(Color color)
	{
		trailMaterial.SetColor("_TintColor", color);
	}

	public void Itterate(float itterateTime)
	{
		position = base.transform.position;
		now = itterateTime;
		if (sections.Count == 0 || (sections[0].point - position).sqrMagnitude > minDistance * minDistance)
		{
			TronTrailSection tronTrailSection = new TronTrailSection();
			tronTrailSection.point = position;
			if (alwaysUp)
			{
				tronTrailSection.upDir = Vector3.up;
			}
			else
			{
				tronTrailSection.upDir = base.transform.TransformDirection(Vector3.up);
			}
			tronTrailSection.time = now;
			sections.Insert(0, tronTrailSection);
		}
	}

	public void Update()
	{
		if (!base.transform.parent)
		{
			base.transform.parent = attchObj;
			base.transform.localPosition = Vector3.zero;
			base.transform.localEulerAngles = new Vector3(0f, 0f, 90f);
		}
		Itterate(Time.time);
		UpdateTrail(Time.time, Time.deltaTime);
	}

	public void UpdateTrail(float currentTime, float deltaTime)
	{
		mesh.Clear();
		while (sections.Count > 0 && currentTime > sections[sections.Count - 1].time + time)
		{
			sections.RemoveAt(sections.Count - 1);
		}
		if (sections.Count < 2)
		{
			return;
		}
		vertices = new Vector3[sections.Count * 2];
		colors = new Color[sections.Count * 2];
		uv = new Vector2[sections.Count * 2];
		currentSection = sections[0];
		localSpaceTransform = base.transform.worldToLocalMatrix;
		for (int i = 0; i < sections.Count; i++)
		{
			currentSection = sections[i];
			float num = 0f;
			if (i != 0)
			{
				num = Mathf.Clamp01((currentTime - currentSection.time) / time);
			}
			Vector3 upDir = currentSection.upDir;
			vertices[i * 2] = localSpaceTransform.MultiplyPoint(currentSection.point);
			vertices[i * 2 + 1] = localSpaceTransform.MultiplyPoint(currentSection.point + upDir * height);
			uv[i * 2] = new Vector2(num, 0f);
			uv[i * 2 + 1] = new Vector2(num, 1f);
			Color color = Color.Lerp(startColor, endColor, num);
			colors[i * 2] = color;
			colors[i * 2 + 1] = color;
		}
		int[] array = new int[(sections.Count - 1) * 2 * 3];
		for (int j = 0; j < array.Length / 6; j++)
		{
			array[j * 6] = j * 2;
			array[j * 6 + 1] = j * 2 + 1;
			array[j * 6 + 2] = j * 2 + 2;
			array[j * 6 + 3] = j * 2 + 2;
			array[j * 6 + 4] = j * 2 + 1;
			array[j * 6 + 5] = j * 2 + 3;
		}
		mesh.vertices = vertices;
		mesh.colors = colors;
		mesh.uv = uv;
		mesh.triangles = array;
		if (time > desiredTime)
		{
			time -= deltaTime * timeTransitionSpeed;
			if (time <= desiredTime)
			{
				time = desiredTime;
			}
		}
		else if (time < desiredTime)
		{
			time += deltaTime * timeTransitionSpeed;
			if (time >= desiredTime)
			{
				time = desiredTime;
			}
		}
	}

	public void ClearTrail()
	{
		desiredTime = 0f;
		time = 0f;
		if (mesh != null)
		{
			mesh.Clear();
			sections.Clear();
		}
	}
}
