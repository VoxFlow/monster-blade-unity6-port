using UnityEngine;

public class UIButtonTouch : MonoBehaviour
{
	public GameObject gameObejctToCall;

	public string methodTocall;

	public void OnClick()
	{
		gameObejctToCall.SendMessage(methodTocall);
	}
}
