// Offline server: achievements, the daily login bonus and pets. Register CD-037.
using System;
using System.Collections.Generic;
using System.IO;
using System.Text;
using System.Text.RegularExpressions;

public static partial class LocalServer
{
	private class AchievementDef
	{
		public string id;
		public string family;
		public int n;
		public int target;
		public int igc;
		public int diamond;
		public int xp;
		public string pet = string.Empty;
	}

	private static List<AchievementDef> s_achievementDefs;

	private static List<AchievementDef> AchievementDefs()
	{
		if (s_achievementDefs != null) return s_achievementDefs;
		s_achievementDefs = new List<AchievementDef>();
		string path = Path.Combine(UnityEngine.Application.streamingAssetsPath, "OfflineServer/cache/Parameters/binary/client/localization/English.txt");
		if (!File.Exists(path)) return s_achievementDefs;
		Regex key = new Regex(@"^content_achievement_([a-z]+)_(\d+)_description\s*\^\s*(.*)$");
		OfflineTable pets = OfflineTables.Get("Pet");
		List<string> t1 = new List<string>(), t2 = new List<string>();
		foreach (string[] r in pets.Rows)
		{
			if (r[0].EndsWith("_T01")) t1.Add(r[0]);
			else if (r[0].EndsWith("_T02")) t2.Add(r[0]);
		}
		foreach (string line in File.ReadAllLines(path))
		{
			Match m = key.Match(line.Trim());
			if (!m.Success) continue;
			AchievementDef d = new AchievementDef { family = m.Groups[1].Value, n = int.Parse(m.Groups[2].Value) };
			d.id = "achievement_" + d.family + "_" + d.n;
			Match num = Regex.Match(m.Groups[3].Value, @"\d+");
			d.target = num.Success ? int.Parse(num.Value) : 1;
			if (d.id == "achievement_event_2") d.target = 3;
			if (d.id == "achievement_event_4") d.target = 1;
			d.igc = 0;
			d.diamond = d.n;
			int[] named;
			if (s_namedAchievements.TryGetValue(d.id, out named))
			{
				d.igc = 0;
				d.diamond = named[0];
				d.xp = named[1];
			}
			if (d.family == "city")
			{
				List<string> l = d.n <= 10 ? t1 : t2;
				int i = (d.n - 1) % 10;
				if (i < l.Count) d.pet = l[i];
			}
			s_achievementDefs.Add(d);
		}
		return s_achievementDefs;
	}

	private static OfflineAchievement AchievementState(OfflineProfile p, string id)
	{
		OfflineAchievement a = p.achievements.Find(x => x.id == id);
		if (a == null)
		{
			a = new OfflineAchievement { id = id };
			p.achievements.Add(a);
		}
		return a;
	}

	private static string AchievementsJson(OfflineProfile p)
	{
		List<string> list = new List<string>();
		List<AchievementDef> defs = AchievementDefs();
		for (int i = 0; i < defs.Count; i++)
		{
			AchievementDef d = defs[i];
			OfflineAchievement a = AchievementState(p, d.id);
			StringBuilder sb = new StringBuilder("{");
			Field(sb, "id", i + 1); Field(sb, "achievement_id", d.id); Field(sb, "ios_achievement_id", "");
			Field(sb, "achievement_name", "content_achievement_" + d.family + "_" + d.n + "_label");
			Field(sb, "description", "content_achievement_" + d.family + "_" + d.n + "_description");
			Field(sb, "reward_igc", d.igc); Field(sb, "diamond", d.diamond); Field(sb, "tracking", d.target);
			if (d.xp > 0) Field(sb, "reward_xp", d.xp);
			if (d.pet.Length > 0) Field(sb, "pet_id", d.pet);
			Field(sb, "unlocked", a.progress >= d.target ? 1 : 0); Field(sb, "is_marked", a.marked ? 1 : 0);
			Field(sb, "is_claimed", a.claimed ? 1 : 0); Field(sb, "current_progress", a.progress);
			list.Add(Close(sb));
		}
		return Array(list);
	}

	private static string SaveAchievementProgress(Dictionary<string, string> f)
	{
		OfflineProfile p = OfflineProfile.Current;
		List<string> unlocked = new List<string>();
		for (int i = 0; Get(f, "achievement_id" + i) != string.Empty; i++)
		{
			string id = Get(f, "achievement_id" + i);
			AchievementDef d = AchievementDefs().Find(x => x.id == id);
			int progress;
			if (d == null || !int.TryParse(Get(f, "progress" + i), out progress)) continue;
			OfflineAchievement a = AchievementState(p, id);
			bool was = a.progress >= d.target;
			a.progress = Math.Max(a.progress, progress);
			if (!was && a.progress >= d.target) unlocked.Add("{\"achievement_id\":" + Str(id) + ",\"is_unlocked\":\"1\"}");
		}
		p.Save();
		return Ok(Array(unlocked));
	}

