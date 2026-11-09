using System.Collections;
using UnityEngine;

namespace ServiceCalls
{
	public class SendDeviceTokenToServer : Services.ServiceHandle
	{
		public string nubeeId = string.Empty;

		public string token = string.Empty;

		public string language = string.Empty;

		public override bool IsKeyRequired()
		{
			return false;
		}

		public override bool IsAnalyticsActivity()
		{
			return false;
		}

		public override IEnumerator Execute()
		{
			string deviceType = "1";
			if (Application.platform == RuntimePlatform.Android)
			{
				deviceType = "2";
			}
			string cmd = GLOBAL.SRVCMD_SEND_DEVICE_TOKEN;
			WWWForm form = new WWWForm();
			form.AddField("nubee_id", nubeeId);
			form.AddField("device_type_id", deviceType);
			form.AddField("game_id", GLOBAL.SERVER_NOTIFICATION_GAME_ID);
			form.AddField("token", token);
			form.AddField("lang", language);
			result = Services.ConnectionResults.Busy;
			yield return SingletonComponent<JobManager>.Instance.StartCoroutine(WaitForQuery(cmd, form));
			if (UrlConnection == null)
			{
				PerformFailClientError(0 - result, "timeout");
				yield break;
			}
			if (UrlConnection.error != null)
			{
				result = Services.ConnectionResults.CommandFailure;
				PerformFail(GLOBAL.SERVER_ERROR_TAG + UrlConnection.error);
				yield break;
			}
			if (UrlConnection.text == null || UrlConnection.text == string.Empty)
			{
				result = Services.ConnectionResults.NoResults;
				PerformFail(GLOBAL.SERVER_ERROR_TAG + UrlConnection.error);
				yield break;
			}
			JSONObject jsonObj = new JSONObject(UrlConnection.text);
			if (jsonObj != null && jsonObj.list != null && jsonObj.list.Count > 0)
			{
				jsonObj = (JSONObject)jsonObj.list[0];
				int code = (int)jsonObj["code"].n;
				if (code != 200)
				{
					result = Services.ConnectionResults.CommandFailure;
					returnValue = code;
					PerformFail(GLOBAL.SERVER_ERROR_TAG + jsonObj["message"].str);
				}
				else
				{
					returnValue = null;
					PerformSuccess();
				}
			}
			else
			{
				Dbg.LogError("The returned jsonObj is null, or jsonObj.list is null, or jsonObj.list has no data, possible reason: the payload from server or HTTP is incorrect JSON string");
				result = Services.ConnectionResults.CommandFailure;
				PerformFail(GLOBAL.SERVER_ERROR_TAG + "Invalid JSON string : " + UrlConnection.text);
			}
		}
	}
}
