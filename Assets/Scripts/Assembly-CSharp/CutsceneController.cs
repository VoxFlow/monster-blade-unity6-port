using System;
using System.Collections;
using UnityEngine;

public class CutsceneController : SingletonComponent<CutsceneController>
{
	public enum CutsceneType
	{
		NONE = -1,
		FRIEND_ASSIST_CUTSCENE_BEGIN = 0,
		FRIEND_ASSIST_ATTACK = 0,
		FRIEND_ASSIST_MASSIVE_DAMAGE = 1,
		FRIEND_ASSIST_CRITICAL_BUFF = 2,
		FRIEND_ASSIST_DAMAGE_BUFF = 3,
		FRIEND_ASSIST_HEAL = 4,
		FRIEND_ASSIST_STUN = 5,
		FRIEND_ASSIST_CUTSCENE_MAX = 5,
		DEATH_CUTSCENE_BEGIN = 6,
		DEATH_BULL = 6,
		DEATH_TRICEP = 7,
		DEATH_WOLF = 8,
		DEATH_HYDRA = 9,
		DEATH_LANDDRAGON = 10,
		DEATH_WINGCHICKEN = 11,
		DEATH_RAPTOR = 12,
		DEATH_WINGSERPENT = 13,
		DEATH_STONEDRAGON = 14,
		DEATH_SHARK = 15,
		DEATH_RHINO = 16,
		DEATH_LIZARD = 17,
		DEATH_MANTA_RAY = 18,
		DEATH_EPICDRAGON = 19,
		DEATH_CUTSCENE_MAX = 19,
		INTRO_CUTSCENE_BEGIN = 20,
		INTRO_BULL = 20,
		INTRO_TRICEP = 21,
		INTRO_WOLF = 22,
		INTRO_HYDRA = 23,
		INTRO_LANDDRAGON = 24,
		INTRO_WINGCHICKEN = 25,
		INTRO_RAPTOR = 26,
		INTRO_WINGSERPENT = 27,
		INTRO_DODOBO = 28,
		INTRO_STONEDRAGON = 29,
		INTRO_SHARK = 30,
		INTRO_LIZARD = 31,
		INTRO_EPICDRAGON = 32,
		INTRO_CUTSCENE_MAX = 32,
		SKILL_WOLF_ARMOR_SELF = 33,
		SKILL_WINGCHICKEN_ARMOR_SELF = 34,
		SKILL_STONEDRAGON_ARMOR_SELF = 35,
		SKILL_HYDRA_ARMOR_SELF = 36,
		SKILL_DARKWOLF_ARMOR_SELF = 37,
		SKILL_LANDDRAGON_ARMOR_SELF = 38,
		SKILL_EPICDRAGON_ARMOR_SELF = 39,
		SKILL_ARMOR_ASSIST_BEGIN = 40,
		SKILL_WOLF_ARMOR_FRIEND = 40,
		SKILL_WINGCHICK_ARMOR_FRIEND = 41,
		SKILL_STONEDRAGON_ARMOR_FRIEND = 42,
		SKILL_HYDRA_ARMOR_FRIEND = 43,
		SKILL_DARKWOLF_ARMOR_FRIEND = 44,
		SKILL_LANDDRAGON_ARMOR_FRIEND = 45,
		SKILL_EPICDRAGON_ARMOR_FRIEND = 46,
		SKILL_ARMOR_ASSIST_END = 46,
		EPICDRAGON_FINAL_PHASE = 47,
		PLAYER_DEAD_EPICDRAGON = 48,
		TUTORIAL1 = 49,
		MAX = 50
	}

	public CutsceneType CurrentPlayingType = CutsceneType.NONE;

	public float globalTimeScale = 1f;

