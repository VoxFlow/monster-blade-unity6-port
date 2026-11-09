using System.Collections;
using System.Collections.Generic;
using Models;
using UnityEngine;

namespace ServiceCalls
{
	public class MultiSell : Services.ServiceHandle
	{
		public string nubeeId = string.Empty;

		public string deviceId = string.Empty;

		public List<string> itemsId = new List<string>();

		public string requestId = string.Empty;

		public override IEnumerator Execute()
		{
			string cmd = GLOBAL.SRVCMD_SELL_ITEM_LIST;
			WWWForm form = new WWWForm();
			form.AddField("nubee_id", nubeeId);
			form.AddField("device_id", deviceId);
			form.AddField("show_char", "1");
			long ts = TimeUtil.GetUnixEpochTime();
			string dataForChecksum = nubeeId + "|" + deviceId + "|1|";
			for (int it = 0; it < itemsId.Count; it++)
			{
				form.AddField("item_db_id" + it, itemsId[it]);
				dataForChecksum = dataForChecksum + itemsId[it] + "|";
			}
			form.AddField("request_id", requestId);
			dataForChecksum = dataForChecksum + requestId + "|";
			string checksum = Services.GenerateChecksum(dataForChecksum + ts, Services.Key);
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
			MultipleSell details = new MultipleSell();
			JSONObject payload = jsonObj["payload"];
			if (payload != null && payload.type != JSONObject.Type.NULL)
			{
				JSONObject sellResult = payload.GetField("sell_results");
				if (sellResult != null && sellResult.type != JSONObject.Type.NULL)
				{
					for (int i = 0; i < sellResult.Count; i++)
					{
						int returnCode = int.Parse(sellResult["item_db_id" + i].str);
						details.returnCode.Add(returnCode);
					}
				}
				JSONObject jsonCharacter = payload.GetField("character");
				if (jsonCharacter != null)
				{
					details.characterDetails = new CharacterAccountDetails();
					ServiceCallUtils.SetCharacterFromPayload(jsonCharacter, ref details.characterDetails);
				}
			}
			returnValue = details;
			PerformSuccess();
		}
	}
}
