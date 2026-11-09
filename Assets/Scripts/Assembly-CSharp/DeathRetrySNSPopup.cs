using UnityEngine;

public class DeathRetrySNSPopup : MonoBehaviour
{
	public GameObject fbButton;

	public GameObject renrenButton;

	public GameObject twitterButton;

	public GameObject weiboButton;

	private void Start()
	{
		if ((bool)renrenButton)
		{
			renrenButton.SetActiveRecursively(false);
		}
		if ((bool)twitterButton)
		{
			twitterButton.SetActiveRecursively(false);
		}
	}

	private void OnEnable()
	{
		if (null != weiboButton)
		{
			weiboButton.SetActiveRecursively(false);
		}
		if (null != fbButton)
		{
			fbButton.SetActiveRecursively(true);
		}
	}
}
