using UnityEngine;

public class StageManager : SingletonComponent<StageManager>
{
	public delegate void PlayerTemplateCB();

	private string m_currentStage = string.Empty;

	private string m_stageToSwitch = string.Empty;

	private bool m_loaded;

	private bool m_switching;

	private GameObject m_levelGameObject;

	public GameObject[] m_friendGameObjects;

	private GameObject m_playerGameObjectTemplate;

	private GameObject[] m_friendGameObjectTemplates;

	private GameObject m_enemyGameObject;

	private GameObject m_playerGameObject;

	private PlayerTemplateCB m_playerTemplateCB;

	private PlayerTemplateCB m_friendTemplateCB;

	public GameObject m_shadowPrefab;

	private string playerAnimConfig = "PlayerCharacter/Male_anim";

	private int m_maxNumberOfFriends = 2;

	private static string m_prefabRoot = "Environments/Prefabs/";

	private bool m_needUpdatePlayerTemp;

	public bool IsGenerating { get; private set; }

	public GameObject PlayerObject
	{
		get
		{
			return m_playerGameObject;
		}
	}

	public GameObject PlayerTemplate
	{
		get
		{
			return m_playerGameObjectTemplate;
		}
		set
		{
			m_playerGameObjectTemplate = value;
		}
	}

	public GameObject[] FriendTemplates
	{
		get
		{
			return m_friendGameObjectTemplates;
		}
	}

	public GameObject[] FriendObjects
	{
		get
		{
			return m_friendGameObjects;
		}
	}

	public GameObject EnemyObject
	{
		get
		{
			return m_enemyGameObject;
		}
	}

	public bool NeedUpdatePlayerTemp
	{
		get
		{
			return m_needUpdatePlayerTemp;
		}
		set
		{
			m_needUpdatePlayerTemp = value;
		}
	}

	public void Awake()
	{
		m_friendGameObjectTemplates = new GameObject[m_maxNumberOfFriends];
		m_friendGameObjects = new GameObject[m_maxNumberOfFriends];
	}

	private static void DestroyPlayerTemplate(GameObject player)
	{
		if (!(player != null))
		{
			return;
		}
		SkinnedMeshRenderer component = player.GetComponent<SkinnedMeshRenderer>();
		if (component != null)
		{
			if (component.sharedMesh != null)
			{
				Object.DestroyImmediate(component.sharedMesh);
			}
			if (component.material != null)
			{
				if (component.material.mainTexture != null)
				{
					Object.DestroyImmediate(component.material.mainTexture);
				}
				Material[] materials = component.materials;
				for (int i = 0; i < materials.Length; i++)
				{
					if (materials[i] != component.material)
					{
						Object.DestroyImmediate(materials[i]);
					}
				}
				Object.DestroyImmediate(component.material);
			}
		}
		Object.DestroyImmediate(player);
	}

	public void SetPlayerTemplate(GameObject p_go)
	{
		if (p_go == null)
		{
			Dbg.LogError("Cannot set Null template.");
			if (m_playerTemplateCB != null)
			{
				m_playerTemplateCB();
				m_playerTemplateCB = null;
			}
			if (IsGenerating)
			{
				Messenger.Invoke("player_generated");
			}
			UIPopupController.Instance.HideLoadingPopup();
			IsGenerating = false;
			return;
		}
		if (m_playerGameObjectTemplate != null)
		{
			DestroyPlayerTemplate(m_playerGameObjectTemplate);
			m_playerGameObjectTemplate = null;
		}
		m_playerGameObjectTemplate = Object.Instantiate(p_go, p_go.transform.position, p_go.transform.rotation) as GameObject;
		PlayerData playerData = SingletonComponent<PlayerDataManager>.Instance.GetPlayerData();
		playerData.m_isMonster = false;
		playerData.ComputeFinalValues();
		m_playerGameObjectTemplate.layer = p_go.layer;
		m_playerGameObjectTemplate.name = "PlayerTemplate";
		m_playerGameObjectTemplate.SetActiveRecursively(false);
		Object.DontDestroyOnLoad(m_playerGameObjectTemplate);
		m_playerGameObjectTemplate.layer = LayerMask.NameToLayer("Player");
		BoxCollider orAddComponent = NBComponentHelper.GetOrAddComponent<BoxCollider>(m_playerGameObjectTemplate);
		if (orAddComponent != null)
		{
			orAddComponent.size = new Vector3(0.6f, 2.1f, 0.77f);
			orAddComponent.center = new Vector3(0f, 1f, 0f);
		}
		SpinWithMouse orAddComponent2 = NBComponentHelper.GetOrAddComponent<SpinWithMouse>(m_playerGameObjectTemplate);
		if (orAddComponent2 != null)
		{
			orAddComponent2.target = m_playerGameObjectTemplate.transform;
		}
		Dbg.Log("player Template Set successfully.", Dbg.NBLogType.CHARACTER_GENERATOR);
		if (m_playerTemplateCB != null)
		{
			m_playerTemplateCB();
			m_playerTemplateCB = null;
		}
		if (IsGenerating)
		{
			Messenger.Invoke("player_generated");
		}
		UIPopupController.Instance.HideLoadingPopup();
		m_playerGameObjectTemplate.AddComponent<DebugPlayerTemplate>();
		AttachShadow(m_playerGameObjectTemplate, "Male_ Pelvis", m_playerGameObjectTemplate.active);
		IsGenerating = false;
		Object.Destroy(p_go);
		p_go = null;
	}

