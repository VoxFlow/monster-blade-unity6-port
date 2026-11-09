using System;
using UnityEngine;

[Serializable]
public class CutsceneEvent : IComparable
{
	public enum Action
	{
		Acitvate = 0,
		Deacitvate = 1,
		PlayAnim = 2,
		PlayParticle = 3,
		StopParticle = 4,
		SetPos = 5,
		SetOrient = 6
	}

	public static string[] actionList = new string[7] { "Activate", "Deactivate", "Play Animation", "Play Particle Effect", "Stop Particle Effect", "Set Position", "Set Orientation" };

	public float time;

	public string actor = string.Empty;

	public Action action = Action.Deacitvate;

	public string stringParam = string.Empty;

	public Vector3 vectorParam = Vector3.zero;

	public Quaternion quaternionParam = Quaternion.identity;

	public int CompareTo(object obj)
	{
		CutsceneEvent cutsceneEvent = (CutsceneEvent)obj;
		if (time < cutsceneEvent.time)
		{
			return -1;
		}
		if (time == cutsceneEvent.time)
		{
			if (action == cutsceneEvent.action)
			{
				return 0;
			}
			if (action == Action.Deacitvate || cutsceneEvent.action == Action.Acitvate)
			{
				return -1;
			}
			if (action == Action.Acitvate || cutsceneEvent.action == Action.Deacitvate)
			{
				return 1;
			}
			return 0;
		}
		return 1;
	}
}
