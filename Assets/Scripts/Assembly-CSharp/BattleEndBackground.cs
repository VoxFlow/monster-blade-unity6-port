using UnityEngine;

public class BattleEndBackground : MonoBehaviour
{
	[SerializeField]
	private UISprite[] m_childUiSprites;

	public BattleCompleteScreen m_screenRef;

	private Animation cachedAnim;

	private bool showContent;

	private float delayShowContentTimer = 0.1f;

	public void Show()
	{
		cachedAnim["Anim_BattleCleared"].speed = 0.35f;
		cachedAnim.Play();
	}

	private void Awake()
	{
		cachedAnim = base.GetComponent<Animation>();
	}

	private void Start()
	{
	}

	private void Update()
	{
		if (cachedAnim["Anim_BattleCleared"].time > 0.5f && !showContent)
		{
			cachedAnim.Stop();
			showContent = true;
		}
		if (showContent && delayShowContentTimer > 0f)
		{
			delayShowContentTimer -= Time.deltaTime;
			if (!(delayShowContentTimer <= 0f))
			{
			}
		}
	}

	public void EnableChildSprites(bool p_enable)
	{
		UISprite[] childUiSprites = m_childUiSprites;
		foreach (UISprite uISprite in childUiSprites)
		{
			uISprite.enabled = p_enable;
		}
	}
}
