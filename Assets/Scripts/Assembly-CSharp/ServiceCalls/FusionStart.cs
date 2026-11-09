using System.Collections;
using System.Collections.Generic;
using Models;
using UnityEngine;

namespace ServiceCalls
{
	public class FusionStart : Services.ServiceHandle
	{
		public string nubeeId = string.Empty;

		public string deviceId = string.Empty;

		public string baseItem = string.Empty;

		public List<string> fuseItems = new List<string>();

		public string requestId = string.Empty;

		public bool IsTutorial;

		public override IEnumerator Execute()
		{
			string cmd = GLOBAL.SRVCMD_FUSION_START;
			WWWForm form = new WWWForm();
			form.AddField("nubee_id", nubeeId);
			form.AddField("device_id", deviceId);
			form.AddField("base_item_db_id", baseItem);
			long ts = TimeUtil.GetUnixEpochTime();
			string dataForChecksum = nubeeId + "|" + deviceId + "|" + baseItem + "|";
			for (int it = 0; it < fuseItems.Count; it++)
			{
				form.AddField("item_db_id" + (it + 1), fuseItems[it]);
				dataForChecksum = dataForChecksum + fuseItems[it] + "|";
			}
			dataForChecksum += "0|1|";
			string text = dataForChecksum;
			dataForChecksum = text + ((!IsTutorial) ? "0" : "1") + "|" + requestId + "|";
			form.AddField("auto_evolve", "0");
			form.AddField("show_char", "1");
			form.AddField("is_tutorial", (!IsTutorial) ? "0" : "1");
			form.AddField("request_id", requestId);
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
			FusionResult fusionResult = new FusionResult();
			JSONObject payload = jsonObj.GetField("payload");
			returnValue = null;
			if (payload != null && payload.type != JSONObject.Type.NULL)
			{
				Dbg.Log("FusionStart payload -->\n" + payload, Dbg.NBLogType.SERVICES);
				ServerInventoryItem item = null;
				if (payload.HasField("new_base_item"))
				{
					JSONObject jsonNewBaseItem = payload["new_base_item"];
					ServiceCallUtils.SetItemFromPayload(jsonNewBaseItem, ref item);
				}
				fusionResult.item = item;
			}
			JSONObject jsonChar = payload.GetField("character");
			if (jsonChar != null)
			{
				fusionResult.characterDetails = new CharacterAccountDetails();
				ServiceCallUtils.SetCharacterFromPayload(jsonChar, ref fusionResult.characterDetails);
			}
			returnValue = fusionResult;
			PerformSuccess();
		}
	}
}
