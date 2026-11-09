using System;
using System.Runtime.InteropServices;
using UnityEngine;

public class MobileMovieTexture : MonoBehaviour
{
	public delegate void OnFinished(MobileMovieTexture sender);

	private const int BUFFER_SIZE = 2;

	private const int CHANNELS = 3;

	private const string PLATFORM_DLL = "theorawrapper";

	[SerializeField]
	private string m_path;

	[SerializeField]
	private Material m_movieMaterial;

	[SerializeField]
	private bool m_playAutomatically = true;

	[SerializeField]
	private bool m_loop = true;

	private IntPtr m_nativeContext = IntPtr.Zero;

	private int m_picWidth;

	private int m_picHeight;

	private int m_picX;

	private int m_picY;

	private int m_yStride;

	private int m_yHeight;

	private int m_uvStride;

	private int m_uvHeight;

	private ChannelTexture[][] m_ChannelTextures;

	private int m_nTextureIndex;

	private bool m_advance = true;

	private double m_elapsedTime;

	private double m_nextFrameTime;

	private double m_lastFrameDelta = double.MaxValue;

	private bool m_reallocTextures;

	private bool m_hasFinished = true;

	public string Path
	{
		get
		{
			return m_path;
		}
		set
		{
			m_path = value;
		}
	}

	public bool playAutomatically
	{
		set
		{
			m_playAutomatically = value;
		}
	}

	public Material MovieMaterial
	{
		get
		{
			return m_movieMaterial;
		}
	}

	public int width
	{
		get
		{
			return m_picWidth;
		}
	}

	public int height
	{
		get
		{
			return m_picHeight;
		}
	}

	public float aspectRatio
	{
		get
		{
			if (m_nativeContext != IntPtr.Zero)
			{
				return GetAspectRatio(m_nativeContext);
			}
			return 1f;
		}
	}

	public bool isPlaying
	{
		get
		{
			return m_nativeContext != IntPtr.Zero && !m_hasFinished && m_advance;
		}
	}

	public bool pause
	{
		get
		{
			return !m_advance;
		}
		set
		{
			m_advance = !value;
		}
	}

	public event OnFinished onFinished;

	[DllImport("theorawrapper")]
	private static extern IntPtr OpenStream(string path, int offset, int size, bool loop, bool pot);

	[DllImport("theorawrapper")]
	private static extern int GetPicWidth(IntPtr context);

	[DllImport("theorawrapper")]
	private static extern int GetPicHeight(IntPtr context);

	[DllImport("theorawrapper")]
	private static extern int GetPicX(IntPtr context);

	[DllImport("theorawrapper")]
	private static extern int GetPicY(IntPtr context);

	[DllImport("theorawrapper")]
	private static extern bool DecodeFrame(IntPtr context);

	[DllImport("theorawrapper")]
	private static extern int GetYStride(IntPtr context);

	[DllImport("theorawrapper")]
	private static extern int GetYHeight(IntPtr context);

	[DllImport("theorawrapper")]
	private static extern int GetUVStride(IntPtr context);

	[DllImport("theorawrapper")]
	private static extern int GetUVHeight(IntPtr context);

	[DllImport("theorawrapper")]
	private static extern bool DecodeAndUpload(IntPtr context, int yHandle, int uHandle, int vHandle);

	[DllImport("theorawrapper")]
	private static extern void GetPlane(IntPtr context, int planeIndex, IntPtr buffer);

	[DllImport("theorawrapper")]
	private static extern double GetNextFrameTime(IntPtr context);

	[DllImport("theorawrapper")]
	private static extern void CloseStream(IntPtr context);

	[DllImport("theorawrapper")]
	private static extern float GetAspectRatio(IntPtr context);

	private void Start()
	{
		if (m_playAutomatically)
		{
			Play();
		}
	}

	public void Play()
	{
		Close();
		m_elapsedTime = 0.0;
		m_nextFrameTime = 0.0;
		Open();
		m_advance = true;
		m_hasFinished = false;
	}

