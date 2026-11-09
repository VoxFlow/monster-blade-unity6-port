using UnityEngine;

public class BlendAnimSeq_MascotEvolve : MonoBehaviour
{
	private string[] animationList = new string[2] { "Excited", "Fusion" };

	private float[] sequenceTime = new float[2] { 0f, 2f };

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
