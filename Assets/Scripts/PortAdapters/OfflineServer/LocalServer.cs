// Offline server: answers the game's service calls in-process with the original response formats. Register CD-005.
using System;
using System.Collections.Generic;
using System.Text;
using UnityEngine;

public static partial class LocalServer
{
	private static readonly HashSet<string> s_loggedUnmapped = new HashSet<string>();

	public static string LastPath;

	public static string LastReply;

	public static bool LastUnmapped;

	public static int UnmappedCount
	{
		get { return s_loggedUnmapped.Count; }
	}

	public static string[] UnmappedPaths()
	{
		var a = new string[s_loggedUnmapped.Count];
		s_loggedUnmapped.CopyTo(a);
		return a;
	}

	public static bool WasUnmapped(string path)
	{
		return path != null && s_loggedUnmapped.Contains(path);
	}

	public static string Handle(string url, WWWForm form)
	{
		LastUnmapped = false;
		string reply = HandleCore(url, form);
		LastPath = EndpointPath(url);
		LastReply = reply;
		return reply;
	}

	private static string HandleCore(string url, WWWForm form)
	{
		string path = EndpointPath(url);
		Dictionary<string, string> f = ParseForm(form);
		try
		{
			switch (path)
			{
			case "check":
				return StartupCheck();
			case "api/security/key/":
				return SecretKey();
			case "api/account/update_device_id":
				return Ok("{}");
			case "api/account/create_update_user":
				TutorialFlag(f, "name_input");
				return Ok("{}");
			case "api/character/update_equipments":
				TutorialFlag(f, "equip_item");
				SaveEquipped(f);
				return Ok("{}");
			case "api/game/startup_sequence":
				MBStartupFade.StartupAnswered();
				return GameInitialization();
			case "api/dungeons/start_event_session":
				return StartEvent(f);
			case "api/dungeons/start":
				return StartDungeon(f);
			case "api/dungeons/end":
				return EndDungeon(f);
			case "api/gacha/all_events":
				return AllGachaEvents();
			case "api/gacha/play":
				return PlayGacha(f);
			case "api/fusion/start":
				return FusionStart(f);
			case "api/gacha/multi_play":
				return PlayGachaOffline(Get(f, "gacha_id"), true, false);
			case "api/friends/approved":
			case "api/friends/pending":
			case "api/friend/strangers":
				return Ok("[]");
			case "api/allies/list":
				return Ok(AlliesJson(OfflineProfile.Current));
			case "api/unlocked/cities":
				return Ok(UnlockedCitiesJson());
			case "api/store/items":
				return Ok(StoreItemsJson());
			case "api/pets/switch_active":
				OfflineProfile.Current.active_pet = Get(f, "pet_id");
				OfflineProfile.Current.Save();
				return Ok("{\"pet_id\":" + Str(Get(f, "pet_id")) + "}");
			case "api/achievements/all":
				return Ok(AchievementsJson(OfflineProfile.Current));
			case "api/server/get_announcements":
				return Ok("[]");
			case "api/achievements/mark":
				return MarkAchievements(f);
			case "api/achievements/claim":
				return ClaimAchievement(f);
			case "api/account/reward_daily_login_bonus":
				return DailyBonusClaim();
			case "api/account/save_session_timestamp":
			case "api/leaderboard/save_score":
				return Ok("{}");
			case "api/achievements/save_progress":
				return SaveAchievementProgress(f);
			case "api/unlocked/dungeons":
				return Ok(UnlockedDungeonsJson(url.Substring(url.TrimEnd('/').LastIndexOf('/') + 1).Split('?')[0]));
			case "api/dungeons/eligible_events":
				return Ok(EventsJson(OfflineProfile.Current));
			case "api/leaderboard/global":
			case "api/leaderboard/local":
				return Ok("{\"rankings\":[]}");
			case "api/dragon_shops":
				return Ok("{\"shops\":[],\"expiry\":\"0\"}");
			case "api/holy_shops":
				return Ok("[]");
			case "api/account/daily_login_bonuses":
				return DailyBonusList();
			case "api/dungeons/active_promotions":
				return Ok(PromotionsJson(OfflineProfile.Current));
			case "api/account/get_premium_gacha_ticket_gift_senders":
				return Ok("[]");
			case "api/character/item/multi_sell":
				return MultiSell(f);
			case "api/fusion/evolve":
				return Evolve(f);
			case "api/dungeon/retry":
				return Retry(f);
			case "api/store/purchase":
				return Purchase(f);
			}
		}
		catch (Exception e)
		{
			Debug.LogError("[OfflineServer] handler for " + path + " threw: " + e);
		}
		if (path.EndsWith("set_device_token"))
		{
			return "[{\"code\":200,\"message\":\"OK\"}]";
		}
		LastUnmapped = true;
		if (s_loggedUnmapped.Add(path))
		{
			Debug.LogWarning("[OfflineServer] UNMAPPED " + path + " form=" + string.Join(",", new List<string>(f.Keys).ToArray()));
		}
		return Ok("{}");
	}

