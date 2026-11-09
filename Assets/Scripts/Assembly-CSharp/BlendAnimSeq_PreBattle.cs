using UnityEngine;

public class BlendAnimSeq_PreBattle : MonoBehaviour
{
	private string[] animationList = new string[3] { "Idle", "Runloop", "Idle" };

	private float[] sequenceTime = new float[3] { 0f, 0.5f, 3f };

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
