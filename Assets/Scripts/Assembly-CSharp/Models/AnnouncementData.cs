using System;
using System.Collections.Generic;
using UnityEngine;

namespace Models
{
	public class AnnouncementData
	{
		public int DbId;

		public AnnoucementTypeId AnnTypeId = AnnoucementTypeId.EVENT;

		public string StartDate = string.Empty;

		public string EndDate = string.Empty;

		public string EventId = string.Empty;

		public string Name = string.Empty;

		public AnnouncementMessage AnnounceMessage = new AnnouncementMessage();

		public Dictionary<int, AnnouncementReward> AnnouncementRewards = new Dictionary<int, AnnouncementReward>();

		public bool IsViewed
		{
			get
			{
				return MBPrefs.GetString("AnnouncementData.DbId." + DbId + ".ViewDate", string.Empty) != string.Empty;
			}
			set
			{
				string text = "0";
				MBPrefs.SetString(value: (!value) ? "0" : DateTime.UtcNow.ToString("u"), key: "AnnouncementData.DbId." + DbId + ".ViewDate");
			}
		}
	}
}
