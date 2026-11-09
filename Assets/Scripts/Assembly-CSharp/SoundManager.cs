using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class SoundManager : SingletonComponent<SoundManager>
{
	private enum MusicSwitchState
	{
		None = 0,
		RequestSwitch = 1,
		Switch_Out = 2,
		Switch_In = 3,
		Fadeout = 4
	}

	public class PlayList
	{
		public enum PlaybackMode
		{
			Linear = 0,
			Randomize = 1,
			EvenlyDistributed = 2,
			Max = 3
		}

		private int m_currentTrack;

		private List<Track> m_tracks = new List<Track>();

		private List<Track> m_used = new List<Track>();

		private PlaybackMode m_mode;

		public Track CurrentTrack
		{
			get
			{
				if (m_currentTrack < m_tracks.Count && m_currentTrack >= 0)
				{
					return m_tracks[m_currentTrack];
				}
				Dbg.LogWarn("Could not find Current Track : " + m_currentTrack + " Track counts : " + m_tracks.Count, Dbg.NBLogType.SOUND);
				return null;
			}
		}

		public PlaybackMode Mode
		{
			get
			{
				return m_mode;
			}
			set
			{
				m_mode = value;
			}
		}

		public PlayList(string p_id, PlaybackMode p_mode)
		{
			m_mode = p_mode;
		}

		public static PlaybackMode FromString(string p_string)
		{
			p_string = p_string.ToLower();
			switch (p_string)
			{
			case "linear":
				return PlaybackMode.Linear;
			case "randomize":
			case "random":
				return PlaybackMode.Randomize;
			case "randomequal":
			case "evenlydistributed":
				return PlaybackMode.EvenlyDistributed;
			default:
				return PlaybackMode.Randomize;
			}
		}

		public Track NextTrack()
		{
			if (m_mode == PlaybackMode.Linear)
			{
				m_currentTrack++;
				if (m_currentTrack >= m_tracks.Count)
				{
					m_currentTrack = 0;
				}
				return m_tracks[m_currentTrack];
			}
			if (m_mode == PlaybackMode.Randomize)
			{
				if (m_tracks.Count < 1)
				{
					return null;
				}
				if (m_tracks.Count == 1)
				{
					m_currentTrack = 0;
					return m_tracks[m_currentTrack];
				}
				m_currentTrack = UnityEngine.Random.Range(0, m_tracks.Count);
				return m_tracks[m_currentTrack];
			}
			if (m_mode == PlaybackMode.EvenlyDistributed)
			{
				if (m_tracks.Count < 1)
				{
					return null;
				}
				if (m_tracks.Count == 1)
				{
					m_currentTrack = 0;
					return m_tracks[m_currentTrack];
				}
				if (m_used.Count == m_tracks.Count)
				{
					m_used.RemoveRange(0, m_used.Count - 1);
				}
				do
				{
					m_currentTrack = UnityEngine.Random.Range(0, m_tracks.Count - 1);
				}
				while (!m_used.Contains(m_tracks[m_currentTrack]));
				m_used.Add(m_tracks[m_currentTrack]);
				return m_tracks[m_currentTrack];
			}
			return null;
		}

		public void AddTrack(SoundAsset p_asset)
		{
			m_tracks.Add(new Track(p_asset));
		}

		public void Destroy()
		{
			foreach (Track track in m_tracks)
			{
				track.Destroy();
			}
		}
	}

	public class Track
	{
		public enum Type
		{
			Music = 0,
			Sfx = 1,
			Max = 2
		}

		private AudioSource m_sound;

		private Type m_type = Type.Sfx;

		private SoundAsset m_asset;

		public Track(SoundAsset p_asset)
		{
			m_asset = p_asset;
			m_type = p_asset.type;
		}

		public static Type FromString(string p_string)
		{
			p_string = p_string.ToLower();
			if (p_string == "bgm")
			{
				return Type.Music;
			}
			if (p_string == "sfx")
			{
				return Type.Sfx;
			}
			return Type.Max;
		}

		public AudioSource Play(float volume)
		{
			if (m_type == Type.Sfx)
			{
				m_sound = SingletonComponent<SoundManager>.Instance.PlaySoundFx(m_asset.id);
				m_sound.volume = SingletonComponent<SoundManager>.Instance.SfxVolume * volume;
				return m_sound;
			}
			if (m_asset.musicIndependent)
			{
				m_sound = SingletonComponent<SoundManager>.Instance.PlayMusicIndependently(m_asset.id, m_asset.id, true);
			}
			else
			{
				m_sound = SingletonComponent<SoundManager>.Instance.PlayMusic(m_asset.id, m_asset.crossfade, m_asset.loop);
			}
			return m_sound;
		}

		public void Stop()
		{
			if (m_type == Type.Sfx)
			{
				m_sound.Stop();
			}
			else
			{
				SingletonComponent<SoundManager>.Instance.StopMusic();
			}
		}

		public void Destroy()
		{
			SingletonComponent<SoundManager>.Instance.UnloadSound(m_asset.id);
		}
	}

	public class SoundAsset
	{
		public string id;

		public Track.Type type = Track.Type.Sfx;

		public AudioClip clip;

		public bool crossfade;

		public bool loop;

		public bool musicIndependent;

		public int samples = 44100;

		public int channels = 1;

		public int frequency = 44100;

		public bool is3D;

		public bool stream;
	}

	public delegate void AsyncCallback(bool p_success);

	private bool m_bStartGarbageCollection;

	private bool m_bStartUnloadUnusedAssets;

	private int m_lastSampleTime;

	private bool isMute;

	public Dictionary<string, SoundAsset> m_musicAssets = new Dictionary<string, SoundAsset>();

	public Dictionary<string, SoundAsset> m_sfxAssets = new Dictionary<string, SoundAsset>();

	public float m_garbageCollectionInterval = 150f;

	public float m_soundExpirationInterval = 300f;

	public int m_soundSubBufferSize = 3;

	public float m_crossFadeSpeed = 0.5f;

	private int m_realSoundSubBufferSize = 1;

	private AudioSource m_bgmSource;

	private AudioSource m_bgmSourceToSwitch;

	private float m_crossFader;

	private Dictionary<string, SfxSubBuffer> m_sfxBuffers;

	private MusicSwitchState m_switchState;

	private PlayList m_currentPlaylist;

	private Dictionary<string, PlayList> m_playlists = new Dictionary<string, PlayList>();

	private List<string> m_monsterPlayList = new List<string>();

	private List<string> m_loadedPlayListFile = new List<string>();

	private string m_titleMusicName = "TitleSounds/TitleMusic";

	private string m_titleTapSoundName = "TitleSounds/TitleTapSound";

	private GameObject m_titleMusic;

	private GameObject m_titleTapSound;

	private bool m_isTapedTitle;

	public readonly float defaultMusicVolume = 1f;

	public readonly float defaultSFXVolume = 1f;

	private float m_musicVolume = 1f;

	private float m_sfxVolume = 1f;

	public float MusicVolume
	{
		get
		{
			return m_musicVolume;
		}
		set
		{
			m_musicVolume = value;
			if (m_bgmSource != null)
			{
				m_bgmSource.volume = m_musicVolume;
			}
		}
	}

	public float SfxVolume
	{
		get
		{
			return m_sfxVolume;
		}
		set
		{
			m_sfxVolume = value;
		}
	}

	public bool IsMute
	{
		get
		{
			return isMute;
		}
		set
		{
			isMute = value;
			if (isMute)
			{
				m_bgmSource.volume = 0f;
				m_musicVolume = 0f;
				m_sfxVolume = 0f;
			}
			else
			{
				m_sfxVolume = MBPrefs.GetFloat("MS:SfxVolume", 0.5f);
				m_musicVolume = MBPrefs.GetFloat("MS:MusicVolume", 0.5f);
				m_bgmSource.volume = m_musicVolume;
			}
			MBPrefs.SetInt("MS:IsMute", isMute ? 1 : 0);
		}
	}

	private void Awake()
	{
		m_musicVolume = defaultMusicVolume;
		m_sfxVolume = defaultSFXVolume;
		m_bgmSource = base.gameObject.AddComponent<AudioSource>();
		m_bgmSource.volume = m_musicVolume;
		m_bgmSource.playOnAwake = false;
		m_bgmSource.bypassEffects = false;
		m_bgmSource.priority = 0;
		m_bgmSourceToSwitch = base.gameObject.AddComponent<AudioSource>();
		m_bgmSourceToSwitch.volume = 0f;
		m_bgmSourceToSwitch.playOnAwake = false;
		m_bgmSourceToSwitch.bypassEffects = false;
		m_bgmSourceToSwitch.priority = 0;
		m_sfxBuffers = new Dictionary<string, SfxSubBuffer>();
		m_realSoundSubBufferSize = m_soundSubBufferSize;
		m_switchState = MusicSwitchState.None;
		m_crossFader = 0f;
		AudioSettings.SetDSPBufferSize(512, 3);
	}

	public GameObject GetGameObj()
	{
		return base.gameObject;
	}

	private void Update()
	{
		if (m_switchState == MusicSwitchState.RequestSwitch)
		{
			m_switchState = MusicSwitchState.Switch_Out;
			m_crossFader = 0f;
		}
		else if (m_switchState == MusicSwitchState.Switch_Out)
		{
			m_crossFader += Time.deltaTime * m_crossFadeSpeed;
			m_bgmSource.volume = m_musicVolume * (1f - m_crossFader * 2f);
			if (m_crossFader >= 0.5f)
			{
				m_bgmSource.Stop();
				AudioSource bgmSource = m_bgmSource;
				m_bgmSource = m_bgmSourceToSwitch;
				m_bgmSourceToSwitch = bgmSource;
				m_crossFader = 0f;
				m_switchState = MusicSwitchState.Switch_In;
				m_bgmSource.Play();
				m_bgmSource.volume = 0f;
			}
		}
		else if (m_switchState == MusicSwitchState.Switch_In)
		{
			m_crossFader += Time.deltaTime * m_crossFadeSpeed;
			m_bgmSource.volume = m_musicVolume * m_crossFader * 2f;
			if (m_crossFader >= 0.5f)
			{
				m_bgmSource.volume = m_musicVolume;
				m_crossFader = 0f;
				m_switchState = MusicSwitchState.None;
			}
		}
		else if (m_switchState == MusicSwitchState.Fadeout)
		{
			m_crossFader += Time.deltaTime * m_crossFadeSpeed;
			m_crossFader = Mathf.Min(m_crossFader, 0.5f);
			m_bgmSource.volume = m_musicVolume * (1f - m_crossFader * 2f);
			if (m_crossFader >= 0.5f)
			{
				m_bgmSource.Stop();
				m_crossFader = 0f;
				m_switchState = MusicSwitchState.None;
			}
		}
	}

	public void InitManager(AsyncCallback p_callback = null)
	{
		NBProfiler.BeginTask("[SOUND_MANAGER][INIT_MANAGER]");
		m_musicVolume = MBPrefs.GetFloat("MS:MusicVolume", m_musicVolume);
		m_sfxVolume = MBPrefs.GetFloat("MS:SfxVolume", m_sfxVolume);
		int num = MBPrefs.GetInt("MS:IsMute", 0);
		SingletonComponent<SoundManager>.Instance.IsMute = ((num > 0) ? true : false);
		NBProfiler.EndTask("[SOUND_MANAGER][INIT_MANAGER]");
	}

	public void CollectGarbage()
	{
		if (m_sfxBuffers == null)
		{
			return;
		}
		List<string> list = new List<string>();
		foreach (KeyValuePair<string, SfxSubBuffer> sfxBuffer in m_sfxBuffers)
		{
			if (Time.time - sfxBuffer.Value.GetLastTimeUsed() > m_soundExpirationInterval)
			{
				sfxBuffer.Value.CollectGarbage();
				list.Add(sfxBuffer.Key);
			}
		}
		foreach (string item in list)
		{
			m_sfxBuffers.Remove(item);
		}
	}

	public void ForceCollectGarbage()
	{
		if (m_sfxBuffers == null)
		{
			return;
		}
		foreach (KeyValuePair<string, SfxSubBuffer> sfxBuffer in m_sfxBuffers)
		{
			sfxBuffer.Value.CollectGarbage();
		}
		m_sfxBuffers.Clear();
	}

	private IEnumerator TryGarbageCollection()
	{
		while (m_bStartGarbageCollection || m_bStartUnloadUnusedAssets)
		{
			if (m_bgmSource != null && m_bgmSource.isPlaying)
			{
				m_lastSampleTime = m_bgmSource.timeSamples;
				while (m_lastSampleTime == m_bgmSource.timeSamples && m_bgmSource.isPlaying)
				{
					yield return null;
				}
			}
			if (m_bStartUnloadUnusedAssets)
			{
				Resources.UnloadUnusedAssets();
				m_bStartUnloadUnusedAssets = false;
			}
			else if (m_bStartGarbageCollection)
			{
				GC.Collect();
				m_bStartGarbageCollection = false;
			}
		}
	}

	public void RequestMonoGarbageCollection()
	{
		if (!m_bStartGarbageCollection)
		{
			m_bStartGarbageCollection = true;
			StartCoroutine(TryGarbageCollection());
		}
	}

	public void RequestUnloadResources()
	{
		if (!m_bStartUnloadUnusedAssets)
		{
			m_bStartUnloadUnusedAssets = true;
			StartCoroutine(TryGarbageCollection());
		}
	}

	public AudioSource PlayMusic(string p_musicId, bool p_crossFade = true, bool p_loop = true)
	{
		if (m_musicAssets.Count < 1)
		{
			Dbg.LogWarn("There are no music assets.", Dbg.NBLogType.SOUND);
			return null;
		}
		if (!m_musicAssets.ContainsKey(p_musicId))
		{
			Dbg.LogWarn("There is no music with key: " + p_musicId, Dbg.NBLogType.SOUND);
			return null;
		}
		if (!p_crossFade)
		{
			m_bgmSource.Stop();
			m_bgmSourceToSwitch.Stop();
			m_bgmSource.clip = m_musicAssets[p_musicId].clip;
			MBClip3D.Apply(m_bgmSource);   // PORT (CD-047)
			m_bgmSource.loop = p_loop;
			m_bgmSource.volume = m_musicVolume;
			m_bgmSource.Play();
			return m_bgmSource;
		}
		if (m_switchState == MusicSwitchState.None)
		{
			if (!m_bgmSource.isPlaying)
			{
				m_bgmSource.clip = m_musicAssets[p_musicId].clip;
				MBClip3D.Apply(m_bgmSource);   // PORT (CD-047)
				m_bgmSource.volume = m_musicVolume;
				m_bgmSource.loop = p_loop;
				m_bgmSource.Play();
				return m_bgmSource;
			}
			m_bgmSourceToSwitch.clip = m_musicAssets[p_musicId].clip;
			MBClip3D.Apply(m_bgmSourceToSwitch);   // PORT (CD-047)
			m_bgmSourceToSwitch.loop = p_loop;
			m_bgmSourceToSwitch.volume = 0f;
			m_crossFader = 0f;
			m_switchState = MusicSwitchState.RequestSwitch;
		}
		else
		{
			m_bgmSource.Stop();
			AudioSource bgmSource = m_bgmSource;
			m_bgmSource = m_bgmSourceToSwitch;
			m_bgmSourceToSwitch = bgmSource;
			m_bgmSource.volume = m_musicVolume;
			m_bgmSource.loop = p_loop;
			m_bgmSourceToSwitch.clip = m_musicAssets[p_musicId].clip;
			MBClip3D.Apply(m_bgmSourceToSwitch);   // PORT (CD-047)
			m_bgmSourceToSwitch.volume = 0f;
			m_bgmSourceToSwitch.loop = p_loop;
			m_crossFader = 0f;
			m_switchState = MusicSwitchState.RequestSwitch;
		}
		return m_bgmSourceToSwitch;
	}

	public void StopSound(Track track)
	{
		if (track != null)
		{
			track.Stop();
		}
	}

	public void StopMusic(bool fadeOut = false)
	{
		if (m_musicAssets.Count < 1)
		{
			Dbg.LogWarn("There are no music assets.", Dbg.NBLogType.SOUND);
		}
		else if (m_bgmSource.isPlaying)
		{
			if (!fadeOut)
			{
				m_bgmSource.Stop();
				return;
			}
			m_switchState = MusicSwitchState.Fadeout;
			m_crossFader = 0f;
		}
	}

	public bool IsPlayingMusic()
	{
		return m_bgmSource.isPlaying;
	}

	public bool IsPlayingMusic(string p_musicId)
	{
		return m_bgmSource.clip == m_musicAssets[p_musicId].clip && m_bgmSource.isPlaying;
	}

	public AudioSource PlayMusicIndependently(string p_identifier, string p_musicId, bool p_destroyOnFinish)
	{
		if (m_musicAssets.Count < 1)
		{
			Dbg.LogWarn("There are no music assets.", Dbg.NBLogType.SOUND);
			return null;
		}
		if (!m_musicAssets.ContainsKey(p_musicId))
		{
			Dbg.LogWarn("There is no music with key: " + p_musicId, Dbg.NBLogType.SOUND);
			return null;
		}
		GameObject gameObject = new GameObject(p_identifier);
		AudioSource audioSource = gameObject.AddComponent<AudioSource>();
		audioSource.clip = m_musicAssets[p_musicId].clip;
		MBClip3D.Apply(audioSource);   // PORT (CD-047)
		audioSource.volume = m_musicVolume;
		audioSource.pitch = 1f;
		audioSource.loop = !p_destroyOnFinish;
		audioSource.playOnAwake = false;
		audioSource.Play();
		if (p_destroyOnFinish)
		{
			MBDestroy.Destroy(gameObject, m_musicAssets[p_musicId].clip.length + 0.1f);   // PORT (CD-053)
		}
		return audioSource;
	}

	public AudioSource PlayMusicIndependently(string p_identifier, string p_musicId, Transform p_where, bool p_destroyOnFinish)
	{
		if (m_musicAssets.Count < 1)
		{
			Dbg.LogWarn("There are no music assets.", Dbg.NBLogType.SOUND);
			return null;
		}
		if (!m_musicAssets.ContainsKey(p_musicId))
		{
			Dbg.LogWarn("There is no music with key: " + p_musicId, Dbg.NBLogType.SOUND);
			return null;
		}
		GameObject gameObject = new GameObject(p_identifier);
		AudioSource audioSource = gameObject.AddComponent<AudioSource>();
		audioSource.clip = m_musicAssets[p_musicId].clip;
		MBClip3D.Apply(audioSource);   // PORT (CD-047)
		audioSource.volume = m_musicVolume;
		audioSource.pitch = 1f;
		audioSource.loop = !p_destroyOnFinish;
		audioSource.playOnAwake = false;
		gameObject.transform.parent = p_where;
		gameObject.transform.position = p_where.position;
		audioSource.Play();
		if (p_destroyOnFinish)
		{
			MBDestroy.Destroy(gameObject, m_musicAssets[p_musicId].clip.length + 0.1f);   // PORT (CD-053)
		}
		return audioSource;
	}

	public AudioSource PlaySoundFx(string p_sfxId)
	{
		if (m_sfxAssets.Count < 1)
		{
			Dbg.LogWarn("There are no sfx assets.", Dbg.NBLogType.SOUND);
			return null;
		}
		if (!m_sfxAssets.ContainsKey(p_sfxId) || m_sfxAssets[p_sfxId] == null)
		{
			Dbg.LogWarn("There is no sfx with key: " + p_sfxId, Dbg.NBLogType.SOUND);
			return null;
		}
		AudioClip clip = m_sfxAssets[p_sfxId].clip;
		SfxSubBuffer value = null;
		if (!m_sfxBuffers.TryGetValue(p_sfxId, out value))
		{
			value = new SfxSubBuffer(p_sfxId, m_realSoundSubBufferSize, clip);
			m_sfxBuffers.Add(p_sfxId, value);
		}
		return value.Play(Time.time);
	}

	public AudioSource PlaySoundFx(string p_identifier, string p_sfxId, Transform p_where)
	{
		if (m_sfxAssets.Count < 1)
		{
			Dbg.LogWarn("There are no sfx assets.", Dbg.NBLogType.SOUND);
			return null;
		}
		if (!m_sfxAssets.ContainsKey(p_sfxId))
		{
			Dbg.LogWarn("There is no sfx with key: " + p_sfxId, Dbg.NBLogType.SOUND);
			return null;
		}
		SfxSubBuffer value = null;
		if (!m_sfxBuffers.TryGetValue(p_identifier, out value))
		{
			value = new SfxSubBuffer(p_identifier, m_realSoundSubBufferSize, m_sfxAssets[p_sfxId].clip, p_where);
			m_sfxBuffers.Add(p_identifier, value);
		}
		return value.Play(Time.time);
	}

	public Track PlayPlaylist(string p_playlist, float volume = 0.5f)
	{
		if (!m_playlists.ContainsKey(p_playlist))
		{
			Dbg.LogWarn("Playlist " + p_playlist + " does not exist.", Dbg.NBLogType.SOUND);
			return null;
		}
		Track result = null;
		m_currentPlaylist = m_playlists[p_playlist];
		if (m_currentPlaylist.CurrentTrack != null)
		{
			m_currentPlaylist.CurrentTrack.Play(volume);
			result = m_currentPlaylist.CurrentTrack;
		}
		else
		{
			Dbg.LogWarn("Playlist " + p_playlist + " have no CurrentTrack.");
		}
		m_currentPlaylist.NextTrack();
		return result;
	}

	private void OnLevelWasLoaded()
	{
	}

	private IEnumerator LoadSoundFromPlayListTrack(string soundListName)
	{
		if (m_loadedPlayListFile.Contains(soundListName))
		{
			yield break;
		}
		ParameterManager pm = SingletonComponent<ParameterManager>.Instance;
		if (pm.GetParametersFor(soundListName) == null || pm.GetParametersFor("PlaylistTracks") == null || pm.GetParametersFor("SoundAssets") == null)
		{
			yield break;
		}
		foreach (string rowName in pm.GetRowsNamesFor(soundListName))
		{
			string id = rowName;
			PlayList.PlaybackMode mode = PlayList.FromString(pm.GetParameter(soundListName, rowName, "MODE"));
			if (!m_playlists.ContainsKey(id))
			{
				PlayList playlist = new PlayList(id, mode);
				m_playlists.Add(id, playlist);
			}
			else
			{
				m_playlists[id].Mode = mode;
			}
		}
		foreach (string rowName2 in pm.GetRowsNamesFor("PlaylistTracks"))
		{
			string id2 = pm.GetParameter("PlaylistTracks", rowName2, "PLAYLIST_ID");
			PlayList playlist2;
			if (m_playlists.TryGetValue(id2, out playlist2))
			{
				string trackName = pm.GetParameter("PlaylistTracks", rowName2, "ASSET_ID");
				SoundAsset asset = null;
				if (m_sfxAssets.ContainsKey(trackName))
				{
					asset = m_sfxAssets[trackName];
					playlist2.AddTrack(asset);
				}
				else if (m_musicAssets.ContainsKey(trackName))
				{
					asset = m_musicAssets[trackName];
					playlist2.AddTrack(asset);
				}
				else
				{
					StartCoroutine(LoadSoundFromFile(trackName, playlist2));
				}
			}
		}
		m_loadedPlayListFile.Add(soundListName);
		yield return null;
	}

	private IEnumerator LoadSoundFromFile(string trackName, PlayList playlist)
	{
		ParameterManager pm = SingletonComponent<ParameterManager>.Instance;
		string file = pm.GetParameter("SoundAssets", trackName, "FILENAME");
		SoundAsset asset = new SoundAsset
		{
			id = trackName
		};
		if (m_sfxAssets.ContainsKey(asset.id))
		{
			asset = m_sfxAssets[asset.id];
		}
		else if (m_musicAssets.ContainsKey(asset.id))
		{
			asset = m_musicAssets[asset.id];
		}
		int flag = 0;
		string soundFile = file.Replace(".mp3", string.Empty);
		soundFile = soundFile.Replace(".wav", string.Empty);
		asset.clip = Resources.Load(soundFile) as AudioClip;
		asset.type = Track.FromString(pm.GetParameter("SoundAssets", trackName, "TYPE"));
		NBGeneric.TryParse<int>(pm.GetParameter("SoundAssets", trackName, "SAMPLES"), out asset.samples);
		NBGeneric.TryParse<int>(pm.GetParameter("SoundAssets", trackName, "CHANNELS"), out asset.channels);
		NBGeneric.TryParse<int>(pm.GetParameter("SoundAssets", trackName, "FREQUENCY"), out asset.frequency);
		if (asset.clip == null)
		{
			Dbg.LogError("Asset clip: " + file + " is null!", Dbg.NBLogType.SOUND);
			yield break;
		}
		asset.clip.name = soundFile;
		if (asset.type == Track.Type.Music)
		{
			NBGeneric.TryParse<int>(pm.GetParameter("SoundAssets", trackName, "MUSIC_CROSSFADE"), out flag);
			asset.crossfade = flag > 0;
			NBGeneric.TryParse<int>(pm.GetParameter("SoundAssets", trackName, "MUSIC_LOOP"), out flag);
			asset.loop = flag > 0;
			NBGeneric.TryParse<int>(pm.GetParameter("SoundAssets", trackName, "MUSIC_INDEPENDENT"), out flag);
			asset.musicIndependent = flag > 0;
			if (!m_musicAssets.ContainsKey(asset.id))
			{
				m_musicAssets.Add(asset.id, asset);
				playlist.AddTrack(asset);
			}
		}
		else if (asset.type == Track.Type.Sfx && !m_sfxAssets.ContainsKey(asset.id))
		{
			m_sfxAssets.Add(asset.id, asset);
			SfxSubBuffer buffer = null;
			if (!m_sfxBuffers.TryGetValue(asset.id, out buffer))
			{
				buffer = new SfxSubBuffer(asset.id, m_realSoundSubBufferSize, asset.clip);
				m_sfxBuffers.Add(asset.id, buffer);
			}
			playlist.AddTrack(asset);
		}
		yield return null;
	}

	public void LoadPlayList(string playListName)
	{
		StartCoroutine(LoadSoundFromPlayListTrack(playListName));
	}

	public void LoadMonsterSoundPlayList(string playListName)
	{
		if (!m_monsterPlayList.Contains(playListName))
		{
			m_monsterPlayList.Add(playListName);
			LoadPlayList(playListName);
		}
	}

	private void UnloadSound(string soundName)
	{
		SoundAsset soundAsset = null;
		if (m_sfxAssets.ContainsKey(soundName))
		{
			soundAsset = m_sfxAssets[soundName];
			m_sfxAssets.Remove(soundName);
		}
		else
		{
			if (!m_musicAssets.ContainsKey(soundName))
			{
				Dbg.LogWarn("UnloadSound: " + soundName + " does not exist!");
				return;
			}
			soundAsset = m_musicAssets[soundName];
			m_musicAssets.Remove(soundName);
		}
		if (m_sfxBuffers.ContainsKey(soundName))
		{
			m_sfxBuffers[soundName].CollectGarbage();
			m_sfxBuffers.Remove(soundName);
			Resources.UnloadAsset(soundAsset.clip);
			soundAsset.clip = null;
		}
	}

	public void UnloadMonsterSoundsInList()
	{
		foreach (string monsterPlay in m_monsterPlayList)
		{
			UnloadSoundsInList(monsterPlay);
		}
		m_monsterPlayList.Clear();
	}

	public void UnloadSoundsInList(string soundListName)
	{
		if (!m_loadedPlayListFile.Contains(soundListName))
		{
			return;
		}
		ParameterManager instance = SingletonComponent<ParameterManager>.Instance;
		if (Dbg.Assert(instance.GetParametersFor(soundListName) != null, "UnloadSound:" + soundListName + " does not exit!"))
		{
			return;
		}
		foreach (string item in instance.GetRowsNamesFor(soundListName))
		{
			if (!m_playlists.ContainsKey(item))
			{
				Dbg.LogWarn("Playlist " + item + " does not exist.", Dbg.NBLogType.SOUND);
				continue;
			}
			PlayList playList = m_playlists[item];
			m_playlists.Remove(item);
			playList.Destroy();
			playList = null;
		}
		m_loadedPlayListFile.Remove(soundListName);
	}

	public void LoadTitleSounds()
	{
		GameObject gameObject = Resources.Load(m_titleMusicName) as GameObject;
		if ((bool)gameObject)
		{
			m_titleMusic = UnityEngine.Object.Instantiate(gameObject) as GameObject;
		}
		GameObject gameObject2 = Resources.Load(m_titleTapSoundName) as GameObject;
		if ((bool)gameObject2)
		{
			m_titleTapSound = UnityEngine.Object.Instantiate(gameObject2) as GameObject;
		}
		if ((bool)SingletonComponent<SoundManager>.Instance.GetGameObj())
		{
			m_titleMusic.transform.parent = SingletonComponent<SoundManager>.Instance.GetGameObj().transform;
			m_titleTapSound.transform.parent = SingletonComponent<SoundManager>.Instance.GetGameObj().transform;
		}
	}

	public void PlayTitleMusic()
	{
		AudioSource component = m_titleMusic.GetComponent<AudioSource>();
		if ((bool)component)
		{
			component.volume = MBPrefs.GetFloat("MS:MusicVolume", m_musicVolume);
			component.Play();
		}
	}

	public void PlayTitleTapSound()
	{
		if (!m_isTapedTitle && !(m_titleTapSound == null))
		{
			AudioSource component = m_titleTapSound.GetComponent<AudioSource>();
			if ((bool)component)
			{
				component.volume = MBPrefs.GetFloat("MS:SfxVolume", m_sfxVolume);
				component.Play();
			}
		}
	}

	public void StopTitleMusic()
	{
		TitleMusic component = m_titleMusic.GetComponent<TitleMusic>();
		if ((bool)component)
		{
			component.StartFadeOut();
		}
	}

	public void UnloadTitleSounds()
	{
		if ((bool)m_titleTapSound)
		{
			UnityEngine.Object.Destroy(m_titleTapSound);
			m_titleTapSound = null;
		}
		if ((bool)m_titleMusic)
		{
			UnityEngine.Object.Destroy(m_titleMusic);
			m_titleMusic = null;
		}
	}

	public void SetSliders(ref UISlider p_musicSlider, ref UISlider p_sfxSlider)
	{
		if (SingletonComponent<SoundManager>.Instance.IsMute)
		{
			p_musicSlider.SetActive(false);
			p_sfxSlider.SetActive(false);
		}
		else
		{
			p_musicSlider.SetActive(true);
			p_sfxSlider.SetActive(true);
		}
	}

	public void OnMusicSliderChange(float p_val, ref UISlider p_musicSlider, ref UISlider p_sfxSlider, ref UILabel p_muteOnOffBtnText, ref UISprite p_muteBtnTex, bool p_needToChangeBtnTex)
	{
		float num = PreProcessVolumeValue(p_val);
		if (SingletonComponent<SoundManager>.Instance.IsMute)
		{
			SingletonComponent<SoundManager>.Instance.MusicVolume = 0f;
		}
		else
		{
			SingletonComponent<SoundManager>.Instance.MusicVolume = num;
		}
		if (num == 0f && p_sfxSlider.sliderValue == 0f)
		{
			SingletonComponent<SoundManager>.Instance.IsMute = true;
			ChangeMuteBtnText(ref p_muteOnOffBtnText);
			if (p_needToChangeBtnTex)
			{
				ChangeOptionBtnTexture(ref p_muteBtnTex, SingletonComponent<SoundManager>.Instance.IsMute);
			}
			p_musicSlider.SetActive(false);
			p_sfxSlider.SetActive(false);
		}
	}

	public void OnSfxSliderChange(float p_val, ref UISlider p_musicSlider, ref UISlider p_sfxSlider, ref UILabel p_muteOnOffBtnText, ref UISprite p_muteBtnTex, bool p_needToChangeBtnTex)
	{
		float num = PreProcessVolumeValue(p_val);
		if (SingletonComponent<SoundManager>.Instance.IsMute)
		{
			SingletonComponent<SoundManager>.Instance.SfxVolume = 0f;
		}
		else
		{
			SingletonComponent<SoundManager>.Instance.SfxVolume = num;
		}
		if (num == 0f && p_musicSlider.sliderValue == 0f)
		{
			SingletonComponent<SoundManager>.Instance.IsMute = true;
			ChangeMuteBtnText(ref p_muteOnOffBtnText);
			if (p_needToChangeBtnTex)
			{
				ChangeOptionBtnTexture(ref p_muteBtnTex, SingletonComponent<SoundManager>.Instance.IsMute);
			}
			p_musicSlider.SetActive(false);
			p_sfxSlider.SetActive(false);
		}
	}

	public void ChangeMuteBtnText(ref UILabel p_muteOnOffBtnText)
	{
		p_muteOnOffBtnText.text = SingletonComponent<NBLocalization>.Instance.Get((!SingletonComponent<SoundManager>.Instance.IsMute) ? "ui_settings_mute_on" : "ui_settings_mute_off");
	}

	public void ChangeOptionBtnTexture(ref UISprite p_sprite, bool p_red)
	{
		p_sprite.spriteName = ((!p_red) ? "blue_option_btn" : "red_option_btn");
	}

	private float PreProcessVolumeValue(float p_val)
	{
		float result = 0f;
		if (p_val <= 1f && p_val >= 0f)
		{
			result = p_val;
		}
		else if (p_val > 1f)
		{
			result = 1f;
		}
		else if (p_val < 0f)
		{
			result = 0f;
		}
		return result;
	}
}