	private void PrepareFirstFriend(GameObject _go)
	{
		PrepareFriend(_go, 0);
	}

	private void PrepareSecondFriend(GameObject _go)
	{
		PrepareFriend(_go, 1);
	}

	public void PrepareFriend(GameObject p_go, int p_index)
	{
		if (p_go == null)
		{
			Dbg.LogError("The given friend object is null. Do not instantiate the friend.");
			if (m_friendTemplateCB != null)
			{
				m_friendTemplateCB();
			}
			return;
		}
		GameObject gameObject = m_friendGameObjectTemplates[p_index];
		if (gameObject != null)
		{
			DestroyPlayerTemplate(gameObject);
		}
		gameObject = Object.Instantiate(p_go) as GameObject;
		gameObject.name = "FriendTemplate_" + p_index;
		gameObject.SetActiveRecursively(false);
		m_friendGameObjectTemplates[p_index] = gameObject;
		Object.DontDestroyOnLoad(m_friendGameObjectTemplates[p_index]);
		if (m_friendTemplateCB != null)
		{
			m_friendTemplateCB();
			Dbg.Log("Friend Template created!");
		}
	}

	public PlayerData GetFriendData(int p_index)
	{
		if (p_index < 0 || p_index >= m_maxNumberOfFriends)
		{
			Dbg.LogError("Friend index out of ranged.");
			return null;
		}
		if (SingletonComponent<PlayerDataManager>.Instance == null)
		{
			Dbg.LogError("No player Data Manager Found.");
			return null;
		}
		return SingletonComponent<PlayerDataManager>.Instance.GetAllyData(p_index);
	}

	public void SetStage(string p_stageId)
	{
		SetStage(p_stageId, false);
	}

	public void SetStage(string p_stageId, bool p_forceReset)
	{
		m_stageToSwitch = p_stageId;
		m_switching = m_stageToSwitch != m_currentStage;
		if (p_forceReset)
		{
			m_switching = true;
		}
		LoadStage();
	}

	private void LoadStage()
	{
		if (m_switching && m_loaded)
		{
			UnloadStage();
		}
		m_loaded = true;
		m_currentStage = m_stageToSwitch;
		Dbg.Log("Creating level: " + m_currentStage);
		string text = m_prefabRoot + m_currentStage;
		Object obj = Resources.Load(text);
		if (obj == null)
		{
			Dbg.LogWarn("The desired stage prefab cannot be found: " + text);
			return;
		}
		m_levelGameObject = Object.Instantiate(obj, Vector3.zero, Quaternion.identity) as GameObject;
		if (m_levelGameObject == null)
		{
			Dbg.LogError("The desired stage wasn't loaded due to instancing problem!");
			m_loaded = false;
		}
	}

	public void UnloadStage()
	{
		if (!m_loaded)
		{
			return;
		}
		m_loaded = false;
		if (m_levelGameObject != null)
		{
			Object.Destroy(m_levelGameObject);
			m_levelGameObject = null;
		}
		if (m_playerGameObject != null)
		{
			Object.Destroy(m_playerGameObject);
			m_playerGameObject = null;
		}
		if (m_playerGameObjectTemplate != null)
		{
			Object.Destroy(m_playerGameObjectTemplate);
			m_playerGameObjectTemplate = null;
		}
		if (m_enemyGameObject != null)
		{
			Object.Destroy(m_enemyGameObject);
			m_enemyGameObject = null;
		}
		for (int i = 0; i < m_friendGameObjects.Length; i++)
		{
			if (m_friendGameObjects[i] != null)
			{
				Object.Destroy(m_friendGameObjects[i]);
				m_friendGameObjects[i] = null;
			}
		}
		for (int j = 0; j < m_friendGameObjectTemplates.Length; j++)
		{
			if (m_friendGameObjectTemplates[j] != null)
			{
				Object.Destroy(m_friendGameObjectTemplates[j]);
				m_friendGameObjectTemplates[j] = null;
			}
		}
		Resources.UnloadUnusedAssets();
	}

