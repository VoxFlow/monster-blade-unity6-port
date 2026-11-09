using UnityEngine;

[RequireComponent(typeof(Animation))]
public class MascotFacial : MonoBehaviour
{
	public enum FaceType
	{
		Default = 0,
		Angry1 = 1,
		Angry2 = 2,
		Cry1 = 3,
		Cry2 = 4,
		Evil1 = 5,
		Evil2 = 6,
		Growl1 = 7,
		Happy1 = 8,
		LookAway1 = 9,
		LookAway2 = 10,
		Love1 = 11,
		Love2 = 12,
		Love3 = 13,
		Moody1 = 14,
		Moody2 = 15,
		Oooh1 = 16,
		Pain1 = 17,
		Shameful1 = 18,
		Shameful2 = 19,
		Shameful3 = 20,
		Smile1 = 21,
		Smug1 = 22,
		Stun1 = 23,
		Stun2 = 24,
		Stun3 = 25,
		Stun4 = 26,
		Stun5 = 27,
		Tear1 = 28,
		Tear2 = 29,
		Worry1 = 30,
		Worry2 = 31
	}

	public bool m_loop;

	public FaceType[] animationList = new FaceType[4]
	{
		FaceType.Default,
		FaceType.Angry1,
		FaceType.Cry1,
		FaceType.Love1
	};

	public float[] sequenceTime = new float[4] { 0f, 2f, 4f, 6f };

	public Texture2D[] m_faceTextures;

	private FaceType m_faceType;

	private Transform m_mascotFace;

	private Renderer m_faceRenderer;

	private float m_expressionTime;

	private int m_index;

	public FaceType faceType
	{
		get
		{
			return m_faceType;
		}
		set
		{
			m_faceType = value;
			Dbg.Log("Change Ocha face: " + value);
			if ((bool)m_faceRenderer)
			{
				int num = (int)m_faceType;
				if (num < m_faceTextures.Length)
				{
					m_faceRenderer.material.mainTexture = m_faceTextures[num];
				}
				else
				{
					Dbg.LogWarn("Face index is out of range: " + num);
				}
				m_faceRenderer.enabled = true;
				Dbg.Log("Face Renderer. Enable: " + m_faceRenderer.enabled + ". Shader: " + m_faceRenderer.sharedMaterial.shader.ToString());
			}
			else
			{
				Dbg.LogWarn("Face Renderer is null!");
			}
		}
	}

	private void Awake()
	{
		m_mascotFace = base.transform.Find("Mascot_Facial");
		m_faceRenderer = m_mascotFace.GetComponent<Renderer>();
	}

	private void Start()
	{
	}

	private void Update()
	{
		if (m_faceRenderer != null && !m_faceRenderer.enabled)
		{
			Dbg.LogWarn("Face renderer was disabled.");
			m_faceRenderer.enabled = true;
		}
		m_expressionTime += Time.deltaTime;
		if (m_index >= sequenceTime.Length)
		{
			return;
		}
		while (m_expressionTime > sequenceTime[m_index])
		{
			faceType = animationList[m_index];
			if (++m_index >= sequenceTime.Length)
			{
				if (m_loop)
				{
					m_expressionTime = 0f;
					m_index = 0;
				}
				break;
			}
		}
	}
}
