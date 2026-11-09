using System.Collections;
using UnityEngine;

namespace ServiceCalls
{
	public class SaveScore : Services.ServiceHandle
	{
		public string m_nubeeId = string.Empty;

		public string m_deviceId = string.Empty;

		public int m_score;

		public override IEnumerator Execute()
		{
			string cmdStr = GLOBAL.SRVCMD_LEADERBOARD_SAVE_SCORE;
			long currentTime = TimeUtil.GetUnixEpochTime();
			string checksum = Services.GenerateChecksum(m_nubeeId + "|" + m_deviceId + "|" + m_score.ToString() + "|" + currentTime, Services.Key);
			Dbg.Log("generated checksum = " + checksum, Dbg.NBLogType.SERVICES);
			WWWForm wwwForm = new WWWForm();
			wwwForm.AddField("nubee_id", m_nubeeId);
			wwwForm.AddField("device_id", m_deviceId);
			wwwForm.AddField("new_score", m_score);
			wwwForm.AddField("ts", currentTime.ToString());
			wwwForm.AddField("checksum", checksum);
			result = Services.ConnectionResults.Busy;
			yield return SingletonComponent<JobManager>.Instance.StartCoroutine(WaitForQuery(cmdStr, wwwForm));
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
			if (UrlConnection.isDone && UrlConnection.text != null)
			{
				Dbg.Log("[Info, Debug] SaveScore raw data: " + UrlConnection.text, Dbg.NBLogType.SERVICES);
			}
			if ("OK" == jsonObj["message"].str)
			{
				returnValue = null;
				PerformSuccess();
			}
		}
	}
}
