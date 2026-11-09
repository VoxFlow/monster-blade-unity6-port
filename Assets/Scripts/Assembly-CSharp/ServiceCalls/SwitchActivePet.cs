using System.Collections;
using UnityEngine;

namespace ServiceCalls
{
	public class SwitchActivePet : Services.ServiceHandle
	{
		public string m_nubeeId = string.Empty;

		public string m_deviceId = string.Empty;

		public string m_petId = string.Empty;

		public override IEnumerator Execute()
		{
			string cmdStr = GLOBAL.SRVCMD_SWITCH_ACTIVE_PET;
			long currentTime = TimeUtil.GetUnixEpochTime();
			string checksum = Services.GenerateChecksum(m_nubeeId + "|" + m_deviceId + "|" + m_petId + "|" + currentTime, Services.Key);
			WWWForm form = new WWWForm();
			form.AddField("nubee_id", m_nubeeId);
			form.AddField("device_id", m_deviceId);
			form.AddField("pet_id", m_petId);
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
				JSONObject payload = jsonObj["payload"];
				JSONObject petID = payload.GetField("pet_id");
				if (petID != null)
				{
					returnValue = petID.str;
				}
				else
				{
					returnValue = string.Empty;
				}
				PerformSuccess();
			}
		}
	}
}
