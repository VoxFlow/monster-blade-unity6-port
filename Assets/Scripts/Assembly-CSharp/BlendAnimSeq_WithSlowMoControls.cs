using UnityEngine;

public class BlendAnimSeq_WithSlowMoControls : MonoBehaviour
{
	private string[] animationList = new string[5] { "A", "B", "C", "D", "E" };

	private float[] blendTime = new float[5] { 0f, 0.3f, 0.3f, 0.3f, 0.3f };

	private float[] timeScale = new float[5] { 1f, 1f, 1f, 1f, 1f };

	private float[] sequenceTime = new float[5] { 0f, 0.5f, 1f, 1.5f, 2f };

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
