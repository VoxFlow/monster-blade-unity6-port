using UnityEngine;

public class FusionFX : MonoBehaviour
{
	public UISprite baseSprite;

	public UISprite baseFrame;

	public UISprite[] otherSprites = new UISprite[6];

	public UISprite[] otherFrames = new UISprite[6];

	public UISprite newItemSprite;

	public FusionMaterialItem baseItem;

	public FusionMaterialItem[] items;

	public ParticleSystem[] specialEffects;

	private void Awake()
	{
		specialEffects = MBHier.GetComponentsInChildren<ParticleSystem>(base.transform);
	}

	private void Start()
	{
	}

	private void Update()
	{
	}

	private void OnFlash()
	{
		if ((bool)baseSprite && (bool)newItemSprite)
		{
			baseSprite.atlas = newItemSprite.atlas;
			baseSprite.spriteName = newItemSprite.spriteName;
		}
	}

	public void EnableAllParticleEffect(bool p_enable)
	{
		ParticleSystem[] array = specialEffects;
		foreach (ParticleSystem particleSystem in array)
		{
			particleSystem.enableEmission = p_enable;
		}
	}
}