	private static readonly string[] m_cutscenePrefabs = new string[50]
	{
		"FriendAssists/Cutscene_Attack_Damage", "FriendAssists/Cutscene_Attack_Damage", "FriendAssists/Cutscene_Buff_Critical", "FriendAssists/Cutscene_Buff_Damage", "FriendAssists/Cutscene_Heal", "FriendAssists/Cutscene_Buff_Stun", "Monsters/Prefabs/Cutscene_MobDeath_S_Bull", "Monsters/Prefabs/Cutscene_MobDeath_S_Triceratops", "Monsters/Prefabs/Cutscene_MobDeath_M_Wolf", "Monsters/Prefabs/Cutscene_MobDeath_M_Hydra",
		"Monsters/Prefabs/Cutscene_MobDeath_L_LandDragon", "Monsters/Prefabs/Cutscene_MobDeath_M_ChickWing", "Monsters/Prefabs/Cutscene_MobDeath_S_Reptar", "Monsters/Prefabs/Cutscene_MobDeath_S_Mosquito", "Monsters/Prefabs/Cutscene_MobDeath_L_StoneDragon", "Monsters/Prefabs/Cutscene_MobDeath_M_Shark", "Monsters/Prefabs/Cutscene_MobDeath_S_Rhino", "Monsters/Prefabs/Cutscene_MobDeath_M_Lizard", "Monsters/Prefabs/Cutscene_MobDeath_S_MantaRay", "Monsters/Prefabs/Cutscene_BossDeath_Mob07_EpicDragon",
		"Monsters/Prefabs/Cutscene_BossIntro_Mob01_Bull", "Monsters/Prefabs/Cutscene_BossIntro_Mob02_Tricep", "Monsters/Prefabs/Cutscene_BossIntro_Mob03_Wolf", "Monsters/Prefabs/Cutscene_BossIntro_Mob05_Hydra", "Monsters/Prefabs/Cutscene_BossIntro_Mob06", "Monsters/Prefabs/Cutscene_BossIntro_Mob08_ChickWing", "Monsters/Prefabs/Cutscene_BossIntro_Mob09_Reptar", "Monsters/Prefabs/Cutscene_BossIntro_Mob10_Mosquito", "Monsters/Prefabs/Cutscene_BossIntro_Mob11_Dodobo", "Monsters/Prefabs/Cutscene_BossIntro_Mob12",
		"Monsters/Prefabs/Cutscene_BossIntro_Mob13_Shark", "Monsters/Prefabs/Cutscene_BossIntro_Mob15_Lizard", "Monsters/Prefabs/Cutscene_BossIntro_Mob07_EpicDragon", "Players/Cutscene_ ArmorAssit_Player (Wolf)", "Players/Cutscene_ ArmorAssit_Player (Wing_Chick)", "Players/Cutscene_ ArmorAssit_Player (StoneDrag)", "Players/Cutscene_ ArmorAssit_Player (Hydra)", "Players/Cutscene_ ArmorAssit_Player (DarkWolf)", "Players/Cutscene_ ArmorAssit_Player (LandDragon)", "Players/Cutscene_ ArmorAssit_Player (EpicDrag)",
		"FriendAssists/Cutscene_ ArmorAssit_Friend (Wolf)", "FriendAssists/Cutscene_ ArmorAssit_Friend (WingChick)", "FriendAssists/Cutscene_ ArmorAssit_Friend (StoneDragon)", "FriendAssists/Cutscene_ ArmorAssit_Friend (Hydra)", "FriendAssists/Cutscene_ ArmorAssit_Friend (DarkWolf)", "FriendAssists/Cutscene_ ArmorAssit_Friend (LandDrag)", "FriendAssists/Cutscene_ ArmorAssit_Friend (EpicDrag)", "Monsters/Prefabs/Cutscene_HillSplit_Mob07_EpicDragon", "Players/Cutscene_PlayerDead_Mob07_Epic", "Tutorial/Cutscene_Tutorial01"
	};

	private static readonly string m_cutscenePrefabRoot = "Cutscenes/";

	private Player m_player;

	private Monster m_monster;

	private GameObject m_friend;

	private bool m_playing;

	private bool m_isSpeedUp;

	public GameObject m_speedUpBtn;

	public bool Playing
	{
		get
		{
			return m_playing;
		}
	}

	public bool Pause { get; set; }

	public static bool IsMobIntroCutscene(CutsceneType p_cutsceneType)
	{
		return p_cutsceneType.CompareTo(CutsceneType.INTRO_CUTSCENE_BEGIN) >= 0 && p_cutsceneType.CompareTo(CutsceneType.INTRO_EPICDRAGON) <= 0;
	}

	public static bool IsMobDeathCutscene(CutsceneType p_cutsceneType)
	{
		return p_cutsceneType.CompareTo(CutsceneType.DEATH_CUTSCENE_BEGIN) >= 0 && p_cutsceneType.CompareTo(CutsceneType.DEATH_EPICDRAGON) <= 0;
	}

