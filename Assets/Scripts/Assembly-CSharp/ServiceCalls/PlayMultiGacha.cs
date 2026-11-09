using System.Collections;
using Models;
using UnityEngine;

namespace ServiceCalls
{
	public class PlayMultiGacha : Services.ServiceHandle
	{
		public string nubeeId = string.Empty;

		public string deviceId = string.Empty;

		public string gachaId = string.Empty;

		public string requestId = string.Empty;

		public override IEnumerator Execute()
		{
			string cmd = GLOBAL.SRVCMD_PLAY_MULTIGACHA;
			long ts = TimeUtil.GetUnixEpochTime();
			string dataForChecksum = nubeeId + "|" + deviceId + "|" + gachaId + "|1|" + requestId + "|";
			string checksum = Services.GenerateChecksum(dataForChecksum + ts, Services.Key);
			WWWForm form = new WWWForm();
			form.AddField("nubee_id", nubeeId);
			form.AddField("device_id", deviceId);
			form.AddField("gacha_id", gachaId);
			form.AddField("show_char", "1");
			form.AddField("request_id", requestId);
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
			JSONObject payload = jsonObj.GetField("payload");
			if (payload == null)
			{
				Dbg.LogError("payload not found", Dbg.NBLogType.SERVICES);
				PerformFailClientError(-8, "Invalid response");
				yield break;
			}
			Models.GachaResult gachaResult = new Models.GachaResult();
			JSONObject jsonCharacter = payload.GetField("character");
			if (jsonCharacter != null)
			{
				gachaResult.characterDetails = new CharacterAccountDetails();
				ServiceCallUtils.SetCharacterFromPayload(jsonCharacter, ref gachaResult.characterDetails);
			}
			JSONObject items = payload.GetField("items");
			if (items == null || items.type == JSONObject.Type.NULL || items.Count <= 0)
			{
				Dbg.LogError("items payload not found", Dbg.NBLogType.SERVICES);
				PerformFailClientError(-8, "Invalid response");
				yield break;
			}
			if (!items.IsEmptyArray())
			{
				foreach (JSONObject details in items.list)
				{
					ServerInventoryItem item = null;
					ServiceCallUtils.SetItemFromPayload(details, ref item);
					gachaResult.items.Add(item);
				}
			}
			returnValue = gachaResult;
			PerformSuccess();
		}
	}
}
