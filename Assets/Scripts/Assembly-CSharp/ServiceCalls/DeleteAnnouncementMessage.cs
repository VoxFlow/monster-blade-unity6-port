using System.Collections;
using UnityEngine;

namespace ServiceCalls
{
	public class DeleteAnnouncementMessage : Services.ServiceHandle
	{
		public string nubeeId = string.Empty;

		public string deviceId = string.Empty;

		public int announcementDbId;

		public override IEnumerator Execute()
		{
			string cmd = GLOBAL.SRVCMD_DELETE_ANNOUNCEMENT;
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
				if ((result == Services.ConnectionResults.ConnectionFailure || result == Services.ConnectionResults.NoResults) && ShouldRetry())
				{
					Dbg.LogWarn("There was a connection-related problem. Auto-retrying...");
					SingletonComponent<Services>.Instance.StartCoroutine(Execute());
				}
			}
			else
			{
				PerformSuccess();
			}
		}
	}
}
