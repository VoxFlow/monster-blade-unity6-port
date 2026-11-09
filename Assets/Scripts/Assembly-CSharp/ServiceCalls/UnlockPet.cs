using System.Collections;
using UnityEngine;

namespace ServiceCalls
{
	public class UnlockPet : Services.ServiceHandle
	{
		public string m_nubeeId = string.Empty;

		public string m_deviceId = string.Empty;

		public string m_petId = string.Empty;

		public override IEnumerator Execute()
		{
			Dbg.Log("[Info] UnlockPet::Execute()");
			string cmdStr = GLOBAL.SRVCMD_UNLOCK_PET;
			long currentTime = TimeUtil.GetUnixEpochTime();
			string checksum = Services.GenerateChecksum(m_nubeeId + "|" + m_deviceId + "|" + m_petId + "|1|" + currentTime, Services.Key);
			WWWForm form = new WWWForm();
			form.AddField("nubee_id", m_nubeeId);
			form.AddField("device_id", m_deviceId);
			form.AddField("pet_id", m_petId);
			form.AddField("is_make_active", "1");
			form.AddField("ts", currentTime.ToString());
			form.AddField("checksum", checksum);
			result = Services.ConnectionResults.Busy;
			yield return SingletonComponent<JobManager>.Instance.StartCoroutine(WaitForQuery(cmdStr, form));
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
			else if ("OK" == jsonObj["message"].str)
			{
				returnValue = null;
				PerformSuccess();
			}
		}
	}
}
