// Offline server: the promotions window with the current event's banners. Register CD-042.
using System;
using System.Collections.Generic;
using System.Text;

public static partial class LocalServer
{
	private class Promo
	{
		public string eventId, type, file;
	}

	private static readonly Promo[] s_promos =
	{
		new Promo { eventId = "GODBEAST_HYDRA", type = "event", file = "51a8830cacf8e_Hydra_Event_Banner_x2_eng.png" },
		new Promo { eventId = "GODBEAST_HYDRA", type = "shop", file = "51a87077bd6a4_Hydra_Nemisis_x2_eng.png" },
		new Promo { eventId = "GODBEAST_NHANG", type = "event", file = "51ae9b24b7869_WingChick_Event_Banner_Eng.png" },
		new Promo { eventId = "GODBEAST_NHANG", type = "shop", file = "51aea7cb92013_Wingchick_Nemisis_Eng.png" },
		new Promo { eventId = "GODBEAST_ARALEZ", type = "event", file = "52171b5935df1_WolfEvent_Repeat_Eng.png" },
		new Promo { eventId = "GODBEAST_ARALEZ", type = "shop", file = "52171bf20ff9b_Wolf_Nemesis_Repeat_En.png" },
		new Promo { eventId = "GODBEAST_ARAMAZ", type = "event", file = "5213059f7663f_Event_Epic_Dragon_En.png" },
		new Promo { eventId = "GODBEAST_ARAMAZ", type = "shop", file = "521306039f39d_Event_Epic_Dragon_Nemesis_En.png" },
	};

	private static string PromotionsJson(OfflineProfile p)
	{
		List<string> list = new List<string>();
		if (!TutorialFinished(p)) return Array(list);
		string current = CurrentGodEvent();
		GodEvent g = FindGodEvent(current);
		long end = g != null ? GodEventEnd(g) : 0;
		DateTime endLocal = new DateTime(1970, 1, 1, 0, 0, 0, DateTimeKind.Utc).AddSeconds(end).ToLocalTime();
		for (int i = 0; i < s_promos.Length; i++)
		{
			Promo pr = s_promos[i];
			if (pr.eventId != current) continue;
			string path = System.IO.Path.Combine(UnityEngine.Application.streamingAssetsPath, "OfflineServer/promos/" + pr.file);
			if (!System.IO.File.Exists(path)) continue;
			string url = new Uri(path).AbsoluteUri, md5;
			using (System.Security.Cryptography.MD5 h = System.Security.Cryptography.MD5.Create())
			{
				md5 = Services.ByteToString(h.ComputeHash(System.IO.File.ReadAllBytes(path)));
			}
			StringBuilder sb = new StringBuilder("{");
			Field(sb, "id", i + 1); Field(sb, "promotion_type", pr.type); Field(sb, "name", g != null ? g.name : pr.file);
			Field(sb, "event_id", pr.type == "event" ? pr.eventId : string.Empty);
			Field(sb, "start_datetime", endLocal.AddDays(-7).ToString("yyyy-MM-dd HH:mm:ss"));
			Field(sb, "end_datetime", endLocal.ToString("yyyy-MM-dd HH:mm:ss"));
			Field(sb, "image_url", url); Field(sb, "image_md5", md5);
			Field(sb, "image_en_url", url); Field(sb, "image_en_md5", md5);
			string body = Close(sb);
			if (pr.type == "event") body = body.TrimEnd('}') + ",\"event_info\":{\"end_time\":" + Str(end) + "}}";
			else body = body.TrimEnd('}') + ",\"items\":[]}";
			list.Add(body);
		}
		return Array(list);
	}
}