	public void Stop()
	{
		Close();
	}

	private void Open()
	{
		bool flag = Application.platform == RuntimePlatform.Android || Application.platform == RuntimePlatform.IPhonePlayer;
		bool pot = !flag;
		long num = 0L;
		long num2 = 0L;
		RuntimePlatform platform = Application.platform;
		string path;
		if (platform == RuntimePlatform.Android)
		{
			path = Application.dataPath;
		}
		else
		{
			path = Application.streamingAssetsPath + "/" + m_path;
			num = 0L;
			num2 = 0L;
		}
		m_nativeContext = OpenStream(path, (int)num, (int)num2, m_loop, pot);
		if (m_nativeContext != IntPtr.Zero)
		{
			m_picWidth = GetPicWidth(m_nativeContext);
			m_picHeight = GetPicHeight(m_nativeContext);
			m_picX = GetPicX(m_nativeContext);
			m_picY = GetPicY(m_nativeContext);
			int yStride = GetYStride(m_nativeContext);
			int yHeight = GetYHeight(m_nativeContext);
			int uVStride = GetUVStride(m_nativeContext);
			int uVHeight = GetUVHeight(m_nativeContext);
			if (yStride != m_yStride || yHeight != m_yHeight || uVStride != m_uvStride || uVHeight != m_uvHeight)
			{
				m_yStride = yStride;
				m_yHeight = yHeight;
				m_uvStride = uVStride;
				m_uvHeight = uVHeight;
				DestroyTextures();
				AllocateTexures();
			}
			Decode(true);
			m_nextFrameTime = GetNextFrameTime(m_nativeContext);
			return;
		}
		throw new Exception("problem opening movie");
	}

	private void AllocateTexures()
	{
		TextureFormat format = TextureFormat.RGBA32;
		m_ChannelTextures = new ChannelTexture[2][];
		for (int i = 0; i < 2; i++)
		{
			m_ChannelTextures[i] = new ChannelTexture[3];
			m_ChannelTextures[i][0] = new ChannelTexture(m_yStride, m_yHeight, format);
			m_ChannelTextures[i][1] = new ChannelTexture(m_uvStride, m_uvHeight, format);
			m_ChannelTextures[i][2] = new ChannelTexture(m_uvStride, m_uvHeight, format);
		}
		Vector2 scale = new Vector2((float)m_picWidth / (float)m_yStride, 0f - (float)m_picHeight / (float)m_yHeight);
		Vector2 vector = new Vector2((float)m_picX / (float)m_yStride, ((float)m_picHeight + (float)m_picY) / (float)m_yHeight);
		Vector2 scale2 = default(Vector2);
		Vector2 vector2 = default(Vector2);
		if (m_uvStride == m_yStride)
		{
			scale2.x = scale.x;
		}
		else
		{
			scale2.x = (float)m_picWidth / 2f / (float)m_uvStride;
		}
		if (m_uvHeight == m_yHeight)
		{
			scale2.y = scale.y;
			vector2 = vector;
		}
		else
		{
			scale2.y = 0f - (float)m_picHeight / 2f / (float)m_uvHeight;
			vector2 = new Vector2((float)m_picX / 2f / (float)m_uvStride, ((float)m_picHeight + (float)m_picY) / 2f / (float)m_uvHeight);
		}
		m_movieMaterial.SetTexture("_YTex", m_ChannelTextures[m_nTextureIndex][0].tex);
		m_movieMaterial.SetTexture("_CrTex", m_ChannelTextures[m_nTextureIndex][1].tex);
		m_movieMaterial.SetTexture("_CbTex", m_ChannelTextures[m_nTextureIndex][2].tex);
		m_movieMaterial.SetTextureScale("_YTex", scale);
		m_movieMaterial.SetTextureOffset("_YTex", vector);
		m_movieMaterial.SetTextureScale("_CbTex", scale2);
		m_movieMaterial.SetTextureOffset("_CbTex", vector2);
	}

