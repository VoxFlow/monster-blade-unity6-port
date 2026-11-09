using System.Collections;
using UnityEngine;

namespace ServiceCalls
{
	public class UpdateDeviceId : Services.ServiceHandle
	{
		public string nubeeId = string.Empty;

		public string newDeviceId = string.Empty;

		public override bool IsAnalyticsActivity()
		{
			return false;
		}

		public override bool IsKeyRequired()
		{
			return false;
		}

		public override IEnumerator Execute()
		{
			long ts = TimeUtil.GetUnixEpochTime();
			string checksum = Services.GenerateChecksum(nubeeId + "|" + newDeviceId + "|update|" + ts, GLOBAL.SERVER_FIXED_KEY_ACCOUNT_UPDATE_DEVICE_ID);
			string cmd = GLOBAL.SRVCMD_UPDATE_DEVICE_ID;
			WWWForm form = new WWWForm();
			form.AddField("nubee_id", nubeeId);
			form.AddField("new_device_id", newDeviceId);
			form.AddField("type", "update");
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
