using System.Collections.Generic;
using Models;
using UnityEngine;

public class GLOBAL
{
	public enum ServerType
	{
		LIVE = 0,
		STAGING = 1,
		DEV = 2,
		Max = 3
	}

	public const float FIXED_ORTHO_SIZE = 360f;

	public static string URL_APP_VERSION = "1.2.0";

	public static string CLIENT_VERSION = URL_APP_VERSION + ".1";

	public static string STORE_APP_VERSION = "1.3.3";

	public static string PATCH_VERSION = "1.2.0";

	public static string PLATFORM_NAME = "android";

	public static string APP_STORE_LINK = "market://details?id=com.nubee.monsterslayer";

	public static string NUBEE_ZHS_WEBSITE = "http://nubee.com/2013/04/monster-blade-4/?lang=zh-hans";

	public static float DELTA_RATIO = 1f;

	public static float WIDTH_RATIO = 1f;

	public static float BASE_WIDTH = 960f;

	public static float BASE_HEIGHT = 640f;

	public static float UI_SCALE_FACTOR = 1f;

	public static float ORTHO_RATIO = 1f;

	public static int SERVER_TIME_CMD = 10;

	public static int SERVER_TIME_KEY = 7200;

	public static string SERVER_CHECK = "http://check.ms.nubee.com/";

	public static string SERVER_URL_LIVE = "https://ms.nubee.com/";

	public static string SERVER_NOTIFICATION_PRODUCTION = "http://notifications.nubee.com/";

	private static string LIVE_SEED_SERVER = "http://ec2-175-41-230-147.ap-northeast-1.compute.amazonaws.com/";

	private static string STAGING_LOAD_TEST = "http://loadtest.ms.nubee.com/";

	public static string SERVER_URL_DEV = "http://dev.ms.nubee.com/";

	public static string SERVER_URL_STAGING = "http://st.ms.nubee.com/";

	public static string SERVER_NOTIFICATION_STAGING = "http://st-notifications.nubee.com/";

	public static string SERVER_NOTIFICATION_GAME_ID = "6";

	public static string SERVER_NUBEE_PLATFORM_APPID = "monsterslayer";

	public static string SERVER_NUBEE_PLATFORM_HASHSALT = "";

	public static string ASSETS_BUNDLE_URL = "info/latest.info";

	public static string SERVER_ERROR_TAG = "MS_ServerCall_Failed ->";

	public static string SERVER_FIXED_KEY_ACCOUNT_UPDATE_DEVICE_ID = "";

	public static string SERVER_FIXED_KEY_ACCOUNT_REGISTER = "";

	public static string SERVER_FIXED_KEY_SECURITY_KEY = "";

	public static string SERVER_FIXED_KEY_SECURITY_KEY_SPECIAL = "";

	public static string SERVER_FIXED_KEY_NEW_INSTALLATION = "";

	public static string SERVER_FIXED_KEY_CACHE = "";

	private static ServerType Server_type = ServerType.LIVE;

	public static string SERVICE_CALL_BASE_PATH = SERVER_URL_LIVE + URL_APP_VERSION + "/";

	public static string NOTIFY_SERVER_BASE_PATH = SERVER_NOTIFICATION_PRODUCTION;

	public static int PROMOTION_UI_WIDTH = 864;

	public static int PROMOTION_UI_HEIGHT = 512;

	public static string CHARACTER_PREF_NAME = "Character Generator Pref";

	public static string[] DEFAULT_PARTS = new string[5] { "set00-t1_head", "set00-t1_shoulder", "set00-t1_body", "set00-t1_leg", "weapon00-t1_weapon" };

	public static string DEFAULT_BELT = "set00-t1_leg";

	public static string[] FRIEND_ASSIST_TUTORIAL_2 = new string[5] { "WEAPON_WOLF_T5_HILL", "ARMOUR_HELM_WOLF_T5_HILL", "ARMOUR_CHEST_WOLF_T5_HILL", "ARMOUR_SHOULDERS_WOLF_T5_HILL", "ARMOUR_LEGGINGS_WOLF_T5_HILL" };

	public static string FRIEND_ASSIST_TUTORIAL_2_NAME = "lone_wanderer";

