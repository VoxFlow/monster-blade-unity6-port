using System.Collections.Generic;
using UnityEngine;

public class NotificationController : MonoBehaviour
{
	[SerializeField]
	private GameObject notificationGameObj;

	[SerializeField]
	private UILabel notificationTitle;

	[SerializeField]
	private UILabel notificationDesc;

	private Queue<AchievementManager.AchievementItem> notificationQueue = new Queue<AchievementManager.AchievementItem>();

	private AchievementManager.AchievementItem curShowingNotification;

	private bool checkQueue;

	private bool isShowing;

	public void Update()
	{
		if (checkQueue && !isShowing)
		{
			if (notificationQueue.Count > 0)
			{
				curShowingNotification = null;
				curShowingNotification = notificationQueue.Dequeue();
				notificationTitle.text = SingletonComponent<NBLocalization>.Instance.Get(curShowingNotification.title);
				notificationDesc.text = SingletonComponent<NBLocalization>.Instance.Get("achievement_completed");
				notificationGameObj.GetComponent<Animation>().Play();
			}
			checkQueue = false;
			isShowing = true;
		}
		if (curShowingNotification != null && isShowing && !notificationGameObj.GetComponent<Animation>().isPlaying)
		{
			if (notificationQueue.Count > 0)
			{
				checkQueue = true;
			}
			else
			{
				curShowingNotification = null;
				notificationGameObj.SetActiveRecursively(false);
			}
			isShowing = false;
		}
	}

	public void ShowNotification(AchievementManager.AchievementItem p_achiItem)
	{
		Dbg.Log("[Info] ShowNotification, achievement: " + p_achiItem.ToString());
		notificationQueue.Enqueue(p_achiItem);
		notificationGameObj.SetActiveRecursively(true);
		checkQueue = true;
	}
}