	public static bool IsFriendAssistCutscene(CutsceneType p_cutsceneType)
	{
		return (p_cutsceneType.CompareTo(CutsceneType.FRIEND_ASSIST_CUTSCENE_BEGIN) >= 0 && p_cutsceneType.CompareTo(CutsceneType.FRIEND_ASSIST_STUN) <= 0) || (p_cutsceneType.CompareTo(CutsceneType.SKILL_ARMOR_ASSIST_BEGIN) >= 0 && p_cutsceneType.CompareTo(CutsceneType.SKILL_EPICDRAGON_ARMOR_FRIEND) <= 0);
	}

	public static GameObject LoadCutscenePrefab(CutsceneType p_cutsceneType)
	{
		string text = m_cutscenePrefabRoot + m_cutscenePrefabs[(int)p_cutsceneType];
		Dbg.Log("[CutsceneController] Instantiating: " + text);
		return Resources.Load(text) as GameObject;
	}

	private void Awake()
	{
		base.gameObject.SetActiveRecursively(false);
		base.gameObject.active = true;
		Pause = false;
		Messenger.AddListener("battle_ready", OnBattleReady);
	}

	private void Start()
	{
		StageManager instance = SingletonComponent<StageManager>.Instance;
		if (instance != null)
		{
			m_player = instance.PlayerObject.GetComponent<Player>();
			m_friend = instance.FriendObjects[0];
		}
	}

	private new void OnDestroy()
	{
		Messenger.RemoveListener("battle_ready", OnBattleReady);
	}

	private void OnBattleReady()
	{
		m_monster = SingletonComponent<StageManager>.Instance.EnemyObject.GetComponent<Monster>();
	}

	private CutscenePlayer InstantiateCutsceneObject(CutsceneType p_cutsceneType)
	{
		if (p_cutsceneType != CutsceneType.NONE && p_cutsceneType < CutsceneType.MAX)
		{
			GameObject gameObject = LoadCutscenePrefab(p_cutsceneType);
			if (gameObject != null)
			{
				GameObject gameObject2 = UnityEngine.Object.Instantiate(gameObject) as GameObject;
				if (gameObject2 != null)
				{
					CutsceneSetup cutsceneSetup = gameObject2.AddComponent<CutsceneSetup>();
					if (cutsceneSetup != null)
					{
						if (IsFriendAssistCutscene(p_cutsceneType) && null != m_friend)
						{
							m_friend.transform.position = Vector3.zero;
							m_friend.transform.rotation = Quaternion.identity;
							m_friend.transform.localScale = Vector3.one;
						}
						cutsceneSetup.m_playerObject = m_player.gameObject;
						cutsceneSetup.m_enemyObject = m_monster.gameObject;
						cutsceneSetup.m_friendObject = m_friend;
						cutsceneSetup.PrepareCutsceneData();
					}
					gameObject2.transform.parent = base.transform;
					return gameObject2.GetComponent<CutscenePlayer>();
				}
			}
		}
		Dbg.LogWarn("Unable to instantiate cutscene game-object of type: " + p_cutsceneType);
		return null;
	}

