using System.Collections.Generic;

namespace Server.Models
{
	public sealed class RankPageInfoHolderModel : IObjectModel
	{
		public List<uint> userList;

		public RankPageInfoTeamModel[] teamList;
	}
}
