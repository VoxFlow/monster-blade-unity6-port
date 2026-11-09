using System.Collections;
using UnityEngine;

namespace ServiceCalls
{
	public class CreateOrUpdateNubeeAccount : Services.ServiceHandle
	{
		public string nubeeId = string.Empty;

		public string deviceId = string.Empty;

		public string name = string.Empty;

		public string lang = string.Empty;

		public bool isTutorial;

		public string fbid = string.Empty;

		public string fbtoken = string.Empty;

		public string weiboId = string.Empty;

		public override bool IsKeyRequired()
		{
			return false;
		}

		public override bool IsAnalyticsActivity()
		{
			return false;
		}

		public override IEnumerator Execute()
		{
			long ts = TimeUtil.GetUnixEpochTime();
			string stringForChecksum = nubeeId + "|" + deviceId + "|" + name + "|" + lang + "|" + fbid + "|" + fbtoken + "|" + weiboId + ((!isTutorial) ? "|0|" : "|1|") + ts;
			string serviceKey = GLOBAL.SERVER_FIXED_KEY_ACCOUNT_REGISTER;
			string checksum = Services.GenerateChecksum(stringForChecksum, serviceKey);
			string cmd = GLOBAL.SRVCMD_REGISTER_UPDATE_NUBEE_ACCOUNT;
			WWWForm form = new WWWForm();
			form.AddField("nubee_id", nubeeId);
			form.AddField("device_id", deviceId);
			form.AddField("name", name);
			form.AddField("lang", lang);
			form.AddField("facebook_id", fbid);
			form.AddField("facebook_token", fbtoken);
			form.AddField("weibo_id", weiboId);
			form.AddField("is_tutorial", (!isTutorial) ? "0" : "1");
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