	private void DestroyTextures()
	{
		m_reallocTextures = false;
		if (m_ChannelTextures != null)
		{
			for (int i = 0; i < 2; i++)
			{
				for (int j = 0; j < 3; j++)
				{
					if (m_ChannelTextures[i][j] != null)
					{
						m_ChannelTextures[i][j].Destroy();
					}
				}
			}
		}
		m_ChannelTextures = null;
	}

	private void Close()
	{
		if (m_nativeContext != IntPtr.Zero)
		{
			CloseStream(m_nativeContext);
			m_nativeContext = IntPtr.Zero;
		}
	}

	private void OnDestroy()
	{
		Close();
		DestroyTextures();
	}

	private void OnApplicationPause(bool pause)
	{
		if (!pause)
		{
			m_reallocTextures = true;
		}
	}

	private void Update()
	{
		if (m_reallocTextures && m_ChannelTextures != null)
		{
			for (int i = 0; i < 2; i++)
			{
				for (int j = 0; j < 3; j++)
				{
					m_ChannelTextures[i][j].ReAlloc();
				}
			}
			m_reallocTextures = false;
		}
		if (!(m_nativeContext != IntPtr.Zero) || m_hasFinished || !m_advance)
		{
			return;
		}
		m_elapsedTime += Time.deltaTime;
		while (m_elapsedTime > m_nextFrameTime)
		{
			bool upload = false;
			if (m_nextFrameTime + m_lastFrameDelta >= m_elapsedTime)
			{
				upload = true;
			}
			Decode(upload);
			double nextFrameTime = GetNextFrameTime(m_nativeContext);
			if (nextFrameTime == m_nextFrameTime)
			{
				m_hasFinished = true;
				if (this.onFinished != null)
				{
					this.onFinished(this);
				}
				break;
			}
			m_lastFrameDelta = nextFrameTime - m_nextFrameTime;
			m_nextFrameTime = nextFrameTime;
		}
	}

#if !UNITY_ANDROID
	// PORT (CD-035)
	private byte[][] m_planeBuffers;

	private void UploadPlanes()
	{
		if (m_planeBuffers == null)
		{
			m_planeBuffers = new byte[3][];
		}
		for (int i = 0; i < 3; i++)
		{
			Texture2D tex = m_ChannelTextures[m_nTextureIndex][i].tex;
			int size = tex.width * tex.height * 4;
			if (m_planeBuffers[i] == null || m_planeBuffers[i].Length != size)
			{
				m_planeBuffers[i] = new byte[size];
			}
			GCHandle pin = GCHandle.Alloc(m_planeBuffers[i], GCHandleType.Pinned);
			try
			{
				GetPlane(m_nativeContext, i, pin.AddrOfPinnedObject());
			}
			finally
			{
				pin.Free();
			}
			tex.LoadRawTextureData(m_planeBuffers[i]);
			tex.Apply(false);
		}
	}
#endif

	private void Decode(bool upload)
	{
		if (upload)
		{
			m_nTextureIndex = (m_nTextureIndex + 1) % 2;
			DecodeAndUpload(m_nativeContext, m_ChannelTextures[m_nTextureIndex][0].tex.GetNativeTextureID(), m_ChannelTextures[m_nTextureIndex][1].tex.GetNativeTextureID(), m_ChannelTextures[m_nTextureIndex][2].tex.GetNativeTextureID());
#if !UNITY_ANDROID
			UploadPlanes();
#endif
			GL.InvalidateState();
			m_movieMaterial.SetTexture("_YTex", m_ChannelTextures[m_nTextureIndex][0].tex);
			m_movieMaterial.SetTexture("_CrTex", m_ChannelTextures[m_nTextureIndex][1].tex);
			m_movieMaterial.SetTexture("_CbTex", m_ChannelTextures[m_nTextureIndex][2].tex);
		}
		else
		{
			DecodeAndUpload(m_nativeContext, -1, -1, -1);
		}
	}
}
