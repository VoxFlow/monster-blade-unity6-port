using System;
using UnityEngine;

public class WeiboUser
{
	public string uid = "-1";

	public string name;

	public string imgUrl;

	private int expiryDay = 1;

	public bool InviteSent
	{
		get
		{
			long p_outResult = 0L;
			long unixEpochTime = TimeUtil.GetUnixEpochTime();
			string p_value = MBPrefs.GetString("weibouser." + uid + ".ts");
			NBGeneric.TryParse<long>(p_value, out p_outResult);
			if (p_outResult == 0L)
			{
				MBPrefs.SetInt("weibouser." + uid + ".InviteSent", 0);
			}
			else
			{
				long num = unixEpochTime - p_outResult;
				TimeSpan timeSpan = new TimeSpan(0, 0, (int)num);
				if (timeSpan.Days >= expiryDay || timeSpan.Days < 0)
				{
					MBPrefs.SetInt("weibouser." + uid + ".InviteSent", 0);
				}
			}
			return MBPrefs.GetInt("weibouser." + uid + ".InviteSent", 0) > 0;
		}
		set
		{
			if (uid != "-1")
			{
				MBPrefs.SetInt("weibouser." + uid + ".InviteSent", value ? 1 : 0);
				MBPrefs.SetString("weibouser." + uid + ".ts", TimeUtil.GetUnixEpochTime().ToString());
			}
		}
	}
}
