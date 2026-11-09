using System.Collections;
using Models;
using UnityEngine;

namespace ServiceCalls
{
	public class RetryBattle : Services.ServiceHandle
	{
		public string nubeeId = string.Empty;

		public string deviceId = string.Empty;

		public string dungeonKey = string.Empty;

		public int monsterIndex;

		public int sns_reward;

		public string requestId = string.Empty;

		public override IEnumerator Execute()
		{
			string cmd = GLOBAL.SRVCMD_RETRY_BATTLE;
			WWWForm form = new WWWForm();
			form.AddField("nubee_id", nubeeId);
			form.AddField("device_id", deviceId);
			form.AddField("dungeon_key", dungeonKey);
			form.AddField("monster_index", monsterIndex);
			form.AddField("request_id", requestId);
			form.AddField("sns_reward", sns_reward);
			long ts = TimeUtil.GetUnixEpochTime();
			form.AddField("ts", ts.ToString());
			string checksum = Services.GenerateChecksum(nubeeId + "|" + deviceId + "|" + dungeonKey + "|" + monsterIndex + "|" + requestId + "|" + sns_reward + "|" + ts, Services.Key);
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
			JSONObject jsonPayload = jsonObj.GetField("payload");
			if (jsonPayload == null)
			{
				PerformFailClientError(-9, "Invalid server response");
				yield break;
			}
			CharacterAccountDetails charDetails = new CharacterAccountDetails();
			ServiceCallUtils.SetCharacterFromPayload(jsonPayload, ref charDetails);
			returnValue = charDetails;
			PerformSuccess();
		}
	}
}
