using System.Collections;
using Models;

namespace ServiceCalls
{
	public class GetAnnouncements : Services.ServiceHandle
	{
		public string nubeeId = string.Empty;

		public string deviceId = string.Empty;

		public override IEnumerator Execute()
		{
			string cmd = GLOBAL.SRVCMD_GET_ANNOUNCEMENTS + "/" + nubeeId + "/" + deviceId;
			result = Services.ConnectionResults.Busy;
			yield return SingletonComponent<JobManager>.Instance.StartCoroutine(WaitForQuery(cmd, null));
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
			AnnounceAndFriendNotifyData tempAnnData = null;
			if (SingletonComponent<AnnouncementManager>.Instance != null && payload != null && payload.HasField("announcements"))
			{
				JSONObject annPayload = payload["announcements"];
				if (annPayload != null && annPayload.type != JSONObject.Type.NULL && !annPayload.IsEmptyArray())
				{
					tempAnnData = new AnnounceAndFriendNotifyData();
					foreach (JSONObject tempAnnPayload in annPayload.list)
					{
						AnnouncementData tempData = ServiceCallUtils.ParseAnnouncementData(tempAnnPayload);
						if (tempAnnData.Annoucements.ContainsKey(tempData.DbId))
						{
							tempAnnData.Annoucements.Remove(tempData.DbId);
						}
						tempAnnData.Annoucements.Add(tempData.DbId, tempData);
					}
				}
			}
			returnValue = tempAnnData;
			PerformSuccess();
		}
	}
}