	public Monster CreateMonster(DbPlayerDataStruct p_playerStruct)
	{
		Dbg.Log("Creating Monster: " + p_playerStruct.id);
		PlayerDataManager instance = SingletonComponent<PlayerDataManager>.Instance;
		m_enemyGameObject = Singleton<MonsterFactory>.Instance.CreateMonster(p_playerStruct.name);
		m_enemyGameObject.tag = "Enemy";
		PlayerData opponentData = instance.GetOpponentData();
		if (opponentData == null)
		{
			Dbg.LogError("Failed to retreive enemy data.");
			return null;
		}
		opponentData.m_isMonster = true;
		opponentData.Name = SingletonComponent<NBLocalization>.Instance.Get(m_enemyGameObject.name);
		instance.PlayerStructToData(p_playerStruct, opponentData);
		opponentData.ComputeFinalValues();
		opponentData.ResetHp();
		Monster component = m_enemyGameObject.GetComponent<Monster>();
		if (component == null)
		{
			Dbg.LogError("No 'Monster' component attached to this game object.");
			return null;
		}
		component.Data = opponentData;
		Dbg.Log("Monster's data: " + opponentData);
		return component;
	}

	public void DestroyMonster(Monster monster)
	{
		if (monster.gameObject == m_enemyGameObject)
		{
			m_enemyGameObject = null;
		}
		Singleton<MonsterFactory>.Instance.DestroyMonster(monster.gameObject);
	}

	public void CreatePlayerTemplate(PlayerTemplateCB _playerTemplateCB)
	{
		m_playerTemplateCB = _playerTemplateCB;
		Singleton<CharacterGenerator>.Instance.GenerateGamePlayCharacter(SetPlayerTemplate, SingletonComponent<PlayerDataManager>.Instance.GetPlayerData(), true);
	}

	public void CreatePlayerMainMenuTemplate(PlayerTemplateCB p_callback, bool p_showLoading)
	{
		CreatePlayerMainMenuTemplate(p_callback, null, true, p_showLoading);
	}

	public void CreatePlayerMainMenuTemplate(PlayerTemplateCB _playerTemplateCB, PlayerData playerData = null, bool p_isMainChara = true, bool p_useLoading = true)
	{
		if (!m_needUpdatePlayerTemp && m_playerGameObjectTemplate != null)
		{
			if (_playerTemplateCB != null)
			{
				_playerTemplateCB();
			}
			return;
		}
		IsGenerating = true;
		m_playerTemplateCB = _playerTemplateCB;
		if (playerData == null)
		{
			playerData = SingletonComponent<PlayerDataManager>.Instance.GetPlayerData();
		}
		if (p_useLoading && UIPopupController.Instance != null)
		{
			UIPopupController.Instance.ShowLoadingPopup(SingletonComponent<NBLocalization>.Instance.Get("character_creation_header"), SingletonComponent<NBLocalization>.Instance.Get("character_creation_content"));
		}
		Singleton<CharacterGenerator>.Instance.GenerateMainMenuCharacter(SetPlayerTemplate, playerData, p_isMainChara);
		m_needUpdatePlayerTemp = false;
	}

	public void StopCharacterGeneration()
	{
		m_playerTemplateCB = null;
		IsGenerating = false;
		m_needUpdatePlayerTemp = true;
	}

	public void CreateFriendTempalte(PlayerTemplateCB _friendTemplateCB, int _friendIndex = 0)
	{
		m_friendTemplateCB = _friendTemplateCB;
		PlayerData allyData = SingletonComponent<PlayerDataManager>.Instance.GetAllyData(_friendIndex);
		if ((allyData == null || allyData.IsDummy) && m_friendTemplateCB != null)
		{
			m_friendTemplateCB();
			return;
		}
		switch (_friendIndex)
		{
		case 0:
			Singleton<CharacterGenerator>.Instance.GenerateGamePlayCharacter(PrepareFirstFriend, allyData, false);
			break;
		case 1:
			Singleton<CharacterGenerator>.Instance.GenerateGamePlayCharacter(PrepareSecondFriend, allyData, false);
			break;
		}
	}

