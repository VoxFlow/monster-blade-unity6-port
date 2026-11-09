// Generated: the game settings the client receives at start-up (shipped defaults and labelled substitutes). Register CD-008.
using System.Globalization;
using System.Text;

public static class LocalServerGameSettings
{
	public const int StaminaResetSeconds = 480;
	public const int ShieldsResetSeconds = 7200;

	public static string Json()
	{
		Models.GameSettings d = new Models.GameSettings();
		StringBuilder sb = new StringBuilder("{");
		Add(sb, "core_game_setting_stamina_reset_time", "480");
		Add(sb, "core_game_setting_shields_reset_time", "7200");
		Add(sb, "core_game_setting_store_bag_slot_increase_amount", "5");
		Add(sb, "core_game_setting_store_bag_slot_limitation", d.slot_limitation);
		Add(sb, "core_game_setting_per_social_points_reward_ticket", d.social_points_per_ticket);
		Add(sb, "core_game_setting_reward_lb_global_1st_premium", d.global_lb_top1_reward_pgc);
		Add(sb, "core_game_setting_reward_lb_global_top_10_premium", d.global_lb_top10_reward_pgc);
		Add(sb, "core_game_setting_reward_lb_global_top_100_premium", d.global_lb_top100_reward_pgc);
		Add(sb, "core_game_setting_reward_lb_global_top_1000_social", d.global_lb_top1000_reward_social);
		Add(sb, "core_game_setting_combat_power_move_score_multiplier", d.power_move_score_multiplier);
		Add(sb, "core_game_setting_combat_medal_criteria_wood", d.medal_criteria_wood);
		Add(sb, "core_game_setting_combat_medal_criteria_bronze", d.medal_criteria_bronze);
		Add(sb, "core_game_setting_combat_medal_criteria_silver", d.medal_criteria_silver);
		Add(sb, "core_game_setting_combat_medal_criteria_gold", d.medal_criteria_gold);
		Add(sb, "core_game_setting_combat_expected_specials_ratio", d.expected_specials_ratio);
		Add(sb, "core_game_setting_combat_multiplier_specials", d.multiplier_specials);
		Add(sb, "core_game_setting_combat_death_retry_cost", d.death_retry_cost);
		Add(sb, "core_game_setting_event_easy_cost", d.event_easy_cost);
		Add(sb, "core_game_setting_event_normal_cost", d.event_normal_cost);
		Add(sb, "core_game_setting_event_hard_cost", d.event_hard_cost);
		Add(sb, "core_game_setting_enable_local_push_notification", (d.enable_local_push_notification ? 1 : 0));
		return sb.Append('}').ToString();
	}

	private static void Add(StringBuilder sb, string key, object value)
	{
		if (sb.Length > 1) sb.Append(',');
		sb.Append('"').Append(key).Append("\":").Append(LocalServer.Str(System.Convert.ToString(value, CultureInfo.InvariantCulture)));
	}
}
