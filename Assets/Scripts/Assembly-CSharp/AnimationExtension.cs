using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public static class AnimationExtension
{
	public static List<string> FindAnimation(this Animation p_anim, string p_name)
	{
		List<string> list = new List<string>();
		string value = p_name.ToLower();
		string text = "Animation Found Containing (" + p_name + ") :\n";
		foreach (AnimationState item in p_anim)
		{
			string text2 = item.name.ToLower();
			if (text2.Contains(value))
			{
				list.Add(item.name);
				text = text + item.name + "\n";
			}
		}
		if (list.Count < 1)
		{
			Dbg.LogError("No Animation found with key : " + p_name);
			return null;
		}
		Dbg.Log(text);
		return list;
	}

	public static List<string> FindAnimationWithPrefix(this Animation p_anim, string p_prefix)
	{
		List<string> list = new List<string>();
		string value = p_prefix.ToLower();
		string text = "Animation Found Starting with (" + p_prefix + ") :\n";
		foreach (AnimationState item in p_anim)
		{
			string text2 = item.name.ToLower();
			if (text2.StartsWith(p_prefix))
			{
				string[] array = text2.Split('_');
				if (array[0].Contains(value))
				{
					list.Add(item.name);
					text = text + item.name + "\n";
				}
			}
		}
		if (list.Count < 1)
		{
			Dbg.LogError("No Animation found with key : " + p_prefix);
			return null;
		}
		Dbg.Log(text);
		return list;
	}

	public static IEnumerator Play(this Animation p_animation, string p_clipName, bool p_ignoreTimeScale, Action p_onComplete = null)
	{
		if (p_ignoreTimeScale)
		{
			AnimationState currState = p_animation[p_clipName];
			if (currState == null)
			{
				yield break;
			}
			float progressTime = 0f;
			float lastTime = 0f;
			float currTime = 0f;
			float deltaTime = 0f;
			p_animation.Play(p_clipName);
			lastTime = UnityEngine.Time.realtimeSinceStartup;
			while ((bool)p_animation && p_animation.isPlaying)
			{
				currTime = UnityEngine.Time.realtimeSinceStartup;
				deltaTime = currTime - lastTime;
				lastTime = currTime;
				progressTime += deltaTime;
				if (currState.length == 0f)
				{
					currState.normalizedTime = 1f;
				}
				else
				{
					currState.normalizedTime = progressTime / currState.length;
				}
				p_animation.Sample();
				if (progressTime >= currState.length)
				{
					if (currState.wrapMode != WrapMode.Loop)
					{
						p_animation.Stop();
					}
					else
					{
						progressTime = 0f;
					}
				}
				yield return null;
			}
			yield return null;
			if (p_onComplete != null)
			{
				p_onComplete();
			}
		}
		else
		{
			p_animation.Play(p_clipName);
		}
	}
}
