using UnityEngine;

public static class AnimationStateExtension
{
	public static int GetFrame(this AnimationState p_anim)
	{
		return (int)(p_anim.time * p_anim.clip.frameRate);
	}

	public static void SetFrame(this AnimationState p_anim, int p_frame)
	{
		p_anim.time = (float)p_frame / p_anim.clip.frameRate;
	}
}
