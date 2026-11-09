using UnityEngine;

public class CutsceneHydraFade : MonoBehaviour
{
	private enum Head
	{
		Left = 0,
		Right = 1,
		Middle = 2
	}

	private float[] delay = new float[3] { 0f, 1f, 2f };

	private Head[] fadeHades = new Head[3]
	{
		Head.Middle,
		Head.Left,
		Head.Right
	};

	private float time;

	private int index;

	private Transform[] heads = new Transform[3];

	private Transform[] necks = new Transform[3];

	private SkinnedMeshRenderer[] headRenderers = new SkinnedMeshRenderer[3];

	public GameObject fxSpiralPrefab;

	public Material wispMaterial;

	public Shader headFadeShader;

	private void Start()
	{
		heads[0] = base.transform.FindInHierarchy("Mob05_LHead");
		heads[1] = base.transform.FindInHierarchy("Mob05_RHead");
		heads[2] = base.transform.FindInHierarchy("Mob05_MHead");
		necks[0] = base.transform.FindInHierarchy("Mob05 Neck Left");
		necks[1] = base.transform.FindInHierarchy("Mob05 Neck Right");
		necks[2] = base.transform.FindInHierarchy("Mob05 Neck1");
		headRenderers[0] = heads[0].gameObject.GetComponent<SkinnedMeshRenderer>();
		headRenderers[1] = heads[1].gameObject.GetComponent<SkinnedMeshRenderer>();
		headRenderers[2] = heads[2].gameObject.GetComponent<SkinnedMeshRenderer>();
		heads[0].gameObject.AddComponent<ModelFader>();
		heads[1].gameObject.AddComponent<ModelFader>();
		heads[2].gameObject.AddComponent<ModelFader>();
		AnimatedUVs animatedUVs = heads[0].gameObject.AddComponent<AnimatedUVs>();
		animatedUVs.enabled = false;
		animatedUVs = heads[1].gameObject.AddComponent<AnimatedUVs>();
		animatedUVs.enabled = false;
		animatedUVs = heads[2].gameObject.AddComponent<AnimatedUVs>();
		animatedUVs.enabled = false;
	}

	public virtual void Update()
	{
		time += Time.deltaTime;
		if (time > delay[index])
		{
			DisableHead(fadeHades[index]);
			index++;
			if (index >= fadeHades.Length)
			{
				base.enabled = false;
				time = 0f;
				index = 0;
			}
		}
	}

	private void DisableHead(Head p_head)
	{
		ModelFader component = heads[(int)p_head].gameObject.GetComponent<ModelFader>();
		component.fadeShader = headFadeShader;
		Renderer renderer = heads[(int)p_head].gameObject.GetComponent<Renderer>();
		renderer.material = wispMaterial;
		renderer.enabled = false;
		renderer.enabled = true;
		AnimatedUVs component2 = heads[(int)p_head].gameObject.GetComponent<AnimatedUVs>();
		component2.enabled = true;
		GameObject gameObject = Object.Instantiate(fxSpiralPrefab) as GameObject;
		gameObject.transform.parent = necks[(int)p_head];
		gameObject.transform.localPosition = new Vector3(-2.1f, 0.7f, 0.8f);
		gameObject.transform.localRotation = Quaternion.Euler(347f, 278f, 350f);
		component.Fade(1f, 0f, 2.5f, OnFadeOutDone);
	}

	private void OnFadeOutDone(Renderer p_renderer)
	{
		AnimatedUVs component = p_renderer.GetComponent<AnimatedUVs>();
		component.enabled = false;
	}
}
