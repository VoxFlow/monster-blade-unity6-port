using UnityEngine;

public class TitleDrangonAnimController : MonoBehaviour
{
	private string[] animationList = new string[4] { "Title_Idle_01", "Title_Idle_02", "Title_Idle_03", "Title_Idle_04" };

	private float timer;

	private int animIndex = -1;

	public int AnimIndex
	{
		get
		{
			return animIndex;
		}
		set
		{
			if (value != animIndex)
			{
				animIndex = value;
				switch (animIndex)
				{
				case 0:
					base.GetComponent<Animation>()[animationList[animIndex]].wrapMode = WrapMode.Loop;
					base.GetComponent<Animation>().CrossFadeQueued(animationList[animIndex], 0.35f, QueueMode.PlayNow);
					timer = Random.Range(5f, 6f);
					break;
				case 1:
					base.GetComponent<Animation>().CrossFadeQueued(animationList[animIndex], 1f, QueueMode.PlayNow);
					timer = base.GetComponent<Animation>()[animationList[animIndex]].length;
					break;
				case 2:
					base.GetComponent<Animation>()[animationList[animIndex]].wrapMode = WrapMode.Loop;
					base.GetComponent<Animation>().CrossFadeQueued(animationList[animIndex], 0.15f, QueueMode.PlayNow);
					timer = Random.Range(5f, 6f);
					break;
				case 3:
					base.GetComponent<Animation>().CrossFadeQueued(animationList[animIndex], 1f, QueueMode.PlayNow);
					timer = base.GetComponent<Animation>()[animationList[animIndex]].length;
					break;
				}
			}
		}
	}

	private void Awake()
	{
	}

	private void Start()
	{
	}

	private void Update()
	{
		timer -= Time.deltaTime;
		if (timer < 0f)
		{
			AnimIndex = (animIndex + 1) % animationList.Length;
		}
	}
}
