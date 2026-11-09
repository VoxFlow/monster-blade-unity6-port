using UnityEngine;

public class BlendAnimSeq_Lose : MonoBehaviour
{
	private string[] animationList = new string[3] { "Death", "Recovery", "Idle" };

	private float[] blendTime = new float[3] { 0.3f, 0.3f, 2f };

	private float[] timeScale = new float[3] { 1f, 1f, 1f };

	private float[] sequenceTime = new float[3] { 0f, 10f, 14f };

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
