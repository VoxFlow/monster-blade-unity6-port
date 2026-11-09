using System;
using System.Collections;
using UnityEngine;

public class CleanUpScene : MonoBehaviour
{
	private GameObject m_motherGo;

	public void Start()
	{
		m_motherGo = GameObject.Find("Game");
		m_motherGo.SendMessage("OnCleanUpBegin", SendMessageOptions.DontRequireReceiver);
		if (UIPopupController.Instance != null)
		{
			UIPopupController.Instance.ShowLoadingPopup(SingletonComponent<NBLocalization>.Instance.Get("common_loading_cleaning_header"), SingletonComponent<NBLocalization>.Instance.Get("common_loading_please_wait"));
		}
		StartCoroutine(DoCleanUp());
	}

	private IEnumerator DoCleanUp()
	{
		if (SingletonComponent<DLC>.Instance != null)
		{
			SingletonComponent<DLC>.Instance.CheckForCleanUp();
		}
		Resources.UnloadUnusedAssets();
		GC.Collect();
		GC.WaitForPendingFinalizers();
		Dbg.Log(" Perform Cleanup ");
		yield return null;
		OnFinishCleanup();
	}

	private void OnFinishCleanup()
	{
		if (UIPopupController.Instance != null)
		{
			UIPopupController.Instance.HideLoadingPopup();
		}
		m_motherGo.SendMessage("OnCleanUpEnd", SendMessageOptions.DontRequireReceiver);
	}
}
