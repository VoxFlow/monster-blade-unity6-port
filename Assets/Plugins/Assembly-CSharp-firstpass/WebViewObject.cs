using System;
using UnityEngine;

public class WebViewObject : MonoBehaviour
{
	private Action<string> callback;

	private AndroidJavaObject webView;

	private Vector2 offset;

	public void Init(Action<string> cb = null)
	{
#if UNITY_ANDROID && !UNITY_EDITOR
		callback = cb;
		offset = new Vector2(0f, 0f);
		webView = new AndroidJavaObject("net.gree.unitywebview.WebViewPlugin");
		webView.Call("Init", base.name);
#else
		callback = cb;
		offset = new Vector2(0f, 0f);
#endif
	}

	private void OnDestroy()
	{
#if UNITY_ANDROID && !UNITY_EDITOR
		if (webView != null)
		{
			webView.Call("Destroy");
		}
#else

#endif
	}

	public void SetMargins(int left, int top, int right, int bottom)
	{
#if UNITY_ANDROID && !UNITY_EDITOR
		if (webView != null)
		{
			offset = new Vector2(left, top);
			webView.Call("SetMargins", left, top, right, bottom);
		}
#else

#endif
	}

	public void SetVisibility(bool v)
	{
#if UNITY_ANDROID && !UNITY_EDITOR
		if (webView != null)
		{
			webView.Call("SetVisibility", v);
		}
#else

#endif
	}

	public void LoadURL(string url)
	{
#if UNITY_ANDROID && !UNITY_EDITOR
		if (webView != null)
		{
			webView.Call("LoadURL", url);
		}
#else

#endif
	}

	public void EvaluateJS(string js)
	{
#if UNITY_ANDROID && !UNITY_EDITOR
		if (webView != null)
		{
			webView.Call("LoadURL", "javascript:" + js);
		}
#else

#endif
	}

	public void CallFromJS(string message)
	{
		if (callback != null)
		{
			callback(message);
		}
	}
}