	public static string CHARACTER_DEFAULT_NAME = "Slayer";

	public static string WINGCHICKEN_SETNAME = "special_wingdragon_t5_a5";

	public static string FORESTSTRIDER_SETNAME = "special_foreststrider_t3_a3";

	public static string HYDRA_SETNAME = "special_hydra_t5_a5";

	public static string STONEDRAGON_SETNAME = "special_stonedragon_t5_a5";

	public static string WOLF_SETNAME = "special_wolf_t5_a5";

	public static string DARK_WOLF_SETNAME = "special_dark_wolf_t5_a5";

	public static string CHARACTER_ASSETBUNDLE_FOLDER = "assetbundles/";

	public static string CHARACTER_AB_ROOT_FOLDER = CHARACTER_ASSETBUNDLE_FOLDER + "characters/";

	public static string CHARACTER_ELEMENT_DATABASE = CHARACTER_AB_ROOT_FOLDER + "character_element_database.assetbundle";

	public static string TITLE_SCENE = "TitleScreen";

	public static string ROOT_UI_SCENE = "Root_UI";

	public static string BATTLE_VIEW_SCENE = "BattleView";

	public static GameSettings GAME_SETTINGS = new GameSettings();

	public static int NUM_BASE_ITEMS = 5;

	public static string GACHA_RESULT_ANIM_PATH = "UI/Prefabs/MainMenu/Gacha/Gacha_Result_Anim_premium";

	public static string ARMOR_COLLECTIONS_PATH = "UI/ArmorCollections/";

	public static readonly Dictionary<Element.Type, string> s_elementIconMap = new Dictionary<Element.Type, string>
	{
		{
			Element.Type.Lightning,
			"lightning_icon"
		},
		{
			Element.Type.Earth,
			"nature_icon"
		},
		{
			Element.Type.Water,
			"water_icon"
		},
		{
			Element.Type.Fire,
			"fire_icon"
		},
		{
			Element.Type.Metal,
			"metal_icon"
		}
	};

	public static readonly string DARKWOLF_SKILL_ICON = "Social/dark_ability";

	public static readonly string DEFAULT_ITEM_SKILL_PREFIX = "_GHOUL";

	public static readonly string DEFAULT_BASE_ITEM_ENDING = "_COMMON";

	public static readonly string DEFAULT_BASE_ITEM_FLAG = "BASE";

	public static ServerType SERVER_TYPE
	{
		get
		{
			return Server_type;
		}
	}

	public static string ASSETS_BASE_PATH
	{
		get
		{
			return SERVICE_CALL_BASE_PATH + PLATFORM_NAME + "/";
		}
	}

	public static string SRVCMD_PATCH_INFO
	{
		get
		{
			return SERVICE_CALL_BASE_PATH + "api/server/patch_info/";
		}
	}

	public static string SRVCMD_PATCH_INFO_FORCED
	{
		get
		{
			return SERVICE_CALL_BASE_PATH + "api/server/patch_info/";
		}
	}

	public static string SRVCMD_REGISTER_NUBEE_ACCOUNT
	{
		get
		{
			return SERVICE_CALL_BASE_PATH + "api/account/register";
		}
	}

	public static string SRVCMD_REGISTER_UPDATE_NUBEE_ACCOUNT
	{
		get
		{
			return SERVICE_CALL_BASE_PATH + "api/account/create_update_user";
		}
	}

	public static string SRVCMD_UPDATE_DEVICE_ID
	{
		get
		{
			return SERVICE_CALL_BASE_PATH + "api/account/update_device_id";
		}
	}

	public static string SRVCMD_IS_REGISTERED
	{
		get
		{
			return SERVICE_CALL_BASE_PATH + "api/account/is_registered/";
		}
	}

	public static string SRVCMD_RETRIEVE_SECRET_KEY
	{
		get
		{
			return SERVICE_CALL_BASE_PATH + "api/security/key/";
		}
	}

	public static string SRVCMD_SAVE_NUBEE_ACCOUNT_VERSION
	{
		get
		{
			return SERVICE_CALL_BASE_PATH + "api/account/save_version";
		}
	}

	public static string SRVCMD_SAVE_SESSION_TIMESTAMP
	{
		get
		{
			return SERVICE_CALL_BASE_PATH + "api/account/save_session_timestamp";
		}
	}

