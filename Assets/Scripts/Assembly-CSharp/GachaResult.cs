using UnityEngine;

public class GachaResult : MonoBehaviour
{
	public enum Type
	{
		Social = 0,
		Premium = 1
	}

	public GameObject[] m_elements = new GameObject[5];

	private GameObject m_activeElement;

	public Type m_type;

	public Element.Type m_elementType;

	public Color[] m_elementColors = new Color[5];

	public UISprite m_background;

	public AnimatedUVs m_uvAnimBackground;

	public UISprite m_spriteHeader;

	public UISprite m_spriteFrame;

	public UISprite m_spriteItem;

	public bool m_display3DModel;

	public float m_3DModelRotationSpeed = -60f;

	public Transform m_3DModelRotationCenter;

	private Transform m_3DModelRoot;

	private GameObject m_3DModel;

	private GameObject m_3DModelCreated;

	private CharacterElement m_characterElement;

	[SerializeField]
	private UISprite m_statsSprite;

	[SerializeField]
	private UISprite m_resistSprite;

	public ItemInfo m_itemInfo;

	public GameObject[] m_particles;

	[SerializeField]
	private GameObject m_upperwardParticle;

	private bool m_showParticles;

	private bool m_isShowing;

	private bool m_isSkipping;

	private bool m_skippable;

	public Element.Type elementType
	{
		set
		{
			m_elementType = value;
			for (int i = 0; i < m_elements.Length; i++)
			{
				if (i != (int)m_elementType)
				{
					if ((bool)m_elements[i])
					{
						m_elements[i].SetActiveRecursively(false);
					}
					continue;
				}
				m_activeElement = m_elements[i];
				if ((bool)m_activeElement && m_showParticles)
				{
					m_activeElement.SetActiveRecursively(true);
					ParticleSystemController component = m_activeElement.GetComponent<ParticleSystemController>();
					if ((bool)component)
					{
						component.Clear();
						component.Simulate(0.5f);
						component.Play();
					}
				}
			}
		}
	}

	public GameObject activeElement
	{
		get
		{
			return m_activeElement;
		}
	}

	public bool IsShowing
	{
		get
		{
			return m_isShowing;
		}
		set
		{
			m_isShowing = value;
		}
	}

	public bool IsSkipping
	{
		get
		{
			return m_isSkipping;
		}
	}

	public bool CanBeSkipped
	{
		get
		{
			return m_skippable;
		}
		set
		{
			m_skippable = value;
		}
	}

	private void OnEnable()
	{
		m_isShowing = true;
		Dbg.Assert(m_elements.Length > 0, "Elements is 0");
		GameObject[] elements = m_elements;
		foreach (GameObject gameObject in elements)
		{
			Dbg.Assert(gameObject, "Missing Element Particle");
		}
		Dbg.Assert(m_particles.Length > 0, "Particles is 0");
		GameObject[] particles = m_particles;
		foreach (GameObject gameObject2 in particles)
		{
			Dbg.Assert(gameObject2, "Missing Particles");
		}
	}

	private void OnDisable()
	{
		m_isShowing = false;
		if (m_3DModelCreated != null)
		{
			CharacterElement.DestroyAssetBundleCharacterModel(m_3DModelCreated);
			m_3DModelCreated = null;
		}
		if (m_characterElement != null)
		{
			m_characterElement.ReleaseAssets();
			m_characterElement = null;
		}
	}

	private void Awake()
	{
		for (int i = 0; i < m_elements.Length; i++)
		{
			if ((bool)m_elements[i])
			{
				m_elements[i].SetActiveRecursively(false);
				if (i == (int)m_elementType)
				{
					m_activeElement = m_elements[i];
				}
			}
		}
		m_itemInfo.alpha = 0f;
		Type type = m_type;
		if (type != Type.Social && type == Type.Premium)
		{
			m_spriteHeader.spriteName = "ItemDetail_Header_Premium";
		}
		else
		{
			m_spriteHeader.spriteName = "ItemDetail_Header_Common";
		}
	}

	private void Update()
	{
		if (Input.GetKeyDown(KeyCode.Escape) && !base.GetComponent<Animation>().isPlaying)
		{
			SubtitlePopup subtitlePopup = Object.FindObjectOfType(typeof(SubtitlePopup)) as SubtitlePopup;
			if (null != subtitlePopup && subtitlePopup.gameObject.active)
			{
				subtitlePopup.gameObject.SendMessage("OnRelease", SendMessageOptions.DontRequireReceiver);
				return;
			}
			ItemInfo[] componentsInChildren = MBHier.GetComponentsInChildren<ItemInfo>(base.gameObject, true);
			if (componentsInChildren.Length > 0)
			{
				UIButtonMessage component = componentsInChildren[0].GetComponent<UIButtonMessage>();
				if (null != component)
				{
					component.target.SendMessage(component.functionName, base.gameObject, SendMessageOptions.DontRequireReceiver);
					return;
				}
			}
		}
		if ((MBInput.GetMouseButton(0) || (MBInput.touchCount > 0 && MBInput.touches[0].phase == TouchPhase.Ended)) && !m_isSkipping && m_skippable)
		{
			m_isSkipping = true;
			foreach (AnimationState item in base.GetComponent<Animation>())
			{
				item.normalizedTime = 1f;
			}
			m_particles[0].gameObject.SetActiveRecursively(false);
			ParticleSystem[] componentsInChildren2 = MBHier.GetComponentsInChildren<ParticleSystem>(m_particles[1]);
			ParticleSystem[] array = componentsInChildren2;
			foreach (ParticleSystem particleSystem in array)
			{
				particleSystem.startDelay = 0f;
				particleSystem.Play();
			}
			if (m_activeElement != null)
			{
				ParticleSystem[] componentsInChildren3 = MBHier.GetComponentsInChildren<ParticleSystem>(m_activeElement);
				ParticleSystem[] array2 = componentsInChildren3;
				foreach (ParticleSystem particleSystem2 in array2)
				{
					particleSystem2.startDelay = 0f;
					particleSystem2.Play();
				}
			}
		}
		if (m_3DModelRoot != null && m_3DModelRotationSpeed != 0f && 0.59f < m_spriteItem.transform.parent.localScale.x)
		{
			m_3DModelRoot.RotateAround(m_3DModelRotationCenter.position, Vector3.up, m_3DModelRotationSpeed * Time.deltaTime);
		}
	}

