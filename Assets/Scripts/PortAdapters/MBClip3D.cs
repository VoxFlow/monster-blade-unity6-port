// Plays audio clips that carried Unity 4's 3D flag as 3D sources. Register CD-047.
using UnityEngine;

public static class MBClip3D
{
	private static readonly string[] s_shipped3D = { "tutorial_ambience_1", "p_attack_voice_3", "p_attack_voice_4", "p_attack_voice_5", "p_attack_voice_6" };

	public static void Apply(AudioSource source)
	{
		if (source == null) return;
		AudioClip clip = source.clip;
		source.spatialBlend = clip != null && Is3D(clip.name) ? 1f : 0f;
	}

	private static bool Is3D(string name)
	{
		string b = name.Substring(name.LastIndexOf('/') + 1);
		for (int i = 0; i < s_shipped3D.Length; i++) if (b == s_shipped3D[i]) return true;
		return false;
	}
}
