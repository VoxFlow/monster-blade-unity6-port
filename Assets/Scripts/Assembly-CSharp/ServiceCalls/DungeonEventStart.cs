using System.Collections;
using Models;
using UnityEngine;

namespace ServiceCalls
{
	public class DungeonEventStart : Services.ServiceHandle
	{
		public string nubeeId = string.Empty;

		public string deviceId = string.Empty;

		public string dungeonEventId = string.Empty;

		public string sessionRequestID = string.Empty;

		public int difficulty;

		public int battleCount;

		public bool isTutorial;

		public override IEnumerator Execute()
		{
			string cmd = GLOBAL.SRVCMD_DUNGEON_EVENT_START;
			WWWForm form = new WWWForm();
			long ts = TimeUtil.GetUnixEpochTime();
			string is_tutorial = ((!isTutorial) ? "0" : "1");
			string dataForChecksum = nubeeId + "|" + deviceId + "|" + dungeonEventId + "|" + difficulty + "|" + is_tutorial + "|" + sessionRequestID + "|";
			form.AddField("nubee_id", nubeeId);
			form.AddField("device_id", deviceId);
			form.AddField("event_id", dungeonEventId);
			form.AddField("difficulty", difficulty);
			form.AddField("is_tutorial", is_tutorial);
			form.AddField("request_id", sessionRequestID);
			form.AddField("ts", ts.ToString());
			string checksum = Services.GenerateChecksum(dataForChecksum + ts, Services.Key);
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
			ServerBattleDetails battleDetail = new ServerBattleDetails();
			result = ServiceCallUtils.GetStartDungeonFromPayload(payload, ref battleDetail);
			returnValue = battleDetail;
			if (result == Services.ConnectionResults.WrongPayload)
			{
				PerformFail("Could not parse the payload");
			}
			else
			{
				PerformSuccess();
			}
		}
	}
}
