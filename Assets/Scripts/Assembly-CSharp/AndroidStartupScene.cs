using System.Collections;
using UnityEngine;

public class AndroidStartupScene : MonoBehaviour
{
	private string m_expFilePath;

	public UILabel m_description;

	private string[] m_descTexts = new string[4] { "Downloading game data...\n(This may take several minutes)", "データをダウンロード中です。\n（作業には数分かかります。）", "奥戛正在努力加载游戏数据... \n (你上个厕所回来，大概就好了。)", "儲存空間不足，\n請清出儲存空間或啟用外部儲存設備。" };

	private void Start()
	{
		m_expFilePath = GooglePlayDownloader.GetExpansionFilePath();
		if (m_expFilePath == null)
		{
			Debug.LogError("External storage is not available!");
			base.enabled = false;
			return;
		}
		string mainOBBPath = GooglePlayDownloader.GetMainOBBPath(m_expFilePath);
		if (mainOBBPath == null)
		{
			StartCoroutine(DownloadOBBExpansion());
		}
		else
		{
			StartCoroutine(LoadNextScene());
		}
		base.enabled = false;
	}

	private IEnumerator DownloadOBBExpansion()
	{
		string mainObbPath = null;
		Debug.Log("Fetching OBB file.");
		GooglePlayDownloader.FetchOBB();
		do
		{
			mainObbPath = GooglePlayDownloader.GetMainOBBPath(m_expFilePath);
			yield return new MBWaitForSeconds(1f);   // PORT (CD-059)
		}
		while (mainObbPath == null);
		string uri = "file://" + mainObbPath;
		Debug.Log("Download finished. Initializing game data.");
		WWW www = WWW.LoadFromCacheOrDownload(uri, 0);
		while (!www.isDone)
		{
			if (m_description != null)
			{
				m_description.text = "Copying game data. Please wait.\nProgress: " + (int)(www.progress * 100f) + "%";
			}
			yield return new MBWaitForSeconds(1f);   // PORT (CD-059)
		}
		if (www.error != null)
		{
			Debug.LogError("Error downloading game data.");
		}
		else
		{
			StartCoroutine(LoadNextScene());
		}
	}

	private IEnumerator LoadNextScene()
	{
		Camera uiCam = MBHier.GetComponentInChildren<Camera>(base.gameObject);
		if (null != uiCam)
		{
			uiCam.gameObject.active = false;
		}
		yield return new WaitForEndOfFrame();
		Application.LoadLevel(1);
	}
}