	public Player CreatePlayer()
	{
		DungeonManager instance = SingletonComponent<DungeonManager>.Instance;
		Dbg.LogWarn(instance.name);
		GameObject gameObject = Object.Instantiate(m_playerGameObjectTemplate, Vector3.zero, Quaternion.identity) as GameObject;
		if (gameObject == null)
		{
			Dbg.LogError("Unable to create player game-obect.");
			return null;
		}
		gameObject.SetActiveRecursively(true);
		gameObject.name = "Player";
		gameObject.tag = "Player";
		m_playerGameObject = gameObject;
		NBAnimation nBAnimation = gameObject.AddComponent<NBAnimation>();
		if (nBAnimation == null)
		{
			Dbg.LogError("Unable to create 'NBAnimation' component.");
			return null;
		}
		nBAnimation.ParseConfig(playerAnimConfig);
		m_playerGameObject.GetComponent<Animation>().Play("Idle");
		m_playerGameObject.GetComponent<Animation>().Sample();
		SkinnedMeshRenderer component = gameObject.GetComponent<SkinnedMeshRenderer>();
		component.gameObject.layer = 9;
		component.material.color = Color.white;
		Player player = gameObject.AddComponent<Player>();
		if (player == null)
		{
			Dbg.LogError("Cannot create 'Player' component.");
			return null;
		}
		PlayerData playerData = (player.Data = SingletonComponent<PlayerDataManager>.Instance.GetPlayerData());
		playerData.ResetHp();
		PlayerController playerController = gameObject.AddComponent<PlayerController>();
		if (playerController == null)
		{
			Dbg.LogError("Cannot create 'PlayerController' component.");
			return null;
		}
		PlayerSpecialFx playerSpecialFx = gameObject.AddComponent<PlayerSpecialFx>();
		if (playerSpecialFx == null)
		{
			Dbg.LogError("Cannot create 'PlayerSpecialFx' component.");
			return null;
		}
		gameObject.AddComponent<PlayerHealFx>();
		gameObject.AddComponent<PlayerDamageDisplay>();
		return player;
	}

	public GameObject CreateFriend(int p_index)
	{
		PlayerDataManager instance = SingletonComponent<PlayerDataManager>.Instance;
		Dbg.Log(instance.name + " -- Creating Friend: " + p_index);
		if (m_friendGameObjectTemplates == null || p_index < 0 || p_index >= m_maxNumberOfFriends || m_friendGameObjectTemplates[p_index] == null)
		{
			Dbg.LogWarn("Do not create friends.");
			return null;
		}
		GameObject gameObject = Object.Instantiate(m_friendGameObjectTemplates[p_index]) as GameObject;
		gameObject.name = "Friend_" + p_index;
		gameObject.tag = "Ally";
		gameObject.GetComponent<Animation>().Stop();
		gameObject.GetComponent<Animation>().clip = null;
		gameObject.GetComponent<Animation>().playAutomatically = false;
		m_friendGameObjects[p_index] = gameObject;
		SkinnedMeshRenderer component = gameObject.GetComponent<SkinnedMeshRenderer>();
		component.gameObject.layer = 9;
		component.updateWhenOffscreen = true;
		PlayerData friendData = GetFriendData(p_index);
		friendData.ComputeFinalValues();
		gameObject.AddComponent<Friend>();
		gameObject.AddComponent<CharacterAmbientColor>();
		gameObject.AddComponent<CharacterSoundFx>();
		AttachShadow(gameObject, "Male_ Pelvis");
		gameObject.transform.position = Vector3.up * 1000f;
		return gameObject;
	}

	private void AttachShadow(GameObject p_character, string p_bone, bool p_activate = false)
	{
		if (m_shadowPrefab != null)
		{
			GameObject gameObject = Object.Instantiate(m_shadowPrefab) as GameObject;
			gameObject.transform.position += p_character.transform.position;
			gameObject.transform.parent = p_character.transform;
			ShadowProjector component = gameObject.GetComponent<ShadowProjector>();
			component.Reference = p_character.transform.FindInHierarchy("Male_ Pelvis");
			component.gameObject.active = p_activate;
		}
	}
}
