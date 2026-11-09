using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace ServiceCalls
{
	public class MarkAchievements : Services.ServiceHandle
	{
		public string nubeeId = string.Empty;

		public string deviceId = string.Empty;

		public List<string> ids;

		public override IEnumerator Execute()
		{
			string cmd = GLOBAL.SRVCMD_MARK_ACHIEVEMENTS;
			long ts = TimeUtil.GetUnixEpochTime();
			string dataForChecksum = nubeeId + "|" + deviceId + "|";
			foreach (string id in ids)
			{
				dataForChecksum = dataForChecksum + id + "|";
			}
			string checksum = Services.GenerateChecksum(dataForChecksum + ts, Services.Key);
			WWWForm form = new WWWForm();
			form.AddField("nubee_id", nubeeId);
			form.AddField("device_id", deviceId);
			for (int i = 0; i < ids.Count; i++)
			{
				form.AddField("achievement_id" + i, ids[i]);
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
			}
			else
			{
				PerformSuccess();
			}
		}
	}
}
