using UnityEngine;

public class TierStars : MonoBehaviour
{
	public UISprite[] stars = new UISprite[5];

	public int _tier;

	private bool _init;

	public int tier
	{
		get
		{
			return _tier;
		}
		set
		{
			_tier = value;
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
	}

	private void Start()
	{
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
				stars[i].enabled = false;
			}
			int num = Mathf.Clamp(_tier, 0, stars.Length);
			for (int j = 0; j < num; j++)
			{
				stars[j].enabled = true;
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
