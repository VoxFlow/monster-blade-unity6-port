public static class MessengerEvent
{
	public const string BATTLE_INIT = "battle_init";

	public const string BATTLE_READY = "battle_ready";

	public const string BATTLE_INTRO = "battle_intro";

	public const string BATTLE_START_UI = "battle_start_ui";

	public const string BATTLE_START = "battle_start";

	public const string BATTLE_END = "battle_end";

	public const string BATTLE_ANNOUNCE = "battle_announce";

	public const string BATTLE_PAUSE = "battle_pause";

	public const string BATTLE_EXIT = "battle_exit";

	public const string BATTLE_CONTINUE = "battle_continue";

	public const string CUTSCENE_START = "cutscene_start";

	public const string CUTSCENE_END = "cutscene_end";

	public const string MAINMENU_START = "mainmenu_start";

	public const string MAINMENU_ENTER_SCREEN = "mainmenu_screen_enter";

	public const string MAINMENU_SHOW_PANEL = "mainmenu_show_panel";

	public const string MAINMENU_HIDE_PANEL = "mainmenu_hide_panel";

	public const string SELECT_ALLIES = "select_allies";

	public const string DUNGEONMENU_LIST_READY = "dungeonmenu_list_ready";

	public const string POPUP_CLOSE = "popup_close";

	public const string BATTLE_UI_BUTTON_PRESSED = "battle_ui_button_pressed";

	public const string DUNGEON_SELECT_EVENT = "dungeon_select_event";

	public const string DUNGEON_SELECT_CANCEL = "dungeon_select_cancel";

	public const string CITY_SELECT_CANCEL = "city_select_cancel";

	public const string DUNGEON_SELECT_FRIEND = "dungeon_select_friend";

	public const string DUNGEON_ENTER = "dungeon_enter";

	public const string DUNGEON_TUTORIAL_TRY_ENTER = "dungeon_tutorial_try_enter";

	public const string DUNGEON_TUTORIAL_TRY_FAIL = "dungeon_tutorial_try_fail";

	public const string HOMEPANEL_UPGRADE_PRESSED = "homepanel_upgrade";

	public const string DUNGEON_CLEARED_START = "dungeon_cleared_start";

	public const string DUNGEON_CLEARED_END = "dungeon_cleared_end";

	public const string ACHIEVEMENT_UPDATE = "achievement_update";

	public const string PROMOCODE_UPDATE_BADGE = "promocode_update_badge";

	public const string TOWN_UPDATE_BADGE = "town_update_badge";

	public const string EXCHANGE_UPDATE_POINTS = "exchange_update_points";

	public const string BATTLEPOINTS_UPDATE_POINTS = "battlepoints_update_points";

	public const string PERFECT_DODGE_WINDOW_START = "perfect_dodge_window_start";

	public const string DODGE_WINDOW_START = "dodge_window_start";

	public const string DODGE_WINDOW_END = "dodge_window_end";

	public const string PERFECT_DODGE_START = "perfect_dodge_start";

	public const string PERFECT_DODGE_END = "perfect_dodge_end";

	public const string PLAYER_GET_HIT = "player_get_hit";

	public const string PLAYER_RECEIVE_DMG = "player_receive_dmg";

	public const string PLAYER_POWER_MOVE = "player_power_move";

	public const string PLAYER_BLOCK = "player_block";

	public const string PLAYER_FINISH_BLOCK = "player_finish_block";

	public const string PLAYER_STRAFE = "player_strafe";

	public const string PLAYER_JUMP = "player_jump";

	public const string PLAYER_LAND = "player_land";

	public const string PLAYER_BEGIN_ATK = "player_begin_atk";

	public const string PLAYER_DAMAGE_WINDOW = "player_damage_window";

	public const string PLAYER_FINISH_ATK = "player_finish_atk";

	public const string PLAYER_REVIVED = "player_revived";

	public const string PLAYER_SUPER_ATK = "player_super_atk";

	public const string PLAYER_SUPER_ATK_READY = "player_super_atk_ready";

	public const string PLAYER_SUPER_ATK_BEGIN = "player_super_atk_begin";

	public const string PLAYER_SUPER_ATK_END = "player_super_atk_end";

	public const string PLAYER_DEAD = "player_dead";

	public const string PLAYER_CONTROLLER_ENABLED = "player_controller_enabled";

	public const string PLAYER_GENERATED = "player_generated";

	public const string MONSTER_BEGIN_ATK = "monster_begin_atk";

	public const string MONSTER_END_ATK = "monster_end_atk";

	public const string MONSTER_DAMAGE_WINDOW = "monster_damage_window";

	public const string MONSTER_ATTACK_WINDOWN = "monster_attack_wind_down";

	public const string MONSTER_GET_HIT = "monster_get_hit";

	public const string MONSTER_RECEIVE_DMG = "monster_receive_dmg";

	public const string MONSTER_STUN = "monster_stun";

	public const string MONSTER_STUN_END = "monster_stun_end";

	public const string BLOCK_NORMAL = "normal_block";

	public const string BLOCK_PERFECT = "perfect_block";

	public const string BLOCK_FAILED = "block_failed";

	public const string PERFECT_BLOCK_END = "perfect_block_end";

	public const string POWER_MOVE = "power_move";

	public const string FUSION_START = "fusion_start";

	public const string FUSION_SHOW_RESULT = "fusion_show_result";

	public const string FUSION_DONE = "fusion_done";

	public const string FUSION_SELECT_BASE = "fusion_select_base";

	public const string FUSION_SELECT_MATERIAL = "fusion_select_material";

	public const string FUSION_BASEITEM_UPDATED = "fusion_baseitem_updated";

	public const string PET_FEED = "pet_feed";

	public const string PET_PLAY = "pet_play";

	public const string PET_HUNT = "pet_hunt";

	public const string PET_HUNT_TIME_SELECTED = "pet_hunt_time_selected";

	public const string GACHA_MENU_SELECTED = "gacha_menu_selected";

	public const string GACHA_PREMIUMTYPE_SELECTED = "gacha_premiumtype_selected";

	public const string GACHA_SHOW_ITEMINFO = "gacha_show_iteminfo";

	public const string GACHA_DONE = "gacha_done";

	public const string CHARACTER_MENU_SELECTED = "character_menu_selected";

	public const string CHARACTER_ITEM_EQUIPPED = "character_item_equipped";

	public const string INVENTORY_ITEM_SELECTED = "inventory_item_selected";

	public const string INVENTORY_READY = "inventory_ready";

	public const string INVENTORY_CLOSED = "inventory_closed";

	public const string INVENTORY_CANCEL = "inventory_cancel";

	public const string INVENTORY_OK = "inventory_ok";

	public const string INVENTORY_UPDATED = "inventory_updated";

	public const string TUTORIAL_COMBAT_STARTED = "tutorial_combat_started";

	public const string TUTORIAL_COMPLETED = "tutorial_completed";

	public const string TUTORIAL_NEXT_ACTION = "tutorial_next_action";

	public const string TUTORIAL_FREE_BATTLE = "tutorial_free_battle";

	public const string TUTORIAL_DUNGEON_END = "tutorial_dungeon_end";

	public const string FRIEND_ASSIST = "friend_assist";

	public const string REWARDS_ITEM_ANIM_COMPLETED = "rewards_item_anim_completed";

	public const string REWARDS_PET_ANIM_STARTED = "rewards_pet_anim_started";

	public const string REWARDS_PET_ANIM_COMPLETED = "rewards_pet_anim_completed";

	public const string PROGRESS_BAR_UPDATE = "progress_bar";
}
