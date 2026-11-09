using UnityEngine;

public class BlendAnimSeq_DungeonProg_Male01 : MonoBehaviour
{
	private string[] animationList = new string[2] { "Idle", "Run" };

	private float[] timeScale = new float[2] { 1f, 1f };

	private float[] sequenceTime = new float[2] { 0f, 2f };

	private float time;

	private int index;

	private void Awake()
	{
		base.GetComponent<Animation>().Play(animationList[0]);
		base.GetComponent<Animation>().Sample();
		base.GetComponent<Animation>().Stop();
	}

	private void Update()
	{
		time += Time.deltaTime / Time.timeScale;
		if (time > sequenceTime[index])
		{
			base.GetComponent<Animation>().CrossFadeQueued(animationList[index], 0.3f, QueueMode.PlayNow);
			Time.timeScale = timeScale[index];
			index++;
			if (index >= animationList.Length)
			{
				base.enabled = false;
			}
		}
	}
}
