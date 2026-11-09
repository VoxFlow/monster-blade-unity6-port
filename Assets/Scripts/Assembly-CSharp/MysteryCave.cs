using UnityEngine;

public class MysteryCave : MonoBehaviour
{
	public GameObject premiumCave;

	public GameObject premiumDoor;

	public GameObject premiumFx;

	public GameObject premiumChest;

	public GameObject premiumCamera1;

	public GameObject premiumCamera2;

	public Transform premiumChestTransform;

	public GameObject socialCave;

	public GameObject socialFx;

	public GameObject socialChest;

	public GameObject socialCamera1;

	public GameObject socialCamera2;

	public Transform socialChestTransform;

	public SlashEffectEmitter slashEmitter;

	private static MysteryCave s_instance;

	public static MysteryCave Instance
	{
		get
		{
			return s_instance;
		}
	}

	private void OnDestroy()
	{
		s_instance = null;
	}

	private void Awake()
	{
		if (s_instance == null)
		{
			s_instance = this;
		}
		if (Instance == null)
		{
			Debug.LogError("No Mystery Cave found.");
		}
	}

	private void Start()
	{
	}

	private void Update()
	{
	}
}
