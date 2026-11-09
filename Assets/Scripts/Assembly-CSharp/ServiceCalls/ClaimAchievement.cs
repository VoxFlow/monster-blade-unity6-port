using System.Collections;
using UnityEngine;

namespace ServiceCalls
{
	public class ClaimAchievement : Services.ServiceHandle
	{
		public string nubeeId = string.Empty;

		public string deviceId = string.Empty;

		public string achievement_id = string.Empty;

		public override IEnumerator Execute()
		{
			string cmd = GLOBAL.SRVCMD_CLAIM_ACHIEVEMENTS;
			long ts = TimeUtil.GetUnixEpochTime();
			string dataForChecksum = nubeeId + "|" + deviceId + "|" + achievement_id + "|";
			string checksum = Services.GenerateChecksum(dataForChecksum + ts, Services.Key);
			WWWForm form = new WWWForm();
			form.AddField("nubee_id", nubeeId);
			form.AddField("device_id", deviceId);
			form.AddField("achievement_id", achievement_id);
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
				yield break;
			}
			if (jsonObj.HasField("payload"))
			{
				JSONObject payload = jsonObj["payload"];
				AchievementManager.AchievementRewards tempRewards = new AchievementManager.AchievementRewards();
				if (payload.HasField("rewards"))
				{
					JSONObject rewardPayload = payload["rewards"];
					tempRewards.achievementId = achievement_id;
					if (rewardPayload.HasField("reward_xp"))
					{
						tempRewards.rewardXP = int.Parse(rewardPayload["reward_xp"].str);
					}
					if (rewardPayload.HasField("reward_igc"))
					{
						tempRewards.rewardIgc = int.Parse(rewardPayload["reward_igc"].str);
					}
					if (rewardPayload.HasField("item_id"))
					{
						string tempItemStr = rewardPayload["item_id"].str;
						if (tempItemStr.Contains(","))
						{
							string[] tempItemStrs = tempItemStr.Split(',');
							string[] array = tempItemStrs;
							foreach (string temp in array)
							{
								tempRewards.rewardItemIds.Add(temp);
							}
						}
						else if (!string.IsNullOrEmpty(tempItemStr))
						{
							tempRewards.rewardItemIds.Add(tempItemStr);
						}
					}
					if (rewardPayload.HasField("pet_id"))
					{
						string tempPetStr = rewardPayload["pet_id"].str;
						if (tempPetStr.Contains(","))
						{
							string[] tempPetStrs = tempPetStr.Split(',');
							string[] array2 = tempPetStrs;
							foreach (string temp2 in array2)
							{
								tempRewards.rewardPetIds.Add(temp2);
							}
						}
						else if (!string.IsNullOrEmpty(tempPetStr))
						{
							tempRewards.rewardPetIds.Add(tempPetStr);
						}
					}
					if (rewardPayload.HasField("diamond"))
					{
						tempRewards.rewardPrem = int.Parse(rewardPayload["diamond"].str);
					}
				}
				if (payload.HasField("character"))
				{
					JSONObject jsonChar = payload["character"];
					if (jsonChar != null)
					{
						tempRewards.currentPlayerLevel = ServiceCallUtils.GetField<int>(jsonChar, "level");
						tempRewards.currentPlayerXP = ServiceCallUtils.GetField<int>(jsonChar, "exp");
						ServiceCallUtils.SetCharacterFromPayload(jsonChar, ref tempRewards.characterData);
					}
				}
				if (payload.HasField("currencies"))
				{
					JSONObject jsonCurrencies = payload["currencies"];
					if (jsonCurrencies != null)
					{
						ServiceCallUtils.SetCurrenciesFromPayload(jsonCurrencies, ref tempRewards.characterData.currencies);
					}
				}
				Dbg.Log("[Info] The Achievement reward: " + tempRewards.ToString());
				returnValue = tempRewards;
			}
			PerformSuccess();
		}
	}
}
