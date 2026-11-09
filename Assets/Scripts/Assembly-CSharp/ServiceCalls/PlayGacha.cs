using System.Collections;
using Models;
using UnityEngine;

namespace ServiceCalls
{
	public class PlayGacha : Services.ServiceHandle
	{
		public string nubeeId = string.Empty;

		public string deviceId = string.Empty;

		public string gachaId = string.Empty;

		public string requestId = string.Empty;

		private string useTicket = string.Empty;

		public bool m_isTutorial;

		public override IEnumerator Execute()
		{
			string cmd = GLOBAL.SRVCMD_PLAY_GACHA;
			long ts = TimeUtil.GetUnixEpochTime();
			string dataForChecksum = nubeeId + "|" + deviceId + "|" + gachaId + "|" + useTicket + "|1|" + ((!m_isTutorial) ? "0" : "1") + "|" + requestId + "|";
			string checksum = Services.GenerateChecksum(dataForChecksum + ts, Services.Key);
			WWWForm form = new WWWForm();
			form.AddField("nubee_id", nubeeId);
			form.AddField("device_id", deviceId);
			form.AddField("gacha_id", gachaId);
			form.AddField("is_use_ticket", useTicket);
			form.AddField("show_char", "1");
			form.AddField("is_tutorial", (!m_isTutorial) ? "0" : "1");
			form.AddField("request_id", requestId);
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
			JSONObject payload = jsonObj.GetField("payload");
			if (payload == null)
			{
				PerformFailClientError(-8, "Invalid response");
				yield break;
			}
			Models.GachaResult gachaResult = new Models.GachaResult();
			JSONObject jsonCharacter = payload.GetField("character");
			if (jsonCharacter != null)
			{
				gachaResult.characterDetails = new CharacterAccountDetails();
				ServiceCallUtils.SetCharacterFromPayload(jsonCharacter, ref gachaResult.characterDetails);
			}
			JSONObject items = payload["items"];
			if (items != null && items.type != JSONObject.Type.NULL && items.Count > 0 && !items.IsEmptyArray())
			{
				foreach (JSONObject details in items.list)
				{
					ServerInventoryItem item = null;
					ServiceCallUtils.SetItemFromPayload(details, ref item);
					gachaResult.items.Add(item);
				}
			}
			returnValue = gachaResult;
			PerformSuccess();
		}

		public void SetTicket(bool _useTicket)
		{
			if (_useTicket)
			{
				useTicket = "1";
			}
			else
			{
				useTicket = "0";
			}
		}
	}
}