	public static string SRVCMD_SAVE_CLIENT_INFO
	{
		get
		{
			return SERVICE_CALL_BASE_PATH + "api/account/save_client_info";
		}
	}

	public static string SRVCMD_REGISTER_CHARACTER
	{
		get
		{
			return SERVICE_CALL_BASE_PATH + "api/character/register";
		}
	}

	public static string SRVCMD_UPDATE_CHARACTER
	{
		get
		{
			return SERVICE_CALL_BASE_PATH + "api/character/update";
		}
	}

	public static string SRVCMD_GET_CHARACTER
	{
		get
		{
			return SERVICE_CALL_BASE_PATH + "api/character/find/";
		}
	}

	public static string SRVCMD_GET_INVENTORY
	{
		get
		{
			return SERVICE_CALL_BASE_PATH + "api/character/inventory/";
		}
	}

	public static string SRVCMD_UPDATE_INVENTORY
	{
		get
		{
			return SERVICE_CALL_BASE_PATH + "api/character/update_equipments";
		}
	}

	public static string SRVCMD_SELL_ITEM
	{
		get
		{
			return SERVICE_CALL_BASE_PATH + "api/character/item/sell";
		}
	}

	public static string SRVCMD_SELL_ITEM_LIST
	{
		get
		{
			return SERVICE_CALL_BASE_PATH + "api/character/item/multi_sell";
		}
	}

	public static string SRVCMD_GET_ACHIEVEMENTS
	{
		get
		{
			return SERVICE_CALL_BASE_PATH + "api/achievements/all";
		}
	}

	public static string SRVCMD_UPDATE_PROGRESS
	{
		get
		{
			return SERVICE_CALL_BASE_PATH + "api/achievements/save_progress";
		}
	}

	public static string SRVCMD_UNLOCK_ACHIEVEMENT
	{
		get
		{
			return SERVICE_CALL_BASE_PATH + "api/achievements/unlock";
		}
	}

	public static string SRVCMD_MARK_ACHIEVEMENTS
	{
		get
		{
			return SERVICE_CALL_BASE_PATH + "api/achievements/mark";
		}
	}

	public static string SRVCMD_CLAIM_ACHIEVEMENTS
	{
		get
		{
			return SERVICE_CALL_BASE_PATH + "api/achievements/claim";
		}
	}

	public static string SRVCMD_GET_ANNOUNCEMENTS
	{
		get
		{
			return SERVICE_CALL_BASE_PATH + "api/server/get_announcements";
		}
	}

	public static string SRVCMD_CLAIM_ANNOUNCEMENT
	{
		get
		{
			return SERVICE_CALL_BASE_PATH + "api/server/claim_announcement_attachments";
		}
	}

	public static string SRVCMD_DELETE_ANNOUNCEMENT
	{
		get
		{
			return SERVICE_CALL_BASE_PATH + "api/server/remove_announcement";
		}
	}

	public static string SRVCMD_UNLOCK_PET
	{
		get
		{
			return SERVICE_CALL_BASE_PATH + "api/pets/unlock";
		}
	}

	public static string SRVCMD_SWITCH_ACTIVE_PET
	{
		get
		{
			return SERVICE_CALL_BASE_PATH + "api/pets/switch_active";
		}
	}

	public static string SRVCMD_GET_PET_INFO
	{
		get
		{
			return SERVICE_CALL_BASE_PATH + "api/pets/find";
		}
	}

	public static string SRVCMD_GET_UNLOCKED_PET_IDS
	{
		get
		{
			return SERVICE_CALL_BASE_PATH + "api/pets/unlocked";
		}
	}

	public static string SRVCMD_FEED_PET
	{
		get
		{
			return SERVICE_CALL_BASE_PATH + "api/pets/feed";
		}
	}

	public static string SRVCMD_PLAY_PET
	{
		get
		{
			return SERVICE_CALL_BASE_PATH + "api/pets/play";
		}
	}

	public static string SRVCMD_HUNT
	{
		get
		{
			return SERVICE_CALL_BASE_PATH + "api/pets/hunt";
		}
	}

