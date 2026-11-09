using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace ServiceCalls
{
	public class UpdateAchievementProgress : Services.ServiceHandle
	{
		public string nubeeId = string.Empty;

		public string deviceId = string.Empty;

		public Dictionary<string, int> achieveIdProgressMap = new Dictionary<string, int>();

		public override IEnumerator Execute()
		{
			Dbg.Log("[Info] UpdateAchievementProgress");
			string cmd = GLOBAL.SRVCMD_UPDATE_PROGRESS;
			long ts = TimeUtil.GetUnixEpochTime();
			string dataForChecksum = nubeeId + "|" + deviceId + "|";
			foreach (string tempKey in achieveIdProgressMap.Keys)
			{
				string text = dataForChecksum;
				dataForChecksum = text + tempKey + "|" + achieveIdProgressMap[tempKey] + "|";
			}
			string checksum = Services.GenerateChecksum(dataForChecksum + ts, Services.Key);
			WWWForm form = new WWWForm();
			form.AddField("nubee_id", nubeeId);
			form.AddField("device_id", deviceId);
			int i = 0;
			foreach (string tempKey2 in achieveIdProgressMap.Keys)
			{
				form.AddField("achievement_id" + i, tempKey2);
				form.AddField("progress" + i, achieveIdProgressMap[tempKey2].ToString());
				i++;
			}
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
			List<string> achievedIdList = new List<string>();
			achievedIdList.Clear();
			if (payload != null && payload.type != JSONObject.Type.NULL && !payload.IsEmptyArray() && payload.Count > 0)
			{
				foreach (JSONObject tempObj in payload.list)
				{
					if (tempObj.HasField("is_unlocked") && tempObj.GetField("is_unlocked").str == "1" && tempObj.HasField("achievement_id"))
					{
						string tempId = tempObj["achievement_id"].str;
						achievedIdList.Add(tempId);
						Dbg.Log("Achievement: " + tempId + " unlocked");
					}
				}
			}
			returnValue = achievedIdList;
			PerformSuccess();
		}
	}
}
