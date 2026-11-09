using UnityEngine;

public class MovieManager : MonoBehaviour
{
	protected struct SubtitleTime
	{
		public float startTime;

		public float endTime;

		public string subtitleString;

		public SubtitleTime(float start, float end, string name)
		{
			startTime = start;
			endTime = end;
			subtitleString = name;
		}
	}

	public GameObject m_renderPlane;

	public Callback m_endCallBack;

	public UILabel m_subtitleLabel;

	private bool m_isLoaded;

	private MobileMovieTexture m_movieTexture;

	private string m_movieName = string.Empty;

	private string m_subtitleName = string.Empty;

	private bool m_isPlaying;

	private AudioSource m_audioSource;

	private bool m_isFadeout;

	public float m_fadeoutTime = 1f;

	public Material m_fadeScreenMaterial;

	private float m_currFadeTime;

	protected float m_time;

	protected string m_currString = string.Empty;

	protected SubtitleTime[] m_subtitleList;

	protected int m_subtitleIndex;

	private string m_movieSound = "Sounds/bgm/maintheme_music_1";

	public bool IsLoaded
	{
		get
		{
			return m_isLoaded;
		}
	}

	public bool IsMoviePlaying()
	{
		return m_isPlaying;
	}

	private void Awake()
	{
		if (m_renderPlane != null)
		{
			m_renderPlane.SetActiveRecursively(false);
		}
		if (m_fadeScreenMaterial != null)
		{
			Color color = m_fadeScreenMaterial.GetColor("_TintColor");
			color.a = 0f;
			m_fadeScreenMaterial.SetColor("_TintColor", color);
		}
		m_isLoaded = false;
		m_movieTexture = GetComponent<MobileMovieTexture>();
		if (m_movieTexture == null)
		{
			Dbg.LogError("MovieManager: m_movieTexture is null");
			return;
		}
		m_movieTexture.onFinished += OnFinished;
		Vector2 vector = new Vector2(base.GetComponent<Camera>().aspect * base.GetComponent<Camera>().orthographicSize * 2f, base.GetComponent<Camera>().orthographicSize * 2f);
		if (m_renderPlane != null)
		{
			float num = 1.775f;
			float num2 = 0f;
			float num3 = 0f;
			if (base.GetComponent<Camera>().aspect > num)
			{
				num3 = (vector.y + 5f) * 0.1f;
				num2 = num3 * num;
			}
			else
			{
				num2 = (vector.x + 5f) * 0.1f;
				num3 = num2 / num;
			}
			m_renderPlane.transform.localScale = new Vector3(Mathf.Ceil(num2), 1f, Mathf.Floor(num3));
		}
		if (m_subtitleLabel != null)
		{
			Vector3 localPosition = m_subtitleLabel.transform.localPosition;
			localPosition.x = 0f;
			localPosition.y = Mathf.Max(0f, (vector.y - m_renderPlane.transform.localScale.z * 10f) * 0.5f);
			localPosition.y += 12f;
			m_subtitleLabel.transform.localPosition = localPosition;
			m_subtitleLabel.lineWidth = (int)(m_renderPlane.transform.localScale.x * 9f);
		}
		m_audioSource = base.gameObject.AddComponent<AudioSource>();
		if (GameManager.qualitySetting == GameManager.QualitySetting.Low)
		{
			Play("OP_Low");
		}
		else
		{
			Play("OP");
		}
	}

	private void Start()
	{
	}

	private void OnFinished(MobileMovieTexture sender)
	{
		if (m_endCallBack != null)
		{
			m_endCallBack();
		}
		if (sender != null)
		{
			Dbg.Log(sender.Path + " has finished ");
		}
	}

