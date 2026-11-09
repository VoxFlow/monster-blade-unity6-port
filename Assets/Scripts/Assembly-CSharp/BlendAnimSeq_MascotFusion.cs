using UnityEngine;

public class BlendAnimSeq_MascotFusion : MonoBehaviour
{
	private string[] animationList = new string[3] { "Happy", "Excited", "Angry" };

	private float[] sequenceTime = new float[3] { 0f, 2f, 3.5f };

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
				base.enabled = false;
			}
		}
	}
}