	private static string MarkAchievements(Dictionary<string, string> f)
	{
		OfflineProfile p = OfflineProfile.Current;
		for (int i = 0; Get(f, "achievement_id" + i) != string.Empty; i++) AchievementState(p, Get(f, "achievement_id" + i)).marked = true;
		p.Save();
		return Ok("{}");
	}

	private static string ClaimAchievement(Dictionary<string, string> f)
	{
		OfflineProfile p = OfflineProfile.Current;
		AchievementDef d = AchievementDefs().Find(x => x.id == Get(f, "achievement_id"));
		if (d == null) return Fail(404, "offline: unknown achievement");
		OfflineAchievement a = AchievementState(p, d.id);
		if (a.progress < d.target || a.claimed) return Fail(404, "offline: achievement not claimable");
		a.claimed = true;
		p.in_game += d.igc;
		p.premium += d.diamond;
		if (d.xp > 0)
		{
			p.exp += d.xp;
			LevelUp(p, Now());
		}
		if (d.pet.Length > 0 && !p.pets.Contains(d.pet)) p.pets.Add(d.pet);
		if (d.pet.Length > 0 && !p.pets.Contains(p.active_pet)) p.active_pet = d.pet;
		p.Save();
		StringBuilder rewards = new StringBuilder("{");
		Field(rewards, "reward_igc", d.igc); Field(rewards, "diamond", d.diamond);
		if (d.xp > 0) Field(rewards, "reward_xp", d.xp);
		if (d.pet.Length > 0) Field(rewards, "pet_id", d.pet);
		return Ok("{\"rewards\":" + Close(rewards) + ",\"character\":" + CharacterJson(p, Now()) + ",\"currencies\":" + CurrenciesJson(p) + "}");
	}

	private static string CurrenciesJson(OfflineProfile p)
	{
		StringBuilder sb = new StringBuilder("{");
		Field(sb, "social", p.social); Field(sb, "premium", p.premium); Field(sb, "in_game", p.in_game);
		Field(sb, "premium_gacha_ticket", p.premium_gacha_ticket); Field(sb, "premium_gacha_gift_ticket", p.premium_gacha_gift_ticket);
		Field(sb, "social_gacha_ticket", p.social_gacha_ticket); Field(sb, "medal", p.medal);
		Field(sb, "dragon_point", p.dragon_point); Field(sb, "holy_diamond", p.holy_diamond);
		return Close(sb);
	}

	private static readonly int[] s_dailyType = { 2, 2, 2, 2, 2 };

	private static readonly int[] s_dailyValue = { 500, 750, 1000, 1250, 1250 };

	private static string Today()
	{
		return DateTime.Now.ToString("yyyy-MM-dd");
	}

	private static string DailyEntry(int day, bool marked)
	{
		StringBuilder sb = new StringBuilder("{");
		Field(sb, "id", day + 1); Field(sb, "daily_login_bonus_type_id", s_dailyType[day]); Field(sb, "sequence", day + 1);
		Field(sb, "is_active", 1); Field(sb, "value", s_dailyValue[day]); Field(sb, "required_logins", day + 1);
		Field(sb, "required_logins_in_week_flag", 0); Field(sb, "marked", marked ? 1 : 0); Field(sb, "progress", 0);
		return Close(sb);
	}

	private static string DailyBonusList()
	{
		OfflineProfile p = OfflineProfile.Current;
		bool claimedToday = p.daily_last == Today();
		int done = claimedToday ? (p.daily_count - 1) % 5 + 1 : p.daily_count % 5;
		List<string> list = new List<string>();
		for (int i = 0; i < 5; i++) list.Add(DailyEntry(i, i < done));
		return Ok(Array(list));
	}

	private static string DailyBonusClaim()
	{
		OfflineProfile p = OfflineProfile.Current;
		if (p.daily_last == Today())
		{
			return Ok(DailyEntry((p.daily_count - 1) % 5, true));
		}
		int day = p.daily_count % 5;
		switch (s_dailyType[day])
		{
		case 2: p.in_game += s_dailyValue[day]; break;
		case 3: p.social += s_dailyValue[day]; break;
		case 4: p.premium += s_dailyValue[day]; break;
		}
		p.daily_count++;
		p.daily_last = Today();
		p.Save();
		return Ok(DailyEntry(day, true));
	}

	private static string PetsJson(OfflineProfile p)
	{
		if (p.pets.Count > 0 && !p.pets.Contains(p.active_pet))
		{
			p.active_pet = p.pets[0];
			p.Save();
		}
		List<string> list = new List<string>();
		foreach (string id in p.pets) list.Add("{\"pet_id\":" + Str(id) + "}");
		return "{\"unlocked\":" + Array(list) + ",\"active\":{\"pet_id\":" + Str(p.pets.Contains(p.active_pet) ? p.active_pet : "") + "}}";
	}
}
