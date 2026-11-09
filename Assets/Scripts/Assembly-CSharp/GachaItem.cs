using System.Collections;
using UnityEngine;

public class GachaItem : MonoBehaviour
{
	public ChangeNGUIAlpha[] alphaEffects = new ChangeNGUIAlpha[2];

	public GameObject eventTarget;

	public UISprite sprite;

	public UISprite frame;

	private ItemDetails _itemDetails;

	public ItemDetails itemDetails
	{
		get
		{
			return _itemDetails;
		}
		set
		{
			_itemDetails = value;
			sprite.atlas = _itemDetails.BGAtlas;
			sprite.spriteName = _itemDetails.BGSpriteName;
			frame.spriteName = _itemDetails.ElementSprite;
		}
	}

	private void Start()
	{
		sprite.enabled = false;
		frame.enabled = false;
	}

	public void PlayEffects()
	{
		StartCoroutine(DoPlayEffects());
	}

	public IEnumerator DoPlayEffects()
	{
		base.GetComponent<Animation>().Play();
		ParticleSystem[] particles = MBHier.GetComponentsInChildren<ParticleSystem>(this);
		ParticleSystem[] array = particles;
		foreach (ParticleSystem p in array)
		{
			p.Play();
		}
		ChangeNGUIAlpha[] array2 = alphaEffects;
		foreach (ChangeNGUIAlpha a in array2)
		{
			a.enabled = true;
		}
		yield return null;
		base.transform.localPosition = Vector3.zero;
		sprite.enabled = true;
		frame.enabled = true;
	}

	private void OnClicked()
	{
		eventTarget.SendMessage("OnGachaItemSelected", base.gameObject, SendMessageOptions.DontRequireReceiver);
	}
}
