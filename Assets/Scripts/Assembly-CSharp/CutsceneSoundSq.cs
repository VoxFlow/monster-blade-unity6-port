public class CutsceneSoundSq : CutsceneSq
{
	public string[] m_soundNames;

	public override bool CanPlay()
	{
		return base.CanPlay() && m_soundNames != null && m_sequenceTime.Length == m_soundNames.Length;
	}

	protected override void PlayNextSequence(int p_index)
	{
		base.PlayNextSequence(p_index);
		if (SingletonComponent<SoundManager>.Instance != null)
		{
			SingletonComponent<SoundManager>.Instance.PlayPlaylist(m_soundNames[p_index]);
		}
	}

	private void PlaySoundFx()
	{
	}
}
