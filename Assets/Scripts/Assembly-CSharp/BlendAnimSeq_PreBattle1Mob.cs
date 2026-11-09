using UnityEngine;

public class BlendAnimSeq_PreBattle1Mob : MonoBehaviour
{
	private string[] animationList = new string[4] { "Idle_01", "Eat", "Idle_03", "Idle_01" };

	private float[] sequenceTime = new float[4] { 0f, 3f, 6.5f, 7.2f };

	private float time;

	private int index;

	private void Awake()
	{
	}

	private void Update()
	{
		time += Time.deltaTime;
		if (time > sequenceTime[index])
		{
			base.GetComponent<Animation>().CrossFadeQueued(animationList[index], 0.3f, QueueMode.PlayNow);
			index++;
			if (index >= animationList.Length)
			{
				time = 0f;
				index = 0;
			}
		}
	}
}