	private static string StartupCheck()
	{
		return "{\"code\":200,\"ts\":" + Str(Now()) + ",\"minimum_version\":" + Str(GLOBAL.CLIENT_VERSION) + ",\"maintenance\":\"0\"}";
	}

	private static string SecretKey()
	{
		long now = Now();
		const long remaining = 86400;
		return Ok("{\"secret_key\":\"offline\",\"remaining\":" + Str(remaining) + ",\"date_created\":" + Str(now) + ",\"date_expiry\":" + Str(now + remaining) + "}");
	}

	private static string GameInitialization()
	{
		StringBuilder files = new StringBuilder();
		foreach (OfflinePatchSeed.Entry e in OfflinePatchSeed.SeedAndList())
		{
			if (files.Length > 0) files.Append(',');
			files.Append("{\"asset_filename\":").Append(Str(e.filename))
				.Append(",\"asset_size\":").Append(Str(e.size))
				.Append(",\"asset_crc\":").Append(Str(e.crc))
				.Append(",\"asset_version\":").Append(Str(e.version))
				.Append(",\"asset_url\":").Append(Str(e.url)).Append('}');
		}
		return Ok("{\"game_settings\":" + LocalServerGameSettings.Json()
			+ ",\"patch_info\":{\"server_version\":" + Str(GLOBAL.PATCH_VERSION) + ",\"files\":[" + files + "]}"
			+ GameInitSections() + "}");
	}

	private static string Ok(string payloadJson)
	{
		return "{\"code\":200,\"message\":\"OK\",\"ts\":" + Str(Now()) + ",\"payload\":" + payloadJson + "}";
	}

	public static long Now()
	{
		long wall = (long)(DateTime.UtcNow - new DateTime(1970, 1, 1, 0, 0, 0, DateTimeKind.Utc)).TotalSeconds;
		return wall;
	}

	public static string Str(object v)
	{
		StringBuilder sb = new StringBuilder("\"");
		foreach (char c in Convert.ToString(v, System.Globalization.CultureInfo.InvariantCulture))
		{
			switch (c)
			{
			case '"': sb.Append("\\\""); break;
			case '\\': sb.Append("\\\\"); break;
			case '\n': sb.Append("\\n"); break;
			case '\r': sb.Append("\\r"); break;
			case '\t': sb.Append("\\t"); break;
			default:
				if (c < ' ') sb.AppendFormat("\\u{0:x4}", (int)c); else sb.Append(c);
				break;
			}
		}
		return sb.Append('"').ToString();
	}

	public static string EndpointPath(string url)
	{
		string u = url;
		int q = u.IndexOf('?');
		if (q >= 0) u = u.Substring(0, q);
		if (u.TrimEnd('/') == GLOBAL.SERVER_CHECK.TrimEnd('/')) return "check";
		string basePath = GLOBAL.SERVICE_CALL_BASE_PATH;
		if (u.StartsWith(basePath)) u = u.Substring(basePath.Length);
		foreach (string known in KnownPathsWithIdSuffix)
		{
			if (u.StartsWith(known)) return known;
		}
		return u;
	}

	private static readonly string[] KnownPathsWithIdSuffix = new string[]
	{
		"api/security/key/", "api/character/inventory/", "api/character/find/", "api/server/patch_info/",
		"api/account/is_registered/", "api/achievements/all", "api/server/get_announcements",
		"api/allies/list", "api/friends/approved", "api/friends/pending", "api/friend/strangers", "api/unlocked/cities",
		"api/unlocked/dungeons", "api/dungeons/eligible_events", "api/leaderboard/global", "api/leaderboard/local",
		"api/dungeons/active_promotions", "api/account/daily_login_bonuses", "api/account/get_premium_gacha_ticket_gift_senders"
	};

	private static Dictionary<string, string> ParseForm(WWWForm form)
	{
		Dictionary<string, string> d = new Dictionary<string, string>();
		if (form == null) return d;
		string body = Encoding.UTF8.GetString(form.data);
		foreach (string pair in body.Split('&'))
		{
			if (pair.Length == 0) continue;
			int eq = pair.IndexOf('=');
			string k = eq < 0 ? pair : pair.Substring(0, eq);
			string v = eq < 0 ? string.Empty : pair.Substring(eq + 1);
			d[Uri.UnescapeDataString(k.Replace('+', ' '))] = Uri.UnescapeDataString(v.Replace('+', ' '));
		}
		return d;
	}
}