	public static string SRVCMD_CLAIM_HUNT_REWARD
	{
		get
		{
			return SERVICE_CALL_BASE_PATH + "api/pets/claim_hunt_rewards";
		}
	}

	public static string SRVCMD_PAY_END_HUNT
	{
		get
		{
			return SERVICE_CALL_BASE_PATH + "api/pets/pay_to_skip_hunt";
		}
	}

	public static string SRVCMD_SYNC_PET_INFO
	{
		get
		{
			return SERVICE_CALL_BASE_PATH + "api/pets/sync_from_client";
		}
	}

	public static string SRVCMD_LEADERBOARD_GLOBAL
	{
		get
		{
			return SERVICE_CALL_BASE_PATH + "api/leaderboard/global";
		}
	}

	public static string SRVCMD_LEADERBOARD_LOCAL
	{
		get
		{
			return SERVICE_CALL_BASE_PATH + "api/leaderboard/local";
		}
	}

	public static string SRVCMD_LEADERBOARD_SAVE_SCORE
	{
		get
		{
			return SERVICE_CALL_BASE_PATH + "api/leaderboard/save_score";
		}
	}

	public static string SRVCMD_DAILY_PROMOTION
	{
		get
		{
			return SERVICE_CALL_BASE_PATH + "api/dungeons/active_promotions";
		}
	}

	public static string SRVCMD_DAILY_BONUS
	{
		get
		{
			return SERVICE_CALL_BASE_PATH + "api/account/daily_login_bonuses";
		}
	}

	public static string SRVCMD_DAILY_CLAIM
	{
		get
		{
			return SERVICE_CALL_BASE_PATH + "api/account/reward_daily_login_bonus";
		}
	}

	public static string SRVCMD_SHOP_ITEMS
	{
		get
		{
			return SERVICE_CALL_BASE_PATH + "api/store/items";
		}
	}

	public static string SRVCMD_SHOP_PURCHASE
	{
		get
		{
			return SERVICE_CALL_BASE_PATH + "api/store/purchase";
		}
	}

	public static string SRVCMD_RETRY_BATTLE
	{
		get
		{
			return SERVICE_CALL_BASE_PATH + "api/dungeon/retry";
		}
	}

	public static string SRVCMD_FUSION_START
	{
		get
		{
			return SERVICE_CALL_BASE_PATH + "api/fusion/start";
		}
	}

	public static string SRVCMD_PLAY_GACHA
	{
		get
		{
			return SERVICE_CALL_BASE_PATH + "api/gacha/play";
		}
	}

	public static string SRVCMD_PLAY_MULTIGACHA
	{
		get
		{
			return SERVICE_CALL_BASE_PATH + "api/gacha/multi_play";
		}
	}

	public static string SRVCMD_GET_ALL_GACHA_EVENTS
	{
		get
		{
			return SERVICE_CALL_BASE_PATH + "api/gacha/all_events";
		}
	}

	public static string SRVCMD_RESET_DATA
	{
		get
		{
			return SERVICE_CALL_BASE_PATH + "api/account/reset_data";
		}
	}

	public static string SRVCMD_ADD_FACEBOOK_FRIEND
	{
		get
		{
			return SERVICE_CALL_BASE_PATH + "api/friend/facebook_multi_add";
		}
	}

	public static string SRVCMD_UPDATE_WEIBO_ID
	{
		get
		{
			return SERVICE_CALL_BASE_PATH + "api/friend/update_weibo_id";
		}
	}

	public static string SRVCMD_ADD_FWEIBO_FRIEND
	{
		get
		{
			return SERVICE_CALL_BASE_PATH + "api/friend/weibo_multi_add";
		}
	}

	public static string SRVCMD_ADD_FRIEND
	{
		get
		{
			return SERVICE_CALL_BASE_PATH + "api/friend/add";
		}
	}

	public static string SRVCMD_REMOVE_FRIEND
	{
		get
		{
			return SERVICE_CALL_BASE_PATH + "api/friend/remove";
		}
	}

	public static string SRVCMD_APPROVE_FRIEND_REQUEST
	{
		get
		{
			return SERVICE_CALL_BASE_PATH + "api/friend/approve";
		}
	}

