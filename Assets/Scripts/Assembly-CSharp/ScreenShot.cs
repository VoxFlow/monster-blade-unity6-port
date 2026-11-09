using System.IO;
using UnityEngine;

public class ScreenShot : MonoBehaviour
{
	public string folder = "ScreenshotFolder";

	public int frameRate = 25;

	private string realFolder = string.Empty;

	private void Start()
	{
		Time.captureFramerate = frameRate;
		realFolder = folder;
		int num = 1;
		while (Directory.Exists(realFolder))
		{
			realFolder = folder + num;
			num++;
		}
		Directory.CreateDirectory(realFolder);
	}

	private void Update()
	{
		string filename = string.Format("{0}/{1:D04} shot.png", realFolder, Time.frameCount);
		ScreenCapture.CaptureScreenshot(filename);
	}
}
