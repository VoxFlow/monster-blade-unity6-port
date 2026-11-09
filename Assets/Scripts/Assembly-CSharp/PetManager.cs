using UnityEngine;

public class PetManager : SingletonComponent<PetManager>
{
	private Services m_services;

	private Callback m_success;

	private Callback m_fail;

	public string currentPetID { get; private set; }

	public GameObject currentPet { get; private set; }

	private void Awake()
	{
		currentPetID = string.Empty;
		currentPet = null;
	}

	public GameObject CreatePet(PlayerData p_playerData)
	{
		if (null != currentPet)
		{
			Object.Destroy(currentPet);
			currentPetID = string.Empty;
		}
		if (!p_playerData.HasPet)
		{
			return null;
		}
		PetSetup instance = Singleton<PetSetup>.Instance;
		if (instance != null && p_playerData.details != null)
		{
			string petPrefabPath = instance.GetPetPrefabPath(p_playerData.details.activePet);
			if (string.IsNullOrEmpty(petPrefabPath))
			{
				return null;
			}
			GameObject gameObject = Resources.Load(petPrefabPath) as GameObject;
			if (null == gameObject)
			{
				return null;
			}
			currentPet = Object.Instantiate(gameObject, Vector3.up * 1000f, Quaternion.identity) as GameObject;
		}
		return currentPet;
	}

	public GameObject CreateMainMenuPet(PlayerData p_playerData)
	{
		GameObject gameObject = CreatePet(p_playerData);
		if (null != gameObject)
		{
			gameObject.AddComponent<MainMenuPet>();
		}
		return gameObject;
	}

	private void Start()
	{
		m_services = SingletonComponent<Services>.Instance;
	}

	private void ChangeTextureBasedOnElement(GameObject p_petModelGo, string p_petFolder, string p_element, bool p_rare)
	{
		string text = "Fire";
		switch (p_element)
		{
		case "Earth":
		case "Nature":
			text = "Nature";
			break;
		case "Lightning":
			text = "Lightning";
			break;
		case "Ice":
		case "Water":
			text = "Water";
			break;
		case "Metal":
			text = "Metal";
			break;
		default:
			text = "Fire";
			break;
		}
		string text2 = "Pet/Textures/Monsters/" + p_petFolder + "/" + p_petFolder + "_" + text;
		text2 = ((!p_rare) ? (text2 + "_T01") : (text2 + "_T02"));
		Texture2D texture2D = Resources.Load(text2, typeof(Texture2D)) as Texture2D;
		Dbg.Log("[Info] Try to load texture: " + text2, Dbg.NBLogType.PET);
		if (texture2D != null)
		{
			SkinnedMeshRenderer componentInChildren = MBHier.GetComponentInChildren<SkinnedMeshRenderer>(p_petModelGo);
			if (componentInChildren != null)
			{
				componentInChildren.material.SetTexture("_MainTex", texture2D);
			}
			else
			{
				Dbg.LogWarn("[Warn] Cannot find skin mesh renderer in game object: " + p_petModelGo.name);
			}
		}
		else
		{
			Dbg.LogWarn("[Warn] Cannot load texture: " + text2);
		}
	}

	public void SwitchActivePet(string p_petId, Callback p_success = null, Callback p_fail = null)
	{
		if (m_services != null)
		{
			m_success = p_success;
			m_fail = p_fail;
			m_services.SwitchActivePet(m_services.NubeeId, m_services.DeviceId, p_petId, OnSwitchPetSuccess, OnSwitchPetFailed);
		}
	}

	private void OnSwitchPetSuccess(Services.ServiceHandle p_handle)
	{
		if (m_success != null)
		{
			m_success();
		}
	}

	private void OnSwitchPetFailed(Services.ServiceHandle p_handle, string p_errMsg)
	{
		if (m_fail != null)
		{
			m_fail();
		}
	}
}