	public static string SRVCMD_DENY_FRIEND_REQUEST
	{
		get
		{
			return SERVICE_CALL_BASE_PATH + "api/friend/deny";
		}
	}

	public static string SRVCMD_BLOCK_FRIEND
	{
		get
		{
			return SERVICE_CALL_BASE_PATH + "api/friend/block";
		}
	}

	public static string SRVCMD_GET_ALLIES_LIST
	{
		get
		{
			return SERVICE_CALL_BASE_PATH + "api/allies/list";
		}
	}

	public static string SRVCMD_GET_FRIENDS_LIST
	{
		get
		{
			return SERVICE_CALL_BASE_PATH + "api/friends/approved";
		}
	}

	public static string SRVCMD_GET_FRIENDS_PENDING_LIST
	{
		get
		{
			return SERVICE_CALL_BASE_PATH + "api/friends/pending";
		}
	}

	public static string SRVCMD_GET_STRANGERS_LIST
	{
		get
		{
			return SERVICE_CALL_BASE_PATH + "api/friend/strangers";
		}
	}

	public static string SRVCMD_FRIEND_ASSISTS
	{
		get
		{
			return SERVICE_CALL_BASE_PATH + "api/friend/assists";
		}
	}

	public static string SRVCMD_SEND_GACHA_GIFT
	{
		get
		{
			return SERVICE_CALL_BASE_PATH + "api/account/send_free_premium_gacha_gift";
		}
	}

	public static string SRVCMD_GET_GIFT_SENDERS
	{
		get
		{
			return SERVICE_CALL_BASE_PATH + "api/account/get_premium_gacha_ticket_gift_senders";
		}
	}

	public static string SRVCMD_GET_NEW_MESSAGES
	{
		get
		{
			return SERVICE_CALL_BASE_PATH + "api/messaging/new_messages";
		}
	}

	public static string SRVCMD_SEND_MESSAGE
	{
		get
		{
			return SERVICE_CALL_BASE_PATH + "api/messaging/send";
		}
	}

	public static string SRVCMD_DUNGEON_START
	{
		get
		{
			return SERVICE_CALL_BASE_PATH + "api/dungeons/start";
		}
	}

	public static string SRVCMD_DUNGEON_EVENT_START
	{
		get
		{
			return SERVICE_CALL_BASE_PATH + "api/dungeons/start_event_session";
		}
	}

	public static string SRVCMD_DUNGEON_CONTINUE
	{
		get
		{
			return SERVICE_CALL_BASE_PATH + "api/dungeons/continue";
		}
	}

	public static string SRVCMD_DUNGEON_END
	{
		get
		{
			return SERVICE_CALL_BASE_PATH + "api/dungeons/end";
		}
	}

	public static string SRVCMD_UNLOCK_DUNGEON
	{
		get
		{
			return SERVICE_CALL_BASE_PATH + "api/dungeons/unlock";
		}
	}

	public static string SRVCMD_END_BATTLE
	{
		get
		{
			return SERVICE_CALL_BASE_PATH + "api/dungeons/end_battle";
		}
	}

	public static string SRVCMD_UNLOCK_SURVIVAL_GAME_MODE
	{
		get
		{
			return SERVICE_CALL_BASE_PATH + "api/dungeons/unlock_survival_game_mode";
		}
	}

	public static string SRVCMD_GET_UNLOCKED_CITIES
	{
		get
		{
			return SERVICE_CALL_BASE_PATH + "api/unlocked/cities";
		}
	}

	public static string SRVCMD_GET_UNLOCKED_DUNGEONS
	{
		get
		{
			return SERVICE_CALL_BASE_PATH + "api/unlocked/dungeons";
		}
	}

	public static string SRVCMD_GET_UNLOCKED_SURVIVAL_GAME_MODE
	{
		get
		{
			return SERVICE_CALL_BASE_PATH + "api/unlocked/survival_game_modes";
		}
	}

	public static string SRVCMD_CLEAR_FRIENDS
	{
		get
		{
			return SERVICE_CALL_BASE_PATH + "api/cache/clear/friends";
		}
	}

	public static string SRVCMD_CLEAR_PENDING_FRIENDS
	{
		get
		{
			return SERVICE_CALL_BASE_PATH + "api/cache/clear/pending_friends";
		}
	}

