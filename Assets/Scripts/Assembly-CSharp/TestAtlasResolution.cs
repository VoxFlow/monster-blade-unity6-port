using System.Collections.Generic;
using UnityEngine;

public class TestAtlasResolution : MonoBehaviour
{
	public UIAtlas[] refs;

	public UIAtlas[] hd;

	public UIAtlas[] sd;

	private Dictionary<string, UIAtlas> hdMap = new Dictionary<string, UIAtlas>();

	private Dictionary<string, UIAtlas> sdMap = new Dictionary<string, UIAtlas>();

	private string buttonLabel = "Switch to HD";

	private bool isHD;

	private void Awake()
	{
		UIAtlas[] array = hd;
		foreach (UIAtlas uIAtlas in array)
		{
			hdMap.Add(uIAtlas.name, uIAtlas);
		}
		UIAtlas[] array2 = sd;
		foreach (UIAtlas uIAtlas2 in array2)
		{
			sdMap.Add(uIAtlas2.name, uIAtlas2);
		}
	}

	private void Start()
	{
	}

	private void OnGUI()
	{
		if (GUI.Button(new Rect(300f, 300f, 250f, 50f), buttonLabel))
		{
			isHD = !isHD;
			buttonLabel = ((!isHD) ? "Switch to SD" : "Switch to HD");
			Dictionary<string, UIAtlas> dictionary = ((!isHD) ? sdMap : hdMap);
			UIAtlas[] array = refs;
			foreach (UIAtlas uIAtlas in array)
			{
				uIAtlas.replacement = dictionary[uIAtlas.replacement.name];
			}
		}
	}
}
