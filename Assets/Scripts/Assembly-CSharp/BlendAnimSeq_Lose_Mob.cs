using UnityEngine;

public class BlendAnimSeq_Lose_Mob : MonoBehaviour
{
	private string[] animationList = new string[5] { "Taunt", "Idle_03", "Idle_05", "Taunt", "Idle_01" };

	private float[] blendTime = new float[5] { 0.3f, 0.3f, 0.3f, 0.3f, 0.3f };

	private float[] timeScale = new float[5] { 1f, 1f, 1f, 1f, 1f };

	private float[] sequenceTime = new float[5] { 0f, 2f, 3f, 11f, 13f };

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
			base.GetComponent<Animation>().CrossFadeQueued(animationList[index], blendTime[index], QueueMode.PlayNow);
			Time.timeScale = timeScale[index];
			index++;
			if (index >= animationList.Length)
			{
				base.enabled = false;
				index = 0;
				time = 0f;
			}
		}
	}
}