	public static string SRVCMD_CLEAR_CHARACTERS
	{
		get
		{
			return SERVICE_CALL_BASE_PATH + "api/cache/clear/characters";
		}
	}

	public static string SRVCMD_CLEAR_PATCHINFO
	{
		get
		{
			return SERVICE_CALL_BASE_PATH + "api/cache/clear/patch_info";
		}
	}

	public static string SRVCMD_EVOLVE
	{
		get
		{
			return SERVICE_CALL_BASE_PATH + "api/fusion/evolve";
		}
	}

	public static string SRVCMD_GAME_SETTING
	{
		get
		{
			return SERVICE_CALL_BASE_PATH + "api/server/game_settings";
		}
	}

	public static string SRVCMD_ALL_ELIGIBLE_EVENTS
	{
		get
		{
			return SERVICE_CALL_BASE_PATH + "api/dungeons/eligible_events";
		}
	}

	public static string SRVCMD_SEND_DEVICE_TOKEN
	{
		get
		{
			return NOTIFY_SERVER_BASE_PATH + "user/set_device_token";
		}
	}

	public static string SRVCMD_ANALYTICS_NEW_INSTALLATION
	{
		get
		{
			return SERVICE_CALL_BASE_PATH + "api/account/log_new_installation";
		}
	}

	public static string SRVCMD_GAME_INITIALIZE
	{
		get
		{
			return SERVICE_CALL_BASE_PATH + "api/game/startup_sequence";
		}
	}

	public static string SRVCMD_PROMO_CODE
	{
		get
		{
			return SERVICE_CALL_BASE_PATH + "api/account/promotion_code";
		}
	}

	public static string SRVCMD_GET_ITEM_COLLECTIONS
	{
		get
		{
			return SERVICE_CALL_BASE_PATH + "api/character/item_collections";
		}
	}

	public static string SRVCMD_GET_DRAGON_SHOP
	{
		get
		{
			return SERVICE_CALL_BASE_PATH + "api/dragon_shops";
		}
	}

	public static string SRVCMD_EXCHANGE_DRAGON_POINTS
	{
		get
		{
			return SERVICE_CALL_BASE_PATH + "api/dragon_shop/exchange";
		}
	}

	public static string SRVCMD_GET_HOLY_SHOP
	{
		get
		{
			return SERVICE_CALL_BASE_PATH + "api/holy_shops";
		}
	}

	public static string SRVCMD_EXCHANGE_HOLY_DIAMONDS
	{
		get
		{
			return SERVICE_CALL_BASE_PATH + "api/holy_shop/exchange";
		}
	}

	public static string SRVCMD_METAPS_CLAIM
	{
		get
		{
			return SERVICE_CALL_BASE_PATH + "api/metaps/claim";
		}
	}

	public static string SRVCMD_DUPLICATE_CHARACTER
	{
		get
		{
			return SERVICE_CALL_BASE_PATH + "api/server/duplicate_character";
		}
	}

	public static Material UI_TEXTURE_MATERIAL
	{
		get
		{
			Material result = null;
			Shader shader = Shader.Find("Unlit/Transparent Colored");
			if (null != shader)
			{
				result = new Material(shader);
			}
			return result;
		}
	}

	public static string FacebookURL
	{
		get
		{
			return "https://graph.facebook.com/";
		}
	}

	public static string TwitterURL
	{
		get
		{
			return "https://api.twitter.com/";
		}
	}

	public static string WeiboURL
	{
		get
		{
			return "http://tp2.sinaimg.cn/";
		}
	}

	public static void Init()
	{
		float num = (float)Screen.width / BASE_WIDTH;
		float num2 = (float)Screen.height / BASE_HEIGHT;
		DELTA_RATIO = num / num2;
		WIDTH_RATIO = (float)Screen.width / BASE_WIDTH;
		ORTHO_RATIO = 360f / ((float)Screen.height * 0.5f);
		float num3 = BASE_WIDTH / BASE_HEIGHT;
		float num4 = (float)Screen.width / (float)Screen.height;
		float num5 = 1f;
		num5 = ((!(num3 < num4)) ? num : num2);
		UI_SCALE_FACTOR = num5 * ORTHO_RATIO;
	}
}
