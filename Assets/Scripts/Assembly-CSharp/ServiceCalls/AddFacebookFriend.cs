using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace ServiceCalls
{
	public class AddFacebookFriend : Services.ServiceHandle
	{
		public List<string> fbids;

		public string nubeeId = string.Empty;

		public string deviceId = string.Empty;

		public override IEnumerator Execute()
		{
			string cmd = GLOBAL.SRVCMD_ADD_FACEBOOK_FRIEND;
			if (fbids == null || fbids.Count <= 0)
			{
				Dbg.LogError("empty fbids");
				PerformFailClientError(-10, "Invalid Parameter");
				yield break;
			}
			long ts = TimeUtil.GetUnixEpochTime();
			string szDataForChecksum = nubeeId + "|" + deviceId + "|";
			WWWForm form = new WWWForm();
			form.AddField("nubee_id", nubeeId);
			form.AddField("device_id", deviceId);
			for (int i = 0; fbids.Count > i; i++)
			{
				form.AddField("facebook_friend_id" + i, fbids[i]);
				szDataForChecksum = szDataForChecksum + fbids[i] + "|";
			}
			form.AddField("ts", ts.ToString());
			string checksum = Services.GenerateChecksum(szDataForChecksum + ts, Services.Key);
			form.AddField("checksum", checksum);
			Dbg.Log("checksum : " + szDataForChecksum + ts);
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
				returnValue = null;
				PerformSuccess();
			}
		}
	}
}