	public IEnumerator PlayCutscene(CutsceneType p_cutsceneType, Action<CutsceneType> p_onFailCallback = null)
	{
		if (!base.enabled)
		{
			if (p_onFailCallback != null)
			{
				p_onFailCallback(p_cutsceneType);
			}
		}
		else
		{
			if (m_playing || p_cutsceneType == CutsceneType.NONE)
			{
				yield break;
			}
			Dbg.Log("Start yo play cutscene, time : " + DateTime.Now.ToString("HH:mm:ss.ffffff"));
			m_playing = true;
			Dbg.Log("Cutscene Started: " + p_cutsceneType);
			BattleUtils.EnableSkills(false, this);
			bool playerReady = false;
			bool monsterReady = false;
			while (!playerReady || !monsterReady)
			{
				if (!playerReady && m_player.CanPlayCutscene())
				{
					playerReady = true;
					PlayerController pc = m_player.GetComponent<PlayerController>();
					if (pc != null)
					{
						pc.enabled = false;
					}
				}
				if (!monsterReady && m_monster.CanPlayCutscene())
				{
					monsterReady = true;
					AIController ai = m_monster.GetComponent<AIController>();
					if (ai != null)
					{
						ai.enabled = false;
					}
				}
				yield return null;
				if (m_player.Data.Hp == 0f && p_cutsceneType != CutsceneType.PLAYER_DEAD_EPICDRAGON)
				{
					Dbg.LogWarn("CutsceneController - Player was already dead!");
					m_playing = false;
					if (p_onFailCallback != null)
					{
						p_onFailCallback(p_cutsceneType);
					}
					yield break;
				}
				if (monsterReady && playerReady)
				{
					if (m_player.CurrentState != Player.State.Idle)
					{
						m_player.Idle();
					}
					m_player.ChangeState(Player.State.Cutscene);
					if (m_monster.CurrentState != Monster.State.Idle)
					{
						m_monster.Idle();
					}
					m_monster.ChangeState(Monster.State.Cutscene);
				}
			}
			FaderGui fader = SingletonComponent<FaderGui>.Instance;
			if (fader != null)
			{
				fader.FadeOut(Color.black, 0.5f);
				while (fader.m_faderValue < 1f)
				{
					yield return null;
				}
			}
			if (IsMobIntroCutscene(p_cutsceneType) || IsMobDeathCutscene(p_cutsceneType))
			{
				m_monster.SetVisible(true);
				if (IsMobDeathCutscene(p_cutsceneType))
				{
					for (int i = 0; i < m_monster.Faders.Length; i++)
					{
						if (null != m_monster.Faders[i])
						{
							m_monster.Faders[i].FadeFromCurrentAlpha(1f, 1f, null);
						}
					}
				}
			}
			Messenger<CutsceneType>.Invoke(CutsceneMessage.CUTSCENE_START, p_cutsceneType);
			Dbg.Log("CutsceneController.Waiter.PauseAll");
			Waiter.PauseAll(true);
			SlowMotionSystem slomo = SingletonComponent<SlowMotionSystem>.Instance;
			Camera mainCamera = Camera.main;
			if ((bool)mainCamera)
			{
				mainCamera.enabled = false;
			}
			if (slomo != null)
			{
				slomo.Stop();
			}
			CutscenePlayer cutscene = InstantiateCutsceneObject(p_cutsceneType);
			CurrentPlayingType = p_cutsceneType;
			if (cutscene != null)
			{
				cutscene.Play();
				m_isSpeedUp = false;
				if (m_speedUpBtn != null && (bool)m_speedUpBtn.transform.parent)
				{
					m_speedUpBtn.transform.parent.gameObject.SetActiveRecursively(true);
				}
				while (cutscene.enabled)
				{
					if (!Pause)
					{
						float timeScale = 1f;
						if (m_isSpeedUp)
						{
							timeScale = 3f;
						}
						Time.timeScale = globalTimeScale * timeScale;
					}
					yield return null;
				}
			}
			if (m_speedUpBtn != null)
			{
				m_speedUpBtn.SetActiveRecursively(false);
			}
			if (!IsMobDeathCutscene(p_cutsceneType))
			{
				m_monster.Idle();
			}
			m_player.Idle();
			CutsceneAnimSq playerCutscene = m_player.GetComponent<CutsceneAnimSq>();
			CutsceneAnimSq monsterCutscene = m_monster.GetComponent<CutsceneAnimSq>();
			if (playerCutscene != null)
			{
				UnityEngine.Object.Destroy(playerCutscene);
			}
			if (monsterCutscene != null)
			{
				UnityEngine.Object.Destroy(monsterCutscene);
			}
			Waiter.PauseAll(false);
			if ((bool)mainCamera)
			{
				mainCamera.enabled = true;
			}
			BattleUtils.EnableSkills(true, this);
			Time.timeScale = (globalTimeScale = 1f);
			UnityEngine.Object.Destroy(cutscene.gameObject);
			m_playing = false;
			Messenger<CutsceneType>.Invoke(CutsceneMessage.CUTSCENE_END, p_cutsceneType);
			Dbg.Log(string.Concat("Cutscene Ended: ", p_cutsceneType, " time : ", DateTime.Now.ToString("HH:mm:ss.ffffff")));
		}
	}

	public void OnSpeedUpPressed()
	{
		m_isSpeedUp = true;
		m_speedUpBtn.transform.localScale = Vector3.one * 0.8f;
	}

	public void OnSpeedUpReleased()
	{
		m_isSpeedUp = false;
		m_speedUpBtn.transform.localScale = Vector3.one;
	}

	private void Update()
	{
	}
}