	public bool LoadSubtitle(string filename)
	{
		m_subtitleName = filename;
		NBBinaryFormat parametersFor = SingletonComponent<ParameterManager>.Instance.GetParametersFor(m_subtitleName);
		if (Dbg.Assert(parametersFor != null, "LoadSubtitle:" + m_subtitleName + " is not exist!"))
		{
			return false;
		}
		m_subtitleList = new SubtitleTime[parametersFor.RowsNames.Count];
		foreach (string rowsName in parametersFor.RowsNames)
		{
			float dataGeneric = parametersFor.GetDataGeneric<float>(rowsName, "STARTTIME");
			float dataGeneric2 = parametersFor.GetDataGeneric<float>(rowsName, "ENDTIME");
			string data = parametersFor.GetData(rowsName, "WORDS");
			string text = SingletonComponent<NBLocalization>.Instance.Get(data);
			SubtitleTime subtitleTime = new SubtitleTime(dataGeneric, dataGeneric2, text);
			m_subtitleList[int.Parse(rowsName)] = subtitleTime;
		}
		return true;
	}

	public bool Play(string filename)
	{
		m_movieName = filename;
		string moviePath = GetMoviePath(filename);
		if (1 == 0)
		{
			Dbg.LogError("PlayMove fail: " + moviePath + " is not exist!");
			OnFinished(null);
			return false;
		}
		if (SingletonComponent<ParameterManager>.Instance != null)
		{
			LoadSubtitle(filename);
		}
		m_audioSource.clip = Resources.Load(m_movieSound) as AudioClip;
		MBClip3D.Apply(m_audioSource);   // PORT (CD-047)
		PlayIntroMovie();
		return true;
	}

	private string GetMoviePath(string p_fileName)
	{
		return "Movies/" + p_fileName + ".ogg";
	}

	private void PlayIntroMovie()
	{
		m_movieTexture.Path = GetMoviePath(m_movieName);
		m_movieTexture.Play();
		m_audioSource.Play();
		m_isLoaded = true;
		m_isPlaying = true;
		m_time = 0f;
		m_subtitleIndex = 0;
		if (m_renderPlane != null)
		{
			m_renderPlane.SetActiveRecursively(true);
		}
	}

	public void Stop(bool fade = true)
	{
		m_movieTexture.Stop();
		m_isFadeout = fade;
		if (!fade)
		{
			if ((bool)m_audioSource)
			{
				m_audioSource.Stop();
			}
			m_isPlaying = false;
		}
	}

	private void Update()
	{
		if ((bool)m_movieTexture && m_isLoaded && !m_movieTexture.isPlaying)
		{
			Stop();
		}
		if (!m_isPlaying)
		{
			return;
		}
		if (m_isFadeout)
		{
			UpdateFade();
		}
		m_time += Time.deltaTime;
		if (m_subtitleList == null)
		{
			m_currString = "No Subtitle loaded.";
			return;
		}
		if (m_subtitleIndex == 0 || m_time >= m_subtitleList[m_subtitleIndex - 1].endTime)
		{
			m_currString = string.Empty;
		}
		else
		{
			m_currString = m_subtitleList[m_subtitleIndex - 1].subtitleString;
		}
		if (m_subtitleIndex >= m_subtitleList.Length)
		{
			return;
		}
		while (m_time > m_subtitleList[m_subtitleIndex].startTime)
		{
			m_subtitleIndex++;
			if (m_subtitleIndex >= m_subtitleList.Length - 1)
			{
				break;
			}
		}
	}

	private void UpdateFade()
	{
		if (!m_isFadeout)
		{
			return;
		}
		m_currFadeTime += Time.deltaTime;
		if (m_currFadeTime >= m_fadeoutTime)
		{
			m_currFadeTime = m_fadeoutTime;
			if ((bool)m_audioSource)
			{
				m_audioSource.Stop();
			}
			m_isPlaying = false;
		}
		else if (!(m_fadeScreenMaterial == null))
		{
			float num = m_currFadeTime / m_fadeoutTime;
			if (num > 1f)
			{
				num = 1f;
			}
			Color color = m_fadeScreenMaterial.GetColor("_TintColor");
			color.a = num;
			m_fadeScreenMaterial.SetColor("_TintColor", color);
			if ((bool)m_audioSource)
			{
				m_audioSource.volume = 1f - num;
			}
		}
	}

	private void OnGUI()
	{
		if (!(m_movieTexture == null) && m_subtitleLabel != null)
		{
			m_subtitleLabel.text = m_currString;
		}
	}
}
