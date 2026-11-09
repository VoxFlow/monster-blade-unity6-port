using System.Collections;
using Models;
using UnityEngine;

namespace ServiceCalls
{
	public class ClaimAnnouncement : Services.ServiceHandle
	{
		public string nubeeId = string.Empty;

		public string deviceId = string.Empty;

		public int announcementDbId;

		public override IEnumerator Execute()
		{
			string cmd = GLOBAL.SRVCMD_CLAIM_ANNOUNCEMENT;
			long ts = TimeUtil.GetUnixEpochTime();
			string dataForChecksum = nubeeId + "|" + deviceId + "|" + announcementDbId + "|";
			string checksum = Services.GenerateChecksum(dataForChecksum + ts, Services.Key);
			WWWForm form = new WWWForm();
			form.AddField("nubee_id", nubeeId);
			form.AddField("device_id", deviceId);
			form.AddField("announcement_id", announcementDbId.ToString());
			form.AddField("ts", ts.ToString());
			form.AddField("checksum", checksum);
			result = Services.ConnectionResults.Busy;
			yield return SingletonComponent<JobManager>.Instance.StartCoroutine(WaitForQuery(cmd, form));
			JSONObject jsonObj = null;
			if (AnalyzeResultsAndBreak(out jsonObj))
			{
				if (IsNoInternet())
				{
					if (ShouldRetry())
					{
						Dbg.LogWarn("There was a connection-related problem. Auto-retrying...");
						SingletonComponent<Services>.Instance.StartCoroutine(Execute());
					}
					else
					{
						PerformFailNoInternet();
					}
				}
				else
				{
					PerformFail(result.ToString());
				}
				yield break;
			}
			JSONObject payload = jsonObj["payload"];
			CurrencyDetails reward = new CurrencyDetails();
			if (payload.HasField("user_currencies"))
			{
				JSONObject currencies = payload["user_currencies"];
				if (currencies != null && currencies.type != JSONObject.Type.NULL)
				{
					NBGeneric.TryParse<float>(currencies["social"].str, out reward.social);
					NBGeneric.TryParse<float>(currencies["premium"].str, out reward.premium);
					NBGeneric.TryParse<float>(currencies["in_game"].str, out reward.ingame);
					NBGeneric.TryParse<float>(currencies["premium_gacha_ticket"].str, out reward.freeGacha);
					NBGeneric.TryParse<float>(currencies["premium_gacha_gift_ticket"].str, out reward.giftGacha);
					NBGeneric.TryParse<float>(currencies["social_gacha_ticket"].str, out reward.freeSocialGacha);
					NBGeneric.TryParse<float>(currencies["medal"].str, out reward.medal);
				}
			}
			if (payload.HasField("character"))
			{
			}
			if (payload.HasField("inventory"))
			{
			}
			returnValue = reward;
			PerformSuccess();
		}
	}
}
