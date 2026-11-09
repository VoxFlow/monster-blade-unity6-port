using UnityEngine;

public class NBUIRatingStars : MonoBehaviour
{
	public UISprite[] stars = new UISprite[5];

	public int _ratingValue;

	private bool _init;

	public int ratingValue
	{
		get
		{
			return _ratingValue;
		}
		set
		{
			_ratingValue = value;
			Refresh();
		}
	}

	private void OnEnable()
	{
		Init();
	}

	private void OnDisable()
	{
		Reset();
	}

	private void Awake()
	{
		UISprite[] componentsInChildren = MBHier.GetComponentsInChildren<UISprite>(this);
		if (componentsInChildren != null && componentsInChildren.Length != 0 && stars.Length != componentsInChildren.Length)
		{
			stars = new UISprite[componentsInChildren.Length];
		}
		for (int i = 0; i < componentsInChildren.Length; i++)
		{
			stars[i] = componentsInChildren[i];
		}
	}

	private void Start()
	{
		Init();
	}

	private void Reset()
	{
		_init = false;
	}

	private void Init()
	{
		if (!_init)
		{
			for (int i = 0; i < stars.Length; i++)
			{
				stars[i].color = Color.grey;
			}
			int num = Mathf.Clamp(_ratingValue, 0, stars.Length);
			for (int j = 0; j < num; j++)
			{
				stars[j].color = Color.white;
			}
			_init = true;
		}
	}

	public void Refresh()
	{
		Reset();
		Init();
	}

	private void Update()
	{
	}
}
