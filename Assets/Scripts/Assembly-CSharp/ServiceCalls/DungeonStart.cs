using System.Collections;
using System.Collections.Generic;
using Models;
using UnityEngine;

namespace ServiceCalls
{
	public class DungeonStart : Services.ServiceHandle
	{
		public string nubeeId = string.Empty;

		public string deviceId = string.Empty;

		public string dungeonId = string.Empty;

		public string dungeonType = string.Empty;

		public string sessionRequestID = string.Empty;

		public List<string> monsterIds;

		public override IEnumerator Execute()
		{
			string cmd = GLOBAL.SRVCMD_DUNGEON_START;
			WWWForm form = new WWWForm();
			long ts = TimeUtil.GetUnixEpochTime();
			string dataForChecksum = nubeeId + "|" + deviceId + "|" + dungeonId + "|" + dungeonType + "|";
			form.AddField("nubee_id", nubeeId);
			form.AddField("device_id", deviceId);
			form.AddField("dungeon_id", dungeonId);
			form.AddField("dungeon_type", dungeonType);
			if (monsterIds != null)
			{
				for (int it = 0; it < monsterIds.Count; it++)
				{
					dataForChecksum = dataForChecksum + monsterIds[it] + "|";
					form.AddField("monster_id" + it, monsterIds[it]);
				}
			}
			form.AddField("request_id", sessionRequestID);
			dataForChecksum = dataForChecksum + sessionRequestID + "|";
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
			JSONObject payload = jsonObj["payload"];
			Dbg.Log("DungeonStart : payload ----> " + payload, Dbg.NBLogType.SERVICES);
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