	public void EnableStatsSprite(bool p_enable)
	{
		if (m_statsSprite != null)
		{
			m_statsSprite.gameObject.SetActiveRecursively(p_enable);
		}
	}

	public void EnableResistSprite(bool p_enable)
	{
		if (m_resistSprite != null)
		{
			m_resistSprite.gameObject.SetActiveRecursively(p_enable);
		}
	}

	public void EnableUpperwardParticle(bool p_enable)
	{
		if (m_upperwardParticle != null)
		{
			m_upperwardParticle.SetActiveRecursively(p_enable);
		}
	}

	public void SetAnimationStartTime(float p_startTime)
	{
		base.GetComponent<Animation>()["Anim_Reveal"].time = p_startTime;
	}

	public void Show3DModel(ItemDetails item, GameObject obj, SkinnedMeshRenderer smr = null)
	{
		if (obj != null)
		{
			if (m_spriteFrame != null)
			{
				m_spriteFrame.gameObject.active = false;
			}
			if (m_spriteItem != null)
			{
				m_spriteItem.gameObject.active = false;
			}
			obj.active = true;
			if (obj.GetComponent<Animation>() != null)
			{
				obj.GetComponent<Animation>().Play("Custom_Idle_02");
				obj.GetComponent<Animation>().Sample();
				obj.GetComponent<Animation>().Stop();
				obj.GetComponent<Animation>().playAutomatically = false;
			}
			m_3DModel = obj;
			Transform transform = null;
			if (m_3DModelRotationCenter != null)
			{
				transform = m_3DModelRotationCenter.transform.Find(item.equipment.part.ToString());
			}
			Dbg.Assert(transform != null);
			if (smr == null)
			{
				smr = obj.GetComponent<Renderer>() as SkinnedMeshRenderer;
			}
			Bounds bounds = smr.sharedMesh.bounds;
			if (item.equipment.part == Equipment.Part.Weapon)
			{
				m_3DModelRoot = smr.bones[0];
			}
			else
			{
				m_3DModelRoot = obj.transform.GetChild(0);
			}
			if (transform != null)
			{
				Vector3 localScale = obj.transform.localScale;
				Quaternion localRotation = obj.transform.localRotation;
				Vector3 localPosition = obj.transform.localPosition;
				obj.transform.parent = transform;
				obj.transform.localScale = localScale;
				obj.transform.localRotation = localRotation;
				obj.transform.localPosition = localPosition;
			}
			else if (m_spriteItem != null)
			{
				float num = m_spriteItem.transform.localScale.x / bounds.size.magnitude;
				Vector3 localScale2 = obj.transform.localScale * num;
				Vector3 localPosition2 = -obj.transform.InverseTransformPoint(m_3DModelRoot.localToWorldMatrix * bounds.center);
				obj.transform.parent = m_3DModelRotationCenter;
				obj.transform.localScale = localScale2;
				obj.transform.localPosition = localPosition2;
			}
			smr.gameObject.layer = LayerMask.NameToLayer("PopUpLayer");
		}
	}

	public void Show3DModel(ItemDetails item)
	{
		if (m_display3DModel)
		{
			if (m_3DModelCreated != null)
			{
				CharacterElement.DestroyAssetBundleCharacterModel(m_3DModelCreated);
				m_3DModelCreated = null;
			}
			if (m_characterElement != null)
			{
				m_characterElement.ReleaseAssets();
				m_characterElement = null;
			}
			CharacterGenerator.GenerateCharacterElementModel(delegate(GameObject obj)
			{
				m_3DModelCreated = obj;
				m_characterElement = item.equipment.characterElement;
				Show3DModel(item, obj);
			}, item.equipment.characterElement);
		}
	}

	private void OnShowParticles()
	{
		m_showParticles = true;
		if ((bool)m_activeElement)
		{
			m_activeElement.SetActiveRecursively(true);
			ParticleSystemController component = m_activeElement.GetComponent<ParticleSystemController>();
			if ((bool)component)
			{
				component.Clear();
				component.Simulate(0.5f);
				component.Play();
			}
		}
	}
}
